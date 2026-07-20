# ADR 0001: Atomic File Creation for Mutual Exclusion

## Context

Two processes racing to acquire the same lock must not both succeed. The acquire operation needs to be atomic. Options include a named OS mutex, a database lock, a compare-and-swap on a shared file, or `FileMode.CreateNew`.

## Decision

Use `FileMode.CreateNew` to create the claim file. The OS guarantees that if the file already exists, the call fails with an `IOException`. This serializes concurrent acquires without any application-level synchronization.

## Consequences

- Closes the TOCTOU gap completely — there is no window between "check if file exists" and "create file."
- Works across processes and across languages — any runtime that can open a file with exclusive-create semantics participates in the same lock.
- Does not work across machines without a shared filesystem (NFS, SMB).
- The lock granularity is per-file. Different resources use different files, so there is no cross-resource contention.
