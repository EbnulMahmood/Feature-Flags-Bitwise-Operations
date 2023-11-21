using System.ComponentModel.DataAnnotations;

namespace FeatureFlags.Core.Enums
{
    // Limit 8 flags [1 - 128]
    [Flags]
    public enum UserFlags
    {
        None = 0,

        [Display(Name = "Dark Mode")]
        DarkMode = 1,

        [Display(Name = "Super Admin")]
        SuperAdmin = 2,

        [Display(Name = "Notification Opt-In")]
        NotificationOptIn = 4,

        [Display(Name = "Metered Billing")]
        MeteredBilling = 8,

        [Display(Name = "Rollout Chat")]
        RolloutChat = 16,

        [Display(Name = "Experiment Blue")]
        ExperimentBlue = 32,

        [Display(Name = "Log Verbose")]
        LogVerbose = 64,

        [Display(Name = "New Legal Disclaimer")]
        NewLegalDisclaimer = 128,
    }
}
