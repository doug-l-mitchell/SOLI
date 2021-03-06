using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using Soli.Models;

namespace Soli.Repositories.Impl
{
    public class UserClaimRepositoryImpl : UserClaimRepository
    {
        private readonly IDbConnection _dbConn;
        private readonly ILogger _logger;

        public UserClaimRepositoryImpl(IDbConnection dbConn, ILogger<UserClaimRepositoryImpl> logger)
        {
            _dbConn = dbConn;
            if(_dbConn.State != ConnectionState.Open) {
                _dbConn.Open();
            }
            _logger = logger;
        }

        public async Task<bool> AddUser(string displayName, Guid objectId, string email, string roles)
        {
            using (var tx = _dbConn.BeginTransaction())
            {
                try
                {
                    await _dbConn.ExecuteAsync("insert into users (id, displayName, email) values (@uid, @name, @email)",
                      new { uid = objectId, name = displayName, email = email }, transaction: tx);

                    await _dbConn.ExecuteAsync("insert into userclaims (user_id, role_id) values (@uid, @role)",
                      roles.Split(",").Where(r => !string.IsNullOrWhiteSpace(r))
                          .Select(r => new { uid = objectId, role = int.Parse(r) }), transaction: tx);
                    tx.Commit();
                }
                catch (DbException ex)
                {
                    tx.Rollback();
                    _logger.LogError(ex, "failed to add user");
                    return false;
                }
                return true;
            }
        }

        public Task AddUserClaims(IEnumerable<RoleType> roles, Guid objectId)
        {
            return _dbConn.ExecuteAsync("insert into userclaims (user_id, role_id) values (@uid, @role)",
                roles.Select(r => new { uid = objectId, role = (int)r }));
        }

        public Task<IEnumerable<string>> GetUserClaims(Guid objectId)
        {
            return _dbConn.QueryAsync<string>("select b.name from UserClaims a join Roles b on a.role_id = b.id where a.user_id = @Id", new { Id = objectId });
        }

        public async Task<IEnumerable<User>> GetUsers()
        {
            using(var multi = await _dbConn.QueryMultipleAsync(@"select id as Id,
                            displayname, email from users;
                        select u.Id, c.role_id from users as u
                            join userclaims as c on u.id = c.user_id;"))
            {
                var users = await multi.ReadAsync<User>();
                IEnumerable<dynamic> roleData = await multi.ReadAsync<dynamic>();
                var roles = roleData.ToLookup(r => r.id);
                
                foreach(var u in users)
                {
                    if(roles.Contains(u.Id))
                        u.Roles = roles[u.Id].Select(r => (int)r.role_id).ToList();
                }

                return users;
            }
        }
    }
}