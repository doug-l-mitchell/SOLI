using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Moq;
using Soli.Controllers;
using Soli.Models;
using Soli.Repositories;
using Xunit;

namespace SoliTests.Controllers
{
    public class TerritoriesControllerTests
    {
        public class GetAllTerritories
        {
            [Theory, AutoMoqData]
            public async Task ShouldReturn204WhenNoTerritories([Frozen]Mock<TerritoriesRepository> repo, TerritoriesController sut)
            {
                repo.Setup(c => c.GetTerritories()).ReturnsAsync(Enumerable.Empty<Territory>);
                var result = await sut.GetAllTerritories();
                (result as IStatusCodeActionResult).StatusCode.Should().Be(204);
            }

            [Theory, AutoMoqData]
            public async Task ShouldReturn200WhenTerritoriesExist([Frozen]Mock<TerritoriesRepository> repo,TerritoriesController sut)
            {
                repo.Setup(c => c.GetTerritories())
                    .ReturnsAsync(new List<Territory>{ new Territory()});

                var result = await sut.GetAllTerritories();
                (result as IStatusCodeActionResult).StatusCode.Should().Be(200);
                var list = (result as OkObjectResult).Value as IEnumerable<Territory>;
                list.Should().NotBeNull();
                list.Should().HaveCount(1);
            }
        }

        public class GetAssigned
        {
            [Theory, AutoMoqData]
            public async Task ShouldReturn204WhenNoData([Frozen]Mock<TerritoriesRepository> repo, TerritoriesController sut) {
                repo.Setup(c => c.GetAssignedInState(It.IsAny<string>())).ReturnsAsync(new AssignedUnits());

                var result = await sut.GetAssigned("AR");
                (result as IStatusCodeActionResult).StatusCode.Should().Be(204);
            }

            [Theory, AutoMoqData]
            public async Task ShouldReturn200WhenDataExists([Frozen]Mock<TerritoriesRepository> repo, TerritoriesController sut)
            {
                repo.Setup(c => c.GetAssignedInState(It.IsAny<string>())).ReturnsAsync(new AssignedUnits{ Zips = new string[]{"12345"}, Churches=new string[]{"A","B"}});

                var result = await sut.GetAssigned("AR");
                (result as IStatusCodeActionResult).StatusCode.Should().Be(200);
                var obj = (result as OkObjectResult).Value as AssignedUnits;

                obj.Should().NotBeNull();
                obj.Zips.Should().HaveCount(1);
                obj.Churches.Should().HaveCount(2);
            }
        }

        public class Save
        {
            [Theory, AutoMoqData]
            public async Task ShouldReturn200WhenSaveSuccessful([Frozen]Mock<TerritoriesRepository> repo, TerritoriesController sut)
            {
                var td = new TerritoryDetail{ Id = Guid.NewGuid()};
                repo.Setup(c => c.SaveTerritory(It.IsAny<TerritoryDetail>())).ReturnsAsync(td);

                var result = await sut.Save(td);

                (result as IStatusCodeActionResult).StatusCode.Should().Be(200);
                (result as OkObjectResult).Value.Should().Be(td);
            }

            [Theory, AutoMoqData]
            public async Task ShouldReturn400WhenSaveFails([Frozen]Mock<TerritoriesRepository> repo, TerritoriesController sut)
            {
                repo.Setup(c => c.SaveTerritory(It.IsAny<TerritoryDetail>())).ReturnsAsync((TerritoryDetail)null);

                var result = await sut.Save(new TerritoryDetail());

                (result as IStatusCodeActionResult).StatusCode.Should().Be(400);
            }
            
        }
    }
}