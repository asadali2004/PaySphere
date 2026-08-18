using System;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using PaySphere.AuthService.Services;
using PaySphere.AuthService.Services.Interfaces;
using PaySphere.AuthService.Repositories.Interfaces;
using PaySphere.AuthService.Entities;
using PaySphere.AuthService.DTOs.Requests;
using PaySphere.AuthService.Helpers;
using PaySphere.BuildingBlocks.Constants;
using PaySphere.BuildingBlocks.Exceptions;
using PaySphere.AuthService.DTOs.Responses;
using System.Threading;
using PaySphere.AuthService.Repositories;
using PaySphere.BuildingBlocks.Base;

namespace PaySphere.AuthService.Tests
{
    /// <summary>
    /// Unit tests for the AuthService class, covering registration, login, profile retrieval, and password change scenarios.
    /// </summary>
    [TestFixture]
    public class AuthServiceTests
    {
        private Mock<IGenericRepository<User>> _genericRepoMock = null!;
        private Mock<IUserRepository> _userRepoMock = null!;
        private Mock<IRoleRepository> _roleRepoMock = null!;
        private Mock<IJwtTokenService> _jwtMock = null!;
        private IAuthService _authService = null!;

        [SetUp]
        public void Setup()
        {
            _genericRepoMock = new Mock<IGenericRepository<User>>();
            _userRepoMock = new Mock<IUserRepository>();
            _roleRepoMock = new Mock<IRoleRepository>();
            _jwtMock = new Mock<IJwtTokenService>();

            // initialize service under test
            _authService = new PaySphere.AuthService.Services.AuthService(
                _genericRepoMock.Object,
                _userRepoMock.Object,
                _roleRepoMock.Object,
                _jwtMock.Object);
        }

        /// <summary>
        /// Tests the RegisterAsync method of AuthService with valid registration data.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task Register_WithValidData_ShouldCreateUser()
        {
            var request = new RegisterRequest
            {
                FullName = "Test User",
                Email = "test@example.com",
                PhoneNumber = "1234567890",
                Password = "password123"
            };

            _userRepoMock.Setup(x => x.EmailExistsAsync(request.Email)).ReturnsAsync(false);
            _userRepoMock.Setup(x => x.PhoneExistsAsync(request.PhoneNumber)).ReturnsAsync(false);
            _roleRepoMock.Setup(x => x.GetByNameAsync("User")).ReturnsAsync(new Role { Id = 2, Name = "User" });

            User? added = null;
            _genericRepoMock.Setup(x => x.AddAsync(It.IsAny<User>())).Callback<User>(u => added = u).Returns(Task.CompletedTask);
            _genericRepoMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

            var result = await _authService.RegisterAsync(request);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.FullName, Is.EqualTo(request.FullName));
            Assert.That(result.Email, Is.EqualTo(request.Email));
            Assert.That(result.Role, Is.EqualTo("User"));
            Assert.That(added, Is.Not.Null);
            Assert.That(string.IsNullOrEmpty(added!.PasswordHash), Is.False);
            Assert.That(added.PasswordHash, Is.Not.EqualTo(request.Password));
        }

        /// <summary>
        /// Tests the RegisterAsync method of AuthService when the email already exists in the system, expecting a failure.
        /// </summary>
        [Test]
        public void Register_WhenEmailExists_ShouldFail()
        {
            var request = new RegisterRequest { FullName = "x", Email = "a@b.com", PhoneNumber = "p", Password = "password" };

            _userRepoMock.Setup(x => x.EmailExistsAsync(request.Email)).ReturnsAsync(true);

            Assert.ThrowsAsync<BaseException>(async () => await _authService.RegisterAsync(request));
        }

