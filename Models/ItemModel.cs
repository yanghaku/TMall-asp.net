using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace TMall.Models {
	public class ItemModel {
		[Display(Name = "编号")]
		public int ItemId { get; set; }

		[Display(Name = "分类编号" )]
		public int ItemCategoryId { get; set; }

		[Display(Name = "名称")]
		public string ItemName { get; set; }

		[Display(Name = "图片URL")]
		public string ItemPicture { get; set; }

		[Display(Name = "具体参数")]
		public string ItemText { get; set; }

		[Display(Name = "价格")]
		public double ItemPrice { get; set; }

		[Display(Name = "销量")]
		public int ItemSales { get; set; }
		
		[Display(Name = "库存")]
		public int ItemNumber { get; set; }

		[Display(Name = "关键词")]
		public string ItemKeyword { get; set; }

		[Display(Name = "最后更新时间")]
		public DateTime LastUpdateTime { get; set; }
	}
}