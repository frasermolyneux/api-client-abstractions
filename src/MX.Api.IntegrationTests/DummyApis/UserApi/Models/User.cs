namespace MX.Api.IntegrationTests.DummyApis.UserApi.Models;

/// <summary>
/// User model
/// </summary>
public class User
{
    /// <summary>
    /// User ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Username
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Email address
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Full name
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// User role
    /// </summary>
    public string Role { get; set; } = "User";

    /// <summary>
    /// Is user active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// User creation date
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Create user request model
/// </summary>
public class CreateUserRequest
{
    /// <summary>
    /// Username
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Email address
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Full name
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// User role
    /// </summary>
    public string Role { get; set; } = "User";
}
