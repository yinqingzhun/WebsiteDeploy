using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using WebDeploy.Model;
using WebDeploy.Repository;

namespace WebDeploy.Business
{
    public class PackageReceivingRecordBusiness
    {
         

        public PackageReceivingRecord AddPackageReceivingRecord(string uuid, string hostName)
        {
            DeployRecordRepository deployRecordRep = new DeployRecordRepository();
            int deployId = deployRecordRep.GetDeployId(uuid);

            PackageReceivingRecord record = new PackageReceivingRecord()
            {
                HasDone = false,
                ReceiverHostName = hostName,
                StartTime = DateTime.Now,
                Successful = false,
                DeployId = deployId
            };
            return new PackageReceivingRecordRepository().Add(record);
        }

        public bool UpdatePackageReceivingRecordMsg(string uuid, string msg)
        {
            PackageReceivingRecordRepository deployRecordRep = new PackageReceivingRecordRepository();
            return deployRecordRep.UpdatePackageReceivingRecordMsg(uuid, msg);
        }

        public bool FinishReceivingPackage(string uuid, string error)
        {
            PackageReceivingRecordRepository deployRecordRep = new PackageReceivingRecordRepository();
            return deployRecordRep.FinishReceivingPackage(uuid, error);
        }

        public bool HasFinishReceivingNewestPackage(string hostName)
        {
            PackageReceivingRecordRepository deployRecordRep = new PackageReceivingRecordRepository();
            return deployRecordRep.HasFinishReceivingNewestPackage(hostName);
        }

        public List<PackageReceivingRecord> GetPackageReceivingRecordList(int deployId)
        {
            PackageReceivingRecordRepository deployRecordRep = new PackageReceivingRecordRepository();
            return deployRecordRep.GetPackageReceivingRecordList(deployId);
        }

    }
}
