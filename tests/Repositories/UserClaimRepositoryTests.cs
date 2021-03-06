using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using FluentAssertions;
using Soli.Models;
using Soli.Repositories.Impl;
using Xunit;

namespace SoliTests.Repositories
{
    public class UserClaimRepositoryTests
    {
        public class GetUserClaims 
        {
            [Theory, AutoMoqDbData]
            public async Task ShouldReturnEmptyListWhenNoUserClaims(UserClaimRepositoryImpl repo)
            {
                System.Console.WriteLine("In test method...");
                var data = await repo.GetUserClaims(Guid.Empty);

                data.Should().NotBeNull();
                data.Should().BeEmpty();
            }

            [Theory, AutoMoqDbData]
            public async Task ShouldReturnValidClaims(IDbConnection dbConn, UserClaimRepositoryImpl repo)
            {
                var uid = Guid.NewGuid();
                await dbConn.ExecuteAsync("insert into users (id, displayName) values (@Id, @Name)", new { Id = uid, Name="test"});
                await dbConn.ExecuteAsync("insert into userclaims(user_id, role_id) values (@Id, @Role)", new { Id =uid, Role=RoleType.TerritoryAdmin});

                var data = await repo.GetUserClaims(uid);

                data.Should().NotBeNull();
                data.Should().HaveCount(1);
            }
        }

        public class AddUserClaims
        {
            [Theory, AutoMoqDbData]
            public async Task ShouldAddClaims(IDbConnection dbConn, UserClaimRepositoryImpl repo)
            {
                var initialCount = await dbConn.QuerySingleAsync<int>("select count(*) from userclaims");
                var uid = Guid.NewGuid();
                await dbConn.ExecuteAsync("insert into users (id, displayName) values (@Id, @Name)",
                    new { Id = uid, Name="Test"});

                await repo.AddUserClaims(new RoleType[]{RoleType.TerritoryAdmin, RoleType.InviteUser}, uid);

                (await dbConn.QuerySingleAsync<int>("select count(*) from userclaims")).Should().Be(initialCount + 2);

            }
        }

        public class AddUser
        {
            [Theory, AutoMoqDbData]
            public async Task ShouldReturnFalseWhenInsertFails(UserClaimRepositoryImpl repo)
            {
                var result = await repo.AddUser(null, Guid.Empty, null, "");

                result.Should().BeFalse();
            }

            [Theory, AutoMoqDbData]
            public async Task ShouldAddUserWhenNoClaims(IDbConnection dbConn, UserClaimRepositoryImpl repo)
            {
                var uid = Guid.NewGuid();
                var result = await repo.AddUser("Test", uid, null, "");

                result.Should().BeTrue();

                (await dbConn.QuerySingleAsync<int>("select count(*) from userclaims where user_id = @Id", new { Id = uid})).Should().Be(0);
                (await dbConn.QuerySingleAsync<int>("select count(*) from users where id = @Id", new { Id = uid})).Should().Be(1);
            }

            [Theory, AutoMoqDbData]
            public async Task ShouldAddUserWithClaims(IDbConnection dbConn, UserClaimRepositoryImpl repo)
            {
                var uid = Guid.NewGuid();
                var result = await repo.AddUser("Test", uid, null, "1");

                result.Should().BeTrue();

                (await dbConn.QuerySingleAsync<int>("select count(*) from userclaims where user_id = @Id", new { Id = uid})).Should().Be(1);
                (await dbConn.QuerySingleAsync<int>("select count(*) from users where id = @Id", new { Id = uid})).Should().Be(1);
            }
        }

        // public class GetUsers
        // {
        //     [Theory, AutoMoqDbData]
        //     public async Task ShouldReturnEmptyListWhenNoUsers(IDbConnection dbConn, UserClaimRepositoryImpl sut)
        //     {
        //         // make sure there are no users
        //         await dbConn.ExecuteAsync("delete from users");

        //         var result = await sut.GetUsers();
        //         result.Should().NotBeNull();
        //         result.Should().BeEmpty();
        //     } 

        //     [Theory, AutoMoqDbData]
        //     public async Task ShouldReturnDataWhenUsersExist(IDbConnection dbConn, UserClaimRepositoryImpl sut)
        //     {
        //         await dbConn.ExecuteAsync(@"insert into users (id, displayName, email)
        //             values (@Id, @Name,@Email)", new { Id = Guid.NewGuid(), Name="Test", Email="test@t.com"});

        //         var result = await sut.GetUsers();
        //         result.Should().NotBeNull();
        //         result.Should().HaveCount(1);
        //     }

        //     [Theory, AutoMoqDbData]
        //     public async Task ShouldContainUserRoles(IDbConnection dbConn, UserClaimRepositoryImpl sut)
        //     {
        //         var uid = Guid.NewGuid();
        //         await dbConn.ExecuteAsync(@"insert into users (id, displayName, email)
        //             values (@Id, @Name,@Email)", new { Id = uid, Name="Test", Email="test@t.com"});
        //         await dbConn.ExecuteAsync(@"insert into userclaims (user_id, role_id)
        //                 values (@Id, @Role)", new []{1,2}.Select(r => new { Id = uid, Role = r}));

        //         var result = await sut.GetUsers();
        //         result.Should().NotBeNull();
        //         var user = result.First();
        //         user.Id.Should().Be(uid);
        //         user.Roles.Should().HaveCount(2);
        //     }
        // }
    }
}