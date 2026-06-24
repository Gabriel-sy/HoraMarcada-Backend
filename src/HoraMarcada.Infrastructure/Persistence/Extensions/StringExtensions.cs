using System.Globalization;
using System.Text;

namespace HoraMarcada.Infrastructure.Persistence.Extensions;

public static class StringExtensions
{
    public static string RemoveDiacritics(this string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return text;
        }

        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();

        foreach (var c in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        var result = stringBuilder.ToString().Normalize(NormalizationForm.FormC);

        if (result == text)
        {
            var sb = new StringBuilder();
            foreach (var c in text)
            {
                sb.Append(GetNonDiacriticChar(c));
            }
            result = sb.ToString();
        }

        return result;
    }

    private static char GetNonDiacriticChar(char c)
    {
        return c switch
        {
            'á' or 'à' or 'â' or 'ã' or 'ä' or 'å' => 'a',
            'Á' or 'À' or 'Â' or 'Ã' or 'Ä' or 'Å' => 'A',
            'é' or 'è' or 'ê' or 'ë' => 'e',
            'É' or 'È' or 'Ê' or 'Ë' => 'E',
            'í' or 'ì' or 'î' or 'ï' => 'i',
            'Í' or 'Ì' or 'Î' or 'Ï' => 'I',
            'ó' or 'ò' or 'ô' or 'õ' or 'ö' or 'ø' => 'o',
            'Ó' or 'Ò' or 'Ô' or 'Õ' or 'Ö' or 'Ø' => 'O',
            'ú' or 'ù' or 'û' or 'ü' => 'u',
            'Ú' or 'Ù' or 'Û' or 'Ü' => 'U',
            'ç' => 'c',
            'Ç' => 'C',
            'ñ' => 'n',
            'Ñ' => 'N',
            _ => c
        };
    }
}
