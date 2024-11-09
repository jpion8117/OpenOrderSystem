using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OpenOrderSystem.Data.DataModels;

namespace OpenOrderSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public enum DefaultRoles
        {
            Global_Admin,
            Org_Admin,
            Org_Manager,
            Org_User
        };

        public static readonly Dictionary<DefaultRoles, string> RoleNames = new Dictionary<DefaultRoles, string>()
        {
            { DefaultRoles.Global_Admin,    "global_admin" },
            { DefaultRoles.Org_Admin,       "organization_admin" },
            { DefaultRoles.Org_Manager,     "organization_manager" },
            { DefaultRoles.Org_User,        "organization_user" }
        };
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder bob)
        {
            base.OnModelCreating(bob);

            var hasher = new PasswordHasher<IdentityUser>();
            var admin = new IdentityUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = "admin",
                NormalizedUserName = "ADMIN",
                Email = string.Empty,
                NormalizedEmail = string.Empty,
                EmailConfirmed = true,
                PhoneNumber = string.Empty,
                PhoneNumberConfirmed = true
            };
            admin.PasswordHash = hasher.HashPassword(admin, "password");
            bob.Entity<IdentityUser>().HasData(admin);

            const string ADMIN = "global_admin";
            const string ORG_ADMIN = "organization_admin";
            const string ORG_MANAGER = "organization_manager";
            const string ORG_USER = "organization_user";

            var roleIds = new Dictionary<string, string>()
            {
                { ADMIN, Guid.NewGuid().ToString() },
                { ORG_ADMIN, Guid.NewGuid().ToString() },
                { ORG_MANAGER, Guid.NewGuid().ToString() },
                { ORG_USER, Guid.NewGuid().ToString() }
            };

            bob.Entity<IdentityRole>().HasData(
                new IdentityRole
                {
                    Id = roleIds[ADMIN],
                    Name = ADMIN,
                    NormalizedName = ADMIN.ToUpper()
                },
                new IdentityRole
                {
                    Id = roleIds[ORG_ADMIN],
                    Name = ORG_ADMIN,
                    NormalizedName = ORG_ADMIN.ToUpper()
                },
                new IdentityRole
                {
                    Id = roleIds[ORG_MANAGER],
                    Name = ORG_MANAGER,
                    NormalizedName = ORG_MANAGER.ToUpper()
                },
                new IdentityRole
                {
                    Id = roleIds[ORG_USER],
                    Name = ORG_USER,
                    NormalizedName = ORG_USER.ToUpper()
                });

            bob.Entity<IdentityUserRole<string>>().HasData(
                new IdentityUserRole<string>
                {
                    RoleId = roleIds[ADMIN],
                    UserId = admin.Id
                });
        }

        /// <summary>
        /// Product categories used to group products by type
        /// </summary>
        public DbSet<ProductCategory> ProductCategories { get; set; }

        /// <summary>
        /// Ingredient categories used to group ingredients.
        /// </summary>
        public DbSet<IngredientCategory> IngredientCategories { get; set; }

        /// <summary>
        /// Customer information
        /// </summary>
        public DbSet<Customer> Customers { get; set; }

        /// <summary>
        /// Available Ingredients
        /// </summary>
        public DbSet<Ingredient> Ingredients { get; set; }

        /// <summary>
        /// Base menu items
        /// </summary>
        public DbSet<MenuItem> MenuItems { get; set; }

        /// <summary>
        /// Customer orders
        /// </summary>
        public DbSet<Order> Orders { get; set; }

        /// <summary>
        /// Order line items
        /// </summary>
        public DbSet<OrderLine> OrderLines { get; set; }

        /// <summary>
        /// Menu item varients
        /// </summary>
        public DbSet<MenuItemVarient> MenuItemVarients { get; set; }

        public DbSet<Organization> Organizations { get; set; }

        public DbSet<OrganizationUser> OrganizationsUser { get; set; }
    }
}
