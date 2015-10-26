using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Globalization;
using System.Web.Mvc;
using System.Web.Security;

namespace WebDeploy.Models
{
    public class PackageViewModel
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
        [Display(Name = "包状态")]
        public byte Status { get; set; }
        [Required]
        [Display(Name="包文件")]
        public string File { get; set; }
        [Display(Name = "备注")]
        public string Description { get; set; }
    }
}
