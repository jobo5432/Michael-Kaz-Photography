using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Azure;
using MKP.Azure;
using web;
using web.Models;

namespace MKP.Web.Controllers
{
    [RoutePrefix("errors")]
    public class ErrorsController : Controller {

        private string AzureConnectionString;
        private AzureManager AzureManager;

        public ErrorsController() {
            AzureConnectionString = CloudConfigurationManager.GetSetting("StorageConnectionString");
            AzureManager = new AzureManager(AzureConnectionString);
        }

        [Route("404")]
        public ActionResult Error404() {
            var Containers = AzureManager.GetContainers();

            var NavLinks = new List<NavLink>();
            foreach (var c in Containers) {
                NavLinks.Add(new NavLink() {
                    Name = Utils.HumanizeAzureContainerName(c.Name)
                });
            }
            ViewBag.Folders = NavLinks;
            
            return View();
        }
    }
}