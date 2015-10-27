using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using WebDeploy.Model;
using WebDeploy.Repository;

namespace WebDeploy.Business
{
    public class PackageBusiness
    {
        public List<Package> GetPackageList(int pageIndex = 1, int pageSize = 10)
        {
            return new PackageRepository().GetPackageList(pageIndex, pageSize);
        }

        public int GetPackageCount()
        {
            return new PackageRepository().GetPackageCount();
        }

        public Package GetPackage(int packageId)
        {
            return new PackageRepository().FindByPrimaryKey<Package>(packageId);
        }

        public bool DeletePackage(int packageId)
        {
            return new PackageRepository().DeletePackage(packageId);
        }

        public Package UpdatePackage(Package package)
        {
            return new PackageRepository().Update(package);
        }

        public Package AddPackage(Package package)
        {
            package.CreateTime = DateTime.Now;
            package.Enable = true;
            return new PackageRepository().Add(package);
        }

        public string GetAvailableFileName(string fingerprint)
        {
            if (string.IsNullOrWhiteSpace(fingerprint))
                return string.Empty;

            return new PackageRepository().GetAvailableFileName(fingerprint);
        }

        public string GetAvailableFileName()
        {
            return new PackageRepository().GetAvailableFileName();
        }
    }
}
