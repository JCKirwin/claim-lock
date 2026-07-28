# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

Nothing yet.

## [0.1.0] - 2026-07-28

Initial reference-implementation release.

### Added

- Core lock implementation: `ClaimManager`, `ClaimRecord`, `AcquireResult`, and `IClaimStore` with a `FileClaimStore` that uses atomic exclusive-create semantics for race-free acquisition.
- TTL-based expiry so orphaned claims self-heal after a crash, owner-only release, and an auditable force-override.
- Console demo (`ClaimLock.Demo`): a shared-kitchen scenario showing concurrent claims, non-owner release, TTL recovery, and force override.
- Test suite covering acquire, release, TTL expiry, force override, and the claim record, including an in-memory store and `FakeTimeProvider`-driven time control.
- Docs: pattern walkthrough (`docs/01`–`05`) and three ADRs (atomic create, TTL over heartbeat, owner-only release).
- CI workflow that builds and runs the tests on every push and pull request.
