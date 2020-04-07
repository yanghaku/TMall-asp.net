using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TMall.Models {
	public class OrdersModel {
		public int OrderId { get; set; }
		public int ItemId { get; set; }
		public string Username { get; set; }
		public int OrderNum { get; set; }
		public double OrderPrice { get; set; }
		public string OrderStatus { get; set; }
		public DateTime OrderCreateTime { get; set; }
	}
}