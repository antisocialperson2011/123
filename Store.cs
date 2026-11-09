public class User { public string Name { get; set; } = ""; public string Salt { get; set; } = ""; public string Hash { get; set; } = ""; }
public class Option { public string Text { get; set; } = ""; public int Votes { get; set; } }
public class Poll
{
    public string Code { get; set; } = ""; public string Title { get; set; } = ""; public string Description { get; set; } = "";
    public string Owner { get; set; } = ""; public bool IsPrivate { get; set; } public bool Multiple { get; set; } public bool Closed { get; set; }
    public string Salt { get; set; } = ""; public string PassHash { get; set; } = "";
    public List<Option> Options { get; set; } = new(); public List<string> Voted { get; set; } = new();
    public int Total => Options.Sum(o => o.Votes);
}
public class Db { public List<User> Users { get; set; } = new(); public List<Poll> Polls { get; set; } = new(); }

public static class Store
{
    static readonly string P = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "VoteApp", "data.json");
    public static Db Data = Load();
    public static User Current;

    public static Db Load()
    {
        try { Data = File.Exists(P) ? JsonSerializer.Deserialize<Db>(File.ReadAllText(P)) ?? new Db() : new Db(); } catch { Data = new Db(); }
        if (!Data.Users.Any(u => u.Name == "admin"))
        {
            var s = NewSalt(); Data.Users.Add(new User { Name = "admin", Salt = s, Hash = Hash("admin", s) }); Save();
        }
        return Data;
    }
    public static bool IsAdmin => Current != null && Current.Name == "admin";
    public static void Save() { Directory.CreateDirectory(Path.GetDirectoryName(P)!); File.WriteAllText(P, JsonSerializer.Serialize(Data, new JsonSerializerOptions { WriteIndented = true })); }

    public static string NewSalt() => Convert.ToHexString(RandomNumberGenerator.GetBytes(8));
    public static string Hash(string pw, string salt) => Convert.ToHexString(Rfc2898DeriveBytes.Pbkdf2(pw, Convert.FromHexString(salt), 50000, HashAlgorithmName.SHA256, 32));

    public static string Register(string name, string pw)
    {
        Load(); name = name.Trim();
        if (name.Length < 3) return "Логин — минимум 3 символа";
        if (pw.Length < 4) return "Пароль — минимум 4 символа";
        if (Data.Users.Any(u => u.Name.Equals(name, StringComparison.OrdinalIgnoreCase))) return "Такой логин уже занят";
        var s = NewSalt(); var u = new User { Name = name, Salt = s, Hash = Hash(pw, s) };
        Data.Users.Add(u); Save(); Current = u; return null;
    }
    public static string Login(string name, string pw)
    {
        Load(); var u = Data.Users.FirstOrDefault(x => x.Name.Equals(name.Trim(), StringComparison.OrdinalIgnoreCase));
        if (u == null || Hash(pw, u.Salt) != u.Hash) return "Неверный логин или пароль";
        Current = u; return null;
    }
    public static string NewCode()
    {
        const string c = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; string code;
        do code = new string(Enumerable.Range(0, 6).Select(_ => c[RandomNumberGenerator.GetInt32(c.Length)]).ToArray()); while (Find(code) != null);
        return code;
    }
    public static Poll Find(string code) => Data.Polls.FirstOrDefault(p => p.Code.Equals(code.Trim(), StringComparison.OrdinalIgnoreCase));
    public static bool CheckPass(Poll p, string pw) => p.PassHash == "" || Hash(pw ?? "", p.Salt) == p.PassHash;
}
