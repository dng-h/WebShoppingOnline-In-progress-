using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;
using WebShoppingOnline.Models;
using WebShoppingOnline.Models.Authentication;
using WebShoppingOnline.Repository;


namespace WebShoppingOnline.Controllers
{
    public class CartController : Controller
    {
        DbShoppingContext myDb = new DbShoppingContext();
        private readonly IObjects _func;

        public CartController(IObjects func)
        {
            _func = func;
        }
        private List<CartDetail> GetCart()
        {
            var username = HttpContext.Session.GetString("UserName");
            var user = myDb.Accounts.Where(a => a.UserName == username).FirstOrDefault();
            var cart = myDb.Carts.Where(c => c.AccountId == user.AccountId).FirstOrDefault();
            var cartDetail = myDb.CartDetails.Where(cd => cd.CartId == cart.CartId).ToList();
            return cartDetail;
        }

        [Authentication]
        [HttpPost]
        public JsonResult AddToCart(string id, int amount, int quantity)
        {
            var cart = GetCart();
            if (cart.Count > 0)
            {
                var item = cart.FirstOrDefault(x => x.ProductId == id);
                if (item != null)
                {
                    if ((item.Quantity + quantity) < amount)
                    {
                        item.Quantity += quantity;
                    }
                    else
                    {
                        item.Quantity = amount;
                    }
                    myDb.CartDetails.Update(item);
                    myDb.SaveChanges();
                }
                else
                {
                    CartDetail cartDetail = new CartDetail();
                    cartDetail.CartDetailId = _func.SetId<CartDetail>("CD", c => c.CartDetailId);
                    cartDetail.CartId = cart[0].CartId;
                    cartDetail.ProductId = id;
                    cartDetail.Quantity = quantity;
                    myDb.CartDetails.Add(cartDetail);
                    myDb.SaveChanges();
                }
            }
            else
            {
                var username = HttpContext.Session.GetString("UserName");
                var user = myDb.Accounts.Where(a => a.UserName == username).FirstOrDefault();
                var cartId = myDb.Carts.Where(c => c.AccountId == user.AccountId).Select(c => c.CartId).FirstOrDefault();
                CartDetail cartDetail = new CartDetail();
                cartDetail.CartDetailId = _func.SetId<CartDetail>("CD", cd => cd.CartDetailId);
                cartDetail.CartId = cartId;
                cartDetail.ProductId = id;
                cartDetail.Quantity = quantity;
                myDb.CartDetails.Add(cartDetail);
                myDb.SaveChanges();
            }

            return Json(new
            {
                status = true,
                message = "Đã thêm sản phẩm vào giỏ hàng"
            });
        }

        public IActionResult ViewCart()
        {
            var cart = GetCart();
            var products = myDb.Products.ToList();
            var model = (cart, products);
            return View(model);
        }
    }
}
