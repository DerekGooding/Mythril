using Microsoft.AspNetCore.Mvc;
using Mythril.Data;
using Newtonsoft.Json;

namespace Mythril.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EnemiesController(IWebHostEnvironment hostingEnvironment) : ControllerBase
{
    private readonly IWebHostEnvironment _hostingEnvironment = hostingEnvironment;

    [HttpGet]
    public ActionResult<List<Enemy>> Get([FromQuery] string? zone = null)
    {
        var filePath = Path.Combine(_hostingEnvironment.ContentRootPath, "Data", "enemies.json");
        var json = System.IO.File.ReadAllText(filePath);
        var enemies = JsonConvert.DeserializeObject<List<Enemy>>(json);

        if (zone is not null)
            enemies = enemies?.Where(e => e.Zone == zone).ToList();

        return Ok(enemies);
    }
}
