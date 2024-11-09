using System.ComponentModel.DataAnnotations.Schema;

namespace OpenOrderSystem.Data.DataModels
{
    public class ProductCategory
    {
        /// <summary>
        /// Table Id column
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Name of the product category (ex: Pizza or Cheese Bread)
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Id of the organization this menu belongs to.
        /// </summary>
        public string OrganizationId { get; set; } = string.Empty;

        /// <summary>
        /// Nav property to organization
        /// </summary>
        public Organization? Organization { get; set; }

        /// <summary>
        /// Normailized version of the name safe for use in HTML as id's or classes
        /// </summary>
        [NotMapped]
        public string NormailizedName
        {
            get => Name.ToLower().Replace(" ", "_");
        }

        /// <summary>
        /// Product category description
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Ingredients allowed in the product category
        /// </summary>
        public List<Ingredient>? Ingredients { get; set; }

        /// <summary>
        /// Menuitems in the product category
        /// </summary>
        public List<MenuItem>? MenuItems { get; set; }

        /// <summary>
        /// Items will be sorted by priority first then by category.
        /// </summary>
        public int Priority { get; set; } = 0;
    }
}
