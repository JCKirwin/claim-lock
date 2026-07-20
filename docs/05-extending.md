# Extending the Claim Lock

This implementation is deliberately minimal. Here are the most common extensions and how to add them.

## Heartbeat renewal

Add a method to `ClaimManager` that extends the TTL of an active claim:

```csharp
public bool Renew(string resource, string owner, TimeSpan newTtl)
{
    var existing = _store.Read(resource);
    if (existing is null || existing.Owner != owner)
        return false;

    existing.ExpiresAt = _timeProvider.GetUtcNow().UtcDateTime.Add(newTtl);
    _store.Delete(resource);
    return _store.TryCreate(existing);
}
```

Call this from a background timer in the holder process. If the holder crashes, the timer stops and the TTL expires naturally.

## Claim metadata

Add a `Dictionary<string, string> Metadata` property to `ClaimRecord` for arbitrary key-value pairs:

```csharp
public Dictionary<string, string> Metadata { get; set; } = new();
```

Use it to store the holder's hostname, the reason for the claim, or a correlation ID. The metadata is persisted in the claim file and visible to anyone reading the claim.

## Waiter queue

For fairness under contention, add a ticket file alongside the claim file:

```csharp
var ticketPath = Path.Combine(dir, $"{resource}.ticket-{sequence}.json");
```

Each waiter writes a ticket with an incrementing sequence number. The claim manager checks tickets in order when releasing, notifying the next waiter (or simply letting the lowest-ticket waiter win the next `Acquire` race).

## Multi-resource locking

To lock multiple resources atomically, sort the resource names and acquire in order:

```csharp
var sorted = resources.OrderBy(r => r).ToList();
foreach (var resource in sorted)
{
    var result = manager.Acquire(resource, owner, ttl);
    if (result.Outcome != AcquireOutcome.Acquired)
    {
        // Roll back: release all previously acquired.
        foreach (var acquired in sorted.TakeWhile(r => r != resource))
            manager.Release(acquired, owner);
        return AcquireResult.Held(result.ExistingHolder!);
    }
}
```

Sorting prevents deadlocks when two processes try to lock overlapping resource sets.

## Event callbacks

Add an event to `ClaimManager` for observability:

```csharp
public event Action<string, string, string>? OnEvent;
// (eventType, resource, owner) — "acquired", "released", "expired", "overridden"
```

Fire it at each state transition. Hook it up to a logger, metrics system, or alert channel.

## Remote store

Replace `FileClaimStore` with an implementation backed by a database, S3, or a key-value store. The `IClaimStore` interface is the extension point — any implementation that provides atomic `TryCreate` will work.
