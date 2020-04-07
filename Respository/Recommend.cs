using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Text;

namespace TMall.Respository {
	public class Recommend {

		// 获取所有的商品编号
		public static List<int> GetItems() {
			string sql = "select item_id from item";
			SqlDataReader data = util.SqlHelper.ExecuteTable(sql, null);
			List<int> items = new List<int>();
			if(data != null) {
				while (data.Read()) {
					items.Add(data.GetInt32(0));
				}
				data.Close();
			}
			return items;
		}

		// 获取某个物品被喜爱的用户列表
		// 对于每个item, 从三个表格orders,collection,cart中抽取并集.
		public static HashSet<string> GetUsers(int item_id) {
			HashSet<string> s = new HashSet<string>();
			SqlParameter[] sqlParameters = new SqlParameter[] {
				new SqlParameter("@ItemId",item_id)
			};
			string sql = "(select distinct username from cart where item_id = @ItemId)"
				+ "union (select distinct username from collection where item_id = @ItemId)"
				+ "union (select distinct username from orders where item_id = @ItemId)";
			SqlDataReader data = util.SqlHelper.ExecuteTable(sql, sqlParameters);
			if (data != null) {
				while (data.Read()) {
					s.Add(data.GetString(0));
				}
				data.Close();
			}
			return s;
		}


		// 更新用户推荐列表
		public static void UpdateRecommend(string username, List<KeyValuePair<int, double>> recommends) {
			if (recommends == null || recommends.Count == 0) return;
			SqlParameter[] sqlParameters = new SqlParameter[] {
				new SqlParameter("@Username",username)
			};
			string sql1 = "delete from recommend where username = @Username";
			util.SqlHelper.ExecuteNoQuery(sql1, sqlParameters); // 将原先的删除

			SqlParameter[] sqlParameters1 = new SqlParameter[] {
				new SqlParameter("@Username",username)
			};
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("insert into recommend values");
			bool start = true;
			foreach(var p in recommends) { // 构造批量添加的语句
				if (start) {
					start = false; // 第一个前面不加逗号
					stringBuilder.Append($" (@Username,{p.Key},{p.Value})");
				} else stringBuilder.Append($",(@Username,{p.Key},{p.Value})");
			}
			util.SqlHelper.ExecuteNoQuery(stringBuilder.ToString(), sqlParameters1);
		}

		// 获取用户推荐的商品列表
		public static List<Models.ItemModel> GetRecommend(string username) {
			SqlParameter[] sqlParameters = new SqlParameter[] {
				new SqlParameter("@Username",username)
			};
			string sql = "select * from item where item.item_id in (select item_id from recommend where username = @Username)";
			SqlDataReader data = util.SqlHelper.ExecuteTable(sql, sqlParameters);
			List<Models.ItemModel> result = new List<Models.ItemModel>();
			if (data != null) {
				while (data.Read()) {
					Models.ItemModel item = new Models.ItemModel {
						ItemId = data.GetInt32(0),
						ItemCategoryId = data.GetInt32(1),
						ItemName = data.GetString(2),
						ItemPicture = data.GetString(3),
						ItemText = data.GetString(4),
						ItemPrice = data.GetDouble(5),
						ItemSales = data.GetInt32(6),
						ItemNumber = data.GetInt32(7),
						ItemKeyword = data.GetString(8),
						LastUpdateTime = data.GetDateTime(9)
					};
					result.Add(item);
				}
				data.Close();
			};
			return result;
		}
	}
}