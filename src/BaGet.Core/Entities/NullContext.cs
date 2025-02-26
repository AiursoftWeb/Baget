using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Aiursoft.BaGet.Core.Entities
{
    public class NullContext : IContext
    {
        public DatabaseFacade Database => throw new NotImplementedException();

        public DbSet<Package> Packages { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool IsUniqueConstraintViolationException(DbUpdateException exception)
        {
            throw new NotImplementedException();
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
