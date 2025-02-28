using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using WebShoppingOnline.Models;
using WebShoppingOnline.Repository;

namespace WebShoppingOnline.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IObjects _func;
    DbShoppingContext myDb = new DbShoppingContext();

    public HomeController(ILogger<HomeController> logger, IObjects func)
    {
        _logger = logger;
        _func = func;
    }

    public IActionResult Index()
    {
        var lstProducts = myDb.Products.OrderByDescending(p => p.ProductId).Take(10).ToList();
        var tmp = myDb.OrderDetails.GroupBy(o => o.ProductId).Select(g => new
        {
            ProductId = g.Key,
            TotalAmount = g.Sum(o => o.Amount)
        }).OrderByDescending(g => g.TotalAmount).Take(10).ToList();

        List<Product> bestSellers = new List<Product>();
        foreach(var item in tmp)
        {
            bestSellers.Add(myDb.Products.Find(item.ProductId));
        }

        var model = (lstProducts, bestSellers);
        return View(model);
    }

    public IActionResult Category(String name)
    {
        String? id = myDb.Categories.Where(c => c.CategoryName == name).Select(c => c.CategoryId).FirstOrDefault();
        List<Product> lstProduct = myDb.Products.Where(p => p.CategoryId == id).ToList();
        return View(lstProduct);
    }

    public IActionResult ProductDetail(string id)
    {
        var product = myDb.Products.Where(p => p.ProductId == id).FirstOrDefault();
        var productImages = myDb.ProductImages.Where(i => i.ProductId == id).ToList();
        var relatedProducts = myDb.Products.Where(p => p.ProductId != id && p.CategoryId == product.CategoryId).Take(3).ToList();

        var model = (product, productImages, relatedProducts);
        return View(model);
    }

    [Route("Register")]
    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [Route("Register")]
    [HttpPost]
    public IActionResult Register(string username, string password, string confirmPassword, string fullName, string phone, string address)
    {
        TempData["Message"] = "";

        var accChecked = myDb.Accounts.Any(a => a.UserName == username);
        if (accChecked)
        {
            TempData["Message"] = "Tên đăng nhập đã tồn tại, vui lòng chọn tên đăng nhập khác";
            return View();
        }

        if (password != confirmPassword)
        {
            TempData["Message"] = "Mật khẩu không khớp, vui lòng kiểm tra lại";
            return View();
        }

        string id = _func.SetId<Account>("AC", a => a.AccountId);
        Account acc = new Account();
        acc.AccountId = id;
        acc.UserName = username;
        acc.Password = password;
        acc.FullName = fullName;
        acc.Phone = phone;
        acc.Address = address;
        acc.CreatedDate = DateTime.Now;
        myDb.Accounts.Add(acc);

        myDb.SaveChanges();
        ViewBag.Message = "OK";

        return View();
    }

    [Route("LogIn")]
    [HttpGet]
    public IActionResult LogIn()
    {
        if (HttpContext.Session.GetString("UserName") == null)
        {
            return View();
        }
        else
        {
            return RedirectToAction("Index");
        }
    }

    [Route("LogIn")]
    [HttpPost]
    public IActionResult LogIn(Account acc)
    {
        TempData["Message"] = "";
        if (HttpContext.Session.GetString("UserName") == null)
        {
            var u = myDb.Accounts.Where(x => x.UserName == acc.UserName && x.Password == acc.Password).FirstOrDefault();
            if (u != null)
            {
                HttpContext.Session.SetString("UserName", u.UserName.ToString());
                if (u.UserName == "admin")
                {
                    return RedirectToAction("Dashboard", "Admin");
                }
                else
                {
                    return RedirectToAction("Index");
                }
            }
        }
        TempData["Message"] = "Tài khoản hoặc mật khẩu không đúng";
        return View();
    }

    public IActionResult LogOut()
    {
        HttpContext.Session.Clear();
        HttpContext.Session.Remove("UserName");
        return RedirectToAction("Index");
    }

    public List<Order> GetOrders()
    {
        var username = HttpContext.Session.GetString("UserName");
        var user = myDb.Accounts.Where(a => a.UserName == username).FirstOrDefault();
        var allOrders = myDb.Orders.Where(o => o.AccountId == user.AccountId).OrderByDescending(o => o.OrderDate).ToList();
        return allOrders;
    }

    public IActionResult ViewOrder()
    {
        var allOrders = GetOrders();
        List<OrderDetail> lstOrderDetails = new List<OrderDetail>();
        foreach(var order in allOrders)
        {
            var orderDetail = myDb.OrderDetails.Where(od => od.OrderId == order.OrderId).ToList();
            lstOrderDetails.AddRange(orderDetail);
        }
        var lstProducts = myDb.Products.ToList();

        var model = (allOrders, lstOrderDetails, lstProducts);

        return View(model);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
