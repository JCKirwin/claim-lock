namespace ClaimLock.Demo.Tests;

public class ClaimRecordTests
{
    [Fact]
    public void ClaimRecord_ContainsValidJson_Fields()
    {
        var now = DateTime.UtcNow;
        var claim = new ClaimRecord
        {
            Resource = "garlic-chicken",
            Owner = "chef-alice",
            AcquiredAt = now,
            ExpiresAt = now.AddMinutes(2),
        };

        Assert.Equal("garlic-chicken", claim.Resource);
        Assert.Equal("chef-alice", claim.Owner);
        Assert.Equal(now, claim.AcquiredAt);
        Assert.Equal(now.AddMinutes(2), claim.ExpiresAt);
    }

    [Fact]
    public void IsExpired_BeforeExpiry_ReturnsFalse()
    {
        var now = DateTime.UtcNow;
        var claim = new ClaimRecord
        {
            Resource = "test",
            Owner = "owner",
            AcquiredAt = now,
            ExpiresAt = now.AddMinutes(2),
        };

        Assert.False(claim.IsExpired(now.AddMinutes(1)));
    }

    [Fact]
    public void IsExpired_AfterExpiry_ReturnsTrue()
    {
        var now = DateTime.UtcNow;
        var claim = new ClaimRecord
        {
            Resource = "test",
            Owner = "owner",
            AcquiredAt = now,
            ExpiresAt = now.AddMinutes(2),
        };

        Assert.True(claim.IsExpired(now.AddMinutes(3)));
    }
}
