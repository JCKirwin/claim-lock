using Microsoft.Extensions.Time.Testing;

namespace ClaimLock.Demo.Tests;

public class TtlExpiryTests
{
    [Fact]
    public void Acquire_AfterTtlExpires_ReclainsResource()
    {
        var store = new InMemoryClaimStore();
        var fakeTime = new FakeTimeProvider(DateTimeOffset.UtcNow);
        var manager = new ClaimManager(store, fakeTime);

        manager.Acquire("recipe-1", "chef-alice", TimeSpan.FromMinutes(2));

        fakeTime.Advance(TimeSpan.FromMinutes(3));

        var result = manager.Acquire("recipe-1", "chef-bob", TimeSpan.FromMinutes(2));

        Assert.Equal(AcquireOutcome.Acquired, result.Outcome);
        Assert.Equal("chef-bob", result.Claim!.Owner);
    }

    [Fact]
    public void Acquire_BeforeTtlExpires_IsBlocked()
    {
        var store = new InMemoryClaimStore();
        var fakeTime = new FakeTimeProvider(DateTimeOffset.UtcNow);
        var manager = new ClaimManager(store, fakeTime);

        manager.Acquire("recipe-1", "chef-alice", TimeSpan.FromMinutes(2));

        fakeTime.Advance(TimeSpan.FromMinutes(1));

        var result = manager.Acquire("recipe-1", "chef-bob", TimeSpan.FromMinutes(2));

        Assert.Equal(AcquireOutcome.AlreadyHeld, result.Outcome);
    }

    [Fact]
    public void CrashedProcess_ClaimSelfHeals_AfterTtl()
    {
        var store = new InMemoryClaimStore();
        var fakeTime = new FakeTimeProvider(DateTimeOffset.UtcNow);
        var manager = new ClaimManager(store, fakeTime);

        // Simulate crash: acquire and never release.
        manager.Acquire("recipe-1", "crashed-chef", TimeSpan.FromMinutes(2));

        fakeTime.Advance(TimeSpan.FromMinutes(2));

        var result = manager.Acquire("recipe-1", "recovery-chef", TimeSpan.FromMinutes(2));

        Assert.Equal(AcquireOutcome.Acquired, result.Outcome);
        Assert.Equal("recovery-chef", result.Claim!.Owner);
    }
}
