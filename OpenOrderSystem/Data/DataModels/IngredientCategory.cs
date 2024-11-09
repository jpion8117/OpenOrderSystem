using System.ComponentModel.DataAnnotations.Schema;

namespace OpenOrderSystem.Data.DataModels
{
    public class IngredientCategory
    {
        /// <summary>
        /// Primary key
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Name associated with ingredient category
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
        /// Number of non-standard (extra) ingredients allowed in category.
        /// </summary>
        public int ExtrasAllowed { get; set; }

        /// <summary>
        /// Ingredients that belong to this category.
        /// </summary>
        public List<Ingredient>? MemberIngredients { get; set; }

        /// <summary>
        /// Items will be sorted by priority first then by category.
        /// </summary>
        public int Priority { get; set; } = 0;
    }
}
