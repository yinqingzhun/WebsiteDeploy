using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebDeploy.Model;
using System.Data.SqlClient;
using System.Data;

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

        public bool UpdatePackageReceivingRecordMsg(string fingerprint, string msg)
        {
            string sql = "update deployrecord set msg=msg+@msg from package p join deployrecord r on p.packageId=r.packageId where p.fingerprint=@fingerprint";
            return base.ExecuteNonQuery(sql,
                new SqlParameter("@fingerprint", SqlDbType.Char, 32) { Value = fingerprint },
                new SqlParameter("@msg", SqlDbType.VarChar) { Value = msg }) > 0;
        }
        public bool FinishReceivingPackage(string fingerprint, string error)
        {
            if (string.IsNullOrWhiteSpace(error))
            {
                string sql = "update deployrecord set hasDone=true,Successful=true from package p join deployrecord r on p.packageId=r.packageId where p.fingerprint=@fingerprint";
                return base.ExecuteNonQuery(sql,
                    new SqlParameter("@fingerprint", SqlDbType.Char, 32) { Value = fingerprint },
                    new SqlParameter("@error", SqlDbType.VarChar) { Value = error }) > 0;
            }
            else
            {
                string sql = "update deployrecord set error=@error,hasDone=true,Successful=false from package p join deployrecord r on p.packageId=r.packageId where p.fingerprint=@fingerprint";
                return base.ExecuteNonQuery(sql,
                    new SqlParameter("@fingerprint", SqlDbType.Char, 32) { Value = fingerprint },
                    new SqlParameter("@error", SqlDbType.VarChar) { Value = error }) > 0;
            }
        }

        public Package GetNewDeployedPackage()
        {
            DeployRecord deploy = DbContext.Set<DeployRecord>().OrderByDescending(p => p.DeployId).FirstOrDefault();
            if (deploy == null)
                return null;
            return DbContext.Set<Package>().Find(deploy.PackageId);
        }

       

    }


}
