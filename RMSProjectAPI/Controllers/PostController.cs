using Microsoft.AspNetCore.Mvc;
using RMSProjectAPI.Database;


    [Route("api/[controller]")]
    [ApiController]
    public class PostController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PostController(AppDbContext context)
        {
            _context = context;
        }

    [HttpGet("GetHello")]
    public IActionResult GetOk()
    {
        return Ok("That's Ok ya man");
    }
}

