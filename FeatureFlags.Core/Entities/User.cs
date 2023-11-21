namespace FeatureFlags.Core.Entities
{
    public sealed record User
    {
        public int Id { get; init; }
        public required string Username { get; init; }
        public required string Email { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? ModifiedAt { get; init; }
        public int? Flags { get; init; }
    }
}
