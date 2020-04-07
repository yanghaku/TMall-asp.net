using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace TMall.Models {
	public class ItemCommentModel {
		[Display(Name = "评论编号")]
		public int ItemCommentId { get; set; }

		[Display(Name = "商品编号")]
		public int ItemId { get; set; }

		[Display(Name = "评价者")]
		public string Username { get; set; }

		[Display(Name = "分数")]
		public int ItemCommentScore { get; set; }

		[Display(Name = "评价内容")]
		public string ItemCommentText { get; set; }

		[Display(Name = "评价时间")]
		public DateTime ItemCommentTime { get; set; }
	}
}