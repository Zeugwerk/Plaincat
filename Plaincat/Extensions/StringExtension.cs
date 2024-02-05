namespace Plaincat.Extensions
{
    public static class String
    {
        public static string JoinNonNulls(string separator, IEnumerable<string> strings)
        {
            if (strings == null)
                return "";

            return string.Join(separator, strings.Where(s => !string.IsNullOrEmpty(s)));
        }
        public static string JoinNonNulls(string separator, params string[] str)
        {
            if (str == null)
                return "";

            return string.Join(separator, str.Where(s => !string.IsNullOrEmpty(s)));
        }
    }
}

