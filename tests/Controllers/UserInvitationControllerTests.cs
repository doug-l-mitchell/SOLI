using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text.Json;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Moq;
using Soli.Controllers;
using Soli.Repositories;
using Soli.Repositories.Impl;
using Xunit;

namespace SoliTests.Controllers
{
    public class UserInvitationControllerTests
    {
        public class IsValidInvitation
        {
            [Theory]
            [InlineAutoMoqData(true)]
            [InlineAutoMoqData(false)]
            public async Task ShouldReturn200WhenValid(bool valid, [Frozen]Mock<InvitationRepository> repo, UserInvitationController sut)
            {
                repo.Setup(c => c.InvitationIsValid(It.IsAny<Guid>())).ReturnsAsync(valid);
                var result = await sut.IsValidInvitation(Guid.Empty);

                (result as IStatusCodeActionResult).StatusCode.Should().Be(200);
                (result as OkObjectResult).Value.Should().Be(valid);
            }
        }
    }
}