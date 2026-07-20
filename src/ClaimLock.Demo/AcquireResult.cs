namespace ClaimLock.Demo;

public enum AcquireOutcome
{
    Acquired,
    AlreadyHeld,
}

public class AcquireResult
{
    public AcquireOutcome Outcome { get; init; }
    public ClaimRecord? Claim { get; init; }
    public ClaimRecord? ExistingHolder { get; init; }

    public static AcquireResult Success(ClaimRecord claim) =>
        new() { Outcome = AcquireOutcome.Acquired, Claim = claim };

    public static AcquireResult Held(ClaimRecord existing) =>
        new() { Outcome = AcquireOutcome.AlreadyHeld, ExistingHolder = existing };
}
