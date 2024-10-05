using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExoskyController : ControllerBase
    {
        [HttpGet("getFile/{fileName}")]
        public async Task<IActionResult> GetStarData(string fileName)
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "data", fileName);
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
    }
}
