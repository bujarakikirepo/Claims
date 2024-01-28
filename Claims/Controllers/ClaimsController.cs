using Application.Interfaces;
using Application.Models;
using Microsoft.AspNetCore.Mvc;

namespace Claims.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ClaimsController : ControllerBase
    {
        private readonly IClaimService _claimService;

        public ClaimsController(IClaimService claimService)
        {
            _claimService = claimService;
        }

        [HttpGet]
        public async Task<IEnumerable<Domain.Entities.Claim>> GetAsync()
        {
            return await _claimService.GetItemsAsync();
        }

        [HttpPost]
        public async Task<ActionResult> CreateAsync(CreateClaimModel model)
        {
            var entity = await _claimService.CreateAsync(model);
            return Ok(entity);
        }

        [HttpDelete("{id}")]
        public async Task DeleteAsync(string id)
        {
            await _claimService.DeleteItemAsync(id);
        }

        [HttpGet("{id}")]
        public Task<Domain.Entities.Claim> GetAsync(string id)
        {
            return _claimService.GetItemAsync(id);
        }
    }
}