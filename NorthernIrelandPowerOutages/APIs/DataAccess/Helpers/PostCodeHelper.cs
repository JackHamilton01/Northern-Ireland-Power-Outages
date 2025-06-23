using System.Text.RegularExpressions;

namespace DataAccess.Helpers
{
    public static class PostCodeHelper
    {
        private static readonly Regex ukPostcodeRegex = new Regex(
            @"^([A-Z]{1,2}[0-9][0-9A-Z]?)[ ]?([0-9][A-Z]{2})$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static bool IsValidUkPostcode(string? postcode)
        {
            if (string.IsNullOrWhiteSpace(postcode))
            {
                return false;
            }

            postcode = postcode.Trim().ToUpperInvariant();

            return ukPostcodeRegex.IsMatch(postcode);
        }
    }
}
