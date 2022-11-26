using System.Text.Json;

namespace Refresh_Token
{
    public class TokenDatabase
    {
        private readonly string _dbPath;

        public TokenDatabase(IWebHostEnvironment env)
        {
            _dbPath = Path.Combine(env.ContentRootPath, "database");
        }

        public Dictionary<string, TokenInfo> Record => File.Exists(_dbPath)
            ? JsonSerializer.Deserialize<Dictionary<string, TokenInfo>>(File.ReadAllText(_dbPath))
            : new();

        public bool Contains(string key) => Record.ContainsKey(key);

        public TokenInfo Get(string key) => Record[key];

        public void Save(string key, TokenInfo tokens)
        {
            var db = Record;
            db[key] = tokens;
            File.WriteAllText(_dbPath, JsonSerializer.Serialize(db));
        }
    }
}

public record TokenInfo(
    string AccessToken, 
    string RefreshToken, 
    DateTime Expires
    );
