using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace WebDeploy.Repository
{
    public class PackageReceivingRecordRepository:RepositoryBase
    {
        public bool HasFinishReceivingNewestPackage(string hostName)
        {
            string sql = "select count(r.deployId) from PackageReceivingRecord r join DeployRecord d on r.deployId=d.deployId where r.hostName=@hostName and r.deployId=(select max(deployId) from DeployRecord)";
            var o = base.ExecuteScalar(sql, new SqlParameter("@hostName", SqlDbType.VarChar, 50) { Value = hostName });
            return int.Parse(o.ToString()) > 0;

        }
    }
}
