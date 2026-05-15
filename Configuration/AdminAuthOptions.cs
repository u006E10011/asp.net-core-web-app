namespace DevLog.Configuration;

public class AdminAuthOptions
{
    public const string SectionName = "AdminAuth";

    public string Username { get; set; } = "admin";

    public string Password { get; set; } = "devlog-admin";

    public string PasswordHash { get; set; } = BCrypt.Net.BCrypt.HashPassword("devlog-admin");
}
