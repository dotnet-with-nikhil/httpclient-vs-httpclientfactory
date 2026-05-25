using HttpClient_vs_HttpClientFactory.Services;
using Microsoft.AspNetCore.Mvc;

namespace HttpClient_vs_HttpClientFactory.Controllers
{
    [ApiController]
    [Route("api/httpclientfactory")]
    public class HttpClientFactoryController : ControllerBase
    {
        private readonly UserServiceFactory _service;

        public HttpClientFactoryController(UserServiceFactory service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var user = await _service.GetUsersAsync();
            return Ok(user);
        }
    }
}
