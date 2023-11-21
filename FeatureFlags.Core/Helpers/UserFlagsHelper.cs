using FeatureFlags.Core.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace FeatureFlags.Core.Helper
{
    public static class UserFlagsHelper
    {
        public static int GetCombinedFlags(params UserFlags[] flags)
        {
            if (flags == null)
            {
                throw new ArgumentNullException(nameof(flags), "Flags cannot be null.");
            }

            return flags.Aggregate(0, (current, flag) => current | (int)flag);
        }

        public static IEnumerable<string> GetIndividualFlags(int? combinedFlags)
        {
            if (!combinedFlags.HasValue || combinedFlags.Value == 0)
            {
                return new List<string> { "None" };
            }

            var displayAttributeCache = new Dictionary<UserFlags, string>();

            var flags = Enum.GetValues(typeof(UserFlags)).Cast<UserFlags>()
                            .Where(flag => (combinedFlags & (int)flag) != 0);

            var result = flags.Select(flag =>
            {
                if (!displayAttributeCache.TryGetValue(flag, out var displayName))
                {
                    var memInfo = typeof(UserFlags).GetMember(flag.ToString());
                    var displayAttribute = memInfo[0].GetCustomAttributes(typeof(DisplayAttribute), false)
                                                    .OfType<DisplayAttribute>()
                                                    .FirstOrDefault();

                    displayName = displayAttribute?.Name ?? flag.ToString();
                    displayAttributeCache[flag] = displayName;
                }

                return displayName;
            }).ToList();

            return result;
        }

        public static IEnumerable<SelectListItem> GetFlagFilterItems()
        {
            var flags = Enum.GetValues(typeof(UserFlags)).Cast<UserFlags>();
            var items = flags.Select(flag => new SelectListItem
            {
                Text = GetDisplayName(flag),
                Value = ((int)flag).ToString()
            });

            return items;
        }

        private static string GetDisplayName(Enum value)
        {
            return value.GetType()
                .GetMember(value.ToString())
                .FirstOrDefault()?
                .GetCustomAttribute<DisplayAttribute>()?
                .GetName() ?? value.ToString();
        }
    }
}
