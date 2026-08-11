using Microsoft.EntityFrameworkCore;
using PaySphere.AuthService.Data;
using PaySphere.AuthService.Entities;
using PaySphere.AuthService.Repositories.Interfaces;

namespace PaySphere.AuthService.Repositories;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    private readonly PaySphereAuthDbContext _context;

    public UserRepository(PaySphereAuthDbContext context)
        : base(context)
    {
        _context = context;
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(x => x.Email == email);
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _context.Users
            .AnyAsync(x => x.Email == email);
    }

    public async Task<bool> PhoneExistsAsync(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return false;

        return await _context.Users
            .AnyAsync(x => x.PhoneNumber == phoneNumber);
    }

    public async Task<User?> GetUserWithRoleAsync(int userId)
    {
        return await _context.Users
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.Id == userId);
    }
}
