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
        public ActionResult Index()
        {
            DeployRecordBusiness b = new DeployRecordBusiness();
            var list = b.GetDeployedRecordList();
            return View(list);
        }

        public ActionResult Create()
        {
            PackageBusiness b = new PackageBusiness();
            var list = b.GetPackageList();
            var count = b.GetPackageCount();

            List<PackageModel> results = new List<PackageModel>();
            list.ForEach(p => results.Add(ObjectCopier.Copy<PackageModel>(p)));

            return View(results);
        }
        [HttpPost]
        public ActionResult Create(int packageId)
        {
            DeployRecordBusiness b = new DeployRecordBusiness();
            var o = b.AddDeployRecord(packageId);
            if (o != null)
                return RedirectToAction("Detail", new { deployId = o.DeployId });
            return View();
        }

        public ActionResult Detail(int deployId)
        {
            DeployRecordBusiness b = new DeployRecordBusiness();
            var o = b.GetDeployRecord(deployId);
            ViewBag.DeployId = deployId;
            return View(o);
        }




        public JsonResult GetNewDeployedPackageUUId(bool verified)
        {
            DeployRecordBusiness b = new DeployRecordBusiness();
            Package p = b.GetNewDeployedPackage(verified);
            return Json(new
            {
                status = 0,
                result = new
                {
                    uuid = p == null ? "" : p.Fingerprint
                }
            }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetPackageReceivingRecordList(int deployId)
        {
            PackageReceivingRecordBusiness b = new PackageReceivingRecordBusiness();
            var list = b.GetPackageReceivingRecordList(deployId);
            return Json(new { status = 0, result = new { list = list } }, JsonRequestBehavior.AllowGet);
        }





    }
}
