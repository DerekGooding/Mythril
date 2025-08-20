using Microsoft.AspNetCore.Mvc;
using Mythril.Data.Items;
using Newtonsoft.Json;

namespace Mythril.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ItemsController(IWebHostEnvironment hostingEnvironment) : ControllerBase
{
    private readonly IWebHostEnvironment _hostingEnvironment = hostingEnvironment;

    [HttpGet]
    public ActionResult<List<Item>> Get()
    {
        var settings = new JsonSerializerSettings
        {
            Converters = { new ItemConverter() }
        };
        var filePath = Path.Combine(_hostingEnvironment.ContentRootPath, "Data", "items.json");
        var json = System.IO.File.ReadAllText(filePath);
        var items = JsonConvert.DeserializeObject<List<Item>>(json, settings);
        return Ok(items);
    }
}
