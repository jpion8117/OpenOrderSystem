using System.ComponentModel.DataAnnotations;

namespace OpenOrderSystem.Data.DataModels
{
    public class MenuItemVarient
    {
        /// <summary>
        /// Primary Key
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Varient descriptor (ex: Large/Medium or 12pc/16pc)
        /// </summary>
        [MaxLength(12, ErrorMessage = "Please keep item varient descriptors shorter than 12 characters.")]
        [Required]
        public string Descriptor { get; set; } = string.Empty;

        /// <summary>
        /// Price of the varient
        /// </summary>
        [Required]
        public float Price { get; set; }

        /// <summary>
        /// Upc used to construct bar code
        /// </summary>
        public string? Upc { get; set; } = string.Empty;

        /// <summary>
        /// Primary key of the menu item this varient belongs to.
        /// </summary>
        [Required]
        public int MenuItemId { get; set; }

        /// <summary>
        /// Navigation property for the varient's corrisponding menu item
        /// </summary>
        public MenuItem? MenuItem { get; set; }
    }
}
