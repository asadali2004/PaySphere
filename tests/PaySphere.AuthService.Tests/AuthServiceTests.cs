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
using PaySphere.BuildingBlocks.Exceptions;

namespace PaySphere.AuthService.Tests
{
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

            _authService = new PaySphere.AuthService.Services.AuthService(
                _genericRepoMock.Object,
                _userRepoMock.Object,
                _roleRepoMock.Object,
                _jwtMock.Object);
        }

        [Test]
        public async Task Register_WithValidData_ShouldCreateUser()
        {
            var request = new RegisterRequest { FullName = "Test", Email = "t@example.com", PhoneNumber = "123", Password = "password123" };

            _userRepoMock.Setup(x => x.EmailExistsAsync(request.Email)).ReturnsAsync(false);
            _userRepoMock.Setup(x => x.PhoneExistsAsync(request.PhoneNumber)).ReturnsAsync(false);
            _roleRepoMock.Setup(x => x.GetByNameAsync("User")).ReturnsAsync(new Role { Id = 2, Name = "User" });
            User? added = null;
            _genericRepoMock.Setup(x => x.AddAsync(It.IsAny<User>())).Callback<User>(u => added = u).Returns(Task.CompletedTask);
            _genericRepoMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

            var result = await _authService.RegisterAsync(request);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Email, Is.EqualTo(request.Email));
            Assert.That(result.Role, Is.EqualTo("User"));
            Assert.That(added, Is.Not.Null);
            Assert.That(string.IsNullOrEmpty(added!.PasswordHash), Is.False);
            Assert.That(added.PasswordHash, Is.Not.EqualTo(request.Password));
        }

        [Test]
        public void Register_WhenEmailExists_ShouldThrow()
        {
            var request = new RegisterRequest { FullName = "x", Email = "a@b.com", PhoneNumber = "p", Password = "password" };
            _userRepoMock.Setup(x => x.EmailExistsAsync(request.Email)).ReturnsAsync(true);

            Assert.That(async () => await _authService.RegisterAsync(request), Throws.TypeOf<BaseException>());
        }

        [Test]
        public void Register_WhenPhoneExists_ShouldThrow()
        {
            var request = new RegisterRequest { FullName = "x", Email = "a@b.com", PhoneNumber = "p", Password = "password" };
            _userRepoMock.Setup(x => x.EmailExistsAsync(request.Email)).ReturnsAsync(false);
            _userRepoMock.Setup(x => x.PhoneExistsAsync(request.PhoneNumber)).ReturnsAsync(true);

            Assert.That(async () => await _authService.RegisterAsync(request), Throws.TypeOf<BaseException>());
        }

        [Test]
        public async Task Register_ShouldHashPassword()
        {
            var request = new RegisterRequest { FullName = "U", Email = "u@example.com", PhoneNumber = "p2", Password = "password123" };
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

        [Test]
        public async Task Register_Assigns_Default_User_Role()
        {
            var request = new RegisterRequest { FullName = "RoleUser", Email = "role@example.com", PhoneNumber = "900", Password = "password123" };

            _userRepoMock.Setup(x => x.EmailExistsAsync(request.Email)).ReturnsAsync(false);
            _userRepoMock.Setup(x => x.PhoneExistsAsync(request.PhoneNumber)).ReturnsAsync(false);
            _roleRepoMock.Setup(x => x.GetByNameAsync("User")).ReturnsAsync(new Role { Id = 2, Name = "User" });

            User? added = null;
            _genericRepoMock.Setup(x => x.AddAsync(It.IsAny<User>())).Callback<User>(u => added = u).Returns(Task.CompletedTask);
            _genericRepoMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

            var result = await _authService.RegisterAsync(request);

            Assert.That(result.Role, Is.EqualTo("User"));
            Assert.That(added, Is.Not.Null);
            Assert.That(added!.RoleId, Is.EqualTo(2));
        }
    }
}
