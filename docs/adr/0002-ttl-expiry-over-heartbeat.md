# ADR 0002: TTL-Based Expiry Over Heartbeat

## Context

A process that crashes while holding a claim leaves an orphaned file. The lock must eventually become available again. Options include OS-level mutex auto-release, heartbeat renewal, or timestamp-based TTL expiry.

## Decision

Store an `ExpiresAt` timestamp in the claim file. When another process attempts to acquire, it checks the timestamp. If the current time is past `ExpiresAt`, the claim is treated as abandoned and deleted.

## Consequences

- No background thread or timer needed in the holder process — TTL is passive.
- Recovery time is bounded by the TTL duration. A 2-minute TTL means at most 2 minutes of unavailability after a crash.
- The TTL must be set longer than the expected hold time. If a slow-but-alive holder exceeds the TTL, another process may reclaim prematurely. Set TTL conservatively.
- Clock skew between processes could cause incorrect expiry decisions. Use UTC consistently and keep machines time-synced.
