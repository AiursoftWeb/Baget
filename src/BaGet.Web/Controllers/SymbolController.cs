using Aiursoft.BaGet.Core.Authentication;
using Aiursoft.BaGet.Core.Configuration;
using Aiursoft.BaGet.Core.Indexing;
using Aiursoft.BaGet.Core.Storage;
using Aiursoft.BaGet.Web.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Aiursoft.BaGet.Web.Controllers
{
    public class SymbolController(
        IAuthenticationService authentication,
        ISymbolIndexingService indexer,
        ISymbolStorageService storage,
        IOptionsSnapshot<BaGetOptions> options,
        ILogger<SymbolController> logger)
        : Controller
    {
        private readonly IAuthenticationService _authentication = authentication ?? throw new ArgumentNullException(nameof(authentication));
        private readonly ISymbolIndexingService _indexer = indexer ?? throw new ArgumentNullException(nameof(indexer));
        private readonly ISymbolStorageService _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        private readonly IOptionsSnapshot<BaGetOptions> _options = options ?? throw new ArgumentNullException(nameof(options));
        private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // See: https://docs.microsoft.com/en-us/nuget/api/package-publish-resource#push-a-package
        public async Task<IActionResult> Upload(CancellationToken cancellationToken)
        {
            if (_options.Value.IsReadOnlyMode || !await _authentication.AuthenticateAsync(Request.GetApiKey(), cancellationToken))
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
                        SymbolIndexingResult.InvalidSymbolPackage => BadRequest(),
                        SymbolIndexingResult.PackageNotFound => NotFound(),
                        SymbolIndexingResult.Success => StatusCode(201),
                        _ => StatusCode(500)
                    };
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception thrown during symbol upload");

                return StatusCode(500);
            }
        }

        public async Task<IActionResult> Get(string file, string key)
        {
            var pdbStream = await _storage.GetPortablePdbContentStreamOrNullAsync(file, key);
            if (pdbStream == null)
            {
                return NotFound();
            }

            return File(pdbStream, "application/octet-stream");
        }
    }
}
