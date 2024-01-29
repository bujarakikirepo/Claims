using Application.Interfaces;
using Application.Models;
using Microsoft.AspNetCore.Mvc;

namespace Claims.Controllers;

[ApiController]
[Route("[controller]")]
public class CoversController : ControllerBase
{
    private readonly ICoverService _coverService;

    public CoversController(ICoverService coverService)
    {
        _coverService = coverService;
    }

    [HttpPost("premium")]
    public async Task<ActionResult> ComputePremiumAsync(DateOnly startDate, DateOnly endDate, Domain.Enums.CoverType coverType)
    {
        var result = _coverService.ComputePremium(startDate, endDate, coverType);
        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<GetCoverModel>>> GetAsync()
    {
        var results = await _coverService.GetItemsAsync();
        return Ok(results);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GetCoverModel>> GetAsync(string id)
    {
        var response = await _coverService.GetItemAsync(id);
        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult> CreateAsync(CreateCoverModel cover)
    {
        var entity = await _coverService.CreateAsync(cover);
        return Ok(entity);
    }

    [HttpDelete("{id}")]
    public async Task DeleteAsync(string id)
    {
        await _coverService.DeleteItemAsync(id);
    }
}