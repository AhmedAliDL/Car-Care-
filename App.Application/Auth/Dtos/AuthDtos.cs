namespace App.Application.Auth.Dtos;

/// <summary>
/// The result of a successful login.
/// </summary>
public sealed class AuthResponse
{
    /// <summary>The JWT access token used to authorize subsequent requests.</summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>The token type, always <c>Bearer</c>.</summary>
    public string TokenType { get; set; } = "Bearer";

    /// <summary>When the access token expires (UTC).</summary>
    public DateTime ExpiresAtUtc { get; set; }

    /// <summary>The authenticated user's id.</summary>
    public Guid UserId { get; set; }

    /// <summary>The user's role (Customer, Staff or Manager).</summary>
    public string Role { get; set; } = string.Empty;
}

/// <summary>
/// A user's profile information.
/// </summary>
public sealed class ProfileResponse
{
    /// <summary>The user's id.</summary>
    public Guid UserId { get; set; }

    /// <summary>The user's email address.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>The user's phone number, if provided.</summary>
    public string? Phone { get; set; }

    /// <summary>The user's role (Customer, Staff or Manager).</summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>The associated customer record id, present only for customers.</summary>
    public Guid? CustomerId { get; set; }

    /// <summary>The customer's full name, present only for customers.</summary>
    public string? Name { get; set; }
}

/// <summary>
/// The result of a successful customer registration.
/// </summary>
public sealed class RegisterResponse
{
    /// <summary>The newly created user's id.</summary>
    public Guid UserId { get; set; }

    /// <summary>The newly created customer record id.</summary>
    public Guid CustomerId { get; set; }

    /// <summary>The registered email address.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>The registered full name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>The assigned role, always <c>Customer</c>.</summary>
    public string Role { get; set; } = string.Empty;
}
