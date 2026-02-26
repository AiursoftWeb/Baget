using Aiursoft.BaGet.Core;
using Aiursoft.BaGet.Core.Authentication;
using Aiursoft.BaGet.Core.Configuration;
using Aiursoft.BaGet.Core.Indexing;
using Aiursoft.BaGet.Web.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NuGet.Versioning;

namespace Aiursoft.BaGet.Web.Controllers
{
    public class PackagePublishController(
        IAuthenticationService authentication,
        IPackageIndexingService indexer,
        PackageDatabase packages,
        IPackageDeletionService deletionService,
        IOptionsSnapshot<BaGetOptions> options,
        ILogger<PackagePublishController> logger)
        : Controller
    {
        private readonly IAuthenticationService _authentication = authentication ?? throw new ArgumentNullException(nameof(authentication));
        private readonly IPackageIndexingService _indexer = indexer ?? throw new ArgumentNullException(nameof(indexer));
        private readonly PackageDatabase _packages = packages ?? throw new ArgumentNullException(nameof(packages));
        private readonly IPackageDeletionService _deleteService = deletionService ?? throw new ArgumentNullException(nameof(deletionService));
        private readonly IOptionsSnapshot<BaGetOptions> _options = options ?? throw new ArgumentNullException(nameof(options));
        private readonly ILogger<PackagePublishController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // See: https://docs.microsoft.com/en-us/nuget/api/package-publish-resource#push-a-package
        public async Task<IActionResult> Upload(CancellationToken cancellationToken)
        {
            if (_options.Value.IsReadOnlyMode ||
                !await _authentication.AuthenticateAsync(Request.GetApiKey(), cancellationToken))
            {
                return Unauthorized();
            }

            try
            {
                await using (var uploadStream = await Request.GetUploadStreamOrNullAsync(cancellationToken))
                {
                    if (uploadStream == null)
                    {
                        return BadRequest();
                    }

                    var result = await _indexer.IndexAsync(uploadStream, cancellationToken);

                    return result switch
                    {
                        PackageIndexingResult.InvalidPackage => BadRequest(),
                        PackageIndexingResult.PackageAlreadyExists => Conflict(),
                        PackageIndexingResult.Success => StatusCode(201),
                        _ => StatusCode(500)
                    };
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception thrown during package upload");

                return StatusCode(500);
            }
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(string id, string version, CancellationToken cancellationToken)
        {
            if (_options.Value.IsReadOnlyMode)
            {
                return Unauthorized();
            }

            if (!NuGetVersion.TryParse(version, out var nugetVersion))
            {
                return NotFound();
            }

            if (!await _authentication.AuthenticateAsync(Request.GetApiKey(), cancellationToken))
            {
                return Unauthorized();
            }

            if (await _deleteService.TryDeletePackageAsync(id, nugetVersion, cancellationToken))
            {
                return NoContent();
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPost]
        public async Task<IActionResult> Relist(string id, string version, CancellationToken cancellationToken)
        {
            if (_options.Value.IsReadOnlyMode)
            {
                return Unauthorized();
            }

            if (!NuGetVersion.TryParse(version, out var nugetVersion))
            {
                return NotFound();
            }

            if (!await _authentication.AuthenticateAsync(Request.GetApiKey(), cancellationToken))
            {
                return Unauthorized();
            }

            if (await _packages.RelistPackageAsync(id, nugetVersion, cancellationToken))
            {
                return Ok();
            }
            else
            {
                return NotFound();
            }
        }
    }
}
