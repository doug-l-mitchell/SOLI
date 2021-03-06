using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Soli.Models;

namespace Soli.Repositories
{
    public interface UserClaimRepository
    {
        Task<IEnumerable<string>> GetUserClaims(Guid objectId);
        Task<bool> AddUser(string displayName, Guid objectId, string email, string roles);  
        Task AddUserClaims(IEnumerable<RoleType> roles, Guid objectId);
        Task<IEnumerable<User>> GetUsers();
    }
}