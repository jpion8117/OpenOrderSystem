using Microsoft.AspNetCore.Identity;
using OpenOrderSystem.Data;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace OpenOrderSystem.Data.DataModels
{
    public partial class Organization
    {
        public static async Task<Organization> CreateAsync(OrganizationOptions options, IdentityUser organizationAdmin, UserManager<IdentityUser> userManager)
        {
            var organization = new Organization
            {
                Id = Guid.NewGuid().ToString(),
                Name = options.Name,
                Description = options.Description,
                UseIndividualBarcode = options.UseIndividualBarcode,
                UseInstorePickup = options.UseInstorePickup,
                OrganizationUsers = new List<IdentityUser> { organizationAdmin },
                OrginzationTimeZoneId = options.OrganizationTimeZone.Id,
                Hours = JsonSerializer.Serialize(options.Hours)
            };

            await userManager.AddToRoleAsync(organizationAdmin, ApplicationDbContext.RoleNames[ApplicationDbContext.DefaultRoles.Org_Admin]);
            await userManager.AddToRoleAsync(organizationAdmin, ApplicationDbContext.RoleNames[ApplicationDbContext.DefaultRoles.Org_Manager]);
            await userManager.AddToRoleAsync(organizationAdmin, ApplicationDbContext.RoleNames[ApplicationDbContext.DefaultRoles.Org_User]);

            return organization;
        }
        public string Id { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string Hours { get; set; } = string.Empty;

        public string OrginzationTimeZoneId { get; set; } = TimeZoneInfo.Local.Id;

        [NotMapped]
        public TimeZoneInfo OrganizationTimeZone 
        {
            get 
            {
                try
                {
                    //get the timezone based on the Id saved to the server
                    return TimeZoneInfo.FindSystemTimeZoneById(OrginzationTimeZoneId); 
                }
                catch(Exception ex) 
                {
                    //if the timezone was not found use the server local time as a fallback
                    if (ex.GetType() == typeof(TimeZoneNotFoundException))
                        return TimeZoneInfo.Local;

                    //continue throwing any other exception.
                    else
                        throw;
                }
            }
        }

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

        public DateTime ConvertTimeToLocal(DateTime dateTime) =>
            TimeZoneInfo.ConvertTimeFromUtc(dateTime, OrganizationTimeZone);

        public DateTime ConvertTimeToUtc(DateTime dateTime) =>
            TimeZoneInfo.ConvertTimeToUtc(dateTime, OrganizationTimeZone);
    }
}
