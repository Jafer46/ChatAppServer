using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Linq;
using System.Linq.Expressions;

namespace ChatAppServer.Interfaces
{
    public interface IEntity<T>
    {
        Task<T> Create(T entity);
        Task<bool> Update(T entity);
        Task<bool> Delete(Func<T, bool> predicate);
        Task<T?> ReadFirst(Func<T, bool> predicate);
        Task<T?> ReadLast(Func<T, bool> predicate);
        Task<List<T>> ReadAll(Func<T, bool> predicate);
    }
}