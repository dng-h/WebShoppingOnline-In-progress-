using Microsoft.AspNetCore.Mvc;
using WebShoppingOnline.Models;
using WebShoppingOnline.Repository;

namespace WebShoppingOnline.ViewComponents
{
    public class CategoriesMenu : ViewComponent
    {
        private readonly IObjects categories;

        public CategoriesMenu(IObjects c)
        {
            categories = c;
        }

        public IViewComponentResult Invoke()
        {
            var category = categories.GetAll<Category>().OrderBy(x => x.CategoryId);
            return View(category);
        }
    }
}
