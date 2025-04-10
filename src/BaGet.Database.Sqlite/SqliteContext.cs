using Aiursoft.BaGet.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Aiursoft.BaGet.Database.Sqlite
{
    public class SqliteContext(DbContextOptions<SqliteContext> options) : AbstractContext(options)
    {
        public override Task<bool> CanConnectAsync()
        {
            return Task.FromResult(true);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Package>()
                .Property(p => p.Id)
                .HasColumnType("TEXT COLLATE NOCASE");

            builder.Entity<Package>()
                .Property(p => p.NormalizedVersionString)
                .HasColumnType("TEXT COLLATE NOCASE");

            builder.Entity<PackageDependency>()
                .Property(d => d.Id)
                .HasColumnType("TEXT COLLATE NOCASE");

            builder.Entity<PackageType>()
                .Property(t => t.Name)
                .HasColumnType("TEXT COLLATE NOCASE");

            builder.Entity<TargetFramework>()
                .Property(f => f.Moniker)
                .HasColumnType("TEXT COLLATE NOCASE");
        }
    }
}
