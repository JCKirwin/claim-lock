# ADR 0003: Owner-Only Release with Silent No-Op

## Context

When a process calls release, it might not be the current holder — the claim may have expired and been reclaimed by someone else. The release operation needs a policy for non-owner attempts.

## Decision

Compare the caller's owner identifier against the claim file's owner. If they match, delete the file. If they don't match, return `false` without modifying anything. No exception, no side effect.

## Consequences

- A non-owner release is safe — it cannot accidentally free someone else's lock.
- The `ForceOverride` method exists as an explicit, auditable escape hatch for operational emergencies.
- The caller must check the return value to know whether release succeeded. Ignoring it is harmless but may mask logic errors.
