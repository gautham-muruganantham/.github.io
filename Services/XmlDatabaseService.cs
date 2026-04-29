using System.Xml.Linq;
using Learning.Models;

namespace Learning.Services;

public class XmlDatabaseService
{
    private readonly string _usersPath;
    private readonly string _profilesPath;
    private readonly string _interestsPath;
    private readonly object _sync = new();

    public XmlDatabaseService(IWebHostEnvironment env)
    {
        _usersPath = Path.Combine(env.ContentRootPath, "users.xml");
        _profilesPath = Path.Combine(env.ContentRootPath, "profiles.xml");
        _interestsPath = Path.Combine(env.ContentRootPath, "interests.xml");
        EnsureInitialized();
    }

    public User? GetUserByEmail(string email)
    {
        lock (_sync)
        {
            return ReadUsers()
                .FirstOrDefault(x => x.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        }
    }

    public bool EmailExists(string email)
    {
        return GetUserByEmail(email) is not null;
    }

    public User? ValidateCredentials(string email, string password)
    {
        var user = GetUserByEmail(email);
        return user is not null && user.Password == password ? user : null;
    }

    public User CreateUser(RegisterViewModel model)
    {
        lock (_sync)
        {
            var users = ReadUsers();
            var profiles = ReadProfiles();

            var user = new User
            {
                Id = Guid.NewGuid().ToString("N"),
                Name = model.Name.Trim(),
                Email = model.Email.Trim(),
                Password = model.Password,
                Gender = model.Gender,
                Age = model.Age
            };

            users.Add(user);

            profiles.Add(new Profile
            {
                UserId = user.Id,
                Religion = model.Religion.Trim(),
                City = model.City.Trim(),
                Profession = model.Profession.Trim(),
                About = $"Hello, I am {model.Name.Trim()}."
            });

            WriteUsers(users);
            WriteProfiles(profiles);

            return user;
        }
    }

    public List<ProfileCardViewModel> SearchProfiles(string currentUserId, string? currentUserGender, SearchFilterViewModel filter)
    {
        lock (_sync)
        {
            var users = ReadUsers();
            var profiles = ReadProfiles().ToDictionary(x => x.UserId, x => x);
            var relations = ReadInterests().Where(x => x.FromUserId == currentUserId).ToList();

            var targetGender = currentUserGender?.Equals("Male", StringComparison.OrdinalIgnoreCase) == true
                ? "Female"
                : "Male";

            var results = users
                .Where(x => x.Id != currentUserId)
                .Where(x => x.Gender.Equals(targetGender, StringComparison.OrdinalIgnoreCase))
                .Where(x => x.Age >= filter.MinAge && x.Age <= filter.MaxAge)
                .Where(x => profiles.ContainsKey(x.Id))
                .Select(x => new { User = x, Profile = profiles[x.Id] })
                .Where(x => string.IsNullOrWhiteSpace(filter.City) || x.Profile.City.Equals(filter.City, StringComparison.OrdinalIgnoreCase))
                .Where(x => string.IsNullOrWhiteSpace(filter.Religion) || x.Profile.Religion.Equals(filter.Religion, StringComparison.OrdinalIgnoreCase))
                .Select(x => new ProfileCardViewModel
                {
                    User = x.User,
                    Profile = x.Profile,
                    IsInterestSent = relations.Any(r => r.ToUserId == x.User.Id && r.Type == "interest"),
                    IsShortlisted = relations.Any(r => r.ToUserId == x.User.Id && r.Type == "shortlist")
                })
                .OrderBy(x => x.User.Name)
                .ToList();

            return results;
        }
    }

    public void ToggleRelation(string fromUserId, string toUserId, string type)
    {
        lock (_sync)
        {
            var relations = ReadInterests();
            var current = relations.FirstOrDefault(x =>
                x.FromUserId == fromUserId &&
                x.ToUserId == toUserId &&
                x.Type == type);

            if (current is null)
            {
                relations.Add(new InterestRecord
                {
                    FromUserId = fromUserId,
                    ToUserId = toUserId,
                    Type = type,
                    CreatedAtUtc = DateTime.UtcNow
                });
            }
            else
            {
                relations.Remove(current);
            }

            WriteInterests(relations);
        }
    }

    public int CountRelations(string fromUserId, string type)
    {
        lock (_sync)
        {
            return ReadInterests().Count(x => x.FromUserId == fromUserId && x.Type == type);
        }
    }

    private void EnsureInitialized()
    {
        lock (_sync)
        {
            if (!File.Exists(_usersPath))
            {
                WriteUsers(new List<User>
                {
                    new() { Id = "u001", Name = "Demo User", Email = "demo@matrimony.com", Password = "Demo@123", Gender = "Male", Age = 29 },
                    new() { Id = "u002", Name = "Ananya R", Email = "ananya@matrimony.com", Password = "Demo@123", Gender = "Female", Age = 26 },
                    new() { Id = "u003", Name = "Priya M", Email = "priya@matrimony.com", Password = "Demo@123", Gender = "Female", Age = 28 },
                    new() { Id = "u004", Name = "Deepa N", Email = "deepa@matrimony.com", Password = "Demo@123", Gender = "Female", Age = 25 },
                    new() { Id = "u005", Name = "Karthik S", Email = "karthik@matrimony.com", Password = "Demo@123", Gender = "Male", Age = 30 }
                });
            }

            if (!File.Exists(_profilesPath))
            {
                WriteProfiles(new List<Profile>
                {
                    new() { UserId = "u001", Religion = "Hindu", City = "Chennai", Profession = "Software Engineer", About = "I enjoy travel and books." },
                    new() { UserId = "u002", Religion = "Hindu", City = "Chennai", Profession = "Data Analyst", About = "Family-oriented and calm." },
                    new() { UserId = "u003", Religion = "Christian", City = "Bengaluru", Profession = "Product Designer", About = "Creative and positive mindset." },
                    new() { UserId = "u004", Religion = "Hindu", City = "Madurai", Profession = "Doctor", About = "Passionate about healthcare." },
                    new() { UserId = "u005", Religion = "Hindu", City = "Coimbatore", Profession = "Project Manager", About = "Simple and practical person." }
                });
            }

            if (!File.Exists(_interestsPath))
            {
                WriteInterests([]);
            }
        }
    }

    private List<User> ReadUsers()
    {
        var doc = XDocument.Load(_usersPath);
        return doc.Root?
            .Elements("user")
            .Select(x => new User
            {
                Id = (string?)x.Attribute("id") ?? string.Empty,
                Name = (string?)x.Attribute("name") ?? string.Empty,
                Email = (string?)x.Attribute("email") ?? string.Empty,
                Password = (string?)x.Attribute("password") ?? string.Empty,
                Gender = (string?)x.Attribute("gender") ?? "Male",
                Age = (int?)x.Attribute("age") ?? 25
            })
            .ToList() ?? [];
    }

    private void WriteUsers(List<User> users)
    {
        var doc = new XDocument(
            new XElement("users",
                users.Select(x => new XElement("user",
                    new XAttribute("id", x.Id),
                    new XAttribute("name", x.Name),
                    new XAttribute("email", x.Email),
                    new XAttribute("password", x.Password),
                    new XAttribute("gender", x.Gender),
                    new XAttribute("age", x.Age)))));

        doc.Save(_usersPath);
    }

    private List<Profile> ReadProfiles()
    {
        var doc = XDocument.Load(_profilesPath);
        return doc.Root?
            .Elements("profile")
            .Select(x => new Profile
            {
                UserId = (string?)x.Attribute("userId") ?? string.Empty,
                Religion = (string?)x.Attribute("religion") ?? string.Empty,
                City = (string?)x.Attribute("city") ?? string.Empty,
                Profession = (string?)x.Attribute("profession") ?? string.Empty,
                About = (string?)x.Attribute("about") ?? string.Empty
            })
            .ToList() ?? [];
    }

    private void WriteProfiles(List<Profile> profiles)
    {
        var doc = new XDocument(
            new XElement("profiles",
                profiles.Select(x => new XElement("profile",
                    new XAttribute("userId", x.UserId),
                    new XAttribute("religion", x.Religion),
                    new XAttribute("city", x.City),
                    new XAttribute("profession", x.Profession),
                    new XAttribute("about", x.About)))));

        doc.Save(_profilesPath);
    }

    private List<InterestRecord> ReadInterests()
    {
        var doc = XDocument.Load(_interestsPath);
        return doc.Root?
            .Elements("entry")
            .Select(x => new InterestRecord
            {
                FromUserId = (string?)x.Attribute("fromUserId") ?? string.Empty,
                ToUserId = (string?)x.Attribute("toUserId") ?? string.Empty,
                Type = (string?)x.Attribute("type") ?? "interest",
                CreatedAtUtc = DateTime.TryParse((string?)x.Attribute("createdAtUtc"), out var dt) ? dt : DateTime.UtcNow
            })
            .ToList() ?? [];
    }

    private void WriteInterests(List<InterestRecord> relations)
    {
        var doc = new XDocument(
            new XElement("interests",
                relations.Select(x => new XElement("entry",
                    new XAttribute("fromUserId", x.FromUserId),
                    new XAttribute("toUserId", x.ToUserId),
                    new XAttribute("type", x.Type),
                    new XAttribute("createdAtUtc", x.CreatedAtUtc.ToString("O"))))));

        doc.Save(_interestsPath);
    }
}
