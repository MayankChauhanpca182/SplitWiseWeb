namespace SplitWiseService.Helpers;

public static class TextHelper
{
    public static string CapitalizeWord(string word)
    {
        if (string.IsNullOrEmpty(word))
        {
            return word;
        }
        return char.ToUpper(word[0]) + word.Substring(1).ToLower();
    }

    public static string NormalizeEmail(string email)
    {
        return email?.ToLowerInvariant();
    }
}
