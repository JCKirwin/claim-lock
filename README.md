# Claim Lock

A file-based mutual exclusion primitive in C# with automatic TTL expiry — no database, no broker, just a filesystem. Fork it, read it, adapt it.

## What you'll learn

- How `FileMode.CreateNew` closes the TOCTOU race for atomic lock acquisition
- How TTL-based expiry self-heals after process crashes
- How owner-only release prevents accidental lock freeing
- How force-override provides an auditable operational escape hatch
- How to test time-dependent logic with `FakeTimeProvider`

## Quick Start

```bash
git clone https://github.com/JCKirwin/claim-lock.git
cd claim-lock
dotnet run --project src/ClaimLock.Demo
```

The demo simulates a shared kitchen where multiple chefs claim recipes. Each chef can only work on one recipe at a time. The demo shows concurrent claims, non-owner release (no-op), TTL recovery after a crash, and force override.

```
=== Claim Lock Demo: Shared Kitchen ===

--- Phase 1: Three chefs claim recipes ---
  chef-alice claimed garlic-chicken
  chef-bob claimed mushroom-risotto
  chef-carol claimed lemon-pasta
  chef-alice finished garlic-chicken

--- Phase 2: Non-owner release ---
  chef-alice claims herb-salmon
  chef-bob tries to release herb-salmon: no-op
  chef-alice releases herb-salmon

--- Phase 3: TTL recovery ---
  chef-dave claims lemon-pasta (1s TTL) and crashes...
  chef-carol reclaims lemon-pasta after TTL: Acquired

--- Phase 4: Force override ---
  chef-alice claims mushroom-risotto
  [TAKEOVER] chef-bob overriding chef-alice's claim on mushroom-risotto
  chef-bob force-overrides: Acquired
```

## Run the tests

```bash
dotnet test tests/ClaimLock.Demo.Tests
```

## Project structure

```
claim-lock/
├── src/ClaimLock.Demo/           Core lock + recipe-box demo
│   ├── ClaimRecord.cs            Lock data model (resource, owner, TTL)
│   ├── AcquireResult.cs          Acquire outcome enum + result type
│   ├── IClaimStore.cs            Storage interface (atomic create/read/delete)
│   ├── FileClaimStore.cs         Filesystem implementation (CreateNew)
│   ├── ClaimManager.cs           Orchestrator (acquire, release, force-override)
│   └── Program.cs                Demo entry point
├── tests/ClaimLock.Demo.Tests/   19 xUnit v3 tests
├── docs/                         Pattern docs, architecture, ADRs
├── samples/demo-data.json        Recipe + chef configuration
└── .github/workflows/ci.yml     Build + test on push/PR
```

## Documentation

- [01 — The Pattern](docs/01-the-pattern.md): What a claim lock is and why you'd use one
- [02 — Architecture](docs/02-architecture.md): Components, acquire/release flows, and how they connect
- [03 — Walkthrough](docs/03-walkthrough.md): File-by-file tour of the source code
- [04 — Tradeoffs](docs/04-tradeoffs.md): What this design trades and when to choose differently
- [05 — Extending](docs/05-extending.md): Common extensions and how to add them

## License

[MIT](LICENSE) — Copyright (c) JCKirwin
