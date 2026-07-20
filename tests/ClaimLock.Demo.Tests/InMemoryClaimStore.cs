namespace ClaimLock.Demo.Tests;

public class InMemoryClaimStore : IClaimStore
{
    private readonly Dictionary<string, ClaimRecord> _claims = new();
    private readonly object _lock = new();

    public bool Exists(string resource)
    {
        lock (_lock)
        {
            return _claims.ContainsKey(resource);
        }
    }

    public ClaimRecord? Read(string resource)
    {
        lock (_lock)
        {
            return _claims.GetValueOrDefault(resource);
        }
    }

    public bool TryCreate(ClaimRecord record)
    {
        lock (_lock)
        {
            if (_claims.ContainsKey(record.Resource))
            {
                return false;
            }

            _claims[record.Resource] = record;
            return true;
        }
    }

    public void Delete(string resource)
    {
        lock (_lock)
        {
            _claims.Remove(resource);
        }
    }
}
