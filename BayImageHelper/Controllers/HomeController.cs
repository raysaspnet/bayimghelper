using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BayImageHelper.Controllers
{


    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Get(string url)
        {
            url = url.ToLower();
            //string imgurl = "http://image.bayimg.com/38ba4e167a8f6ee9dc800b604cb0cfc6663bd720.jpg";
            if (!url.StartsWith("http://image.bayimg.com/"))
            {
                url = string.Format("http://image.bayimg.com/{0}.jpg", url);
            }
            var wc = new System.Net.WebClient();
            var imageData = wc.DownloadData(url);
            return File(imageData, "image/jpeg"); // Might need to adjust the content type based on your actual image type
        }

        [HttpPost]
        public JsonResult Upload(HttpPostedFileBase file)
        {
            var t = Request.Files[0] as HttpPostedFileBase;
            var img = new BayImage.Models.BayimgClient();
            return Json(img.POSTBayImage(t, t.FileName));
        }
        [HttpPost]
        public string Index(HttpPostedFileBase file)
        {
            return "ok";

            if (file != null && file.ContentLength > 0)
            {
                var img = new BayImage.Models.BayimgClient();
                return img.POSTBayImage(file, file.FileName);
            }
            return "";
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}