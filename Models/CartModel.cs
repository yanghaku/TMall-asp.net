using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;


namespace TMall.Models {
	public class CartModel {

		[Display(Name = "商品编号")]
		public int ItemId { get; set; }

		[Display(Name = "名称")]
		public string ItemName { get; set; }

		[Display(Name = "图片URL")]
		public string ItemPicture { get; set; }

		[Display(Name = "价格")]
		public double ItemPrice { get; set; }

		[Display(Name = "销量")]
		public int ItemSales { get; set; }

		[Display(Name = "库存")]
		public int ItemNumber { get; set; }

		[Display(Name = "购物车内个数")]
		public int CartItemNumber { get; set; }

		[Display(Name = "加入时间")]
		public DateTime CartInTime { get; set; }
	}
}