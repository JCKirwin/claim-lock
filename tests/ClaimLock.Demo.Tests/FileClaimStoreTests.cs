namespace ClaimLock.Demo.Tests;

public class FileClaimStoreTests : IDisposable
{
    private readonly string _testDir;
    private readonly FileClaimStore _store;

    public FileClaimStoreTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), $"claim-lock-test-{Guid.NewGuid():N}");
        _store = new FileClaimStore(_testDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDir))
        {
            Directory.Delete(_testDir, true);
        }
    }

    [Fact]
    public void TryCreate_NewResource_ReturnsTrue()
    {
        var claim = MakeClaim("recipe-1", "chef-alice");
        Assert.True(_store.TryCreate(claim));
    }

    [Fact]
    public void TryCreate_ExistingResource_ReturnsFalse()
    {
        var claim = MakeClaim("recipe-1", "chef-alice");
        _store.TryCreate(claim);

        var duplicate = MakeClaim("recipe-1", "chef-bob");
        Assert.False(_store.TryCreate(duplicate));
    }

    [Fact]
    public void Read_ExistingClaim_ReturnsRecord()
    {
        var claim = MakeClaim("recipe-1", "chef-alice");
        _store.TryCreate(claim);

        var loaded = _store.Read("recipe-1");
        Assert.NotNull(loaded);
        Assert.Equal("chef-alice", loaded.Owner);
        Assert.Equal("recipe-1", loaded.Resource);
    }

    [Fact]
    public void Read_NonExistent_ReturnsNull()
    {
        Assert.Null(_store.Read("nope"));
    }

    [Fact]
    public void Delete_RemovesClaimFile()
    {
        _store.TryCreate(MakeClaim("recipe-1", "chef-alice"));
        _store.Delete("recipe-1");
        Assert.False(_store.Exists("recipe-1"));
    }

    private static ClaimRecord MakeClaim(string resource, string owner) => new()
    {
        Resource = resource,
        Owner = owner,
        AcquiredAt = DateTime.UtcNow,
        ExpiresAt = DateTime.UtcNow.AddMinutes(2),
    };
}
