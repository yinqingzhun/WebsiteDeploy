﻿using System;
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
            string sql = "select top 1 r.deployId from package p join deployrecord r on p.packageId=r.packageId where p.fingerprint=@fingerprint order by r.deployTime desc";
            var o = base.ExecuteScalar(sql, new SqlParameter("@fingerprint", SqlDbType.Char, 32) { Value = packageFingerprint });
            int i = 0;
            int.TryParse(o.ToString(), out i);
            return i;
        }


        /// <summary>
        /// 获取最新的发布包
        /// </summary>
        /// <param name="verified"></param>
        /// <returns></returns>
        public Package GetNewDeployedPackage(bool verified)
        {
            string sql = " select top 1 p.* from DeployRecord d join Package p on d.packageId=p.packageId where p.enable=1 and p.Verified=@Verified  and d.DeployId=(select max(deployid) from DeployRecord)";
            var list = ExecuteQuery<Package>(sql, new SqlParameter("@Verified", SqlDbType.Bit) { Value = verified });
            return list.Count == 0 ? null : list.First();


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
                        File = p.File,
                        Verified = p.Verified
                    }).FirstOrDefault();

        }

        public Dictionary<int, int> GetDeployCountForPackage(IEnumerable<int> packageIdList)
        {
            return DbContext.Set<DeployRecord>().Where(p => packageIdList.Contains(p.PackageId)).GroupBy(p => p.PackageId).ToDictionary(m => m.Key, m => m.Count());
        }

    }


}
