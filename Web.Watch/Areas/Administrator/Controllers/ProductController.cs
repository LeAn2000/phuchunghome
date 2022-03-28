using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Web.Core.Dto;
using Web.Watch.Service;

namespace Web.Watch.Areas.Administrator.Controllers
{
    public class ProductController : BaseController
    {
        ProductService productService;
        MenuService menuService;
        AttributeService attributeService;

        public ProductController()
        {
            this.productService = new ProductService();
            this.menuService = new MenuService();
            this.attributeService = new AttributeService();
        }

        // GET: Administrator/Product
        public ActionResult Index()
        {
            if (!this.CheckAuth())
                return this.RedirectToLogin();

            return View(this.productService.GetAll());
        }

        public ActionResult Add()
        {
            if (!this.CheckAuth())
                return this.RedirectToLogin();

            ViewData["menus"] = this.menuService.GetAll();
            ViewData["attributes"] = this.attributeService.GetAll().Select(x => new ProductAttributeDto()
            {
                Attribute = x,
                AttributeId = x.Id,
                ValueString = ""
            }).ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(ProductDto product, HttpPostedFileBase[] UploadImages, HttpPostedFileBase Images)
        {
            if (!this.CheckAuth())
                return this.RedirectToLogin();

            if (Images != null && Images.ContentLength > 0)
            {
                string ImageFileName = Path.GetFileName(Images.FileName);
                string FolderPath = Path.Combine(Server.MapPath("~/Resources/files/Product/"), ImageFileName);
                Images.SaveAs(FolderPath);
                var link = "/Resources/files/Product/" + ImageFileName;
                product.Image = link;
            }

            foreach (var image in UploadImages)
            {
                if (image == null) break;
                string ImageFileName = Path.GetFileName(image.FileName);
                string FolderPath = Path.Combine(Server.MapPath("~/Resources/files/Product/"), ImageFileName);
                image.SaveAs(FolderPath);
                var link = "/Resources/files/Product/" + ImageFileName;
                var ProductImage = new ProductImageDto()
                {
                    Image = link
                };
                if (product.ProductImages == null)
                {
                    var files = new List<ProductImageDto>();
                    files.Add(ProductImage);
                    product.ProductImages = files;
                }
                else
                {
                    product.ProductImages.Add(ProductImage);
                }
            }

            this.productService.Insert(product);
            return RedirectToAction("Index");
        }

        public ActionResult Update(int id)
        {
            if (!this.CheckAuth())
                return this.RedirectToLogin();

            ViewData["menus"] = this.menuService.GetAll();
            List<ProductAttributeDto> attributes = this.attributeService.GetAll().Select(x => new ProductAttributeDto()
            {
                Attribute = x,
                AttributeId = x.Id,
                ValueString = ""
            }).ToList();

            ProductDto product = this.productService.GetById(id);

            if (product.ProductAttributes != null)
            {
                attributes.ForEach(x =>
                {
                    var attrExist = product.ProductAttributes.FirstOrDefault(y => y.AttributeId == x.AttributeId);
                    if (attrExist != null)
                    {
                        x.ValueString = attrExist.ValueString;
                    }
                });
            }

            ViewData["attributes"] = attributes;
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Update(ProductDto product, HttpPostedFileBase[] UploadImages, HttpPostedFileBase Images)
        {
            if (!this.CheckAuth())
                return this.RedirectToLogin();

            if (Images != null && Images.ContentLength > 0)
            {
                string ImageFileName = Path.GetFileName(Images.FileName);
                string FolderPath = Path.Combine(Server.MapPath("~/Resources/files/Product/"), ImageFileName);
                Images.SaveAs(FolderPath);
                var link = "/Resources/files/Product/" + ImageFileName;
                product.Image = link;
            }

            foreach (var image in UploadImages)
            {
                if (image == null) break;
                if (image.ContentLength > 0)
                {
                    string ImageFileName = Path.GetFileName(image.FileName);
                    string FolderPath = Path.Combine(Server.MapPath("~/Resources/files/Product/"), ImageFileName);
                    image.SaveAs(FolderPath);
                    var link = "/Resources/files/Product/" + ImageFileName;
                    var ProductImage = new ProductImageDto()
                    {
                        Image = link
                    };
                    product.ProductImages.Add(ProductImage);
                }
            }

            this.productService.Update(product.Id, product);
            return RedirectToAction("Index");
        }

        public ActionResult Delete(int id)
        {
            if (!this.CheckAuth())
                return this.RedirectToLogin();

            this.productService.DeleteById(id);
            return RedirectToAction("Index");
        }
    }
}