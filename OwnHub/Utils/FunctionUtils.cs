using System;
using System.IO;
using System.Reflection;

namespace OwnHub.Utils
{
    public static class FunctionUtils
    {
        private static readonly Assembly _assembly = typeof(OwnHub.Program).Assembly;

        public static string? GetApplicationRoot()
        {
            return Path.GetDirectoryName(_assembly.Location);
        }

        public static Assembly GetApplicationAssembly() {
            return _assembly;
        }

        public static string RandomString(int length = 8)
        {
            const string? chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[length];
            var random = new Random();

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            return new String(stringChars);
        }
    }
}
