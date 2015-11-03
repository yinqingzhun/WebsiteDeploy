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
            var o = b.AddPackageReceivingRecord(uuid, hostName);
            if (o != null)
                return Json(new { status = 0, result = new { logId = o.RecordId } });
            else
                return Json(new { status = 101 });

        }
        [HttpPost]
        public JsonResult UpdateReceiveInfo(int logId, string msg)
        {
            PackageReceivingRecordBusiness b = new PackageReceivingRecordBusiness();
            if (b.UpdatePackageReceivingRecordMsg(logId, msg))
                return Json(new { status = 0 });
            else
                return Json(new { status = 101 });

        }
        [HttpPost]
        public JsonResult FinishReceiving(int logId, string error)
        {
            PackageReceivingRecordBusiness b = new PackageReceivingRecordBusiness();
            if (b.FinishReceivingPackage(logId, error))
                return Json(new { status = 0 });
            else
                return Json(new { status = 101 });

        }

        [HttpGet]
        public JsonResult NeedToDownloadDeployedPackage(string hostName)
        {
            PackageReceivingRecordBusiness b = new PackageReceivingRecordBusiness();
            bool need = !b.HasFinishReceivingNewestPackage(hostName);
            return Json(new { status = 0, needToDownload = need }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult Delete(int recordId)
        {
            PackageReceivingRecordBusiness b = new PackageReceivingRecordBusiness();
            b.Delete(recordId);
            return Json(new { status = 0 });
        }

    }
}
