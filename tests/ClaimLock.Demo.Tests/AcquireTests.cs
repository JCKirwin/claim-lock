namespace ClaimLock.Demo.Tests;

public class AcquireTests
{
    [Fact]
    public void Acquire_WhenResourceFree_ReturnsAcquired()
    {
        var store = new InMemoryClaimStore();
        var manager = new ClaimManager(store);

        var result = manager.Acquire("recipe-1", "chef-alice", TimeSpan.FromMinutes(2));

        Assert.Equal(AcquireOutcome.Acquired, result.Outcome);
        Assert.NotNull(result.Claim);
        Assert.Equal("recipe-1", result.Claim.Resource);
        Assert.Equal("chef-alice", result.Claim.Owner);
    }

    [Fact]
    public void Acquire_WhenResourceHeld_ReturnsAlreadyHeld()
    {
        var store = new InMemoryClaimStore();
        var manager = new ClaimManager(store);

        manager.Acquire("recipe-1", "chef-alice", TimeSpan.FromMinutes(2));
        var result = manager.Acquire("recipe-1", "chef-bob", TimeSpan.FromMinutes(2));

        Assert.Equal(AcquireOutcome.AlreadyHeld, result.Outcome);
        Assert.NotNull(result.ExistingHolder);
        Assert.Equal("chef-alice", result.ExistingHolder.Owner);
    }

    [Fact]
    public void Acquire_SameOwerWithoutRelease_IsConflict()
    {
        var store = new InMemoryClaimStore();
        var manager = new ClaimManager(store);

        manager.Acquire("recipe-1", "chef-alice", TimeSpan.FromMinutes(2));
        var result = manager.Acquire("recipe-1", "chef-alice", TimeSpan.FromMinutes(2));

        Assert.Equal(AcquireOutcome.AlreadyHeld, result.Outcome);
    }
}
