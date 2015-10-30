using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using WebDeploy.Business;
namespace WebDeploy.Web.Controllers
{
    public class PackageReceiveController : Controller
    {
        [HttpPost]
        public JsonResult BeginToReceive(string uuid, string hostName)
        {
            PackageReceivingRecordBusiness b = new PackageReceivingRecordBusiness();
            if (b.AddPackageReceivingRecord(uuid, hostName) != null)
                return Json(JsonConvert.SerializeObject(new { status = 0 }), JsonRequestBehavior.AllowGet);
            else
                return Json(JsonConvert.SerializeObject(new { status = 101 }), JsonRequestBehavior.AllowGet);

        }
        [HttpPost]
        public JsonResult UpdateReceiveInfo(string uuid, string msg)
        {
            PackageReceivingRecordBusiness b = new PackageReceivingRecordBusiness();
            if (b.UpdatePackageReceivingRecordMsg(uuid, msg))
                return Json(JsonConvert.SerializeObject(new { status = 0 }), JsonRequestBehavior.AllowGet);
            else
                return Json(JsonConvert.SerializeObject(new { status = 101 }), JsonRequestBehavior.AllowGet);

        }
        [HttpPost]
        public JsonResult FinishReceiving(string uuid, string error)
        {
            PackageReceivingRecordBusiness b = new PackageReceivingRecordBusiness();
            if (b.FinishReceivingPackage(uuid, error))
                return Json(JsonConvert.SerializeObject(new { status = 0 }), JsonRequestBehavior.AllowGet);
            else
                return Json(JsonConvert.SerializeObject(new { status = 101 }), JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public JsonResult NeedToDownloadDeployedPackage(string hostName)
        {
            PackageReceivingRecordBusiness b = new PackageReceivingRecordBusiness();
            bool need = !b.HasFinishReceivingNewestPackage(hostName);
            return Json(JsonConvert.SerializeObject(new { status = 0, needToDownload = need }), JsonRequestBehavior.AllowGet);
        }

    }
}
