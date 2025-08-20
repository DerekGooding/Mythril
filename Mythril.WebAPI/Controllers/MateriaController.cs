using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Mythril.Data.Materia;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace Mythril.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MateriaController : ControllerBase
{
    private readonly IWebHostEnvironment _hostingEnvironment;

    public MateriaController(IWebHostEnvironment hostingEnvironment)
    {
        _hostingEnvironment = hostingEnvironment;
    }

    [HttpGet]
    public ActionResult<List<Materia>> Get()
    {
        var settings = new JsonSerializerSettings
        {
            Converters = { new MateriaConverter() }
        };
        var filePath = Path.Combine(_hostingEnvironment.ContentRootPath, "Data", "materia.json");
        var json = System.IO.File.ReadAllText(filePath);
        var materia = JsonConvert.DeserializeObject<List<Materia>>(json, settings);
        return Ok(materia);
    }
}
