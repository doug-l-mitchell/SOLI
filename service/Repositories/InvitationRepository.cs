using System;
using System.Threading.Tasks;
using Soli.Models;

namespace Soli.Repositories
{
    public interface InvitationRepository
    {
        Task<bool> InvitationIsValid(Guid invitationId);
        Task<Invitation> Get(Guid invitationId);
        Task<Invitation> Save(Invitation invitation);
        Task Delete(Guid invitationId);
    }
}