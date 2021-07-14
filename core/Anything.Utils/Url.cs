using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Anything.Utils
{
    public record Url
    {
        private static readonly Regex _schemePattern = new("^\\w[\\w\\d+.-]*$", RegexOptions.Compiled | RegexOptions.ECMAScript);

        private static readonly Regex _uriRegex =
            new("^(([^:/?#]+?):)?(\\/\\/([^/?#]*))?([^?#]*)(\\?([^#]*))?(#(.*))?", RegexOptions.Compiled | RegexOptions.ECMAScript);

        /// <summary>
        ///     https://tools.ietf.org/html/rfc3986#section-2.2
        /// </summary>
        private static readonly Dictionary<char, string> _encodeTable = new()
        {
            { ':', "%3A" }, // gen-delims
            { '/', "%2F" },
            { '?', "%3F" },
            { '#', "%23" },
            { '[', "%5B" },
            { ']', "%5D" },
            { '@', "%40" },
            { '!', "%21" }, // sub-delims
            { '$', "%24" },
            { '&', "%26" },
            { '\'', "%27" },
            { '(', "%28" },
            { ')', "%29" },
            { '*', "%2A" },
            { '+', "%2B" },
            { ',', "%2C" },
            { ';', "%3B" },
            { '=', "%3D" },
            { ' ', "%20" }
        };

        private readonly string _path = "";


        private readonly string _scheme = "";

        public Url(string scheme, string authority)
        {
            Scheme = scheme;
            Authority = authority;
        }

        public Url(string scheme, string authority, string path)
        {
            Scheme = scheme;
            Authority = authority;
            Path = path;
        }

        public Url(string scheme, string authority, string path, string query, string fragment)
        {
            Scheme = scheme;
            Authority = authority;
            Path = path;
            Query = query;
            Fragment = fragment;
        }

        public string Scheme
        {
            get => _scheme;
            init
            {
                if (value == string.Empty)
                {
                    throw new ArgumentException("[UriError]: Scheme is missing");
                }

                if (!_schemePattern.IsMatch(value))
                {
                    throw new ArgumentException("[UriError]: Scheme contains illegal characters.");
                }

                _scheme = value;
            }
        }

        public string Authority { get; init; } = "";

        public string Path
        {
            get => _path;

            init
            {
                value = value.Trim();
                if (value == string.Empty)
                {
                    _path = "/";
                }
                else if (value[0] != '/')
                {
                    _path = '/' + value;
                }
                else if (value.StartsWith("//"))
                {
                    throw new ArgumentException("[UriError]: The path cannot begin with two slash characters (\"//\")");
                }
                else
                {
                    _path = value;
                }
            }
        }

        public string Query { get; init; } = "";

        public string Fragment { get; init; } = "";

        public static Url Parse(string value)
        {
            var match = _uriRegex.Match(value);
            return new Url(
                match.Groups[2].Value,
                Uri.UnescapeDataString(match.Groups[4].Value),
                Uri.UnescapeDataString(match.Groups[5].Value),
                match.Groups[7].Value,
                Uri.UnescapeDataString(match.Groups[9].Value));
        }

        public Url JoinPath(string fragment)
        {
            return this with { Path = PathLib.Join(Path, fragment) };
        }

        public Url Dirname()
        {
            return this with { Path = PathLib.Dirname(Path) };
        }

        public string Basename()
        {
            return PathLib.Basename(Path);
        }

        public override string ToString()
        {
            return AsFormatted();
        }

        public string AsFormatted()
        {
            var res = "";
            if (Scheme != string.Empty)
            {
                res += Scheme;
                res += ':';
            }

            res += '/';
            res += '/';

            var authority = Authority;
            if (authority != string.Empty)
            {
                var idx = authority.IndexOf('@');
                if (idx != -1)
                {
                    // <user>@<auth>
                    var userinfo = authority.Substring(0, idx);
                    authority = authority.Substring(idx + 1);
                    idx = userinfo.IndexOf(':');
                    if (idx == -1)
                    {
                        res += UrlEscapeComponent(userinfo, false);
                    }
                    else
                    {
                        // <user>:<pass>@<auth>
                        res += UrlEscapeComponent(userinfo.Substring(0, idx), false);
                        res += ':';
                        res += UrlEscapeComponent(userinfo.Substring(idx + 1), false);
                    }

                    res += '@';
                }

                authority = authority.ToLower();
                idx = authority.IndexOf(':');
                if (idx == -1)
                {
                    res += UrlEscapeComponent(authority, false);
                }
                else
                {
                    // <auth>:<port>
                    res += UrlEscapeComponent(authority.Substring(0, idx), false);
                    res += authority.Substring(idx);
                }
            }

            var path = Path;
            if (path != string.Empty)
            {
                // encode the rest of the path
                res += UrlEscapeComponent(path, true);
            }

            if (Query != string.Empty)
            {
                res += '?';
                res += UrlEscapeComponent(Query, false);
            }

            if (Fragment != string.Empty)
            {
                res += '#';
                res += UrlEscapeComponent(Fragment, false);
            }

            return res;
        }

        private static string UrlEscapeComponent(string uriComponent, bool allowSlash)
        {
            string? res = null;
            var nativeEncodePos = -1;

            for (var pos = 0; pos < uriComponent.Length; pos++)
            {
                var code = uriComponent[pos];

                // unreserved characters: https://tools.ietf.org/html/rfc3986#section-2.3
                if (
                    code is (>= 'a' and <= 'z')
                        or (>= 'A' and <= 'Z')
                        or (>= '0' and <= '9')
                        or '-' or '.' or '_' or '~' ||
                    (code == '/' && allowSlash)
                )
                {
                    // check if we are delaying native encode
                    if (nativeEncodePos != -1)
                    {
                        res += Uri.EscapeDataString(uriComponent.Substring(nativeEncodePos, pos - nativeEncodePos));
                        nativeEncodePos = -1;
                    }

                    // check if we write into a new string (by default we try to return the param)
                    if (res != null)
                    {
                        res += uriComponent[pos];
                    }
                }
                else
                {
                    // encoding needed, we need to allocate a new string
                    if (res == null)
                    {
                        res = uriComponent.Substring(0, pos);
                    }

                    // check with default table first
                    _encodeTable.TryGetValue(code, out var escaped);
                    if (escaped != null)
                    {
                        // check if we are delaying native encode
                        if (nativeEncodePos != -1)
                        {
                            res += Uri.EscapeDataString(uriComponent.Substring(nativeEncodePos, pos - nativeEncodePos));
                            nativeEncodePos = -1;
                        }

                        // append escaped variant to result
                        res += escaped;
                    }
                    else if (nativeEncodePos == -1)
                    {
                        // use native encode only when needed
                        nativeEncodePos = pos;
                    }
                }
            }

            if (nativeEncodePos != -1)
            {
                res += Uri.EscapeDataString(uriComponent.Substring(nativeEncodePos));
            }

            return res != null ? res : uriComponent;
        }
    }
}
