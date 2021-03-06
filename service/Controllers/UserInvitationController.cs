using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Soli.Models;
using Soli.Repositories;

namespace Soli.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserInvitationController : ControllerBase
    {
        private readonly InvitationRepository _invitationRepo;
        private readonly UserClaimRepository _claimRepo;

        public UserInvitationController(InvitationRepository invitationRepo,
                            UserClaimRepository claimRepo)
        {
            _invitationRepo = invitationRepo;
            _claimRepo = claimRepo;
        }

        [AllowAnonymous]
        [HttpGet("valid/{invitationId}")]
        public async Task<IActionResult> IsValidInvitation(Guid invitationId)
        {
            return Ok(await _invitationRepo.InvitationIsValid(invitationId));
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ExecuteInvitation([FromBody] Invitation invitation)
        {
            // we have an authenticated user at this point.
            var invite = await _invitationRepo.Get(invitation.Id);
            if(await _claimRepo.AddUser(User.FindFirstValue("name") ?? invitation.Name, Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)), User.FindFirstValue(ClaimTypes.Email), invite.Roles))
            {
                await _invitationRepo.Delete(invitation.Id);
            }
            return Ok();
        }
    }
}
