using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Web.Core.Dto;
using Web.Watch.Service;

namespace Web.Watch.Controllers
{
    public class HomeController : Controller
    {
        WebsiteService websiteService;
        ProductService productService;
        MenuService menuService;
        GalleryService galleryService;
        OrderService orderService;
        ArticleService articleService;

        public HomeController()
        {
            this.websiteService = new WebsiteService();
            this.productService = new ProductService();
            this.menuService = new MenuService();
            this.galleryService = new GalleryService();
            this.orderService = new OrderService();
            this.articleService = new ArticleService();
        }

        public ActionResult Index()
        {
            this.SetSEO_Main();
            ViewData["galleries"] = this.galleryService.GetAll();
            ViewData["sellings"] = this.productService.GetAllSelling();
            ViewData["menus"] = this.menuService.GetAllShowHomePage();

            return View();
        }

        public ActionResult Category(string alias, string orderBy = "name-asc", int page = 1, int pageSize = 12)
        {
            MenuDto menu = this.menuService.GetByAlias(alias);
            int totalRecord = 0;
            List<ProductDto> products = this.productService.GetByMenu(menu.Id, orderBy, ref totalRecord, page, pageSize);
            menu.Products = products;
            ViewBag.Total = totalRecord;
            ViewBag.Page = page;
            int max = 5;
            int totalPage = 0;
            totalPage = (int)Math.Ceiling(((double)totalRecord / (double)pageSize));
            ViewData["OrderbyList"] = this.productService.GetStringOrderBy();
            ViewBag.TotalPage = totalPage;
            ViewBag.maxPage = max;
            ViewBag.First = 1;
            ViewBag.Last = totalPage;
            ViewBag.Next = page + 1;
            ViewBag.Pre = page - 1;
            ViewBag.MetaTitle = menu.Name;
            ViewBag.MetaDescription = menu.MetaDescription;
            ViewBag.MetaRobots = menu.MetaRobots;
            ViewBag.MetaRevisitAfter = menu.MetaRevisitAfter;
            ViewBag.MetaContentLanguage = menu.MetaContentLanguage;
            ViewBag.MetaContentType = menu.MetaContentType;
            ViewBag.orderBy = orderBy;
            return View(menu);
        }

        public ActionResult ProductDetail(string alias)
        {
            ProductDto product = this.productService.GetByAlias(alias);
            List<ProductDto> products = this.productService.GetByMenu(product.MenuId.Value);
            ViewData["products"] = products;

            ViewBag.MetaTitle = product.Name;
            ViewBag.MetaDescription = product.MetaDescription;
            ViewBag.MetaRobots = product.MetaRobots;
            ViewBag.MetaRevisitAfter = product.MetaRevisitAfter;
            ViewBag.MetaContentLanguage = product.MetaContentLanguage;
            ViewBag.MetaContentType = product.MetaContentType;
            return View(product);
        }

        public ActionResult Introduce()
        {
            return View();
        }

        public ActionResult Contact()
        {           
            return View();
        }
        public ActionResult Promotion()
        {
            return View();
        }

        public ActionResult Buy(int id)
        {
            List<OrderDetailDto> cart = (List<OrderDetailDto>)Session["cart"];
            if (cart == null)
            {
                cart = new List<OrderDetailDto>();
            }

            if (cart.Any(x => x.ProductId == id))
            {
                cart.FirstOrDefault(x => x.ProductId == id).Qty++;
            }
            else
            {
                ProductDto product = this.productService.GetById(id);
                cart.Add(new OrderDetailDto()
                {
                    ProductId = product.Id,
                    ProductImage = product.Image,
                    Qty = 1,
                    ProductName = product.Name,
                    ProductPrice = product.Price,
                    ProductDiscountPrice = product.DiscountPrice
                });
            }
            Session["cart"] = cart;
            Session["cartCount"] = cart.Sum(x => x.Qty ?? 0);
            return RedirectToAction("ShoppingCart");
        }

        [HttpPost]
        public ActionResult UpdateCart(List<OrderDetailDto> products)
        {
            List<OrderDetailDto> cart = (List<OrderDetailDto>)Session["cart"];
            if (cart == null)
            {
                cart = new List<OrderDetailDto>();
            }

            cart.ForEach(x =>
            {
                x.Qty = products.FirstOrDefault(y => y.ProductId == x.ProductId).Qty;
            });

            Session["cart"] = cart.FindAll(x => x.Qty > 0);
            Session["cartCount"] = cart.Sum(x => x.Qty ?? 0);
            return RedirectToAction("ShoppingCart");
        }

        public ActionResult ShoppingCart()
        {
            this.SetSEO_Main();
            List<OrderDetailDto> cart = (List<OrderDetailDto>)Session["cart"];
            if (cart == null)
            {
                cart = new List<OrderDetailDto>();
            }
            return View(cart);
        }

        [HttpPost]
        public ActionResult Order(OrderDto order)
        {
            List<OrderDetailDto> cart = (List<OrderDetailDto>)Session["cart"];
            if (cart == null || cart.Count == 0)
            {
                return RedirectToAction("ShoppingCart");
            }

            order.OrderDetails = cart;
            this.orderService.Insert(order);
            Session["cart"] = null;
            Session["cartCount"] = null;
            return RedirectToAction("OrderSuccess");
        }

        public ActionResult OrderSuccess()
        {
            this.SetSEO_Main();
            return View();
        }

        public ActionResult Article(string alias)
        {
            this.SetSEO_Main();
            ViewData["articles"] = this.articleService.GetAll();
            return View(this.articleService.GetByAlias(alias));
        }

        public ActionResult Search(string q = "", string orderBy = "name-asc", string Typeid = "", string price = "", int page = 1, int pageSize = 12)
        {
            this.SetSEO_Main();
            ViewBag.q = q;
            int totalrecord = 0;
            List<ProductDto> products = this.productService.Search(q, orderBy, Typeid, price, ref totalrecord, page, pageSize);
            ViewData["OrderbyList"] = this.productService.GetStringOrderBy();
            ViewData["TypeMenu"] = this.productService.GetBranch();
            ViewData["price"] = this.productService.FilterPrice();
            ViewBag.Page = page;
            int max = 5;
            int totalPage = 0;
            totalPage = (int)Math.Ceiling(((double)totalrecord / (double)pageSize));
            ViewBag.TotalPage = totalPage;
            ViewBag.maxPage = max;
            ViewBag.First = 1;
            ViewBag.Last = totalPage;
            ViewBag.Next = page + 1;
            ViewBag.Pre = page - 1;
            ViewBag.orderBy = orderBy;
            ViewBag.Typeid = Typeid;
            ViewBag.Filterprice = price;
            return View(products);

        }


        public void SetSEO_Main()
        {
            WebsiteDto website = this.websiteService.GetAll().FirstOrDefault();
            ViewBag.MetaTitle = website.MetaTitle;
            ViewBag.MetaDescription = website.MetaDescription;
            ViewBag.MetaRobots = website.MetaRobots;
            ViewBag.MetaRevisitAfter = website.MetaRevisitAfter;
            ViewBag.MetaContentLanguage = website.MetaContentLanguage;
            ViewBag.MetaContentType = website.MetaContentType;
        }
    }
}