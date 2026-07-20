namespace ClaimLock.Demo;

public interface IClaimStore
{
    bool Exists(string resource);
    ClaimRecord? Read(string resource);
    bool TryCreate(ClaimRecord record);
    void Delete(string resource);
}
