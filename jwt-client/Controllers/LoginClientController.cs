using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using jwt_client.Dto;
using System.Net.Http.Headers;

namespace jwt_client.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginClientController(IHttpClientFactory clientFactory) : ControllerBase
    {
        private readonly string loginApi = "https://localhost:7065/api/Login";

        [HttpPost]
        public async Task<ActionResult<string>> Login()
        {
            var httpClient = clientFactory.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Post, loginApi);
            
            request.Content = new StringContent(
                JsonSerializer.Serialize(new LoginDto (){ Name = "enosh"}), 
                Encoding.UTF8, 
                "application/json"
            );
            var response = await httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                HttpContext.Session.SetString("JwtToken", content);
                return Ok(content);
            }
            return BadRequest("Login failure");
        }

        [HttpGet("after-login")]
        public async Task<ActionResult<string>> AfterLogin()
        {
            var token = HttpContext.Session.GetString("JwtToken");
            if (token == null) { return BadRequest("Not authenticated"); }
            
            var httpClient = clientFactory.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Get, $"{loginApi}/protected");
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode) 
            {
                var content =  response.Content.ReadAsStringAsync();
                return Ok(content);
            }
            return BadRequest();
        }
    }
}
