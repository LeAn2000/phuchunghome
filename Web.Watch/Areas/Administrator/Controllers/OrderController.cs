using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Web.Core.Dto;
using Web.Watch.Service;

namespace Web.Watch.Areas.Administrator.Controllers
{
    public class OrderController : BaseController
    {
        public OrderService orderService { get; set; }
        ProductService productService;
        public OrderController()
        {
            this.productService = new ProductService();
            this.orderService = new OrderService();

        }

        // GET: Administrator/Order
        public ActionResult Index()
        {
            Response.Headers.Add("Refresh", "900");
            if (!this.CheckAuth())
                return this.RedirectToLogin();
            
            return View(this.orderService.GetAll());
        }

        public ActionResult Update(int id)
        {
            if (!this.CheckAuth())
                return this.RedirectToLogin();

            return View(this.orderService.GetById(id));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Update(OrderDto order)
        {
            if (!this.CheckAuth())
                return this.RedirectToLogin();

            this.orderService.Update(order.Id, order);
            return RedirectToAction("Index");
        }


        public ActionResult Add()
        {
            if (!this.CheckAuth())
                return this.RedirectToLogin();

            List<OrderDetailDto> cart = (List<OrderDetailDto>)Session["cartOrder"];
            if (cart == null)
            {
                cart = new List<OrderDetailDto>();
            }
            ViewData["gihong"] = cart;

            return View(this.productService.GetAll());
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(int[] model)
        {
            if (!this.CheckAuth())
                return this.RedirectToLogin();
            foreach (var item in model)
            {
                List<OrderDetailDto> cart = (List<OrderDetailDto>)Session["cartOrder"];
                if (cart == null)
                {
                    cart = new List<OrderDetailDto>();
                }

                if (cart.Any(x => x.ProductId == item))
                {
                    cart.FirstOrDefault(x => x.ProductId == item).Qty++;
                }
                else
                {
                    ProductDto product = this.productService.GetById(item);
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
                Session["cartOrder"] = cart;
                Session["cartCountOrder"] = cart.Sum(x => x.Qty ?? 0);
            }
            

            return RedirectToAction("Add");
        }
        [HttpPost]
        public ActionResult UpdateCartOrder(List<OrderDetailDto> products)
        {
            List<OrderDetailDto> cart = (List<OrderDetailDto>)Session["cartOrder"];
            if (cart == null)
            {
                cart = new List<OrderDetailDto>();
            }

            cart.ForEach(x =>
            {
                x.Qty = products.FirstOrDefault(y => y.ProductId == x.ProductId).Qty;
                x.ProductPrice = products.FirstOrDefault(y => y.ProductId == x.ProductId).ProductPrice ?? x.ProductPrice;
                x.ProductDiscountPrice = products.FirstOrDefault(y => y.ProductId == x.ProductId).ProductDiscountPrice ?? x.ProductDiscountPrice;
            });

            Session["cartOrder"] = cart.FindAll(x => x.Qty > 0);
            Session["cartCountOrder"] = cart.Sum(x => x.Qty ?? 0);
            return RedirectToAction("Add");
        }

        [HttpPost]
        public ActionResult Order(OrderDto order)
        {
            List<OrderDetailDto> cart = (List<OrderDetailDto>)Session["cartOrder"];
            if (cart == null || cart.Count == 0)
            {
                return RedirectToAction("Add");
            }

            order.OrderDetails = cart;
            this.orderService.InsertInOrder(order);
            Session["cartOrder"] = null;
            Session["cartCountOrder"] = null;
            return RedirectToAction("Index");
        }
    }
}