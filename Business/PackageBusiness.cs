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

        public List<Package> GetVerifiedPackageList(int pageIndex = 1, int pageSize = 10)
        {
            return new PackageRepository().GetVerifiedPackageList(pageIndex, pageSize);
        }

        public int GetVerifiedPackageCount()
        {
            return new PackageRepository().GetVerifiedPackageCount();
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
            
            PackageRepository rep=new PackageRepository();
            Package p=rep.GetPackage(package.Fingerprint);
            if (p != null)
                return p;

            package.CreateTime = DateTime.Now;
            package.Enable = true;
            return new PackageRepository().Add(package);
        }

        public string GetFileName(string fingerprint)
        {
            if (string.IsNullOrWhiteSpace(fingerprint))
                return string.Empty;

            return new PackageRepository().GetFileName(fingerprint);
        }

        

        public bool SetPackageVerified(int packageId)
        {
            return new PackageRepository().SetPackageVerified(packageId);
        }
    }
}
