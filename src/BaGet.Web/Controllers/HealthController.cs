using Microsoft.AspNetCore.Mvc;

namespace Aiursoft.BaGet.Web.Controllers
{
    public class HealthController : Controller
    {
        [HttpGet]
        [Route("health")]
        public IActionResult Get()
        {
            return Ok(new { status = "Healthy" });
        }
    }
}
