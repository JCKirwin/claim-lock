# The Pattern

When multiple processes need exclusive access to a shared resource, you need a lock. This document describes a file-based claim lock — a mutual exclusion primitive that uses the filesystem as the coordination layer, with automatic TTL expiry to recover from crashes.

## What is a claim lock?

A claim lock is a file-based mutex. To acquire it, a process atomically creates a claim file containing its identity and an expiry timestamp. Other processes check for this file before proceeding. When the holder finishes, it deletes the file. If the holder crashes, the TTL expires and another process can reclaim it.

No database, no message broker, no external service. Just a filesystem.

## The problem it solves

Consider a shared kitchen where multiple chefs prepare dishes from a recipe book. Each recipe can only be worked on by one chef at a time — two chefs reaching for the same pan is a disaster. A chef claims a recipe by placing a card on the counter. Other chefs see the card and move on to a different recipe. When the chef finishes, they remove the card.

Now imagine a chef claims a recipe and then leaves the kitchen without removing the card. The recipe is stuck — no one else can work on it. A TTL solves this: each card has an expiry time written on it. Once expired, any chef can pick up the recipe.

## How it works

### Claim file

A JSON file on disk containing four fields: the resource being locked, the owner who locked it, when it was acquired, and when it expires. The file's existence is the lock — if it's there, the resource is claimed.

```json
{
  "resource": "garlic-chicken",
  "owner": "chef-alice",
  "acquiredAt": "2026-07-19T14:30:00Z",
  "expiresAt": "2026-07-19T14:32:00Z"
}
```

### Atomic create

Acquiring uses `FileMode.CreateNew` — the operating system guarantees that if two processes race to create the same file, exactly one succeeds and the other gets an `IOException`. This closes the check-then-act (TOCTOU) race without any application-level locking.

### TTL expiry

Every acquire checks whether an existing claim file has expired by comparing its `expiresAt` timestamp against the current time. If expired, the file is deleted and the acquire proceeds. This means a crashed process's orphaned claim self-heals after the TTL window.

### Owner-only release

Only the process whose owner identifier matches the claim file can release it. A non-owner calling release is a safe no-op — it doesn't throw, it just does nothing.

### Force override

A force-override deletes the existing claim regardless of owner and acquires a new one. This is the escape hatch for operational emergencies. The override is logged (written to the console in the demo) for auditability.

## What it does not do

- No distributed consensus. The lock works on a shared filesystem, not across network partitions.
- No fairness or queuing. If two processes race after a TTL expiry, the winner is arbitrary.
- No re-entrant locking. The same owner acquiring twice without releasing is a conflict.
- No notification on release. Waiters must poll.

These constraints keep the implementation small and the behavior predictable.
