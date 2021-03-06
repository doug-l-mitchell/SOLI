using System;
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
    public class UserControllerTests
    {
        public class GetClaims
        {
            [Theory, AutoMoqData]
            public async Task ShouldReturnClaimsWhenExists([Frozen]Mock<UserClaimRepository> repo, UserController sut)
            {
                var claim = "territoryadmin";
                repo.Setup(c => c.GetUserClaims(It.IsAny<Guid>())).ReturnsAsync(new []{claim});
                var result = await sut.GetClaims(new ClaimRequest{ objectId = Guid.NewGuid() });

                (result as IStatusCodeActionResult).StatusCode.Should().Be(200);
                var data = (result as OkObjectResult).Value as IDictionary<string,bool>;
                data.Should().NotBeNull();
                data[claim].Should().BeTrue();
            }
            
            [Theory, AutoMoqData]
            public async Task ShouldReturn409WhenNoClaims([Frozen]Mock<UserClaimRepository> repo, UserController sut)
            {
                repo.Setup(c => c.GetUserClaims(It.IsAny<Guid>())).ReturnsAsync(Enumerable.Empty<string>());
                var result = await sut.GetClaims(new ClaimRequest{ objectId = Guid.NewGuid() });

                (result as IStatusCodeActionResult).StatusCode.Should().Be(409);
            }
        }

        public class GetUsers
        {
            [Theory, AutoMoqData]
            public async Task ShouldReturn204WhenNoUsers([Frozen]Mock<UserClaimRepository> repo, UserController sut)
            {
                repo.Setup(c => c.GetUsers()).ReturnsAsync(Enumerable.Empty<User>());

                var result = await sut.GetUsers();
                (result as IStatusCodeActionResult).StatusCode.Should().Be(204);
            }

            [Theory, AutoMoqData]
            public async Task ShouldReturn200WhenUsersExist([Frozen]Mock<UserClaimRepository> repo, UserController sut)
            {
                repo.Setup(c => c.GetUsers()).ReturnsAsync(new []{ new User{Id = Guid.Empty, DisplayName="Test"}});

                var result = await sut.GetUsers();
                (result as IStatusCodeActionResult).StatusCode.Should().Be(200);
            }

            [Theory, AutoMoqData]
            public async Task ShouldReturnAppropriateData([Frozen]Mock<UserClaimRepository> repo, UserController sut)
            {
                repo.Setup(c => c.GetUsers()).ReturnsAsync(new []{new User{Id = Guid.Empty, DisplayName="Test1", Roles = new List<int>{1,2}}});
                var result = await sut.GetUsers();
                var data = (result as OkObjectResult).Value as IEnumerable<User>;

                data.Should().HaveCount(1);
                data.First().Roles.Should().HaveCount(2);
            }
        }
    }
}