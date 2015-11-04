using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using WebDeploy.Model;
using WebDeploy.Models;
using WebDeploy.Repository;

namespace WebDeploy.Business
{
    public class DeployRecordBusiness
    {
        public Package GetNewDeployedPackage(bool verified)
        {
            return new DeployRecordRepository().GetNewDeployedPackage(verified);
        }

        public List<DeployRecordModel> GetDeployedRecordList()
        {
            return new DeployRecordRepository().GetDeployedRecordList();
        }

        public DeployRecord AddDeployRecord(int packageId)
        {
            DeployRecord o = new DeployRecord()
            {
                PackageId = packageId,
                UserId = 0,
                UserName = "",
                DeployTime = DateTime.Now,
                Enabled = true
            };
            return new DeployRecordRepository().Add(o);

        }

        public DeployRecordModel GetDeployRecord(int deployRecordId)
        {
            return new DeployRecordRepository().GetDeployedRecord(deployRecordId);
        }

        public Dictionary<int, int> GetDeployCountForPackage(IEnumerable<int> packageIdList)
        {
            return new DeployRecordRepository().GetDeployCountForPackage(packageIdList);
        }






    }
}
