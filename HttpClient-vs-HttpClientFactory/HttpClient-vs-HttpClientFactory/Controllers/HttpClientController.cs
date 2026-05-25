using HttpClient_vs_HttpClientFactory.Services;
using Microsoft.AspNetCore.Mvc;

namespace HttpClient_vs_HttpClientFactory.Controllers
{
    [ApiController]
    [Route("api/httpclient")]
    public class HttpClientController : ControllerBase
    {
        private readonly UserServiceHttpClient _service;

        public HttpClientController(UserServiceHttpClient service)
        {
            _service = service;
        }


        //This example with httpclient
        [HttpGet]
        public async Task<IActionResult> GetUser()
        {
            var users = await _service.GetUsersAsync();
            return Ok(users);
        }
    }
}
