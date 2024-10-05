using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExoskyController : ControllerBase
    {
        [HttpGet("getFile/{fileName}")]
        public async Task<IActionResult> GetFile(string fileName)
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "exosky", fileName);
            if(!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }


            var memory = new MemoryStream();
            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }

            memory.Position = 0;
            return File(memory, "application/octet-stream", fileName);
        }

        [HttpGet("getStarData/{number}")]
        public async Task<IActionResult> GetStarData(int number)
        {
            if (!System.IO.File.Exists(SpaceData.StarDataFilePath))
                if (!await SpaceData.RequestStarDataFromGAIA(number))
                    return NotFound();

            return Ok();
        }

        [HttpGet("getExoplanetData")]
        public async Task<IActionResult> GetExoplanetData()
        {
            if (!System.IO.File.Exists(SpaceData.ExoplanetFilePath))
                if (!await SpaceData.RequestExoplanetDataFromNASA())
                    return NotFound();
            
            return Ok();
        }
    }
}
