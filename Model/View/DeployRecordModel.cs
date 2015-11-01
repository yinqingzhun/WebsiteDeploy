using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebDeploy.Model
{
    public class DeployRecordModel
    {
        [Display(Name = "发布编号")]
        public int DeployId { get; set; }
        [Display(Name="发布时间")]
        public System.DateTime DeployTime { get; set; }
        [Display(Name = "用户名")]
        public string UserName { get; set; }
        [Display(Name = "包编号")]
        public int PackageId { get; set; }
        [Required]
        [Display(Name = "包名")]
        public string PackageName { get; set; }
        [Display(Name = "包大小")]
        public int PackageSize { get; set; }
        [Display(Name = "上传时间")]
        public System.DateTime CreateTime { get; set; }
        [Required]
        [Display(Name="包文件")]
        public string File { get; set; }
    }
}
