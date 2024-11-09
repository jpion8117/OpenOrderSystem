using OpenOrderSystem.Data.DataModels;
using OpenOrderSystem.ViewModels.Shared;
using System.Text.Json;

namespace OpenOrderSystem.Areas.Staff.ViewModels.Categories.Ingredients
{
    public class CreateEditVM
    {
        private IngredientCategory _category;

        public CreateEditVM()
        {
            _category = new IngredientCategory();
            Ingredients = new List<Ingredient>();
            IngredientIds = "";
        }

        public CreateEditVM(List<Ingredient> ingredients, IngredientCategory? ingredientCategory = null)
        {
            _category = ingredientCategory ?? new IngredientCategory();
            Action = ingredientCategory == null ? CrudAction.Create : CrudAction.Edit;
            Ingredients = ingredients;

            var ids = new List<int>();

            foreach (var ingredient in ingredients)
            {
                if (ingredient.Categories?.AsQueryable().FirstOrDefault(ic => ic.Id == Id) != null)
                    ids.Add(ingredient.Id);
            }

            IngredientIds = JsonSerializer.Serialize(ids);
        }

        public IngredientCategory Category { get => _category; }

        public int Id
        {
            get => _category.Id;
            set => _category.Id = value;
        }

        public string Name
        {
            get => _category.Name;
            set => _category.Name = value;
        }

        public int ExtrasAllowed
        {
            get => _category.ExtrasAllowed;
            set => _category.ExtrasAllowed = value;
        }

        public string IngredientIds { get; set; }

        public List<Ingredient> Ingredients { get; set; }

        public CrudAction Action { get; set; } = CrudAction.Create;
    }
}
