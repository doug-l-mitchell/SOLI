using System;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using FluentAssertions;
using Soli.Models;
using Soli.Repositories.Impl;
using Xunit;

namespace SoliTests.Repositories
{
    public class InvitationRepositoryTests
    {
        public class Save
        {
            [Theory, AutoMoqDbData]
            public async Task ShouldSaveAnInvitation(InvitationRepositoryImpl repo)
            {
                var inv = new Invitation {
                                Id = Guid.NewGuid(),
                                Name="Test",
                                Email="test@t.com",
                                Roles= "1"
                };

                var result = await repo.Save(inv);

                result.Should().NotBeNull();
                result.Id.Should().Be(inv.Id);
            }

            [Theory, AutoMoqDbData]
            public async Task ShouldNotSaveWhenMissingData(InvitationRepositoryImpl repo)
            {
                var inv = new Invitation{Id = Guid.NewGuid()};
                var result = await repo.Save(inv);

                result.Should().BeNull();
            }
        }

        public class Get
        {
            [Theory, AutoMoqDbData]
            public async Task ShouldReturnNullWhenInvitationDoesNotExist(InvitationRepositoryImpl repo)
            {
                var inv = await repo.Get(Guid.Empty);
                inv.Should().BeNull();
            }

            [Theory, AutoMoqDbData]
            public async Task ShouldReturnExistingInvitation(InvitationRepositoryImpl repo, IDbConnection dbConn)
            {
                var uid = Guid.NewGuid();
                await dbConn.ExecuteAsync(@"insert into invitations (id, invitee, email, roles)
                                values (@Id, @Name, @Email, @Roles)", 
                                new { Id = uid, Name="Test", Email="test@t.com", Roles="1,2"});

                var result = await repo.Get(uid);

                result.Should().NotBeNull();
                result.Id.Should().Be(uid);
            }
        }
    }
}