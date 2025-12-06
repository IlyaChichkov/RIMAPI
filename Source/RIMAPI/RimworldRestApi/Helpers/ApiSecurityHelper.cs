using System.Text;

namespace RIMAPI.Helpers
{
    public static class ApiSecurityHelper
    {
        public static string SanitizeLetterInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // Remove null characters and control characters (except newlines and tabs)
            var cleaned = new StringBuilder(input.Length);
            foreach (char c in input)
            {
                // Allow: letters, numbers, punctuation, spaces, newlines, tabs
                // Block: control characters, null bytes, special Unicode
                if (
                    c == '\n'
                    || c == '\r'
                    || c == '\t'
                    || (c >= 32 && c < 127)
                    || (c >= 160 && c <= 255)
                )
                {
                    cleaned.Append(c);
                }
            }

            // Trim excessive whitespace
            string result = cleaned.ToString().Trim();

            // Collapse multiple consecutive newlines (max 2)
            while (result.Contains("\n\n\n"))
            {
                result = result.Replace("\n\n\n", "\n\n");
            }

            return result;
        }
    }
}
