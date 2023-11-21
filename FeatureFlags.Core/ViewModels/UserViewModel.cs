using FeatureFlags.Core.Enums;

namespace FeatureFlags.Core.ViewModels
{
    public sealed class UserViewModel
    {
        public required string Username { get; set; }
        public required string Email { get; set; }
        public UserFlags Flags { get; set; }
    }
}
