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
            var cartDetail = myDb.CartDetails.Where(cd => cd.AccountId == user.AccountId).ToList();
            return cartDetail;
        }

        [Authentication]
        [HttpPost]
        public JsonResult AddToCart(string id, int amount, int quantity)
        {
            var username = HttpContext.Session.GetString("UserName");
            var user = myDb.Accounts.Where(a => a.UserName == username).FirstOrDefault();

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
                    cartDetail.AccountId = user.AccountId;
                    cartDetail.ProductId = id;
                    cartDetail.Quantity = quantity;
                    myDb.CartDetails.Add(cartDetail);
                    myDb.SaveChanges();
                }
            }
            else
            {
                CartDetail cartDetail = new CartDetail();
                cartDetail.CartDetailId = _func.SetId<CartDetail>("CD", cd => cd.CartDetailId);
                cartDetail.AccountId = user.AccountId;
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

        public JsonResult ReduceQuantity(string id)
        {
            var cart = GetCart();
            var item = cart.Where(c => c.ProductId == id).FirstOrDefault();
            if(item.Quantity > 1)
            {
                item.Quantity--;
            }
            myDb.CartDetails.Update(item);
            myDb.SaveChanges();

            return Json(new
            {
                status = true
            });
        }

        public JsonResult IncreaseQuantity(string id)
        {
            var cart = GetCart();
            var item = cart.Where(c => c.ProductId == id).FirstOrDefault();
            var product = myDb.Products.Find(id);
            if (item.Quantity < product.Amount)
            {
                item.Quantity++;
            }
            myDb.CartDetails.Update(item);
            myDb.SaveChanges();

            return Json(new
            {
                status = true
            });
        }

        public JsonResult RemoveFromCart(string id)
        {
            var cart = GetCart();
            var item = cart.Where(c => c.ProductId == id).FirstOrDefault();
            myDb.CartDetails.Remove(item);
            myDb.SaveChanges();

            return Json(new
            {
                status = true
            });
        }

        public JsonResult MakeOrder(string delivery, string name, string phone, string address, string payment)
        {
            var username = HttpContext.Session.GetString("UserName");
            var user = myDb.Accounts.Where(a => a.UserName == username).FirstOrDefault();

            Order order = new Order();
            order.OrderId = _func.SetId<Order>("OR", o => o.OrderId);
            order.AccountId = user.AccountId;
            order.OrderType = delivery;
            order.PaymentId = myDb.Payments.Where(p => p.PaymentName == payment).Select(p => p.PaymentId).FirstOrDefault();
            order.RecipientName = name;
            order.RecipientPhone = phone;
            order.ShippingAddress = address;
            if(payment == "Tiền mặt")
            {
                order.Status = "Chưa thanh toán";
            }
            else
            {
                order.Status = "Đã thanh toán";
            }
            order.OrderDate = DateTime.Now;

            myDb.Orders.Add(order);
            myDb.SaveChanges();

            var cart = GetCart();
            foreach(var item in cart)
            {
                OrderDetail detail = new OrderDetail();
                detail.OrderDetailId = _func.SetId<OrderDetail>("OD", o => o.OrderDetailId);
                detail.OrderId = order.OrderId;
                detail.ProductId = item.ProductId;
                detail.Price = myDb.Products.Where(p => p.ProductId == item.ProductId).Select(p => p.Price).FirstOrDefault();
                detail.Amount = item.Quantity;
                detail.Discount = myDb.Products.Where(p => p.ProductId == item.ProductId).Select(p => p.Discount).FirstOrDefault();
                detail.Total = Math.Round(Convert.ToDecimal(detail.Price * (100 - detail.Discount) / Convert.ToDecimal(100) * detail.Amount), 2);

                myDb.OrderDetails.Add(detail);
                myDb.SaveChanges();

                var otherCarts = myDb.CartDetails.Where(c => c.AccountId != user.AccountId && c.ProductId == item.ProductId).ToList();
                var product = myDb.Products.Find(detail.ProductId);
                product.Amount -= detail.Amount;
                myDb.Products.Update(product);
                myDb.SaveChanges();

                foreach (var i in otherCarts)
                {

                    if(product.Amount == 0)
                    {
                        myDb.CartDetails.Remove(i);
                        myDb.SaveChanges();
                    }
                    else
                    {
                        if(product.Amount < i.Quantity)
                        {
                            i.Quantity = product.Amount;
                            myDb.CartDetails.Update(i);
                            myDb.SaveChanges();
                        }
                    }
                }

                myDb.CartDetails.Remove(item);
                myDb.SaveChanges();
            }

            return Json(new
            {
                status = true,
                message = "Đặt hàng thành công, bạn có thể xem thông tin đơn hàng trong mục Đơn hàng của tôi"
            });
        }
    }
}
