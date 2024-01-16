using System.Net;
using System.Reflection;
using Aiursoft.BaGet.Core.Authentication;
using Aiursoft.BaGet.Core.Configuration;
using Aiursoft.BaGet.Core.Content;
using Aiursoft.BaGet.Core.Entities;
using Aiursoft.BaGet.Core.Indexing;
using Aiursoft.BaGet.Core.Metadata;
using Aiursoft.BaGet.Core.Search;
using Aiursoft.BaGet.Core.ServiceIndex;
using Aiursoft.BaGet.Core.Storage;
using Aiursoft.BaGet.Core.Upstream;
using Aiursoft.BaGet.Core.Upstream.Clients;
using Aiursoft.BaGet.Core.Validation;
using Aiursoft.BaGet.Protocol;
using Aiursoft.BaGet.Protocol.ClientFactories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Aiursoft.BaGet.Core.Extensions
{
    public static class DependencyInjectionExtensions
    {
        /// <summary>
        /// Configures and validates options.
        /// </summary>
        /// <typeparam name="TOptions">The options type that should be added.</typeparam>
        /// <param name="services">The dependency injection container to add options.</param>
        /// <param name="key">
        /// The configuration key that should be used when configuring the options.
        /// If null, the root configuration will be used to configure the options.
        /// </param>
        /// <returns>The dependency injection container.</returns>
        public static IServiceCollection AddBaGetOptions<TOptions>(
            this IServiceCollection services,
            string key = null)
            where TOptions : class
        {
            services.AddSingleton<IValidateOptions<TOptions>>(new ValidateBaGetOptions<TOptions>(key));
            services.AddSingleton<IConfigureOptions<TOptions>>(provider =>
            {
                var config = provider.GetRequiredService<IConfiguration>();
                if (key != null)
                {
                    config = config.GetSection(key);
                }

                return new BindOptions<TOptions>(config);
            });

            return services;
        }

        public static void AddConfiguration(this IServiceCollection services)
        {
            services.AddBaGetOptions<BaGetOptions>();
            services.AddBaGetOptions<FileSystemStorageOptions>(nameof(BaGetOptions.Storage));
            services.AddBaGetOptions<MirrorOptions>(nameof(BaGetOptions.Mirror));
            services.AddBaGetOptions<SearchOptions>(nameof(BaGetOptions.Search));
            services.AddBaGetOptions<StorageOptions>(nameof(BaGetOptions.Storage));
        }

        public static void AddBaGetServices(this IServiceCollection services)
        {
            services.TryAddSingleton<IFrameworkCompatibilityService, FrameworkCompatibilityService>();
            services.TryAddSingleton<IPackageDownloadsSource, PackageDownloadsJsonSource>();

            services.TryAddSingleton<ISearchResponseBuilder, SearchResponseBuilder>();
            services.TryAddSingleton<NuGetClient>();
            services.TryAddSingleton<NullSearchIndexer>();
            services.TryAddSingleton<NullSearchService>();
            services.TryAddSingleton<RegistrationBuilder>();
            services.TryAddSingleton<SystemTime>();
            services.TryAddSingleton<ValidateStartupOptions>();

            services.TryAddSingleton(HttpClientFactory);
            services.TryAddSingleton(NuGetClientFactoryFactory);

            services.TryAddScoped<DownloadsImporter>();

            services.TryAddTransient<IAuthenticationService, ApiKeyAuthenticationService>();
            services.TryAddTransient<IPackageContentService, DefaultPackageContentService>();
            services.TryAddTransient<IPackageDeletionService, PackageDeletionService>();
            services.TryAddTransient<IPackageIndexingService, PackageIndexingService>();
            services.TryAddTransient<IPackageMetadataService, DefaultPackageMetadataService>();
            services.TryAddTransient<IPackageService, PackageService>();
            services.TryAddTransient<IPackageStorageService, PackageStorageService>();
            services.TryAddTransient<IServiceIndexService, BaGetServiceIndex>();
            services.TryAddTransient<ISymbolIndexingService, SymbolIndexingService>();
            services.TryAddTransient<ISymbolStorageService, SymbolStorageService>();

            services.TryAddTransient<DatabaseSearchService>();
            services.TryAddTransient<FileStorageService>();
            services.TryAddTransient<PackageService>();
            services.TryAddTransient<V2UpstreamClient>();
            services.TryAddTransient<V3UpstreamClient>();
            services.TryAddTransient<DisabledUpstreamClient>();
            services.TryAddSingleton<NullStorageService>();
            services.TryAddTransient<PackageDatabase>();

            services.TryAddTransient(UpstreamClientFactory);
        }

        public static void AddDefaultProviders(this IServiceCollection services)
        {
            services.AddProvider((provider, configuration) =>
            {
                if (!configuration.HasSearchType("null")) return null;

                return provider.GetRequiredService<NullSearchService>();
            });

            services.AddProvider((provider, configuration) =>
            {
                if (!configuration.HasSearchType("null")) return null;

                return provider.GetRequiredService<NullSearchIndexer>();
            });

            services.AddProvider<IStorageService>((provider, configuration) =>
            {
                if (configuration.HasStorageType("filesystem"))
                {
                    return provider.GetRequiredService<FileStorageService>();
                }

                if (configuration.HasStorageType("null"))
                {
                    return provider.GetRequiredService<NullStorageService>();
                }

                return null;
            });
        }

        public static void AddFallbackServices(this IServiceCollection services)
        {
            services.TryAddScoped<IContext, NullContext>();
            services.TryAddTransient<ISearchIndexer>(provider => provider.GetRequiredService<NullSearchIndexer>());
            services.TryAddTransient<ISearchService>(provider => provider.GetRequiredService<DatabaseSearchService>());
        }

        private static HttpClient HttpClientFactory(IServiceProvider provider)
        {
            var options = provider.GetRequiredService<IOptions<MirrorOptions>>().Value;

            var assembly = Assembly.GetEntryAssembly();
            var assemblyName = assembly?.GetName().Name ?? "Unknown";
            var assemblyVersion = assembly?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "0.0.0";

            var client = new HttpClient(new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            });

            client.DefaultRequestHeaders.Add("User-Agent", $"{assemblyName}/{assemblyVersion}");
            client.Timeout = TimeSpan.FromSeconds(options.PackageDownloadTimeoutSeconds);

            return client;
        }

        private static NuGetClientFactory NuGetClientFactoryFactory(IServiceProvider provider)
        {
            var httpClient = provider.GetRequiredService<HttpClient>();
            var options = provider.GetRequiredService<IOptions<MirrorOptions>>();

            return new NuGetClientFactory(
                httpClient,
                options.Value.PackageSource.ToString());
        }

        private static IUpstreamClient UpstreamClientFactory(IServiceProvider provider)
        {
            var options = provider.GetRequiredService<IOptionsSnapshot<MirrorOptions>>();

            if (!options.Value.Enabled)
            {
                return provider.GetRequiredService<DisabledUpstreamClient>();
            }

            if (options.Value.Legacy)
            {
                return provider.GetRequiredService<V2UpstreamClient>();
            }

            return provider.GetRequiredService<V3UpstreamClient>();
        }
        
        private static readonly string SearchTypeKey = $"{nameof(BaGetOptions.Search)}:{nameof(SearchOptions.Type)}";
        private static readonly string StorageTypeKey = $"{nameof(BaGetOptions.Storage)}:{nameof(StorageOptions.Type)}";

        public static readonly string DatabaseSearchType = "Database";

        /// <summary>
        /// Add a new provider to the dependency injection container. The provider may
        /// provide an implementation of the service, or it may return null.
        /// </summary>
        /// <typeparam name="TService">The service that may be provided.</typeparam>
        /// <param name="services">The dependency injection container.</param>
        /// <param name="func">A handler that provides the service, or null.</param>
        /// <returns>The dependency injection container.</returns>
        public static IServiceCollection AddProvider<TService>(
            this IServiceCollection services,
            Func<IServiceProvider, IConfiguration, TService> func)
            where TService : class
        {
            services.AddSingleton<IProvider<TService>>(new DelegateProvider<TService>(func));
            return services;
        }

        /// <summary>
        /// Determine whether a search type is currently active.
        /// </summary>
        /// <param name="config">The application's configuration.</param>
        /// <param name="value">The search type that should be checked.</param>
        /// <returns>Whether the search type is active.</returns>
        public static bool HasSearchType(this IConfiguration config, string value)
        {
            return config[SearchTypeKey]?.Equals(value, StringComparison.OrdinalIgnoreCase) ?? false;
        }

        /// <summary>
        /// Determine whether a storage type is currently active.
        /// </summary>
        /// <param name="config">The application's configuration.</param>
        /// <param name="value">The storage type that should be checked.</param>
        /// <returns>Whether the database type is active.</returns>
        public static bool HasStorageType(this IConfiguration config, string value)
        {
            return config[StorageTypeKey]?.Equals(value, StringComparison.OrdinalIgnoreCase) ?? false;
        }

        /// <summary>
        /// Runs through all providers to resolve the <typeparamref name="TService"/>.
        /// </summary>
        /// <typeparam name="TService">The service that will be resolved using providers.</typeparam>
        /// <param name="services">The dependency injection container.</param>
        /// <returns>An instance of the service created by the providers.</returns>
        public static TService GetServiceFromProviders<TService>(IServiceProvider services)
            where TService : class
        {
            // Run through all the providers for the type. Find the first provider that results a non-null result.
            var providers = services.GetRequiredService<IEnumerable<IProvider<TService>>>();
            var configuration = services.GetRequiredService<IConfiguration>();

            foreach (var provider in providers)
            {
                var result = provider.GetOrNull(services, configuration);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }
    }
}
