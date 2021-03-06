using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Soli.Models;
using Soli.Repositories;

namespace Soli.Controllers
{
    [ApiController]
    [Authorize(Policy="TerritoryAdmin")]
    [Route("[controller]")]
    public class TerritoriesController : ControllerBase
    {
        public readonly TerritoriesRepository _terrRepo;

        public TerritoriesController(TerritoriesRepository terrRepo)
        {
            _terrRepo = terrRepo;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllTerritories()
        {
            var territories = await _terrRepo.GetTerritories();
            if(territories == null || !territories.Any())
            {
                return NoContent();
            }

            return Ok(territories);
        }

        [HttpGet("{id}")]
        [Authorize(Policy="TerritoryAdmin")]
        public async Task<IActionResult> GetTerritory(Guid id)
        {
            var t = await _terrRepo.GetTerritory(id);
            if(t != null)
                return Ok(t);
            return NoContent();
        }

        [HttpPost]
        [Authorize(Policy="TerritoryAdmin")]
        public async Task<IActionResult> Save(TerritoryDetail detail)
        {
            var res = await _terrRepo.SaveTerritory(detail);
            if(res != null)
                return Ok(res);
            
            return BadRequest();
        }

        [HttpGet("assigned/{state}")]
        public async Task<IActionResult> GetAssigned(string state)
        {
            var au = await _terrRepo.GetAssignedInState(state);
            if((au.Zips == null || !au.Zips.Any()) && (au.Churches == null || !au.Churches.Any())) {
                return NoContent();
            }
            return Ok(au);
        }
    }
}
