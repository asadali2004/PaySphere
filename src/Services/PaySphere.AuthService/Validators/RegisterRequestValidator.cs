using System.Text.RegularExpressions;
using PaySphere.BuildingBlocks.Exceptions;
using PaySphere.AuthService.DTOs.Requests;

namespace PaySphere.AuthService.Validators;

internal static class RegisterRequestValidator
{
    private static readonly Regex EmailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

    public static void Validate(RegisterRequest request)
    {
        if (request is null)
            throw new BaseException("Request cannot be null.");

        if (string.IsNullOrWhiteSpace(request.FullName))
            throw new BaseException("FullName is required.");

        if (string.IsNullOrWhiteSpace(request.Email))
            throw new BaseException("Email is required.");

        if (!EmailRegex.IsMatch(request.Email))
            throw new BaseException("Email format is invalid.");

        if (string.IsNullOrWhiteSpace(request.PhoneNumber))
            throw new BaseException("PhoneNumber is required.");

        if (string.IsNullOrEmpty(request.Password) || request.Password.Length < 8)
            throw new BaseException("Password must be at least 8 characters long.");
    }
}
