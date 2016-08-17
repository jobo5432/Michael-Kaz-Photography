using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Azure;
using MKP.Azure;
using MKP.Web.Extensions;
using web.Models;

namespace web.Controllers
{
    public class GalleryController : Controller {

        private string AzureConnectionString;
        private AzureManager AzureManager;

        public GalleryController() {
            AzureConnectionString = CloudConfigurationManager.GetSetting("StorageConnectionString");
            AzureManager = new AzureManager(AzureConnectionString);
        }

        [Route("gallery/{id}")]
        public ActionResult Index(string id) {
            ViewBag.GalleryName = id.ToTitleCase() + " Gallery";
            
            var Containers = AzureManager.GetContainers();
            var GalleryContainer = Containers.FirstOrDefault(c => c.Name.StartsWith( id + "-date-"));
            var NavLinks = new List<NavLink>();
            var blobs = AzureManager.GetBlobs(GalleryContainer.Name);
            var imgList = new List<HomepageImageInfo>();

            foreach (var c in Containers) {
                NavLinks.Add(new NavLink() {
                    Name = Utils.HumanizeAzureContainerName(c.Name)
                });
            }

            foreach (var b in blobs) {
                imgList.Add(new HomepageImageInfo() { src = b.Uri.AbsoluteUri });
            }

            ViewBag.Folders = NavLinks;
            return View(model: imgList);
        }
    }
}