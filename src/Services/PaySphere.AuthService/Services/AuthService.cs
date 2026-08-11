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

public class AuthService : IAuthService
{
    private readonly IGenericRepository<User> _userRepoGeneric;
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IJwtTokenService _jwtTokenService;

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
}
