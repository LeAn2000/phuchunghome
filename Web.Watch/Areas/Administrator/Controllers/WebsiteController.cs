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
    public class WebsiteController : BaseController
    {
        WebsiteService websiteService;

        public WebsiteController()
        {
            this.websiteService = new WebsiteService();
        }

        public ActionResult Index()
        {
            if (!this.CheckAuth())
                return this.RedirectToLogin();

            return View(this.websiteService.GetAll().FirstOrDefault());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(WebsiteDto website, HttpPostedFileBase Favicons, HttpPostedFileBase Logos)
        {
            if (!this.CheckAuth())
                return this.RedirectToLogin();

            if (Favicons != null && Favicons.ContentLength > 0)
            {
                string ImageFileName = Path.GetFileName(Favicons.FileName);
                string FolderPath = Path.Combine(Server.MapPath("~/Resources/files/website/"), ImageFileName);
                Favicons.SaveAs(FolderPath);
                var link = "/Resources/files/website/" + ImageFileName;
                website.Favicon = link;
            }
            if (Logos != null && Logos.ContentLength > 0)
            {
                string ImageFileName = Path.GetFileName(Logos.FileName);
                string FolderPath = Path.Combine(Server.MapPath("~/Resources/files/website/"), ImageFileName);
                Logos.SaveAs(FolderPath);
                var link = "/Resources/files/website/" + ImageFileName;
                website.Logo = link;
            }

            this.websiteService.Update(website.Id, website);
            return RedirectToAction("Index");
        }
    }
}