namespace PaySphere.AuthService.DTOs.Responses;

public class InternalUserValidationResponse
{
    public int UserId { get; set; }

    public bool Exists { get; set; }

    public bool IsActive { get; set; }
}
