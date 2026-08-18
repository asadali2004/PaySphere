using Microsoft.EntityFrameworkCore;
using PaySphere.AuthService.Data;
using PaySphere.AuthService.Repositories.Interfaces;
using PaySphere.BuildingBlocks.Base;

namespace PaySphere.AuthService.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
{
    /// <summary>
    /// Generic EF Core repository implementation providing basic CRUD operations.
    /// Concrete repositories can inherit this to reuse common data-access logic.
    /// Repositories should avoid implementing business rules.
    /// </summary>
    protected readonly PaySphereAuthDbContext Context;
    protected readonly DbSet<T> DbSet;

    public GenericRepository(PaySphereAuthDbContext context)
    {
        Context = context;
        DbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(int id)
    {
        return await DbSet.FindAsync(id);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await DbSet.ToListAsync();
    }

    public virtual async Task AddAsync(T entity)
    {
        await DbSet.AddAsync(entity);
    }

    public virtual void Update(T entity)
    {
        DbSet.Update(entity);
    }

    public virtual void Delete(T entity)
    {
        DbSet.Remove(entity);
    }

    public virtual async Task SaveChangesAsync()
    {
        await Context.SaveChangesAsync();
    }
}
