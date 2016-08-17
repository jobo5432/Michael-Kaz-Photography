using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Channels;
using System.Web.Mvc;
using Google;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage.Blob;
using MKP.Google;
using MKP.Azure;
using MKP.Google;
using web.Models;

namespace web.Controllers
{
    public class HomeController : Controller {

        private string AzureConnectionString;
        private AzureManager AzureManager;

        public HomeController() {
            AzureConnectionString = CloudConfigurationManager.GetSetting("StorageConnectionString");
            AzureManager = new AzureManager(AzureConnectionString);
        }

        public ActionResult Index() {
            var Containers = AzureManager.GetContainers();

            if (Containers == null || Containers.Count == 0) {
                //not found... 404 that shit.
                return RedirectToAction("Error404", "Errors");
            }
            else {
                var HomeContainer = Containers.FirstOrDefault(c => c.Name.StartsWith("home-date-"));
                var NavLinks = new List<NavLink>();
                var blobs = AzureManager.GetBlobs(HomeContainer.Name);
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

        [Route("StorageManager")]
        public ActionResult StorageManager() {
            var containers = AzureManager.GetContainers();
            var FolderList = new List<FolderInfo>();

            foreach (var c in containers) {
                var fi = new FolderInfo() {
                    Name = Utils.HumanizeAzureContainerName(c.Name),
                    IsHomepage = c.Name.ToLower() == "home" ? true : false,
                    Images = new List<Image>()
                };

                try {
                    foreach (var b in c.ListBlobs(null, false)) {
                        if (b.GetType() == typeof(CloudBlockBlob)) {
                            var temp = (CloudBlockBlob) b;
                            temp.FetchAttributes();
                            /*var wc = new WebClient();
                            var stream = wc.OpenRead(temp.Uri);
                            var i = System.Drawing.Image.FromStream(stream);*/

                            var req = WebRequest.Create(temp.Uri.AbsoluteUri);

                            using (var resp = req.GetResponse()) {
                                using (var stream = resp.GetResponseStream()) {
                                    var i = System.Drawing.Image.FromStream(stream);

                                    fi.Images.Add(new MKP.Google.Image() {
                                        ByteCount = temp.Properties.Length,
                                        Filename = temp.Name,
                                        FullPath = temp.Uri.AbsoluteUri,
                                        Height = i.Height,
                                        Width = i.Width
                                    });
                                }
                            }
                        }
                    }
                }
                catch {
                    //log it... 
                }
                finally {
                    FolderList.Add(fi);
                }
            }

            if (FolderList != null) {
                //update the config
                using (var ci = new Models.CacheInfo(Server.MapPath("/Content/Cache/cache-config.json"))) {
                    ci.UpdateCacheConfig();
                    ViewBag.FolderList = FolderList;
                }
            }
            else {
                //redirect to an error page
            }

            return View();

        }

        [HttpPost]
        public ActionResult UpdateStorage() {
            var GoogleManager = new MKP.Google.AssetManager(Server.MapPath("/mkp-mvc.json"));
            var AzureManager = new MKP.Azure.AzureManager(AzureConnectionString);

            //remove all storage containers
            AzureManager.ClearStorage();

            //get the Folders from Google Drive
            var FolderList = GoogleManager.GetFolders();

            //create Azure containers
            foreach (var folder in FolderList) {
                var ContainerName = folder.Replace(" ", "-").ToLower() + Utils.ContainerNameDelimiter + DateTime.Now.ToString("yyyyMMdd-hhmmss");
                AzureManager.CreateContainer(ContainerName);

                //for each folder/container get images
                var GoogleImages = GoogleManager.GetImages(folder);

                //create azure blobs
                foreach (var img in GoogleImages) {
                    var url = "https://googledrive.com/host/" + img.Id;

                    var wc = new WebClient();
                    var bytes = wc.DownloadData(url);
                    AzureManager.AddBlob(ContainerName, img.Name, Utils.DefaultMimeType, bytes);
                    bytes = null;

                    //AzureManager.AddBlob(ContainerName, img.Name, Utils.DefaultMimeType, File: new Uri(url));
                }
            }

            return RedirectToAction("StorageManager");
        }
    }
}