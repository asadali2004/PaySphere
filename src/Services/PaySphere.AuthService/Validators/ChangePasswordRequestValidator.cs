using PaySphere.BuildingBlocks.Exceptions;
using PaySphere.AuthService.DTOs.Requests;

namespace PaySphere.AuthService.Validators;

/// <summary>
/// Validates ChangePasswordRequest DTOs and raises <see cref="PaySphere.BuildingBlocks.Exceptions.BaseException"/>
/// when validation fails. This keeps simple guard checks out of service code.
/// </summary>
internal static class ChangePasswordRequestValidator
{
    public static void Validate(ChangePasswordRequest request)
    {
        if (request is null)
            throw new BaseException("Request cannot be null.");

        if (string.IsNullOrWhiteSpace(request.CurrentPassword))
            throw new BaseException("CurrentPassword is required.");

        if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 8)
            throw new BaseException("NewPassword is required and must be at least 8 characters long.");
    }
}
