using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebDeploy.Model;
using System.Data.SqlClient;
using System.Data;

namespace WebDeploy.Repository
{
    public class PackageRepository : RepositoryBase
    {

        public bool DeletePackage(int packageId)
        {
            string sql = "update package set enable=0 where packageId=@packageId";
            return base.ExecuteNonQuery(sql, new SqlParameter("@packageId", SqlDbType.Int) { Value = packageId }) > 0;
        }

        public List<Package> GetPackageList(int pageIndex = 1, int pageSize = 10)
        {
            return DbContext.Set<Package>().Where(p => p.Enable).OrderByDescending(p => p.CreateTime).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
        }

        public int GetPackageCount()
        {
            return DbContext.Set<Package>().Count(p => p.Enable);
        }

        public List<Package> GetVerifiedPackageList(int pageIndex = 1, int pageSize = 10)
        {
            return DbContext.Set<Package>().Where(p => p.Enable & p.Verified).OrderByDescending(p => p.CreateTime).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
        }

        public int GetVerifiedPackageCount()
        {
            return DbContext.Set<Package>().Count(p => p.Enable & p.Verified);
        }

        public string GetAvailableFileName(string fingerprint)
        {
            return DbContext.Set<Package>().Where(p => p.Fingerprint == fingerprint && p.Verified).Select(p => p.File).SingleOrDefault();
        }

        public string GetAvailableFileName()
        {
            return DbContext.Set<Package>().Where(p => p.Verified).Select(p => p.File).SingleOrDefault();
        }

        public bool SetPackageVerified(int packageId)
        {
            var o = DbContext.Set<Package>().Find(packageId);
            o.Verified = true;
            return DbContext.SaveChanges() > 0;
        }
    }


}
