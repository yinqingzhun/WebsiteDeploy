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
            return DbContext.Set<Package>().Where(p => p.Enable).OrderByDescending(p=>p.CreateTime).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
        }

        public int GetPackageCount()
        {
            return DbContext.Set<Package>().Count(p => p.Enable);
        }

        public string GetAvailableFileName(string fingerprint)
        {
            return DbContext.Set<Package>().Where(p => p.Fingerprint == fingerprint && p.Status == 1).Select(p => p.File).SingleOrDefault();
        }

        public string GetAvailableFileName()
        {
            return DbContext.Set<Package>().Where(p =>  p.Status == 1).Select(p => p.File).SingleOrDefault();
        }
    }


}
