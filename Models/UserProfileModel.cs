using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TMall.Models {
	public class UserProfileModel {
		[Display(Name = "用户名")]
		[Required]
		[MaxLength(120, ErrorMessage = "{0}长度不能超过120")]
		public string Username { get; set; }

		[Display(Name = "密码")]
		[Required]
		[DataType(DataType.Password)]
		[MaxLength(120, ErrorMessage = "{0}长度不能超过120")]
		public string Passwd { get; set; }

		[Display(Name = "邮箱")]
		[Required]
		[DataType(DataType.EmailAddress)]//邮箱验证方式
		public string Email { get; set; }

		[Display(Name = "用户等级")]
		public int Level { get; set; }// 用户等级

		[Display(Name = "出生日期")]
		[DataType(DataType.Time)]
		public DateTime Birthday { get; set; }

		[Display(Name = "电话")]
		[DataType(DataType.PhoneNumber)]
		public string PhoneNumber { get; set; }

		[Display(Name = "注册时间")]
		public DateTime RegisterTime { get; set; }

		[Display(Name = "QQ号码")]
		public int QQNumber { get; set; }

		[Display(Name = "收货地址")]
		[Required]
		public string Address { get; set; }

		[Display(Name = "验证码")]
		[Required]
		public string Captcha { get; set; }
	}
}