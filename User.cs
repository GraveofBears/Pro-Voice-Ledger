public class User
{
    public string Id { get; set; }
    public string Username { get; set; }
    public string PasswordHash { get; set; }
    public string DisplayName { get; set; }
    public bool IsSuspended { get; set; }
    public string Role { get; set; }
}