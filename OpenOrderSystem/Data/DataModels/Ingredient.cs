using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenOrderSystem.Data.DataModels
{
    public class Ingredient
    {
        /// <summary>
        /// Primary key
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Name of the ingredient
        /// </summary>
        [Required]
        [MaxLength(40, ErrorMessage = "Please keep ingredient names shorter than 40 characters.")]
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
        /// Price of the ingredient when added to another pizza a la carte
        /// </summary>
        [Required]
        public float Price { get; set; }

        /// <summary>
        /// Category of ingredients this ingredient belongs to
        /// </summary>
        public List<IngredientCategory>? Categories { get; set; }

        /// <summary>
        /// Menu items this ingredient is included in.
        /// </summary>
        public List<MenuItem>? MenuItems { get; set; }

        /// <summary>
        /// Product categories this ingredient may be used in.
        /// </summary>
        public List<ProductCategory>? ProductCategories { get; set; }

        public List<OrderLine>? OrderLines { get; set; }

        /// <summary>
        /// Items will be sorted by priority first then by category.
        /// </summary>
        public int Priority { get; set; } = 0;
    }
}
