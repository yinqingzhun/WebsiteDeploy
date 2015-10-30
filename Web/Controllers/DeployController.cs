using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebDeploy.Utils;
using WebDeploy.Business;
using WebDeploy.Models;
using System.IO;
using System.Security.Cryptography;
using WebDeploy.Model;
using WebDeploy.Web.Extend;
using Newtonsoft.Json;

namespace WebDeploy.Web.Controllers
{

    public class DeployController : Controller
    {

        public JsonResult GetNewDeployedPackageUUId()
        {
            DeployBusiness b = new DeployBusiness();
            Package p = b.GetNewDeployedPackage();
            return Json(new
            {
                status = 0,
                result = new
                {
                    uuid = p == null ? "" : p.Fingerprint
                }
            });
        }





    }
}
