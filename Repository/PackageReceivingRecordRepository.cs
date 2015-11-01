using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using WebDeploy.Model;

namespace WebDeploy.Repository
{
    public class PackageReceivingRecordRepository:RepositoryBase
    {
        public bool HasFinishReceivingNewestPackage(string hostName)
        {
            string sql = "select count(r.deployId) from PackageReceivingRecord r join DeployRecord d on r.deployId=d.deployId where r.ReceiverHostName=@hostName and r.deployId=(select max(deployId) from DeployRecord)";
            var o = base.ExecuteScalar(sql, new SqlParameter("@hostName", SqlDbType.VarChar, 50) { Value = hostName });
            return int.Parse(o.ToString()) > 0;

        }

        public bool UpdatePackageReceivingRecordMsg(string fingerprint, string msg)
        {
            string sql = "update PackageReceivingRecord set msg=msg+@msg from package p join deployrecord r on p.packageId=r.packageId join PackageReceivingRecord v on r.deployId=v.deployId where p.fingerprint=@fingerprint";
            return base.ExecuteNonQuery(sql,
                new SqlParameter("@fingerprint", SqlDbType.Char, 32) { Value = fingerprint },
                new SqlParameter("@msg", SqlDbType.VarChar) { Value = msg }) > 0;
        }
        public bool FinishReceivingPackage(string fingerprint, string error)
        {
            if (string.IsNullOrWhiteSpace(error))
            {
                string sql = "update PackageReceivingRecord set hasDone=1,Successful=1,endtime=getdate() from package p join deployrecord r on p.packageId=r.packageId join PackageReceivingRecord v on r.deployId=v.deployId where p.fingerprint=@fingerprint";
                return base.ExecuteNonQuery(sql,
                    new SqlParameter("@fingerprint", SqlDbType.Char, 32) { Value = fingerprint },
                    new SqlParameter("@error", SqlDbType.VarChar) { Value = error }) > 0;
            }
            else
            {
                string sql = "update PackageReceivingRecord set error=@error,hasDone=1,Successful=0 from package p join deployrecord r on p.packageId=r.packageId join PackageReceivingRecord v on r.deployId=v.deployId where p.fingerprint=@fingerprint";
                return base.ExecuteNonQuery(sql,
                    new SqlParameter("@fingerprint", SqlDbType.Char, 32) { Value = fingerprint },
                    new SqlParameter("@error", SqlDbType.VarChar) { Value = error }) > 0;
            }
        }

        public List<PackageReceivingRecord> GetPackageReceivingRecordList(int deployId)
        {
            return DbContext.Set<PackageReceivingRecord>().Where(p => p.DeployId == deployId).ToList();
        }
    }
}
