using Aiursoft.BaGet.Core.Configuration;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Options;

namespace Aiursoft.BaGet.Web
{
    /// <summary>
    /// BaGet's options configuration, specific to the default BaGet application.
    /// Don't use this if you are embedding BaGet into your own custom ASP.NET Core application.
    /// </summary>
    public class ConfigureBaGetOptions
        : IConfigureOptions<CorsOptions>
        , IConfigureOptions<FormOptions>
        , IConfigureOptions<ForwardedHeadersOptions>
        , IValidateOptions<BaGetOptions>
    {
        public const string CorsPolicy = "AllowAll";

        private static readonly HashSet<string> ValidStorageTypes
            = new(StringComparer.OrdinalIgnoreCase)
            {
                "Filesystem"
            };

        private static readonly HashSet<string> ValidSearchTypes
            = new(StringComparer.OrdinalIgnoreCase)
            {
                "Database"
            };

        public void Configure(CorsOptions options)
        {
            // TODO: Consider disabling this on production builds.
            options.AddPolicy(
                CorsPolicy,
                builder => builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader());
        }

        public void Configure(FormOptions options)
        {
            options.MultipartBodyLengthLimit = int.MaxValue;
        }

        public void Configure(ForwardedHeadersOptions options)
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

            // Do not restrict to local network/proxy
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();
        }

        public ValidateOptionsResult Validate(string name, BaGetOptions options)
        {
            var failures = new List<string>();

            if (options.Mirror == null) failures.Add($"The '{nameof(BaGetOptions.Mirror)}' config is required");
            if (options.Search == null) failures.Add($"The '{nameof(BaGetOptions.Search)}' config is required");
            if (options.Storage == null) failures.Add($"The '{nameof(BaGetOptions.Storage)}' config is required");

            if (!ValidStorageTypes.Contains(options.Storage?.Type))
            {
                failures.Add(
                    $"The '{nameof(BaGetOptions.Storage)}:{nameof(StorageOptions.Type)}' config is invalid. " +
                    $"Allowed values: {string.Join(", ", ValidStorageTypes)}");
            }

            if (!ValidSearchTypes.Contains(options.Search?.Type))
            {
                failures.Add(
                    $"The '{nameof(BaGetOptions.Search)}:{nameof(SearchOptions.Type)}' config is invalid. " +
                    $"Allowed values: {string.Join(", ", ValidSearchTypes)}");
            }

            if (failures.Any()) return ValidateOptionsResult.Fail(failures);

            return ValidateOptionsResult.Success;
        }
    }
}
