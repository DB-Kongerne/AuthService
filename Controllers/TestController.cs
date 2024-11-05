using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;

namespace AuthService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestController : ControllerBase
    {
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok("You're authorized"); //Husk at export Secret og Issuer (Test med echoo $)//
        }
    }
}
