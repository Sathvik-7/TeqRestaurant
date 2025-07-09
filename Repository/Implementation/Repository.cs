

using Microsoft.EntityFrameworkCore;
using System.Linq;
using TeqRestaurant.Data;

namespace TeqRestaurant.Repository.Implementation
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected ApplicationDbContext _dbContext { get; set; }

        private DbSet<T> _dbSet { get; set; }

        public Repository(ApplicationDbContext dbContext) 
        {
            this._dbContext = dbContext;
           _dbSet = dbContext.Set<T>();   
        }

        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            T entity = await _dbSet.FindAsync(id);
            _dbSet.Remove(entity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<T> GetByIdAsync(int id, QueryOptions<T> options)
        {
            IQueryable<T> query = _dbSet;

            if(options.HasWhere)
            {
                query = query.Where(options.Where); 
            }
            if (options.HasOrderBy)
            {
                query = query.OrderBy(options.OrderBy);
            }
            foreach(string include in options.GetIncludes())
            {
                query = query.Include(include);
            }

            var key = _dbContext.Model.FindEntityType(typeof(T)).FindPrimaryKey().Properties.FirstOrDefault();

            string primaryKeyName = key?.Name;

            return await query.FirstOrDefaultAsync(e => EF.Property<int>(e,primaryKeyName)==id);
        }

        public async Task UpdateAsync(T entity)
        {
            _dbContext.Update(entity);
            await _dbContext.SaveChangesAsync();
        }
    }
}
