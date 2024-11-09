using System.ComponentModel.DataAnnotations.Schema;

namespace OpenOrderSystem.Data.DataModels
{
    public class MenuItem
    {
        private int _varient = 0;
        private string? _imageUrl = null;
        private readonly string _defaultImageUrl = "/media/images/VMLogo.png";

        /// <summary>
        /// Primary key
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Name of the item
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
        /// Item description
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Url to the item image defaults to village market logo if null.
        /// </summary>
        public string? ImageUrl
        {
            get => _imageUrl ?? _defaultImageUrl;
            set => _imageUrl = value;
        }

        /// <summary>
        /// Foreign key to the item's product category
        /// </summary>
        public int ProductCategoryId { get; set; }

        /// <summary>
        /// Nav property to the item's product category
        /// </summary>
        public ProductCategory? ProductCategory { get; set; }

        /// <summary>
        /// Base item ingredients
        /// </summary>
        public List<Ingredient>? Ingredients { get; set; }

        /// <summary>
        /// Menu item varients 
        /// </summary>
        public List<MenuItemVarient>? MenuItemVarients { get; set; }

        /// <summary>
        /// OrderLines containing item.
        /// </summary>
        public List<OrderLine>? OrderLines { get; set; }

        /// <summary>
        /// Items will be sorted by priority first then by category.
        /// </summary>
        public int Priority { get; set; } = 0;

        /// <summary>
        /// Selected varient
        /// </summary>
        [NotMapped]
        public int Varient
        {
            get => _varient;
            set
            {
                if (Varient < 0)
                    _varient = 0;

                if (Varient > MenuItemVarients.Count - 1)
                    _varient = MenuItemVarients.Count - 1;

                _varient = value;
            }
        }

        /// <summary>
        /// Price of this menu item based on the selected varient.
        /// </summary>
        [NotMapped]
        public float Price
        {
            get
            {
                //retrieve the varient price
                if (MenuItemVarients.Count > 0)
                {
                    if (Varient >= 0 && Varient < MenuItemVarients.Count)
                        return MenuItemVarients[Varient].Price;
                }

                return 0;
            }
        }

        public bool ContainsIngredient(int ingredientId)
        {
            if (Ingredients != null)
            {
                var ing = Ingredients.FirstOrDefault(i => i.Id == ingredientId);
                return ing != null;
            }

            return false;
        }
    }
}
