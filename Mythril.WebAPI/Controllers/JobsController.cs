using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Mythril.Data.Jobs;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace Mythril.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class JobsController : ControllerBase
{
    private readonly IWebHostEnvironment _hostingEnvironment;

    public JobsController(IWebHostEnvironment hostingEnvironment)
    {
        _hostingEnvironment = hostingEnvironment;
    }

    [HttpGet]
    public ActionResult<List<Job>> Get()
    {
        var settings = new JsonSerializerSettings
        {
            Converters = { new JobConverter() }
        };
        var filePath = Path.Combine(_hostingEnvironment.ContentRootPath, "Data", "jobs.json");
        var json = System.IO.File.ReadAllText(filePath);
        var jobs = JsonConvert.DeserializeObject<List<Job>>(json, settings);
        return Ok(jobs);
    }
}
