// ReSharper disable UnusedAutoPropertyAccessor.Global

using Aiursoft.BaGet.Core.Entities.Converters;
using Aiursoft.DbTools;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aiursoft.BaGet.Core.Entities
{
    public abstract class AbstractContext(DbContextOptions options) : DbContext(options), ICanMigrate
    {
        public const int DefaultMaxStringLength = 4000;
        public const int MaxPackageIdLength = 128;
        public const int MaxPackageVersionLength = 64;
        public const int MaxPackageMinClientVersionLength = 44;
        public const int MaxPackageLanguageLength = 20;
        public const int MaxPackageTitleLength = 256;
        public const int MaxPackageTypeNameLength = 512;
        public const int MaxPackageTypeVersionLength = 64;
        public const int MaxRepositoryTypeLength = 100;
        public const int MaxTargetFrameworkLength = 256;
        public const int MaxPackageDependencyVersionRangeLength = 256;

        public virtual Task MigrateAsync(CancellationToken cancellationToken) =>
            Database.MigrateAsync(cancellationToken);

        public virtual Task<bool> CanConnectAsync() =>
            Database.CanConnectAsync();

        public DbSet<Package> Packages { get; set; }
        public DbSet<PackageDependency> PackageDependencies { get; set; }
        public DbSet<PackageType> PackageTypes { get; set; }
        public DbSet<TargetFramework> TargetFrameworks { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Package>(BuildPackageEntity);
            builder.Entity<PackageDependency>(BuildPackageDependencyEntity);
            builder.Entity<PackageType>(BuildPackageTypeEntity);
            builder.Entity<TargetFramework>(BuildTargetFrameworkEntity);
        }

        private void BuildPackageEntity(EntityTypeBuilder<Package> package)
        {
            package.HasKey(p => p.Key);
            package.HasIndex(p => p.Id);
            package.HasIndex(p => new { p.Id, p.NormalizedVersionString })
                .IsUnique();

            package.Property(p => p.Id)
                .HasMaxLength(MaxPackageIdLength)
                .IsRequired();

            package.Property(p => p.NormalizedVersionString)
                .HasColumnName("Version")
                .HasMaxLength(MaxPackageVersionLength)
                .IsRequired();

            package.Property(p => p.OriginalVersionString)
                .HasColumnName("OriginalVersion")
                .HasMaxLength(MaxPackageVersionLength);

            package.Property(p => p.ReleaseNotes)
                .HasColumnName("ReleaseNotes");

            package.Property(p => p.Authors)
                .HasMaxLength(DefaultMaxStringLength)
                .HasConversion(StringArrayToJsonConverter.Instance)
                .Metadata.SetValueComparer(StringArrayComparer.Instance);

            package.Property(p => p.IconUrl)
                .HasConversion(UriToStringConverter.Instance)
                .HasColumnType("TEXT");  // Change from VARCHAR(4000) to TEXT

            package.Property(p => p.LicenseUrl)
                .HasConversion(UriToStringConverter.Instance)
                .HasColumnType("TEXT");  // Change from VARCHAR(4000) to TEXT

            package.Property(p => p.ProjectUrl)
                .HasConversion(UriToStringConverter.Instance)
                .HasColumnType("TEXT");  // Change from VARCHAR(4000) to TEXT

            package.Property(p => p.RepositoryUrl)
                .HasConversion(UriToStringConverter.Instance)
                .HasColumnType("TEXT");  // Change from VARCHAR(4000) to TEXT

            package.Property(p => p.Tags)
                .HasMaxLength(DefaultMaxStringLength)
                .HasConversion(StringArrayToJsonConverter.Instance)
                .Metadata.SetValueComparer(StringArrayComparer.Instance);

            package.Property(p => p.Tags).HasColumnType("TEXT");
            package.Property(p => p.Description).HasColumnType("TEXT");
            package.Property(p => p.ReleaseNotes).HasColumnType("TEXT");
            package.Property(p => p.Summary).HasColumnType("TEXT");

            package.Property(p => p.Description).HasMaxLength(DefaultMaxStringLength);
            package.Property(p => p.Language).HasMaxLength(MaxPackageLanguageLength);
            package.Property(p => p.MinClientVersion).HasMaxLength(MaxPackageMinClientVersionLength);
            package.Property(p => p.Summary).HasMaxLength(DefaultMaxStringLength);
            package.Property(p => p.Title).HasMaxLength(MaxPackageTitleLength);
            package.Property(p => p.RepositoryType).HasMaxLength(MaxRepositoryTypeLength);

            package.Ignore(p => p.Version);
            package.Ignore(p => p.IconUrlString);
            package.Ignore(p => p.LicenseUrlString);
            package.Ignore(p => p.ProjectUrlString);
            package.Ignore(p => p.RepositoryUrlString);

            // TODO: This is needed to make the dependency to package relationship required.
            // Unfortunately, this would generate a migration that drops a foreign key, which
            // isn't supported by SQLite. The migrations will be need to be recreated for this.
            // Consumers will need to recreate their database and reindex all their packages.
            // To make this transition easier, I'd like to finish this change:
            // https://github.com/loic-sharma/BaGet/pull/174
            //package.HasMany(p => p.Dependencies)
            //    .WithOne(d => d.Package)
            //    .IsRequired();

            package.HasMany(p => p.PackageTypes)
                .WithOne(d => d.Package)
                .IsRequired();

            package.HasMany(p => p.TargetFrameworks)
                .WithOne(d => d.Package)
                .IsRequired();

            package.Property(p => p.RowVersion).IsRowVersion();
        }

        private void BuildPackageDependencyEntity(EntityTypeBuilder<PackageDependency> dependency)
        {
            dependency.HasKey(d => d.Key);
            dependency.HasIndex(d => d.Id);

            dependency.Property(d => d.Id).HasMaxLength(MaxPackageIdLength);
            dependency.Property(d => d.VersionRange).HasMaxLength(MaxPackageDependencyVersionRangeLength);
            dependency.Property(d => d.TargetFramework).HasMaxLength(MaxTargetFrameworkLength);
        }

        private void BuildPackageTypeEntity(EntityTypeBuilder<PackageType> type)
        {
            type.HasKey(d => d.Key);
            type.HasIndex(d => d.Name);

            type.Property(d => d.Name).HasMaxLength(MaxPackageTypeNameLength);
            type.Property(d => d.Version).HasMaxLength(MaxPackageTypeVersionLength);
        }

        private void BuildTargetFrameworkEntity(EntityTypeBuilder<TargetFramework> targetFramework)
        {
            targetFramework.HasKey(f => f.Key);
            targetFramework.HasIndex(f => f.Moniker);

            targetFramework.Property(f => f.Moniker).HasMaxLength(MaxTargetFrameworkLength);
        }
    }
}
