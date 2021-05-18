using System;

namespace StagingBox.Utils
{
    public static class PathLib
    {
        public static readonly char DirectorySeparatorChar = '/';

        public static string Resolve(params string[] paths)
        {
            string resolvedPath = "";
            var resolvedAbsolute = false;

            for (var i = paths.Length - 1; i >= -1 && !resolvedAbsolute; i--)
            {
                string path = i >= 0 ? paths[i] : "/";

                // Skip empty entries
                if (path.Length == 0)
                {
                    continue;
                }

                resolvedPath = path + "/" + resolvedPath;
                resolvedAbsolute = path[0] == DirectorySeparatorChar;
            }

            // At this point the path should be resolved to a full absolute path, but
            // handle relative paths to be safe (might happen when process.cwd() fails)

            // Normalize the path
            resolvedPath = NormalizeString(resolvedPath, !resolvedAbsolute);

            if (resolvedAbsolute)
            {
                return "/" + resolvedPath;
            }

            return resolvedPath.Length > 0 ? resolvedPath : ".";
        }

        public static string Join(params string[] paths)
        {
            if (paths.Length == 0)
            {
                return ".";
            }

            string joined = "";
            foreach (string path in paths)
            {
                if (path.Length <= 0)
                {
                    continue;
                }

                if (joined == "")
                {
                    joined = path;
                }
                else
                {
                    joined += DirectorySeparatorChar + path;
                }
            }

            if (joined == "")
            {
                return ".";
            }

            return Normalize(joined);
        }

        public static string Normalize(string path)
        {
            if (path.Length == 0)
            {
                return ".";
            }

            var isAbsolute = path[0] == DirectorySeparatorChar;
            var trailingSeparator = path[^1] == DirectorySeparatorChar;

            // Normalize the path
            path = NormalizeString(path, !isAbsolute);

            if (path.Length == 0)
            {
                if (isAbsolute)
                {
                    return "/";
                }

                return trailingSeparator ? "./" : ".";
            }

            if (trailingSeparator)
            {
                path += "/";
            }

            return isAbsolute ? "/" + path : path;
        }

        private static bool IsPathSeparator(char code)
        {
            return code == DirectorySeparatorChar;
        }

        /// <summary>
        ///     Resolves . and .. elements in a path with directory names
        /// </summary>
        private static string NormalizeString(string path, bool allowAboveRoot)
        {
            string res = "";
            var lastSegmentLength = 0;
            var lastSlash = -1;
            var dots = 0;
            var code = '\0';
            for (var i = 0; i <= path.Length; ++i)
            {
                if (i < path.Length)
                {
                    code = path[i];
                }
                else if (IsPathSeparator(code))
                {
                    break;
                }
                else
                {
                    code = DirectorySeparatorChar;
                }

                if (IsPathSeparator(code))
                {
                    if (lastSlash == i - 1 || dots == 1)
                    {
                        // NOOP
                    }
                    else if (dots == 2)
                    {
                        if (res.Length < 2 || lastSegmentLength != 2 ||
                            res[^1] != '.' ||
                            res[^2] != '.')
                        {
                            if (res.Length > 2)
                            {
                                var lastSlashIndex = res.LastIndexOf(DirectorySeparatorChar);
                                if (lastSlashIndex == -1)
                                {
                                    res = "";
                                    lastSegmentLength = 0;
                                }
                                else
                                {
                                    res = res.Substring(0, lastSlashIndex);
                                    lastSegmentLength = res.Length - 1 - res.LastIndexOf(DirectorySeparatorChar);
                                }

                                lastSlash = i;
                                dots = 0;
                                continue;
                            }

                            if (res.Length != 0)
                            {
                                res = "";
                                lastSegmentLength = 0;
                                lastSlash = i;
                                dots = 0;
                                continue;
                            }
                        }

                        if (allowAboveRoot)
                        {
                            res += res.Length > 0 ? DirectorySeparatorChar + ".." : "..";
                            lastSegmentLength = 2;
                        }
                    }
                    else
                    {
                        if (res.Length > 0)
                        {
                            res += DirectorySeparatorChar + path.Substring(lastSlash + 1, i - (lastSlash + 1));
                        }
                        else
                        {
                            res = path.Substring(lastSlash + 1, i - (lastSlash + 1));
                        }

                        lastSegmentLength = i - lastSlash - 1;
                    }

                    lastSlash = i;
                    dots = 0;
                }
                else if (code == '.' && dots != -1)
                {
                    ++dots;
                }
                else
                {
                    dots = -1;
                }
            }

            return res;
        }

