using System.Text.Json.Serialization;
using Aiursoft.BaGet.Core;
using Aiursoft.BaGet.Core.Configuration;
using Aiursoft.BaGet.Core.Entities;
using Aiursoft.BaGet.Core.Extensions;
using Aiursoft.BaGet.Core.Search;
using Aiursoft.BaGet.Core.Storage;
using Aiursoft.BaGet.Database.Sqlite;
using Aiursoft.BaGet.Web.Controllers;
using Aiursoft.BaGet.Web.Extensions;
using Aiursoft.DbTools.Sqlite;
using Aiursoft.WebTools.Abstractions.Models;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Aiursoft.BaGet.Web
{
    public class Startup : IWebStartup
    {
        public void ConfigureServices(IConfiguration configuration, IWebHostEnvironment environment, IServiceCollection services)
        {
            services.AddTransient<IConfigureOptions<CorsOptions>, ConfigureBaGetOptions>();
            services.AddTransient<IConfigureOptions<FormOptions>, ConfigureBaGetOptions>();
            services.AddTransient<IConfigureOptions<ForwardedHeadersOptions>, ConfigureBaGetOptions>();
            services.AddTransient<IValidateOptions<BaGetOptions>, ConfigureBaGetOptions>();

            services
                .AddRouting(options => options.LowercaseUrls = true)
                .AddControllers()
                .AddApplicationPart(typeof(PackageContentController).Assembly)
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                });

            services.AddRazorPages();

            services.AddHttpContextAccessor();
            services.AddTransient<IUrlGenerator, BaGetUrlGenerator>();

            var app = new BaGetApplication(services);

            services.AddConfiguration();
            services.AddBaGetServices();
            services.AddDefaultProviders();

            var connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            
            services.TryAddScoped<IContext>(provider => provider.GetRequiredService<SqliteContext>());
            services.TryAddTransient<IPackageDatabase>(provider => provider.GetRequiredService<PackageDatabase>());

            services.AddAiurSqliteWithCache<SqliteContext>(connectionString);

            services.AddProvider<IContext>((provider, _) => provider.GetRequiredService<SqliteContext>());

            services.AddProvider<IPackageDatabase>((provider, _) => provider.GetRequiredService<PackageDatabase>());

            services.AddProvider<ISearchIndexer>((provider, config) => 
                !config.HasSearchType(DependencyInjectionExtensions.DatabaseSearchType) ? null : provider.GetRequiredService<NullSearchIndexer>());

            services.AddProvider<ISearchService>((provider, config) => 
                !config.HasSearchType(DependencyInjectionExtensions.DatabaseSearchType) ? null : provider.GetRequiredService<DatabaseSearchService>());
            
            app.AddFileStorage();
            services.AddFallbackServices();
            services.AddScoped(DependencyInjectionExtensions.GetServiceFromProviders<IContext>);
            services.AddTransient(DependencyInjectionExtensions.GetServiceFromProviders<IStorageService>);
            services.AddTransient(DependencyInjectionExtensions.GetServiceFromProviders<IPackageDatabase>);
            services.AddTransient(DependencyInjectionExtensions.GetServiceFromProviders<ISearchService>);
            services.AddTransient(DependencyInjectionExtensions.GetServiceFromProviders<ISearchIndexer>);

            services.AddSingleton<IConfigureOptions<MvcRazorRuntimeCompilationOptions>, ConfigureRazorRuntimeCompilation>();
            services.AddCors();
        }

        public void Configure(WebApplication app)
        {
            var options = app.Configuration.Get<BaGetOptions>();
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseStatusCodePages();
            }
            app.UsePathBase(options.PathBase);
            app.UseStaticFiles();
            app.UseRouting();
            app.UseCors(ConfigureBaGetOptions.CorsPolicy);
            app.UseOperationCancelledMiddleware();
            new BaGetEndpointBuilder().MapEndpoints(app);
        }
    }
}
