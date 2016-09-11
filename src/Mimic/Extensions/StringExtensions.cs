namespace Mimic.Extensions
{
    internal static class StringExtensions
    {
        public static string MakeAliasSafe(this string str)
        {
            return str.ToLower().Trim();
        }
    }
}
