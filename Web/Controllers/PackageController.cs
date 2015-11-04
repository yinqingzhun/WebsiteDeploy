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

    public class PackageController : Controller
    {
        const string inputFileFieldName = "File";
        //
        // GET: /Package/

        public ActionResult Index(int pageIndex = 1, int pageSize = 10)
        {
            PackageBusiness b = new PackageBusiness();
            var list = b.GetPackageList();
            var count = b.GetPackageCount();

            DeployRecordBusiness drb = new DeployRecordBusiness();
            var packageIdAndDeployedCountPairs = drb.GetDeployCountForPackage(list.Select(p => p.PackageId));

            List<PackageModel> results = new List<PackageModel>();
            list.ForEach(p =>
            {
                var q = ObjectCopier.Copy<PackageModel>(p);
                q.ExtraInfo = "暂未发布";
                if (packageIdAndDeployedCountPairs.ContainsKey(p.PackageId))
                    q.ExtraInfo = string.Format("已发布{0}次", packageIdAndDeployedCountPairs[p.PackageId]);
                results.Add(q);
            });



            return View(results);
        }

        //
        // GET: /Package/Details/5

        //public ActionResult Details(int id)
        //{
        //    return View();
        //}

        //
        // GET: /Package/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Package/Create

        [HttpPost]
        public ActionResult Create(PackageModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    CaculateModel(model);
                    string msg = "";
                    string generatedfileName = SaveFile(inputFileFieldName, out msg);
                    model.File = generatedfileName;
                    if (string.IsNullOrWhiteSpace(generatedfileName))
                    {
                        ModelState.AddModelError(inputFileFieldName, msg);
                        return View(model);
                    }

                    PackageBusiness b = new PackageBusiness();
                    b.AddPackage(ObjectCopier.Copy<Package>(model));

                }

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
        private void CaculateModel(PackageModel model)
        {
            model.CreateTime = DateTime.Now;
            model.Enable = true;
            model.Fingerprint = HashHelper.ComputeHashString(HashHelper.HashName.MD5, Request.Files[inputFileFieldName].InputStream);
            model.PackageName = Request.Files[inputFileFieldName].FileName;
            model.PackageSize = Request.Files[inputFileFieldName].ContentLength;
            model.Verified = false;
        }
        [NonAction]
        private string SaveFile(string name, out string error)
        {
            error = "";
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException("name");

            var file = Request.Files[name];
            if (file == null)
            {
                error = "文件不存在或长度为0";
                return string.Empty;
            }

            if (Path.GetExtension(file.FileName) != ".zip")
            {
                error = "文件必须为zip格式";
                return string.Empty;
            }

            string fileName = name + DateTime.Now.Ticks + ".zip";
            Request.Files[name].SaveAs(Path.Combine(GetUploadDir(), fileName));
            return fileName;

        }

        private string GetUploadDir()
        {
            string dir = Server.MapPath("/upload");
            if (!System.IO.Directory.Exists(dir))
                System.IO.Directory.CreateDirectory(dir);
            return dir;
        }

        //
        // GET: /Package/Delete/5

        public ActionResult Delete(int id)
        {
            return View();
        }

        //
        // POST: /Package/Delete/5

        [HttpPost]
        public ActionResult Delete(int id, FormCollection c)
        {

            PackageBusiness b = new PackageBusiness();
            b.DeletePackage(id);

            return RedirectToAction("Index");

        }

        public ActionResult SetVerified(int packageId)
        {
            PackageBusiness b = new PackageBusiness();
            b.SetPackageVerified(packageId);

            return RedirectToAction("Index");
        }

        public FileResult GetPackageFile(string uuid)
        {
            var b = new PackageBusiness();
            string fileName = b.GetFileName(uuid);
            return File(Path.Combine(GetUploadDir(), fileName),
                System.Net.Mime.MediaTypeNames.Application.Zip, fileName);
        }






    }
}
