using PaySphere.AuthService.DTOs.Requests;
using PaySphere.AuthService.DTOs.Responses;
using PaySphere.AuthService.Entities;
using PaySphere.AuthService.Helpers;
using PaySphere.AuthService.Repositories.Interfaces;
using PaySphere.AuthService.Services.Interfaces;
using PaySphere.AuthService.Validators;
using PaySphere.BuildingBlocks.Constants;
using PaySphere.BuildingBlocks.Exceptions;

namespace PaySphere.AuthService.Services;

/// <summary>
/// Service responsible for handling authentication-related operations such as user registration,
/// login, profile retrieval, and password changes.
/// </summary>
public class AuthService : IAuthService
{
    private readonly IGenericRepository<User> _userRepoGeneric;
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IJwtTokenService _jwtTokenService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthService"/> 
    /// class with the specified repositories and JWT token service.
    /// </summary>
    /// <param name="userRepoGeneric"></param>
    /// <param name="userRepository"></param>
    /// <param name="roleRepository"></param>
    /// <param name="jwtTokenService"></param>
    public AuthService(
        IGenericRepository<User> userRepoGeneric,
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IJwtTokenService jwtTokenService)
    {
        _userRepoGeneric = userRepoGeneric;
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _jwtTokenService = jwtTokenService;
    }

    /// <summary>
    /// Registers a new user with the provided registration details. Validates the request,
    /// checks for duplicate email and phone number, assigns a default role, and saves the user to the database.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    /// <exception cref="BaseException"></exception>

    public async Task<UserResponse> RegisterAsync(RegisterRequest request)
    {
        RegisterRequestValidator.Validate(request);

        if (await _userRepository.EmailExistsAsync(request.Email))
            throw new BaseException(ErrorMessages.DuplicateUser);

        if (await _userRepository.PhoneExistsAsync(request.PhoneNumber))
            throw new BaseException("Phone number already exists.");

        var role = await _roleRepository.GetByNameAsync("User");

        if (role is null)
            throw new BaseException("Default role not found.");

        var user = new User
        {
            FullName = request.FullName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            PasswordHash = PasswordHasher.HashPassword(request.Password),
            RoleId = role.Id,
            IsActive = true
        };

        await _userRepoGeneric.AddAsync(user);
        await _userRepoGeneric.SaveChangesAsync();

        return new UserResponse
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Role = role.Name
        };
    }


    /// <summary>
    /// Authenticates a user with the provided login credentials. Validates the request,
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    /// <exception cref="BaseException"></exception>
    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        LoginRequestValidator.Validate(request);

        var user = await _userRepository.GetByEmailAsync(request.Email);

        if (user is null || !user.IsActive || !PasswordHasher.VerifyPassword(request.Password, user.PasswordHash))
            throw new BaseException(ErrorMessages.InvalidCredentials);

        var role = await _roleRepository.GetByIdAsync(user.RoleId);
        var roleName = role?.Name ?? string.Empty;

        var (token, expiresAt) = _jwtTokenService.GenerateToken(user, roleName);

        return new LoginResponse
        {
            Token = token,
            ExpiresAt = expiresAt
        };
    }


    /// <summary>
    /// Retrieves the profile information of a user by their user ID. If the user is not found, an exception is thrown.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    /// <exception cref="BaseException"></exception>
    public async Task<UserResponse> GetProfileAsync(int userId)
    {
        var user = await _userRepository.GetUserWithRoleAsync(userId);

        if (user is null)
            throw new BaseException(ErrorMessages.UserNotFound);

        return new UserResponse
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Role = user.Role?.Name ?? string.Empty
        };
    }


    /// <summary>
    /// Changes the password of a user. Validates the request, checks if the current password is correct,
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    /// <exception cref="BaseException"></exception>
    public async Task ChangePasswordAsync(int userId, ChangePasswordRequest request)
    {
        ChangePasswordRequestValidator.Validate(request);

        var user = await _userRepoGeneric.GetByIdAsync(userId);

        if (user is null)
            throw new BaseException(ErrorMessages.UserNotFound);

        if (!PasswordHasher.VerifyPassword(request.CurrentPassword, user.PasswordHash))
            throw new BaseException("Current password is incorrect.");

        user.PasswordHash = PasswordHasher.HashPassword(request.NewPassword);

        _userRepoGeneric.Update(user);
        await _userRepoGeneric.SaveChangesAsync();
    }

    /// <summary>
    /// Validates if a user exists and is active based on the provided user ID.
    /// Returns an InternalUserValidationResponse indicating the user's existence and active status.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<InternalUserValidationResponse> GetInternalUserValidationAsync(int userId)
    {
        var user = await _userRepoGeneric.GetByIdAsync(userId);

        return new InternalUserValidationResponse
        {
            UserId = userId,
            Exists = user is not null,
            IsActive = user?.IsActive ?? false
        };
    }
}
