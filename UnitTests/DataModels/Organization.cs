using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using Moq;
using OpenOrderSystem.Data;
using OpenOrderSystem.Data.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable disable

namespace UnitTests.DataModels
{
    internal partial class Organization
    {
        private OpenOrderSystem.Data.DataModels.Organization _organization;
        private UserManager<IdentityUser> _userManager;
        private OrganizationOptions _organizationOptions;
        private IdentityUser _user;
        private List<string> _roles = new List<string>();

        [SetUp]
        public void SetUp()
        {
            _user = new IdentityUser
            {
                Id = "JustATest",
                UserName = "User",
                NormalizedUserName = "USER"
            };

            var userStore = new Mock<IUserStore<IdentityUser>>();
            var manager = new Mock<UserManager<IdentityUser>>(userStore.Object, null, null, null, null, null, null, null, null);

            manager.Setup(x => x.AddToRoleAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success)
                .Callback<IdentityUser, string>((u, role) =>
                {
                    _roles.Add(role);
                });

            _userManager = manager.Object;
        }

        [Test(Author = "Josh Pion", Description = "Tests organization creation", TestOf = typeof(Organization))]
        public async Task CreateOrganization()
        {
            _organizationOptions = new OrganizationOptions
            {
                Name = "ABCorp #111",
                Description = "AB Corporation Location #111",
                OrganizationTimeZone = TimeZoneInfo.Local,
                Hours = new Dictionary<DayOfWeek, OrganizationOpenCloseTimes>
                {
                    { DayOfWeek.Sunday,     new OrganizationOpenCloseTimes(8,00, 20,00) },
                    { DayOfWeek.Monday,     new OrganizationOpenCloseTimes(8,00, 20,00) },
                    { DayOfWeek.Tuesday,    new OrganizationOpenCloseTimes(8,00, 20,00) },
                    { DayOfWeek.Wednesday,  new OrganizationOpenCloseTimes(8,00, 20,00) },
                    { DayOfWeek.Thursday,   new OrganizationOpenCloseTimes(8,00, 20,00) },
                    { DayOfWeek.Friday,     new OrganizationOpenCloseTimes(8,00, 21,00) },
                    { DayOfWeek.Saturday,   new OrganizationOpenCloseTimes(8,00, 21,00) }
                },
                UseIndividualBarcode = true,
                UseInstorePickup = true
            };

            _organization = await OpenOrderSystem.Data.DataModels.Organization
                .CreateAsync(_organizationOptions, _user, _userManager);

            Assert.IsNotNull(_organization.Id);
            Assert.That(_organization.Name, Is.EqualTo("ABCorp #111"));
            Assert.That(_organization.Description, Is.EqualTo("AB Corporation Location #111"));

            //check that user was added to roles successfully.
            Assert.IsTrue(_roles.Contains("organization_admin"));
            Assert.IsTrue(_roles.Contains("organization_manager"));
            Assert.IsTrue(_roles.Contains("organization_user"));
        }
    }
}
