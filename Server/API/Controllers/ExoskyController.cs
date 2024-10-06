using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExoskyController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ExoskyController(IConfiguration configuration, AppDbContext context)
        {
            _context = context;
            SpaceData.PythonPath = configuration["ExternalTools:PythonPath"];
        }


        [HttpGet("getExoplanetStars/{exoplanet}")]
        public async Task<IActionResult> GetExoplanetStars(string exoplanet)
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), $"cache/{exoplanet}.json");
            if(!System.IO.File.Exists(filePath))
                if(!SpaceData.GetExoplanetStars(exoplanet, _context))
                    return NotFound();


            var memory = new MemoryStream();
            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }

            memory.Position = 0;
            return File(memory, "application/octet-stream", $"{exoplanet}.json");
        }

        [HttpGet("getExoplanets")]
        public async Task<IActionResult> GetExoplanets()
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), $"cache/EXOPLANETS.json");
            if (!System.IO.File.Exists(filePath))
                SpaceData.GetExoplanets(_context);

            var memory = new MemoryStream();
            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }

            memory.Position = 0;
            return File(memory, "application/octet-stream", $"EXOPLANET.json");
        }

        [HttpGet("getStarData/{number}")]
        public async Task<IActionResult> GetStarData(int number)
        {
            await SpaceData.AddGAIAStarDataToDB(number, _context);
            return Ok(_context.Stars.Count());
        }

        [HttpGet("getExoplanetData")]
        public async Task<IActionResult> GetExoplanetData()
        {
            await SpaceData.AddNASAExoplanetDataToDB(_context);
            return Ok(_context.Exoplanets.Count());
        }
    }
}