        /// <summary>
        /// Tests the RegisterAsync method of AuthService when the phone number already exists in the system, expecting a failure.
        /// </summary>
        [Test]
        public void Register_WhenPhoneExists_ShouldFail()
        {
            var request = new RegisterRequest { FullName = "x", Email = "a@b.com", PhoneNumber = "p", Password = "password" };

            _userRepoMock.Setup(x => x.EmailExistsAsync(request.Email)).ReturnsAsync(false);
            _userRepoMock.Setup(x => x.PhoneExistsAsync(request.PhoneNumber)).ReturnsAsync(true);

            Assert.ThrowsAsync<BaseException>(async () => await _authService.RegisterAsync(request));
        }

        /// <summary>
        /// Tests that the RegisterAsync method hashes the password before storing it in the database.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task Register_ShouldHashPassword()
        {
            var request = new RegisterRequest
            {
                FullName = "Test User",
                Email = "test2@example.com",
                PhoneNumber = "0987654321",
                Password = "password123"
            };

            _userRepoMock.Setup(x => x.EmailExistsAsync(request.Email)).ReturnsAsync(false);
            _userRepoMock.Setup(x => x.PhoneExistsAsync(request.PhoneNumber)).ReturnsAsync(false);
            _roleRepoMock.Setup(x => x.GetByNameAsync("User")).ReturnsAsync(new Role { Id = 2, Name = "User" });

            User? added = null;
            _genericRepoMock.Setup(x => x.AddAsync(It.IsAny<User>())).Callback<User>(u => added = u).Returns(Task.CompletedTask);
            _genericRepoMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

            var result = await _authService.RegisterAsync(request);

            Assert.That(added, Is.Not.Null);
            Assert.That(PasswordHasher.VerifyPassword(request.Password, added!.PasswordHash), Is.True);
        }

        /// <summary>
        /// Tests that the RegisterAsync method assigns the default "User" role to newly registered users.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task Register_ShouldAssignUserRole()
        {
            var request = new RegisterRequest
            {
                FullName = "Test User",
                Email = "role@example.com",
                PhoneNumber = "000",
                Password = "password123"
            };

            _userRepoMock.Setup(x => x.EmailExistsAsync(request.Email)).ReturnsAsync(false);
            _userRepoMock.Setup(x => x.PhoneExistsAsync(request.PhoneNumber)).ReturnsAsync(false);
            _roleRepoMock.Setup(x => x.GetByNameAsync("User")).ReturnsAsync(new Role { Id = 2, Name = "User" });

            User? added = null;
            _genericRepoMock.Setup(x => x.AddAsync(It.IsAny<User>())).Callback<User>(u => added = u).Returns(Task.CompletedTask);
            _genericRepoMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

            var result = await _authService.RegisterAsync(request);

            Assert.That(result.Role, Is.EqualTo("User"));
            Assert.That(added!.RoleId, Is.EqualTo(2));
        }


