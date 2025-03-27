using System.Globalization;

namespace Gamesbakery.Core
{
    public static class CountryProvider
    {
        private static readonly HashSet<string> ValidCountries;

        static CountryProvider()
        {
            ValidCountries = new HashSet<string>();

            foreach (var region in CultureInfo.GetCultures(CultureTypes.SpecificCultures)
                .Select(culture => new RegionInfo(culture.Name))
                .DistinctBy(region => region.EnglishName))
            {
                ValidCountries.Add(region.EnglishName);
            }
        }

        public static bool IsValidCountry(string country)
        {
            return ValidCountries.Contains(country);
        }

        public static IReadOnlyCollection<string> GetValidCountries()
        {
            return ValidCountries;
        }
    }
}