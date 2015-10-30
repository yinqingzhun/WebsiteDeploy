using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using WebDeploy.Model;
using WebDeploy.Repository;

namespace WebDeploy.Business
{
    public class DeployBusiness
    {
        public Package GetNewDeployedPackage()
        {
            return new DeployRecordRepository().GetNewDeployedPackage();
        }
         
    }
}
