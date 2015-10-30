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
        public List<PackageReceivingRecord> GetPackageReceiveRecordList()
        {
            return new List<PackageReceivingRecord>();
        }

        public PackageReceivingRecord AddPackageReceivingRecord(string uuid, string hostName)
        {
            DeployRecordRepository deployRecordRep = new DeployRecordRepository();
            int deployId = deployRecordRep.GetDeployId(uuid);

            PackageReceivingRecord record = new PackageReceivingRecord()
            {
                HasDone = false,
                ReceiverHostName = hostName,
                ReceiveTime = DateTime.Now,
                Successful = false,
                DeployId = deployId
            };
            return new PackageReceivingRecordRepository().Add(record);
        }

        public bool UpdatePackageReceivingRecordMsg(string uuid, string msg)
        {
            DeployRecordRepository deployRecordRep = new DeployRecordRepository();
            return deployRecordRep.UpdatePackageReceivingRecordMsg(uuid, msg);
        }

        public bool FinishReceivingPackage(string uuid, string error)
        {
            DeployRecordRepository deployRecordRep = new DeployRecordRepository();
            return deployRecordRep.FinishReceivingPackage(uuid, error);
        }

        public bool HasFinishReceivingNewestPackage(string hostName)
        {
            PackageReceivingRecordRepository deployRecordRep = new PackageReceivingRecordRepository();
            return deployRecordRep.HasFinishReceivingNewestPackage(hostName);
        }


    }
}
