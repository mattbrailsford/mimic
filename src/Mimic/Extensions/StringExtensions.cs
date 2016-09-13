using System;

namespace Mimic.Extensions
{
    internal static class StringExtensions
    {
        public static string MakeAliasSafe(this string str)
        {
            return str.ToLower().Trim();
        }

        public static string ToPascalCase(this string str)
        {
            // If there are 0 or 1 characters, just return the string.
            if (str == null) return str;
            if (str.Length < 2) return str.ToUpper();

            // Split the string into words.
            var words = str.Split(
                new char[] { },
                StringSplitOptions.RemoveEmptyEntries);

            // Combine the words.
            var result = "";
            foreach (var word in words)
            {
                result +=
                    word.Substring(0, 1).ToUpper() +
                    word.Substring(1);
            }

            return result;
        }

        public static string ToCamelCase(this string str)
        {
            // If there are 0 or 1 characters, just return the string.
            if (str == null || str.Length < 2)
                return str;

            // Split the string into words.
            var words = str.Split(
                new char[] { },
                StringSplitOptions.RemoveEmptyEntries);

            // Combine the words.
            var result = "";
            for (var i = 0; i < words.Length; i++)
            {
                result +=
                    (i == 0 ? words[i].Substring(0, 1).ToUpper() : words[i].Substring(0, 1).ToLower()) +
                    words[i].Substring(1);
            }

            return result;
        }
    }
}
