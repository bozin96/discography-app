using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discography.Core.Models;
using Discography.Data.Context;
using Discography.Data.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Discography.Data.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
    {
        protected readonly ApplicationDbContext _dbContext;

        protected readonly DbSet<T> _entities;

        public GenericRepository(ApplicationDbContext context)
        {
            _dbContext = context;
            _entities = context.Set<T>();
        }

        public async Task<T> GetByIdAsync(Guid id)
        {
            return await _entities.FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<List<T>> ListAsync()
        {
            return await _entities.ToListAsync();
        }

        public async Task<T> AddAsync(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            await _entities.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        public async Task<T> UpdateAsync(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _entities.Update(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        public async Task DeleteAsync(Guid id)
        {
            T entity = await _entities.FirstOrDefaultAsync(e => e.Id == id);
            if (entity == null)
                throw new KeyNotFoundException();

            _entities.Remove(entity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(id));
            }

            return await _entities.AnyAsync(a => a.Id == id);
        }
    }
}