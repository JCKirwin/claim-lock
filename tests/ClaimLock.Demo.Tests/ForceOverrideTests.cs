namespace ClaimLock.Demo.Tests;

public class ForceOverrideTests
{
    [Fact]
    public void ForceOverride_ReplacesExistingClaim()
    {
        var store = new InMemoryClaimStore();
        var manager = new ClaimManager(store);

        manager.Acquire("recipe-1", "chef-alice", TimeSpan.FromMinutes(2));
        var result = manager.ForceOverride("recipe-1", "chef-bob", TimeSpan.FromMinutes(2));

        Assert.Equal(AcquireOutcome.Acquired, result.Outcome);
        Assert.Equal("chef-bob", result.Claim!.Owner);
    }

    [Fact]
    public void ForceOverride_WhenNoClaim_Acquires()
    {
        var store = new InMemoryClaimStore();
        var manager = new ClaimManager(store);

        var result = manager.ForceOverride("recipe-1", "chef-alice", TimeSpan.FromMinutes(2));

        Assert.Equal(AcquireOutcome.Acquired, result.Outcome);
    }
}
