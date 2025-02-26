using Microsoft.AspNetCore.Mvc;
using WebShoppingOnline.Models;
using WebShoppingOnline.Controllers;
using WebShoppingOnline.Repository;
using System.Diagnostics;
using Microsoft.AspNetCore.Hosting.Server;
using static System.Net.Mime.MediaTypeNames;

namespace WebShoppingOnline.Controllers
{
    public class AdminController : Controller
    {
        DbShoppingContext myDb = new DbShoppingContext();
        private readonly IObjects _func;

        public AdminController(IObjects func)
        {
            _func = func;
        }

        public IActionResult Dashboard()
        {
            var userName = HttpContext.Session.GetString("UserName");

            if (userName != "admin")
            {
                return RedirectToAction("LogIn", "Home");
            }
            return View();
        }

        public IActionResult ProductManagement()
        {
            List<Product> lstProduct = myDb.Products.OrderByDescending(p => p.ProductId).ToList();
            var categories = myDb.Categories.ToDictionary(c => c.CategoryId, c => c.CategoryName);
            ViewBag.Categories = categories;
            return View(lstProduct);
        }

        [Route("AddProduct")]
        [HttpGet]
        public IActionResult AddProduct()
        {
            var id = _func.SetId<Product>("PR", p => p.ProductId);
            var categories = myDb.Categories.ToList();
            var suppliers = myDb.Suppliers.ToList();

            var model = (id, categories, suppliers);
            return View(model);
        }

        [Route("AddProduct")]
        [HttpPost]
        public IActionResult AddProduct(string productName, string categoryName, string supplierName, string price, string amount, string discount, IFormFile avatar, List<IFormFile> images, string shortDescription, string productDescription)
        {
            var id = _func.SetId<Product>("PR", p => p.ProductId);
            var categories = myDb.Categories.ToList();
            var suppliers = myDb.Suppliers.ToList();

            var model = (id, categories, suppliers);

            TempData["Message"] = "";

            if (string.IsNullOrEmpty(categoryName) || string.IsNullOrEmpty(supplierName))
            {
                TempData["Message"] = "Phân loại sản phẩm và nhà cung cấp phải được chọn";
                return View(model);
            }

            var category = myDb.Categories.Where(c => c.CategoryName == categoryName).FirstOrDefault();
            var supplier = myDb.Suppliers.Where(s => s.SupplierName == supplierName).FirstOrDefault();

            Product product = new Product();
            product.ProductId = id;
            product.ProductName = productName;
            product.ShortDescription = shortDescription;
            product.Description = productDescription;
            product.Price = Math.Round(Convert.ToDecimal(price), 2);
            if(avatar != null && avatar.Length > 0)
            {
                product.Image = avatar.FileName;
            }
            else
            {
                product.Image = "";
            }

            product.CategoryId = category.CategoryId;
            product.Amount = Convert.ToInt32(amount);
            product.SupplierId = supplier.SupplierId;
            product.Discount = Convert.ToInt32(discount);

            myDb.Products.Add(product);
            myDb.SaveChanges();

            ProductImage pImg = new ProductImage();

            if (images != null && images.Count > 0)
            {
                foreach (var image in images)
                {
                    if (image.Length > 0)
                    {
                        pImg.ImageId = _func.SetId<ProductImage>("IM", i => i.ImageId);
                        pImg.ProductId = id;
                        pImg.Image = image.FileName;
                        string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/ProductImages", image.FileName);
                        using (var stream = new FileStream(path, FileMode.Create))
                        {
                            image.CopyTo(stream);
                        }
                        myDb.ProductImages.Add(pImg);
                        myDb.SaveChanges();
                    }
                }
            }

            return RedirectToAction("ProductManagement");
        }

        [Route("EditProduct/{id}")]
        [HttpGet]
        public IActionResult EditProduct(string id)
        {
            var product = myDb.Products.Where(p => p.ProductId == id).FirstOrDefault();
            var categories = myDb.Categories.ToList();
            var suppliers = myDb.Suppliers.ToList();

            var model = (product, categories, suppliers);
            return View(model);
        }

