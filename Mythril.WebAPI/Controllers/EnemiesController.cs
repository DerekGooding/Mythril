using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Mythril.Data;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace Mythril.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EnemiesController : ControllerBase
{
    private readonly IWebHostEnvironment _hostingEnvironment;

    public EnemiesController(IWebHostEnvironment hostingEnvironment)
    {
        _hostingEnvironment = hostingEnvironment;
    }

    [HttpGet]
    public ActionResult<List<Enemy>> Get()
    {
        var filePath = Path.Combine(_hostingEnvironment.ContentRootPath, "Data", "enemies.json");
        var json = System.IO.File.ReadAllText(filePath);
        var enemies = JsonConvert.DeserializeObject<List<Enemy>>(json);
        return Ok(enemies);
    }
}
