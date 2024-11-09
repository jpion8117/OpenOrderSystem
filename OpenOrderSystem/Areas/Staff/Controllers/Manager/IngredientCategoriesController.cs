using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenOrderSystem.Areas.Staff.ViewModels.Categories.Ingredients;
using OpenOrderSystem.Data;
using OpenOrderSystem.Data.DataModels;
using OpenOrderSystem.ViewModels.Shared;
using System.Text.Json;

namespace OpenOrderSystem.Areas.Staff.Controllers.Manager
{
    [Area("Staff")]
    [Authorize(Roles = "global_admin,manager")]
    [Route("Staff/Manager/Categories/Ingredient/{action=Index}")]
    public class IngredientCategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public IngredientCategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: IngredientCategoryController
        public ActionResult Index()
        {
            var model = _context.IngredientCategories
                .Include(ic => ic.MemberIngredients)
                .ToList();

            return View(model);
        }

        // GET: IngredientCategoryController/Create
        public ActionResult Create()
        {
            var ingredients = _context.Ingredients
                .OrderBy(i => i.Name)
                .ToList();

            return View("CreateEdit", new CreateEditVM(ingredients));
        }

        // POST: IngredientCategoryController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateEditVM model)
        {
            if (ModelState.IsValid)
            {
                int[] ids = JsonSerializer.Deserialize<int[]>(model.IngredientIds) ?? Array.Empty<int>();

                foreach (var ingredientId in ids)
                {
                    var ingredient = _context.Ingredients.FirstOrDefault(i => i.Id == ingredientId);
                    if (ingredient != null)
                    {
                        model.Category.MemberIngredients?.Add(ingredient);
                    }
                }

                _context.IngredientCategories.Add(model.Category);
                _context.SaveChanges();

                return RedirectToActionPermanent(nameof(Index));
            }
            else
            {
                return View("CreateEdit", model);
            }
        }

        // GET: IngredientCategoryController/Edit/5
        public ActionResult Edit(int id)
        {
            var ingredients = _context.Ingredients
                .OrderBy(i => i.Name)
                .ToList();
            var category = _context.IngredientCategories.FirstOrDefault(c => c.Id == id);

            return View("CreateEdit", new CreateEditVM(ingredients, category));
        }

        // POST: IngredientCategoryController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(CreateEditVM model)
        {
            if (ModelState.IsValid)
            {
                var category = _context.IngredientCategories.FirstOrDefault(c => c.Id == model.Id);
                int[] ids = JsonSerializer.Deserialize<int[]>(model.IngredientIds) ?? Array.Empty<int>();

                if (category == null)
                {
                    return NotFound();
                }

                foreach (var ingredientId in ids)
                {
                    var ingredient = _context.Ingredients.FirstOrDefault(i => i.Id == ingredientId);
                    if (ingredient != null)
                    {
                        model.Category.MemberIngredients?.Add(ingredient);
                    }
                }

                _context.IngredientCategories.Update(category);
                _context.SaveChanges();

                return RedirectToActionPermanent(nameof(Index));
            }
            else
            {
                return View("CreateEdit", model);
            }
        }

        // GET: IngredientCategoryController/Delete/5
        public ActionResult Delete(int id)
        {
            var category = _context.IngredientCategories.FirstOrDefault(i => i.Id == id);
            if (category == null)
                return NoContent();

            var model = new ConfirmDeleteVM
            {
                Action = "/Staff/Manager/Categories/Ingredient/DeleteConfirmed",
                Id = id,
                Title = $"Delete {category.Name}?",
                Description = $"Are you sure you want to permanently delete menu category #{id} {category.Name}? This action cannot be undone!"
            };

            return PartialView("_ConfirmDeleteModal", model);
        }

        // POST: IngredientCategoryController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var deadBitsComputing = _context.IngredientCategories.FirstOrDefault(i => i.Id == id);
            if (deadBitsComputing == null)
                return NotFound();

            _context.IngredientCategories.Remove(deadBitsComputing);
            _context.SaveChanges();

            return RedirectToActionPermanent(nameof(Index));
        }
    }
}
