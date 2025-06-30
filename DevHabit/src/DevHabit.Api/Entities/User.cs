namespace DevHabit.Api.Entities;

public class User
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public DateTime CreatedAtUtc{ get; set; }
    public DateTime? UpdatedAtUtc { get; set; }

    /// <summary>
    /// We'll use this to store IdentityId from the Identity Provider
    /// Like Azure AD, Cognito, Keycloak, Auth0, etc
    /// </summary>
    public string IdentityId { get; set; }
}
