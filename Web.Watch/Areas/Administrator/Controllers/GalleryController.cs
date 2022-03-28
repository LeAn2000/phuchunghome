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
    public class GalleryController : BaseController
    {
        GalleryService galleryService;
        public GalleryController()
        {
            this.galleryService = new GalleryService();
        }

        public ActionResult Index()
        {
            if (!this.CheckAuth())
                return this.RedirectToLogin();

            return View(this.galleryService.GetAll());
        }

        public ActionResult Add()
        {
            if (!this.CheckAuth())
                return this.RedirectToLogin();

            return View();
        }

        public ActionResult AddImage()
        {

            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(GalleryDto gallery, HttpPostedFileBase UploadImage)
        {
            if (!this.CheckAuth())
                return this.RedirectToLogin();
            if (UploadImage == null)
                return RedirectToAction("Index");
            if (UploadImage.ContentLength > 0)
            {
                string ImageFileName = Path.GetFileName(UploadImage.FileName);
                string FolderPath = Path.Combine(Server.MapPath("~/Resources/files/Gallery/"), ImageFileName);
                UploadImage.SaveAs(FolderPath);
                gallery.Image = "/Resources/files/Gallery/" + ImageFileName;
                GalleryService galleryService = new GalleryService();
                galleryService.Insert(gallery);
            }
            return RedirectToAction("Index");
        }

        public ActionResult Delete(int id)
        {
            if (!this.CheckAuth())
                return this.RedirectToLogin();

            this.galleryService.DeleteById(id);
            return RedirectToAction("Index");
        }
    }
}