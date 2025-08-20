using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Mythril.Data;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace Mythril.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CardsController : ControllerBase
{
    private readonly IWebHostEnvironment _hostingEnvironment;

    public CardsController(IWebHostEnvironment hostingEnvironment)
    {
        _hostingEnvironment = hostingEnvironment;
    }

    [HttpGet]
    public ActionResult<List<CardData>> Get()
    {
        var filePath = Path.Combine(_hostingEnvironment.ContentRootPath, "Data", "cards.json");
        var json = System.IO.File.ReadAllText(filePath);
        var cards = JsonConvert.DeserializeObject<List<CardData>>(json);
        return Ok(cards);
    }
}
