using Microsoft.AspNetCore.Mvc;
using WaveFunctionCollapseWeb.Data;

namespace WaveFunctionCollapseWeb.Controllers;

[ApiController]
public class ApiController : Controller
{
    private readonly ILogger<ApiController> _logger;

    public ApiController(ILogger<ApiController> logger) {
        _logger = logger;
    }

    [HttpGet("TileImage")]
    public IActionResult TileImage(Guid id)
    {
        var tile = DataManager.GetTileFromDb(id);
        return File(tile.Image.Data, tile.Image.DataType);
    }
}