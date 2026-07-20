# Code Walkthrough

This walkthrough tours the source files in order, explaining what each piece does and how it connects to the claim lock pattern.

## ClaimRecord.cs

The data model for a claim. Four properties: `Resource` (what's locked), `Owner` (who locked it), `AcquiredAt` (when), and `ExpiresAt` (when the TTL runs out). The `IsExpired` method compares `ExpiresAt` against a given UTC time — this is the only TTL logic in the system.

```csharp
public bool IsExpired(DateTime utcNow) => utcNow >= ExpiresAt;
```

## AcquireResult.cs

The return type for acquire operations. An `AcquireOutcome` enum (`Acquired` or `AlreadyHeld`) plus optional references to the new claim or the existing holder. Static factory methods keep construction clean:

```csharp
AcquireResult.Success(claim)  // you got the lock
AcquireResult.Held(existing)  // someone else has it
```

## IClaimStore.cs

The storage interface: `Exists`, `Read`, `TryCreate`, and `Delete`. `TryCreate` is the critical one — it must be atomic. If two callers race, exactly one returns `true`.

## FileClaimStore.cs

The filesystem implementation. `TryCreate` opens the file with `FileMode.CreateNew` — the OS rejects the call with an `IOException` if the file already exists. This closes the TOCTOU race at the OS level.

```csharp
using var fs = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None);
JsonSerializer.Serialize(fs, record, SerializerOptions);
```

The claim file path is derived from the resource name: `{resource}.claim.json`.

## ClaimManager.cs

The orchestrator with three operations:

**Acquire:** Read the existing claim. If it exists and hasn't expired, return `AlreadyHeld`. If it has expired, delete it. Then try to create a new claim. If `TryCreate` fails (another process raced in), read the winner and return `AlreadyHeld`.

**Release:** Read the claim, check the owner matches, delete if it does. Non-matching owners get a `false` return — no exception, no side effect.

**ForceOverride:** Delete the existing claim unconditionally, log a takeover notice, and create a new one. This is the operational escape hatch.

The constructor accepts an optional `TimeProvider` for testability. Tests inject `FakeTimeProvider` to control time without `Thread.Sleep`.

## InMemoryClaimStore.cs (tests)

A thread-safe in-memory implementation of `IClaimStore` used by the test suite. A `Dictionary<string, ClaimRecord>` behind a `lock` provides the same atomic semantics as the filesystem store without touching disk.

## Program.cs

The demo runs four phases:

1. **Concurrent claims:** Three chefs race for recipes. Only one gets each recipe; others move on.
2. **Non-owner release:** Alice claims a recipe, Bob tries to release it (no-op), Alice releases.
3. **TTL recovery:** Dave claims with a 1-second TTL and "crashes." After the TTL, Carol reclaims.
4. **Force override:** Alice claims, Bob force-overrides with a logged takeover notice.
