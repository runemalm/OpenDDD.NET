namespace OpenDDD.API.Extensions
{
    public static class OpenDddStringExtensions
    {
        public static string Pluralize(this string word)
        {
            if (string.IsNullOrWhiteSpace(word))
                return word;

            if (word.EndsWith("y") && !"aeiou".Contains(word[word.Length - 2]))
                return word.Substring(0, word.Length - 1) + "ies"; // e.g., "category" -> "categories"
            if (word.EndsWith("s") || word.EndsWith("x") || word.EndsWith("z") || word.EndsWith("sh") || word.EndsWith("ch"))
                return word + "es"; // e.g., "box" -> "boxes"
            return word + "s"; // Default pluralization
        }
    }
}
