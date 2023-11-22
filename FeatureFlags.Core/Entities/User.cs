namespace FeatureFlags.Core.Entities
{
    public sealed record User
    {
        public int Id { get; init; }
        public required string Username { get; init; }
        public required string Email { get; init; }
        public DateTimeOffset CreatedAt { get; init; }
        public DateTimeOffset? ModifiedAt { get; init; }
        public int? Flags { get; init; }
    }
}
