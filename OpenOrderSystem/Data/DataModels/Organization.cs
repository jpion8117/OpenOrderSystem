using Microsoft.AspNetCore.Identity;
using OpenOrderSystem.Data;

namespace OpenOrderSystem.Data.DataModels
{
    public class Organization
    {
        public struct OrganizationOptions
        {
            public OrganizationOptions()
            {
                Name = string.Empty;
                Description = null;
                UseInstorePickup = true;
                UseIndividualBarcode = false;
            }

            public string Name { get; set; }
            public string? Description { get; set; }
            public bool UseInstorePickup { get; set; }
            public bool UseIndividualBarcode { get; set; }
        }
        public static async Task<Organization> CreateAsync(OrganizationOptions options, IdentityUser organizationAdmin, UserManager<IdentityUser> userManager)
        {
            var organization = new Organization
            {
                Id = Guid.NewGuid().ToString(),
                Name = options.Name,
                Description = options.Description,
                UseIndividualBarcode = options.UseIndividualBarcode,
                UseInstorePickup = options.UseInstorePickup,
                OrganizationUsers = new List<IdentityUser> { organizationAdmin }
            };

            await userManager.AddToRoleAsync(organizationAdmin, ApplicationDbContext.RoleNames[ApplicationDbContext.DefaultRoles.Org_Admin]);
            await userManager.AddToRoleAsync(organizationAdmin, ApplicationDbContext.RoleNames[ApplicationDbContext.DefaultRoles.Org_Manager]);
            await userManager.AddToRoleAsync(organizationAdmin, ApplicationDbContext.RoleNames[ApplicationDbContext.DefaultRoles.Org_User]);

            return organization;
        }
        public string Id { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public bool UseInstorePickup { get; set; } = true;

        public bool UseIndividualBarcode { get; set; } = false;

        public List<IdentityUser>? OrganizationUsers { get; set; }

        public List<Customer>? Customers { get; set; }
        public List<Ingredient>? Ingredients { get; set; }
        public List<IngredientCategory>? IngredientCategories { get; set; }
        public List<MenuItem>? Menu { get; set; }
        public List<Order>? Orders { get; set; }
        public List<ProductCategory>? ProductCategories { get; set; }

        public List<MenuItemVarient> GetItemVarients(int id)
        {
            return Menu?
                .AsQueryable()
                .FirstOrDefault(m => m.Id == id)?
                .MenuItemVarients ?? new List<MenuItemVarient>();
        }

        public List<OrderLine> GetOrderLines(int id)
        {
            return Orders?
                .AsQueryable()
                .FirstOrDefault(o => o.Id == id)?
                .LineItems ?? new List<OrderLine>();
        }
    }
}