        [Route("EditProduct/{id}")]
        [HttpPost]
        public IActionResult EditProduct(string id, string productName, string categoryName, string supplierName, string price, string amount, string discount, IFormFile avatar, List<IFormFile> images, string shortDescription, string productDescription)
        {

            var category = myDb.Categories.Where(c => c.CategoryName == categoryName).FirstOrDefault();
            var supplier = myDb.Suppliers.Where(s => s.SupplierName == supplierName).FirstOrDefault();

            var product = myDb.Products.Find(id);
            product.ProductName = productName;
            product.CategoryId = category.CategoryId;
            product.SupplierId = supplier.SupplierId;
            product.Price = Math.Round(Convert.ToDecimal(price), 2);
            product.Amount = Convert.ToInt32(amount);
            product.Discount = Convert.ToInt32(discount);
            product.ShortDescription = shortDescription;
            product.Description = productDescription;

            myDb.SaveChanges();

            ProductImage pImg = new ProductImage();

            if (avatar != null && avatar.Length > 0)
            {
                product.Image = avatar.FileName;
                string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/ProductImages", avatar.FileName);
                if (!System.IO.File.Exists(path))
                {
                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        avatar.CopyTo(stream);
                    }
                    pImg.ImageId = _func.SetId<ProductImage>("IM", i => i.ImageId);
                    pImg.ProductId = id;
                    pImg.Image = avatar.FileName;
                    myDb.SaveChanges();
                }
            }

            if (images != null && images.Count > 0)
            {
                var oldImages = myDb.ProductImages.Where(i => i.ProductId == id).ToList();
                myDb.ProductImages.RemoveRange(oldImages);

                foreach (var img in oldImages)
                {
                    string imgPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/ProductImages", img.Image);
                    if (System.IO.File.Exists(imgPath))
                    {
                        System.IO.File.Delete(imgPath);
                    }
                }

                foreach (var image in images)
                {
                    if (image.Length > 0)
                    {
                        pImg.ImageId = _func.SetId<ProductImage>("IM", i => i.ImageId);
                        pImg.ProductId = id;
                        pImg.Image = image.FileName;
                        string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/ProductImages", image.FileName);
                        using (var stream = new FileStream(path, FileMode.Create))
                        {
                            image.CopyTo(stream);
                        }
                        myDb.ProductImages.Add(pImg);
                        myDb.SaveChanges();
                    }
                }
            }

            return RedirectToAction("ProductManagement");
        }

        [Route("DeleteProduct")]
        [HttpGet]
        public IActionResult DeleteProduct(string id)
        {
            TempData["Message"] = "";
            var order = myDb.OrderDetails.Where(od => od.ProductId == id).ToList();
            if(order.Count > 0)
            {
                TempData["Message"] = "Không thể xóa sản phẩm vì sẽ ảnh hưởng đến thông tin hóa đơn trước đó";
                return RedirectToAction("ProductManagement");
            }

            myDb.Products.Remove(myDb.Products.Where(p => p.ProductId == id).FirstOrDefault());

            var lstImage = myDb.ProductImages.Where(img => img.ProductId == id).ToList();
            if (lstImage.Count > 0)
            {
                myDb.ProductImages.RemoveRange(myDb.ProductImages.Where(img => img.ProductId == id).ToList());
                foreach (var image in lstImage)
                {
                    string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/ProductImages", image.Image);

                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                    }
                }
            }
            myDb.SaveChanges();

            TempData["Message"] = "Sản phẩm đã được xóa thành công";
            return RedirectToAction("ProductManagement");
        }

        [Route("CategoryManagement")]
        [HttpGet]
        public IActionResult CategoryManagement()
        {
            var categories = myDb.Categories.ToList();
            var newId = _func.SetId<Category>("CA", c => c.CategoryId);
            var model = (categories, newId);
            return View(model);
        }

        [Route("CategoryManagement")]
        [HttpPost]
        public IActionResult CategoryManagement(string categoryName)
        {
            var newId = _func.SetId<Category>("CA", c => c.CategoryId);
            Category category = new Category();
            category.CategoryId = newId;
            category.CategoryName = categoryName;
            myDb.Categories.Add(category);
            myDb.SaveChanges();

            var categories = myDb.Categories.ToList();
            newId = _func.SetId<Category>("CA", c => c.CategoryId);
            var model = (categories, newId);
            return View(model);
        }

        [Route("SupplierManagement")]
        [HttpGet]
        public IActionResult SupplierManagement()
        {
            var suppliers = myDb.Suppliers.ToList();
            var newId = _func.SetId<Supplier>("SU", c => c.SupplierId);
            var model = (suppliers, newId);
            return View(model);
        }

        [Route("SupplierManagement")]
        [HttpPost]
        public IActionResult SupplierManagement(string supplierName, string phone, string address)
        {
            var newId = _func.SetId<Supplier>("SU", c => c.SupplierId);
            Supplier supplier = new Supplier();
            supplier.SupplierId = newId;
            supplier.SupplierName = supplierName;
            supplier.Phone = phone;
            supplier.Address = address;
            myDb.Suppliers.Add(supplier);
            myDb.SaveChanges();

            var suppliers = myDb.Suppliers.ToList();
            newId = _func.SetId<Supplier>("SU", c => c.SupplierId);
            var model = (suppliers, newId);
            return View(model);
        }
    }
}
