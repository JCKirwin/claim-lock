namespace ClaimLock.Demo;

public class ClaimManager
{
    private readonly IClaimStore _store;
    private readonly TimeProvider _timeProvider;

    public ClaimManager(IClaimStore store, TimeProvider? timeProvider = null)
    {
        _store = store;
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public AcquireResult Acquire(string resource, string owner, TimeSpan ttl)
    {
        var existing = _store.Read(resource);
        if (existing is not null)
        {
            var now = _timeProvider.GetUtcNow().UtcDateTime;
            if (!existing.IsExpired(now))
            {
                return AcquireResult.Held(existing);
            }

            _store.Delete(resource);
        }

        var claim = new ClaimRecord
        {
            Resource = resource,
            Owner = owner,
            AcquiredAt = _timeProvider.GetUtcNow().UtcDateTime,
            ExpiresAt = _timeProvider.GetUtcNow().UtcDateTime.Add(ttl),
        };

        if (_store.TryCreate(claim))
        {
            return AcquireResult.Success(claim);
        }

        var raceWinner = _store.Read(resource);
        return AcquireResult.Held(raceWinner ?? existing!);
    }

    public bool Release(string resource, string owner)
    {
        var existing = _store.Read(resource);
        if (existing is null)
        {
            return false;
        }

        if (!string.Equals(existing.Owner, owner, StringComparison.Ordinal))
        {
            return false;
        }

        _store.Delete(resource);
        return true;
    }

    public AcquireResult ForceOverride(string resource, string newOwner, TimeSpan ttl)
    {
        var existing = _store.Read(resource);
        if (existing is not null)
        {
            Console.WriteLine($"  [TAKEOVER] {newOwner} overriding {existing.Owner}'s claim on {resource}");
            _store.Delete(resource);
        }

        var claim = new ClaimRecord
        {
            Resource = resource,
            Owner = newOwner,
            AcquiredAt = _timeProvider.GetUtcNow().UtcDateTime,
            ExpiresAt = _timeProvider.GetUtcNow().UtcDateTime.Add(ttl),
        };

        _store.TryCreate(claim);
        return AcquireResult.Success(claim);
    }
}
