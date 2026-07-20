using System.Text.Json;

namespace ClaimLock.Demo;

public class FileClaimStore : IClaimStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
    };

    private readonly string _directory;

    public FileClaimStore(string directory)
    {
        _directory = directory;
        Directory.CreateDirectory(_directory);
    }

    public bool Exists(string resource)
    {
        return File.Exists(GetPath(resource));
    }

    public ClaimRecord? Read(string resource)
    {
        var path = GetPath(resource);
        if (!File.Exists(path))
        {
            return null;
        }

        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<ClaimRecord>(json, SerializerOptions);
    }

    public bool TryCreate(ClaimRecord record)
    {
        var path = GetPath(record.Resource);
        try
        {
            using var fs = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None);
            JsonSerializer.Serialize(fs, record, SerializerOptions);
            return true;
        }
        catch (IOException)
        {
            return false;
        }
    }

    public void Delete(string resource)
    {
        var path = GetPath(resource);
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    private string GetPath(string resource) =>
        Path.Combine(_directory, $"{resource}.claim.json");
}
