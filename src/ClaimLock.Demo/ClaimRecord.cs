namespace ClaimLock.Demo;

public class ClaimRecord
{
    public string Resource { get; set; } = "";
    public string Owner { get; set; } = "";
    public DateTime AcquiredAt { get; set; }
    public DateTime ExpiresAt { get; set; }

    public bool IsExpired(DateTime utcNow) => utcNow >= ExpiresAt;
}
