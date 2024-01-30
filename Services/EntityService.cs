using ChatAppServer.Database;
using ChatAppServer.Interfaces;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Linq.Expressions;

namespace ChatAppServer.Services
{
    public class EntityService<T> : IEntity<T> where T : class
    {
        private readonly DataContext _context;
        public EntityService(DataContext context)
        {
            _context = context;
        }
        public Task<T?> ReadFirst(Func<T, bool> predicate)
        {
            T? result = _context.Set<T>().FirstOrDefault(predicate);
            return Task.FromResult(result);
        }

        public Task<List<T>> ReadAll(Func<T, bool> predicate)
        {
            List<T> result = _context.Set<T>().Where(predicate).ToList();
            return Task.FromResult(result);
        }
        public Task<T?> ReadLast(Func<T, bool> predicate)
        {
            T? result = _context.Set<T>().LastOrDefault(predicate);
            return Task.FromResult(result);
        }
        public async Task<T> Create(T entity)
        {
            try
            {
                _context.Set<T>().Add(entity);
                await _context.SaveChangesAsync();

                _context.ChangeTracker.Clear();
                System.Console.WriteLine("entity created");
                return entity;
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("Error: {0}.\n{1}", ex.Message, ex.InnerException));
            }

            return null;
        }
        public async Task<bool> Update(T entity)
        {
            try
            {
                _context.Set<T>().Update(entity);
                await _context.SaveChangesAsync();

                _context.ChangeTracker.Clear();


                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("Error: {0}.\n{1}", ex.Message, ex.InnerException));
            }

            return false;
        }

        public async Task<bool> Delete(Func<T, bool> predicate)
        {
            try
            {
                IEnumerable<T> entity = _context.Set<T>().Where(predicate);
                if (entity != null)
                {
                    _context.Set<T>().RemoveRange(entity);
                    await _context.SaveChangesAsync();
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("Error: {0}.\n{1}", ex.Message, ex.InnerException));
            }

            return false;
        }

    }
}