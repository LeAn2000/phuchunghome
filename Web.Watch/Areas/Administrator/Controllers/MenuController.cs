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
    public class MenuController : BaseController
    {
        MenuService menuService;
        public MenuController()
        {
            this.menuService = new MenuService();
        }

        // GET: Administrator/Menu
        public ActionResult Index()
        {
            if (!this.CheckAuth())
                return this.RedirectToLogin();

            return View(this.menuService.GetAll());
        }

        public ActionResult Add()
        {
            if (!this.CheckAuth())
                return this.RedirectToLogin();

            ViewData["parentMenus"] = this.menuService.GetParentMenu();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(MenuDto menu, HttpPostedFileBase UploadImage)
        {
            if (!this.CheckAuth())
                return this.RedirectToLogin();

            if (menu.ParentMenu < 0)
                menu.ParentMenu = null;

            if (UploadImage == null)
            { }

            else if (UploadImage.ContentLength > 0)
            {
                string ImageFileName = Path.GetFileName(UploadImage.FileName);
                string FolderPath = Path.Combine(Server.MapPath("~/Resources/files/Product/"), ImageFileName);
                UploadImage.SaveAs(FolderPath);
                menu.Image = "/Resources/files/Product/" + ImageFileName;
            }

            this.menuService.Insert(menu);
            return RedirectToAction("Index");
        }

        public ActionResult Update(int id)
        {
            if (!this.CheckAuth())
                return this.RedirectToLogin();

            ViewData["parentMenus"] = this.menuService.GetParentMenu();
            return View(this.menuService.GetById(id));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Update(MenuDto menu, HttpPostedFileBase UploadImage)
        {
            if (!this.CheckAuth())
                return this.RedirectToLogin();

            if (menu.ParentMenu < 0)
                menu.ParentMenu = null;

            if (UploadImage == null) { }
            else if (UploadImage.ContentLength > 0)
            {
                string ImageFileName = Path.GetFileName(UploadImage.FileName);
                string FolderPath = Path.Combine(Server.MapPath("~/Resources/files/Product/"), ImageFileName);
                UploadImage.SaveAs(FolderPath);
                menu.Image = "/Resources/files/Product/" + ImageFileName;
            }

            this.menuService.Update(menu.Id, menu);
            return RedirectToAction("Index");
        }

        public ActionResult Delete(int id)
        {
            if (!this.CheckAuth())
                return this.RedirectToLogin();

            this.menuService.DeleteById(id);
            return RedirectToAction("Index");
        }
    }
}