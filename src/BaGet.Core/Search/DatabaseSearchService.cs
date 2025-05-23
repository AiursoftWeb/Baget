using Aiursoft.BaGet.Core.Entities;
using Aiursoft.BaGet.Core.Indexing;
using Aiursoft.BaGet.Core.Metadata;
using Aiursoft.BaGet.Protocol.Models;
using Microsoft.EntityFrameworkCore;

namespace Aiursoft.BaGet.Core.Search
{
    public class DatabaseSearchService : ISearchService
    {
        private readonly AbstractContext _context;
        private readonly IFrameworkCompatibilityService _frameworks;
        private readonly ISearchResponseBuilder _searchBuilder;

        public DatabaseSearchService(
            AbstractContext context,
            IFrameworkCompatibilityService frameworks,
            ISearchResponseBuilder searchBuilder)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _frameworks = frameworks ?? throw new ArgumentNullException(nameof(frameworks));
            _searchBuilder = searchBuilder ?? throw new ArgumentNullException(nameof(searchBuilder));
        }

        public async Task<SearchResponse> SearchAsync(SearchRequest request,  CancellationToken cancellationToken)
        {
            var frameworks = GetCompatibleFrameworksOrNull(request.Framework);

            IQueryable<Package> search = _context.Packages;
            search = ApplySearchQuery(search, request.Query);
            search = ApplySearchFilters(
                search,
                request.IncludePrerelease,
                request.IncludeSemVer2,
                request.PackageType,
                frameworks);

            var packageIds = await search
                .Distinct()
                .OrderBy(x => x.Id)
                .Select(p => p.Id)
                .Skip(request.Skip)
                .Take(request.Take)
                .ToListAsync(cancellationToken);

            search = _context.Packages.Where(p => EF.Constant(packageIds).Contains(p.Id));
            search = ApplySearchFilters(
                search,
                request.IncludePrerelease,
                request.IncludeSemVer2,
                request.PackageType,
                frameworks);

            var results = await search.ToListAsync(cancellationToken);
            var groupedResults = results
                .GroupBy(p => p.Id, StringComparer.OrdinalIgnoreCase)
                .Select(group => new PackageRegistration(group.Key, group.ToList()))
                .ToList();

            return _searchBuilder.BuildSearch(groupedResults);
        }

        public async Task<AutocompleteResponse> AutocompleteAsync(
            AutocompleteRequest request,
            CancellationToken cancellationToken)
        {
            IQueryable<Package> search = _context.Packages;

            search = ApplySearchQuery(search, request.Query);
            search = ApplySearchFilters(
                search,
                request.IncludePrerelease,
                request.IncludeSemVer2,
                request.PackageType,
                frameworks: null);

            var packageIds = await search
                .OrderBy(p => p.Id)
                .Select(p => p.Id)
                .Distinct()
                .Skip(request.Skip)
                .Take(request.Take)
                .ToListAsync(cancellationToken);

            return _searchBuilder.BuildAutocomplete(packageIds);
        }

        public async Task<AutocompleteResponse> ListPackageVersionsAsync(
            VersionsRequest request,
            CancellationToken cancellationToken)
        {
            var packageId = request.PackageId.ToLower();
            var search = _context
                .Packages
                .Where(p => p.Id.ToLower().Equals(packageId));

            search = ApplySearchFilters(
                search,
                request.IncludePrerelease,
                request.IncludeSemVer2,
                packageType: null,
                frameworks: null);

            var packageVersions = await search
                .Select(p => p.NormalizedVersionString)
                .ToListAsync(cancellationToken);

            return _searchBuilder.BuildAutocomplete(packageVersions);
        }

        public async Task<DependentsResponse> FindDependentsAsync(string packageId, CancellationToken cancellationToken)
        {
            var dependents = await _context
                .Packages
                .Where(p => p.Listed)
                .OrderBy(p => p.Id)
                .Where(p => p.Dependencies.Any(d => d.Id == packageId))
                .Take(20)
                .Select(r => new PackageDependent
                {
                    Id = r.Id,
                    Description = r.Description
                })
                .Distinct()
                .ToListAsync(cancellationToken);

            return _searchBuilder.BuildDependents(dependents);
        }

        private IQueryable<Package> ApplySearchQuery(IQueryable<Package> query, string search)
        {
            if (string.IsNullOrEmpty(search))
            {
                return query;
            }

            search = search.ToLowerInvariant();

            return query.Where(p => p.Id.ToLower().Contains(search));
        }

        private IQueryable<Package> ApplySearchFilters(
            IQueryable<Package> query,
            bool includePrerelease,
            bool includeSemVer2,
            string packageType,
            IReadOnlyList<string> frameworks)
        {
            if (!includePrerelease)
            {
                query = query.Where(p => !p.IsPrerelease);
            }

            if (!includeSemVer2)
            {
                query = query.Where(p => p.SemVerLevel != SemVerLevel.SemVer2);
            }

            if (!string.IsNullOrEmpty(packageType))
            {
                query = query.Where(p => p.PackageTypes.Any(t => t.Name == packageType));
            }

            if (frameworks != null)
            {
                query = query.Where(p => p.TargetFrameworks.Any(f => frameworks.Contains(f.Moniker)));
            }

            return query.Where(p => p.Listed);
        }

        private IReadOnlyList<string> GetCompatibleFrameworksOrNull(string framework)
        {
            if (framework == null) return null;

            return _frameworks.FindAllCompatibleFrameworks(framework);
        }
    }
}
