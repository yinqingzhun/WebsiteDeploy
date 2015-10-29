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

            List<PackageViewModel> results = new List<PackageViewModel>();
            list.ForEach(p => results.Add(ObjectCopier.Copy<PackageViewModel>(p)));

            PagerHtmlHelper.Generate(ViewData, pageIndex, pageSize, count);
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
        public ActionResult Create(PackageViewModel model)
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
        private void CaculateModel(PackageViewModel model)
        {
            model.CreateTime = DateTime.Now;
            model.Enable = true;
            model.Fingerprint = HashHelper.ComputeHashString(HashHelper.HashName.MD5, Request.Files[inputFileFieldName].InputStream);
            model.PackageName = Request.Files[inputFileFieldName].FileName;
            model.PackageSize = Request.Files[inputFileFieldName].ContentLength;
            model.Status = 0;
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
        // GET: /Package/Edit/5

        //public ActionResult Edit(int id)
        //{
        //    return View();
        //}

        ////
        //// POST: /Package/Edit/5

        //[HttpPost]
        //public ActionResult Edit(int id, FormCollection collection)
        //{
        //    try
        //    {
        //        // TODO: Add update logic here

        //        return RedirectToAction("Index");
        //    }
        //    catch
        //    {
        //        return View();
        //    }
        //}

        //
        // GET: /Package/Delete/5

        public ActionResult Delete(int id)
        {
            return View();
        }

        //
        // POST: /Package/Delete/5

        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                PackageBusiness b = new PackageBusiness();
                b.DeletePackage(id);

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        public FileResult GetPackageFile(string ticket)
        {
            var b = new PackageBusiness();
            string fileName = b.GetAvailableFileName(ticket);
            return File(Path.Combine(GetUploadDir(), fileName), System.Net.Mime.MediaTypeNames.Application.Zip);
        }

        public FileResult GetPackageFile()
        {
            var b = new PackageBusiness();
            string fileName = b.GetAvailableFileName();
            return File(Path.Combine(GetUploadDir(), fileName), System.Net.Mime.MediaTypeNames.Application.Zip);
        }

        public JsonResult GetNewestPackageInfo()
        {

            return Json(JsonConvert.SerializeObject(new
            {
                status = 0,
                result = new
                {
                    package = new
                    {

                    }
                }
            }));
        }


    }
}
