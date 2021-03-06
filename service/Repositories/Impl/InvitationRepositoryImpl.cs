using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Dapper;
using Soli.Models;
using Soli.Repositories;

namespace Soli.Repositories.Impl
{
    public class InvitationRepositoryImpl : InvitationRepository
    {
        public readonly IDbConnection _connection;

        public InvitationRepositoryImpl(IDbConnection connection)
        {
            _connection = connection;
            if(_connection.State != ConnectionState.Open) {
                _connection.Open();
            }
        }

        public Task Delete(Guid invitationId)
        {
            return _connection.ExecuteAsync("delete from invitations where id = @id", new { id = invitationId });
        }

        public async Task<Invitation> Get(Guid invitationId)
        {
            var inv = await _connection.QuerySingleOrDefaultAsync<Invitation>(@"select  
                invitor as InvitedBy, invitee as Name, email, roles from invitations where id = @id",
                new { id = invitationId });
            if(inv != null)
                inv.Id = invitationId;
            return inv;
        }

        public async Task<bool> InvitationIsValid(Guid invitationId)
        {
            var exists = await _connection.ExecuteScalarAsync<int>(
                "select count(id) from invitations where id = @id",
                new { id = invitationId });
            return exists != 0;
        }

        public async Task<Invitation> Save(Invitation invitation)
        {
            try
            {
                if (invitation.Id == Guid.Empty)
                {
                    invitation.Id = Guid.NewGuid();
                }
                await _connection.ExecuteAsync(@"insert into invitations (id, invitor, invitee, email, roles) 
                values (@Id,  @InvitedBy, @Name, @Email, @Roles)", invitation);
                return invitation;
            }
            catch (DbException)
            {
                // log error
                return null;
            }
        }
    }
}