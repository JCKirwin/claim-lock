namespace ClaimLock.Demo.Tests;

public class ReleaseTests
{
    [Fact]
    public void Release_ByOwner_DeletesClaim()
    {
        var store = new InMemoryClaimStore();
        var manager = new ClaimManager(store);

        manager.Acquire("recipe-1", "chef-alice", TimeSpan.FromMinutes(2));
        var released = manager.Release("recipe-1", "chef-alice");

        Assert.True(released);
        Assert.False(store.Exists("recipe-1"));
    }

    [Fact]
    public void Release_ByNonOwner_IsNoOp()
    {
        var store = new InMemoryClaimStore();
        var manager = new ClaimManager(store);

        manager.Acquire("recipe-1", "chef-alice", TimeSpan.FromMinutes(2));
        var released = manager.Release("recipe-1", "chef-bob");

        Assert.False(released);
        Assert.True(store.Exists("recipe-1"));
    }

    [Fact]
    public void Release_WhenNoClaim_ReturnsFalse()
    {
        var store = new InMemoryClaimStore();
        var manager = new ClaimManager(store);

        var released = manager.Release("recipe-1", "chef-alice");

        Assert.False(released);
    }
}
