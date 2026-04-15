using Microsoft.AspNetCore.Mvc;

namespace StartStop.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HashController : ControllerBase
    {
        [HttpGet("{senha}")]
        public IActionResult GerarHash(string senha)
        {
            string hash = BCrypt.Net.BCrypt.HashPassword(senha);
            return Ok(hash);
        }
    }
}
