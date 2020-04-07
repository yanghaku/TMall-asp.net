using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TMall.Models {
	public class UserLoginModel {
		[Display(Name = "用户名")]
		[Required]
		[MaxLength(120, ErrorMessage = "{0}长度不能超过120")]
		public string Username { get; set; }

		[Display(Name = "密码")]
		[Required]
		[MaxLength(120, ErrorMessage ="{0}长度不能超过120")]
		public string Passwd { get; set; }

		[Display(Name = "验证码")]
		[Required]
		public string Captcha { get; set; }
	}
}