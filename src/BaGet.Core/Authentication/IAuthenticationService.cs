using System.Threading;
using System.Threading.Tasks;

namespace Aiursoft.BaGet.Core
{
    public interface IAuthenticationService
    {
        Task<bool> AuthenticateAsync(string apiKey, CancellationToken cancellationToken);
    }
}
