# Tradeoffs

Every design choice trades something. This page makes the tradeoffs explicit so you can decide whether they fit your context.

## File-based lock vs database advisory lock

This implementation uses the filesystem as the coordination layer. A database advisory lock (`pg_advisory_lock`, MySQL `GET_LOCK`) would give you network-accessible locking and query-based status checks, but it requires a running database.

**Choose a database lock if:** you already have a database in your stack and need locking across machines.

## TTL-based expiry vs heartbeat

Expired claims are detected lazily — the next acquirer checks the timestamp. A heartbeat model would have the holder periodically refresh the claim file, with other processes watching for missed heartbeats. Heartbeats detect crashes faster but require active polling.

**Choose heartbeats if:** your TTL must be very short (sub-second) and you need fast crash detection. You'll need a background thread in the holder to send heartbeats.

## FileMode.CreateNew vs Mutex

`FileMode.CreateNew` gives cross-process atomic creation through the OS. A `System.Threading.Mutex` (named, system-wide) is more efficient for in-process coordination and releases automatically when the owning process exits.

**Choose a named Mutex if:** all participants are on one machine and you want automatic release on crash. You lose the JSON claim file (and its owner/timestamp metadata) but gain cleaner semantics.

## Lazy stale cleanup vs background sweeper

Stale claims are cleaned up by the next acquirer. A background sweeper would periodically scan for expired claim files and delete them proactively. The lazy approach is simpler but means stale files sit on disk until someone tries to acquire the same resource.

**Choose a sweeper if:** you have many resources and want to keep the claim directory clean. Run the sweeper on a timer and delete any claim file past its `ExpiresAt`.

## No re-entrant locking

The same owner acquiring the same resource twice is treated as a conflict. A re-entrant lock would track an acquisition count and require matching release calls. This keeps the model simple but means a process must track whether it already holds a claim before attempting to acquire.

**Choose re-entrant locking if:** your code paths naturally re-enter the critical section. Add a counter field to `ClaimRecord` and increment on each re-acquire.

## No queuing or fairness

When a claim expires, the first process to call `Acquire` wins. There is no queue of waiters. This means under high contention, some processes may be starved indefinitely.

**Choose a fair lock if:** you have many competing processes and need guaranteed progress for all of them. Implement a ticket-based queue alongside the claim file.
