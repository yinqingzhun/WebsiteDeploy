using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebDeploy.Model;
using System.Data.SqlClient;
using System.Data;
using WebDeploy.Models;

namespace WebDeploy.Repository
{
    public class DeployRecordRepository : RepositoryBase
    {

        public int GetDeployId(string packageFingerprint)
        {
            string sql = "select top 1 r.deployId from package p join deployrecord r on p.packageId=r.packageId where p.fingerprint=@fingerprint";
            var o = base.ExecuteScalar(sql, new SqlParameter("@fingerprint", SqlDbType.Char, 32) { Value = packageFingerprint });
            int i = 0;
            int.TryParse(o.ToString(), out i);
            return i;
        }

       

        public Package GetNewDeployedPackage()
        {
            DeployRecord deploy = DbContext.Set<DeployRecord>().OrderByDescending(p => p.DeployId).FirstOrDefault();
            if (deploy == null)
                return null;
            return DbContext.Set<Package>().Find(deploy.PackageId);
        }

        public List<DeployRecordModel> GetDeployedRecordList()
        {
            return (from d in DbContext.Set<DeployRecord>()
                    join p in DbContext.Set<Package>()
                    on d.PackageId equals p.PackageId
                    orderby d.DeployTime descending
                    select new DeployRecordModel()
                    {
                        DeployId = d.DeployId,
                        UserName = d.UserName,
                        DeployTime = d.DeployTime,
                        PackageId = d.PackageId,
                        CreateTime = p.CreateTime,
                        PackageName = p.PackageName,
                        PackageSize = p.PackageSize,
                        File = p.File
                    }).ToList();

        }

        public DeployRecordModel GetDeployedRecord(int deployId)
        {
            return (from d in DbContext.Set<DeployRecord>()
                    join p in DbContext.Set<Package>()
                    on d.PackageId equals p.PackageId
                    where d.DeployId == deployId
                    select new DeployRecordModel()
                    {
                        DeployId = d.DeployId,
                        UserName = d.UserName,
                        DeployTime = d.DeployTime,
                        PackageId = d.PackageId,
                        CreateTime = p.CreateTime,
                        PackageName = p.PackageName,
                        PackageSize = p.PackageSize,
                        File = p.File
                    }).FirstOrDefault();

        }

    }


}
