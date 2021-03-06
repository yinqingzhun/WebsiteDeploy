﻿using System;
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
                EndTime = DateTime.Now,
                Successful = false,
                DeployId = deployId,
                Enabled = true
            };
            return new PackageReceivingRecordRepository().Add(record);
        }

        public bool UpdatePackageReceivingRecordMsg(int logId, string msg)
        {
            PackageReceivingRecordRepository deployRecordRep = new PackageReceivingRecordRepository();
            return deployRecordRep.UpdatePackageReceivingRecordMsg(logId, msg);
        }

        public bool FinishReceivingPackage(int logId, string error)
        {
            PackageReceivingRecordRepository deployRecordRep = new PackageReceivingRecordRepository();
            return deployRecordRep.FinishReceivingPackage(logId, error);
        }

        public bool HasFinishReceivingNewestPackage(string hostName, bool verified)
        {
            PackageReceivingRecordRepository deployRecordRep = new PackageReceivingRecordRepository();
            return deployRecordRep.HasFinishReceivingNewestPackage(hostName,verified);
        }

        public List<PackageReceivingRecordModel> GetPackageReceivingRecordList(int deployId)
        {
            PackageReceivingRecordRepository deployRecordRep = new PackageReceivingRecordRepository();
            return deployRecordRep.GetPackageReceivingRecordList(deployId);
        }

        public void Delete(int recordId)
        {
            PackageReceivingRecordRepository deployRecordRep = new PackageReceivingRecordRepository();
            var o = deployRecordRep.FindByPrimaryKey<PackageReceivingRecord>(recordId);
            o.Enabled = false;
            deployRecordRep.Update(o);
        }

    }
}
