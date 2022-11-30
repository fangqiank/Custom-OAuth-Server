using System.Text.Json;

public class PermissionDatabase
{
    private readonly string _dbPath;

    public PermissionDatabase(IWebHostEnvironment env)
	{
        _dbPath = Path.Combine(env.ContentRootPath, "permission.json");
    }

    private Dictionary<string, HashSet<string>> Row => File.Exists(_dbPath)
        ? JsonSerializer.Deserialize<Dictionary<string, HashSet<string>>>(File.ReadAllText(_dbPath))
        :new();

    public bool HasPermission(string userId, string permission)
    {
        var db = Row;

        return db.ContainsKey(userId) && db[userId].Contains(permission);
    }

    public void AddPermission(string userId, string permission) 
    {
        var db = Row;
        if(!db.ContainsKey(userId))
            db[userId] = new HashSet<string>();

        db[userId].Add(permission);
        File.WriteAllText(_dbPath, JsonSerializer.Serialize(db));
    }

    public void RemovePermission(string userId, string permission) 
    {
        var db = Row;
        if (db[userId] == null || !db[userId].Contains(permission))
            return;

        db[userId].Remove(permission);
        File.WriteAllText(_dbPath, JsonSerializer.Serialize(db, new JsonSerializerOptions()
        {
            WriteIndented= true,
        }));
    }


}