        public static string Basename(string path)
        {
            var start = 0;
            var end = -1;
            var matchedSlash = true;

            for (var i = path.Length - 1; i >= 0; --i)
            {
                if (path[i] == DirectorySeparatorChar)
                {
                    // If we reached a path separator that was not part of a set of path
                    // separators at the end of the string, stop now
                    if (!matchedSlash)
                    {
                        start = i + 1;
                        break;
                    }
                }
                else if (end == -1)
                {
                    // We saw the first non-path separator, mark this as the end of our
                    // path component
                    matchedSlash = false;
                    end = i + 1;
                }
            }

            if (end == -1)
            {
                return "";
            }

            return path.Substring(start, end - start);
        }

        /// <summary>
        ///     The Extname() method returns the extension of the path, from the last occurrence
        ///     of the . (period) character to end of string in the last portion of the path.
        ///     If there is no . in the last portion of the path, or if there are no . characters
        ///     other than the first character of the basename of path (see <see cref="Basename" />),
        ///     an emptystring is returned.
        /// </summary>
        public static string Extname(string path)
        {
            var startDot = -1;
            var startPart = 0;
            var end = -1;
            var matchedSlash = true;
            // Track the state of characters (if any) we see before our first dot and
            // after any path separator we find
            var preDotState = 0;
            for (var i = path.Length - 1; i >= 0; --i)
            {
                var code = path[i];
                if (code == DirectorySeparatorChar)
                {
                    // If we reached a path separator that was not part of a set of path
                    // separators at the end of the string, stop now
                    if (!matchedSlash)
                    {
                        startPart = i + 1;
                        break;
                    }

                    continue;
                }

                if (end == -1)
                {
                    // We saw the first non-path separator, mark this as the end of our
                    // extension
                    matchedSlash = false;
                    end = i + 1;
                }

                if (code == '.')
                {
                    // If this is our first dot, mark it as the start of our extension
                    if (startDot == -1)
                    {
                        startDot = i;
                    }
                    else if (preDotState != 1)
                    {
                        preDotState = 1;
                    }
                }
                else if (startDot != -1)
                {
                    // We saw a non-dot and non-path separator before our dot, so we should
                    // have a good chance at having a non-empty extension
                    preDotState = -1;
                }
            }

            if (startDot == -1 ||
                end == -1 ||
                // We saw a non-dot character immediately before the dot
                preDotState == 0 ||
                // The (right-most) trimmed path component is exactly '..'
                preDotState == 1 &&
                startDot == end - 1 &&
                startDot == startPart + 1)
            {
                return "";
            }

            return path.Substring(startDot, end - startDot);
        }

        /// <summary>
        /// The method returns the directory name of a path, similar to the Unix dirname command. Trailing directory separators are ignored.
        /// </summary>
        public static string Dirname(string path)
        {
            if (path.Length == 0)
            {
                return ".";
            }

            var hasRoot = path[0] == DirectorySeparatorChar;
            var end = -1;
            var matchedSlash = true;
            for (var i = path.Length - 1; i >= 1; --i)
            {
                if (path[i] == DirectorySeparatorChar)
                {
                    if (!matchedSlash)
                    {
                        end = i;
                        break;
                    }
                }
                else
                {
                    // We saw the first non-path separator
                    matchedSlash = false;
                }
            }

            if (end == -1)
            {
                return hasRoot ? "/" : ".";
            }

            if (hasRoot && end == 1)
            {
                return "//";
            }

            return path.Substring(0, end);
        }

        public static string[] Split(string path)
        {
            return Normalize(path).Split(DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
