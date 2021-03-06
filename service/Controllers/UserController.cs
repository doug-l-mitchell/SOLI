using System;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Soli.Models;
using Soli.Repositories;
using Microsoft.AspNetCore.Authorization;

namespace Soli.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserClaimRepository _userClaimRepo;

        public UserController(UserClaimRepository userClaimRepo)
        {
            _userClaimRepo = userClaimRepo;
        }

        [HttpPost]
        [Route("getUserClaims")]
        public async Task<IActionResult> GetClaims([FromBody] ClaimRequest request)
        {
            var claims = await _userClaimRepo.GetUserClaims(request.objectId);
            if(claims == null || !claims.Any())
            {
                return Conflict(); // no registered user
            }

            // convert the array to an object where property names are the roles/claims
            return Ok(claims.ToDictionary(c => c, c => true));
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _userClaimRepo.GetUsers();
            if(users == null || !users.Any())
            {
                return NoContent();
            }
            return Ok(users);
        }
    }
}
