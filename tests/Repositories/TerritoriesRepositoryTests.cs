using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using FluentAssertions;
using Soli.Models;
using Soli.Repositories;
using Soli.Repositories.Impl;
using Xunit;

namespace SoliTests.Repositories
{
    public class TerritoriesRepositoryTests
    {
        public class GetTerritories
        {
            [Theory, AutoMoqDbData]
            public async Task ShouldReturnEmptyListWhenNoTerritories(TerritoriesRepositoryImpl repo)
            {
                var data = await repo.GetTerritories();
                data.Should().NotBeNull();
                data.Should().BeEmpty();
            }

            [Theory, AutoMoqDbData]
            public async Task ShouldReturnExistingTerritories(TerritoriesRepositoryImpl repo, IDbConnection dbConn)
            {
                await dbConn.ExecuteAsync("insert into Territories (name, color, state) values (@Name, @Color, @State)",
                        new { Name = "Test", Color="0xFF0000", State ="TX"});

                var data = await repo.GetTerritories();
                data.Should().NotBeNull();
                data.Should().HaveCount(1);
                
                var t = data.First();
                t.Name.Should().Be("Test");
                t.State.Should().Be("TX");
            }
        }

        public class GetTerritory
        {
            [Theory, AutoMoqDbData]
            public async Task ShouldReturnNullWhenTerritoryDoesNotExist(TerritoriesRepositoryImpl repo)
            {
                var t = await repo.GetTerritory(Guid.Empty);
                t.Should().BeNull();
            }

            [Theory, AutoMoqDbData]
            public async Task ShouldReturnExistingTerritory(TerritoriesRepositoryImpl repo, IDbConnection dbConn)
            {
                var id = Guid.NewGuid();
                await dbConn.ExecuteAsync(@"insert into Territories (id, name, color, state) values (@Id, @Name, @Color, @State);",
                        new { Id = id, Name = "Test", Color="0xFF0000", State ="TX"});
                await dbConn.ExecuteAsync("insert into TerritoryZips (territory_id, zip) values (@Id, @Zip)",
                            new object[]{new {Id =id, Zip="76058"},new {Id =id, Zip="76028"}});
                await dbConn.ExecuteAsync("insert into TerritoryChurches (territory_id, church) values (@Id, @Ch)",
                        new []{ new { Id=id, Ch="Joshua"}, new { Id=id,Ch="Burleson"}});

                var t = await repo.GetTerritory(id);
                t.Should().NotBeNull();
                t.Name.Should().Be("Test");
                t.Zips.Should().HaveCount(2);
                t.Churches.Should().HaveCount(2);
            }
        }

        public class SaveTerritory
        {
            [Theory, AutoMoqDbData]
            public async Task ShouldRollbackWhenInsertFails(TerritoriesRepositoryImpl repo, IDbConnection dbConn)
            {
                (await dbConn.QuerySingleAsync<int>("select count(*) from territories")).Should().Be(0);

                var t = new TerritoryDetail();
                var res = await repo.SaveTerritory(t);

                res.Should().BeNull();
                (await dbConn.QuerySingleAsync<int>("select count(*) from territories")).Should().Be(0);
            }

            [Theory, AutoMoqDbData]
            public async Task ShouldInsertTerritory(TerritoriesRepositoryImpl repo, IDbConnection dbConn)
            {
                (await dbConn.QuerySingleAsync<int>("select count(*) from territories")).Should().Be(0);

                var t = new TerritoryDetail{ Name="Test", Color="0x00ff00", State="TX", Churches= new []{"A", "B"}, Zips=new []{ "a", "b"}};
                var res = await repo.SaveTerritory(t);

                res.Should().NotBeNull();
                (await dbConn.QuerySingleAsync<int>("select count(*) from territories")).Should().Be(1);
            }

            [Theory, AutoMoqDbData]
            public async Task ShouldReturnNullWhenExceptionOccurs(TerritoriesRepositoryImpl repo)
            {
                // color and state are required in the db
                var t = new TerritoryDetail{ Name="Test", Churches= new []{"A", "B"}, Zips=new []{ "a", "b"}};
                var res = await repo.SaveTerritory(t);

                res.Should().BeNull();
            }
        }

        public class GetAssignedInState
        {
            [Theory, AutoMoqDbData]
            public async Task ShouldReturnNullWhenNoData(TerritoriesRepositoryImpl repo)
            {
                var res = await repo.GetAssignedInState("AR");
                res.Should().NotBeNull();
                res.Zips.Should().BeEmpty();
                res.Churches.Should().BeEmpty();
            }

            [Theory, AutoMoqDbData]
            public async Task ShouldReturnObjectWhenDataExists(TerritoriesRepositoryImpl repo)
            {
                var t = new TerritoryDetail{ Name="Test", State="AR", Color="#ffffff", Churches = new []{"A","B"}, Zips = new []{"a","b"}};
                await repo.SaveTerritory(t);

                var au = await repo.GetAssignedInState("AR");
                au.Should().NotBeNull();
                au.Zips.Should().HaveCount(2);
                au.Churches.Should().HaveCount(2);
            }
        }
    }
}