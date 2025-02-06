using CompanyManager.Logic.Contracts;
using Microsoft.EntityFrameworkCore;

namespace CompanyManager.WebApi.Contracts
{
    public interface IContextAccessor : IDisposable
    {
        IContext GetContext();
        DbSet<TEntity>? GetDbSet<TEntity>() where TEntity : class;
    }
}