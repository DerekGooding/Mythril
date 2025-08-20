using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Mythril.Data;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace Mythril.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CharactersController : ControllerBase
{
    private readonly IWebHostEnvironment _hostingEnvironment;

    public CharactersController(IWebHostEnvironment hostingEnvironment)
    {
        _hostingEnvironment = hostingEnvironment;
    }

    [HttpGet]
    public ActionResult<List<Character>> Get()
    {
        var filePath = Path.Combine(_hostingEnvironment.ContentRootPath, "Data", "characters.json");
        var json = System.IO.File.ReadAllText(filePath);
        var characters = JsonConvert.DeserializeObject<List<Character>>(json);
        return Ok(characters);
    }
}
