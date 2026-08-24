using System.Collections.Generic;
using System.Text;
namespace Maqloub.Services;

public sealed class KeyboardLayoutConverter
{
    private static readonly Dictionary<char, string> EnglishToArabic = new()
    {
        ['`'] = "ذ",
        ['q'] = "ض",
        ['w'] = "ص",
        ['e'] = "ث",
        ['r'] = "ق",
        ['t'] = "ف",
        ['y'] = "غ",
        ['u'] = "ع",
        ['i'] = "ه",
        ['o'] = "خ",
        ['p'] = "ح",
        ['['] = "ج",
        [']'] = "د",
        ['a'] = "ش",
        ['s'] = "س",
        ['d'] = "ي",
        ['f'] = "ب",
        ['g'] = "ل",
        ['h'] = "ا",
        ['j'] = "ت",
        ['k'] = "ن",
        ['l'] = "م",
        [';'] = "ك",
        ['\''] = "ط",
        ['z'] = "ئ",
        ['x'] = "ء",
        ['c'] = "ؤ",
        ['v'] = "ر",
        ['b'] = "لا",
        ['n'] = "ى",
        ['m'] = "ة",
        [','] = "و",
        ['.'] = "ز",
        ['/'] = "ظ"
    };

    private static readonly Dictionary<char, char> ArabicToEnglish = new()
    {
        ['ذ'] = '`',
        ['ض'] = 'q',
        ['ص'] = 'w',
        ['ث'] = 'e',
        ['ق'] = 'r',
        ['ف'] = 't',
        ['غ'] = 'y',
        ['ع'] = 'u',
        ['ه'] = 'i',
        ['خ'] = 'o',
        ['ح'] = 'p',
        ['ج'] = '[',
        ['د'] = ']',
        ['ش'] = 'a',
        ['س'] = 's',
        ['ي'] = 'd',
        ['ب'] = 'f',
        ['ل'] = 'g',
        ['ا'] = 'h',
        ['ت'] = 'j',
        ['ن'] = 'k',
        ['م'] = 'l',
        ['ك'] = ';',
        ['ط'] = '\'',
        ['ئ'] = 'z',
        ['ء'] = 'x',
        ['ؤ'] = 'c',
        ['ر'] = 'v',
        ['ى'] = 'n',
        ['ة'] = 'm',
        ['و'] = ',',
        ['ز'] = '.',
        ['ظ'] = '/'
    };

    public string Convert(string text, string firstLanguage, string secondLanguage)
    {
        var isArabicEnglishPair =
    IsArabicLayout(firstLanguage) &&
    secondLanguage == "English" ||
    firstLanguage == "English" &&
    IsArabicLayout(secondLanguage);

        if (!isArabicEnglishPair)
            return text;

        return ContainsArabic(text)
            ? ConvertArabicToEnglish(text)
            : ConvertEnglishToArabic(text);
    }

    private static bool ContainsArabic(string text)
    {
        foreach (var character in text)
        {
            if (character is >= '\u0600' and <= '\u06FF')
                return true;
        }

        return false;
    }

    private static string ConvertEnglishToArabic(string text)
    {
        var result = new StringBuilder();

        foreach (var character in text)
        {
            var lowerCharacter =
                char.ToLowerInvariant(character);

            if (EnglishToArabic.TryGetValue(
                    lowerCharacter,
                    out var replacement))
            {
                result.Append(replacement);
            }
            else
            {
                result.Append(character);
            }
        }

        return result.ToString();
    }

    private static string ConvertArabicToEnglish(string text)
    {
        var result = new StringBuilder();

        for (var index = 0; index < text.Length; index++)
        {
            if (index + 1 < text.Length &&
                text[index] == 'ل' &&
                text[index + 1] == 'ا')
            {
                result.Append('b');
                index++;
                continue;
            }

            var character = text[index];

            if (ArabicToEnglish.TryGetValue(
                    character,
                    out var replacement))
            {
                result.Append(replacement);
            }
            else
            {
                result.Append(character);
            }
        }

        return result.ToString();
    }



    private static bool IsArabicLayout(string layout)
    {
        return layout is "Arabic" or "العربية";
    }
}