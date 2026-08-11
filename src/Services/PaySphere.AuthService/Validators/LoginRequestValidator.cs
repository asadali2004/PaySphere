using System.Text.RegularExpressions;
using PaySphere.BuildingBlocks.Exceptions;
using PaySphere.AuthService.DTOs.Requests;

namespace PaySphere.AuthService.Validators;

internal static class LoginRequestValidator
{
    private static readonly Regex EmailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

    public static void Validate(LoginRequest request)
    {
        if (request is null)
            throw new BaseException("Request cannot be null.");

        if (string.IsNullOrWhiteSpace(request.Email))
            throw new BaseException("Email is required.");

        if (!EmailRegex.IsMatch(request.Email))
            throw new BaseException("Email format is invalid.");

        if (string.IsNullOrWhiteSpace(request.Password))
            throw new BaseException("Password is required.");
    }
}