        /// <summary>
        /// Tests the LoginAsync method of AuthService with valid credentials, expecting a successful login and token generation.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task Login_WithValidCredentials_ShouldReturnToken()
        {
            var password = "securePass1";
            var user = new User { Id = 5, Email = "login@example.com", FullName = "Login", IsActive = true, RoleId = 2 };
            user.PasswordHash = PasswordHasher.HashPassword(password);

            _userRepoMock.Setup(x => x.GetByEmailAsync(user.Email)).ReturnsAsync(user);
            _roleRepoMock.Setup(x => x.GetByIdAsync(user.RoleId)).ReturnsAsync(new Role { Id = 2, Name = "User" });
            _jwtMock.Setup(x => x.GenerateToken(It.IsAny<User>(), It.IsAny<string>())).Returns(("token-abc", DateTime.UtcNow.AddMinutes(30)));

            var result = await _authService.LoginAsync(new LoginRequest { Email = user.Email, Password = password });

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Token, Is.Not.Empty);
        }

        /// <summary>
        /// Tests the LoginAsync method of AuthService when the user does not exist, expecting a failure and an exception to be thrown.
        /// </summary>
        [Test]
        public void Login_WhenUserDoesNotExist_ShouldFail()
        {
            _userRepoMock.Setup(x => x.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);

            Assert.ThrowsAsync<BaseException>(async () => await _authService.LoginAsync(new LoginRequest { Email = "no@x.com", Password = "p" }));
        }

        /// <summary>
        /// Tests the LoginAsync method of AuthService when the provided password is incorrect,
        /// expecting a failure and an exception to be thrown.
        /// </summary>
        [Test]
        public void Login_WhenPasswordIsIncorrect_ShouldFail()
        {
            var user = new User { Id = 6, Email = "wrong@example.com", FullName = "Wrong", IsActive = true, RoleId = 2 };
            user.PasswordHash = PasswordHasher.HashPassword("rightpass");

            _userRepoMock.Setup(x => x.GetByEmailAsync(user.Email)).ReturnsAsync(user);

            Assert.ThrowsAsync<BaseException>(async () => await _authService.LoginAsync(new LoginRequest { Email = user.Email, Password = "wrong" }));
        }

        /// <summary>
        /// Tests the LoginAsync method of AuthService when the user account is inactive,
        /// </summary>
        [Test]
        public void Login_WhenUserIsInactive_ShouldFail()
        {
            var user = new User { Id = 7, Email = "inactive@example.com", FullName = "Inactive", IsActive = false, RoleId = 2 };
            user.PasswordHash = PasswordHasher.HashPassword("pass");

            _userRepoMock.Setup(x => x.GetByEmailAsync(user.Email)).ReturnsAsync(user);

            Assert.ThrowsAsync<BaseException>(async () => await _authService.LoginAsync(new LoginRequest { Email = user.Email, Password = "pass" }));
        }

        /// <summary>
        /// Tests the GetProfileAsync method of AuthService to ensure it returns the current user's profile information correctly.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task GetProfile_ShouldReturnCurrentUser()
        {
            var user = new User { Id = 8, Email = "profile@example.com", FullName = "Profile", PhoneNumber = "111", Role = new Role { Id = 2, Name = "User" } };

            _userRepoMock.Setup(x => x.GetUserWithRoleAsync(user.Id)).ReturnsAsync(user);

            var result = await _authService.GetProfileAsync(user.Id);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Email, Is.EqualTo(user.Email));
            Assert.That(result.Role, Is.EqualTo("User"));
        }

        /// <summary>
        /// Tests the ChangePasswordAsync method of AuthService with the correct current password,
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task ChangePassword_WithCorrectCurrentPassword_ShouldUpdate()
        {
            var user = new User { Id = 9, PasswordHash = PasswordHasher.HashPassword("oldpass") };

            _genericRepoMock.Setup(x => x.GetByIdAsync(user.Id)).ReturnsAsync(user);
            _genericRepoMock.Setup(x => x.Update(It.IsAny<User>()));
            _genericRepoMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

            var oldHash = user.PasswordHash;

            await _authService.ChangePasswordAsync(user.Id, new ChangePasswordRequest { CurrentPassword = "oldpass", NewPassword = "newpassword" });

            _genericRepoMock.Verify(x => x.Update(It.Is<User>(u => u.PasswordHash != null && u.PasswordHash != oldHash)), Times.Once);
        }

        /// <summary>
        /// Tests the ChangePasswordAsync method of AuthService with an incorrect current password,
        /// expecting a failure and an exception to be thrown.
        /// </summary>
        [Test]
        public void ChangePassword_WithIncorrectCurrentPassword_ShouldFail()
        {
            var user = new User { Id = 10, PasswordHash = PasswordHasher.HashPassword("oldpass2") };

            _genericRepoMock.Setup(x => x.GetByIdAsync(user.Id)).ReturnsAsync(user);

            Assert.ThrowsAsync<BaseException>(async () => await _authService.ChangePasswordAsync(user.Id, new ChangePasswordRequest { CurrentPassword = "wrong", NewPassword = "newpass" }));
        }
    }
}
