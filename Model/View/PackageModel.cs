using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace WebDeploy.Models
{
    public class PackageModel
    {
        [Display(Name = "包编号")]
        public int PackageId { get; set; }
        [Required]
        [Display(Name = "包名")]
        public string PackageName { get; set; }
        [Display(Name = "包大小")]
        public int PackageSize { get; set; }
        [Display(Name = "上传时间")]
        public System.DateTime CreateTime { get; set; }
        public bool Enable { get; set; }
        [Display(Name = "包指纹")]
        public string Fingerprint { get; set; }
        [Display(Name = "已验证")]
        public bool Verified { get; set; }
        [Required]
        [Display(Name="包文件")]
        public string File { get; set; }
        [Display(Name = "备注")]
        public string Description { get; set; }
    }
}
