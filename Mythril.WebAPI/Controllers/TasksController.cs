using Microsoft.AspNetCore.Mvc;
using Mythril.Data;
using Newtonsoft.Json;

namespace Mythril.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController(IWebHostEnvironment hostingEnvironment) : ControllerBase
{
    private readonly IWebHostEnvironment _hostingEnvironment = hostingEnvironment;

    [HttpGet]
    public ActionResult<List<TaskData>> Get()
    {
        var filePath = Path.Combine(_hostingEnvironment.ContentRootPath, "Data", "tasks.json");
        var json = System.IO.File.ReadAllText(filePath);
        var tasks = JsonConvert.DeserializeObject<List<TaskData>>(json);
        return Ok(tasks);
    }
}
