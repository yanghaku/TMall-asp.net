using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Diagnostics;

namespace TMall.util {
	public class RecommendCal {
		private RecommendCal() { } // 禁止实例化

		private const int K = 2000;
		private const int recommendNumber = 200; // 设置每个用户最多保存的推荐的物品的个数
		public static void Run() {
			List<int> items = Respository.Recommend.GetItems(); // 获取所有的物品id列表

			List<HashSet<string>> itemsOfusers = new List<HashSet<string>>();// 每个物品对应的被喜爱的用户列表
			foreach(int item_id in items) { // 每个物品依次查询
				itemsOfusers.Add(Respository.Recommend.GetUsers(item_id));
			}

			// 获取物品的相似度矩阵
			List<Dictionary<int,double>>similityMatrix = GetSimilityMatrix(itemsOfusers);

			// 获取每个用户喜爱的物品集合, 相当于把itemOfUsers键值反转一下 (这里保存的是items列表里第i个物品, 而不是物品id)
			Dictionary<string, SortedSet<int>> usersOfitems = new Dictionary<string, SortedSet<int>>();
			for(int i =0;i<itemsOfusers.Count;++i) {
				foreach(string username in itemsOfusers[i]) {
					if (usersOfitems.TryGetValue(username, out SortedSet<int> s)) {
						s.Add(i);
					} else {// 如果没有包含这个user,就新建一个这个user代表的集合
						s = new SortedSet<int> { i };
						usersOfitems.Add(username, s);
					}
				}
				itemsOfusers[i].Clear(); // 用过了就马上清空释放内存
				itemsOfusers[i] = null;
			}
			itemsOfusers.Clear();

			// 获取每个物品的, 与它相似度前k大的列表
			// 其实可以自己定义一个结构体,也可以用这个KeyVaulePair<int,double>, 表示对第int个item的相似度为double
			List<List<KeyValuePair<int, double>>> similityItems = new List<List<KeyValuePair<int, double>>>();
			for(int i = 0; i < items.Count; ++i) {
				// 对称矩阵只保存了主对角线的下面一半, 所以对于i来说查询分两部分,
				// 一部分是大于i的, 在similityMatrix[i]里面存着, 
				// 另一部分是小于i的, 需要查找similityMatrix[k] 里面是否包含i

				// 第一部分
				List<KeyValuePair<int, double>> tmp = similityMatrix[i].ToList();
				
				// 第二部分
				for(int j = 0; j < i; ++j) {
					if (similityMatrix[j].TryGetValue(i, out double sim)) {// 如果包含i,j的关系,就保存下来
						tmp.Add(new KeyValuePair<int, double>(j, sim));
					}
				}

				// 获取之后,就要排序, 只保留前K大的.
				tmp.Sort((x1,x2)=> {
					if (x1.Value < x2.Value) return 1;
					else return 0;
				});
				// 然后把后面超过K的删除(省点内存)
				while (tmp.Count > K) {
					tmp.RemoveAt(tmp.Count - 1);
				}
				// 最后把tmp加入对应的列表中
				similityItems.Add(tmp);
			}
			similityMatrix.Clear(); // 不用了就马上清空

			// 到了这里, 就可以计算每个用户的推荐值了:
			foreach(var userPair in usersOfitems) {
				Dictionary<int, double> recommend = new Dictionary<int, double>(); // 给用户推荐的值, 这个int就保存item_id了
				foreach(var userLike in userPair.Value) { // 遍历用户喜欢的物品
					foreach(var itemSimility in similityItems[userLike]) { // 遍历与用户喜欢的物品相似的前K个
						if (userPair.Value.Contains(itemSimility.Key)) continue; // 如果这个物品用户已经喜爱了, 就不推荐了
						int item_id = items[itemSimility.Key]; // 获取item_id
						if (recommend.TryGetValue(item_id,out double sim)) {
							recommend[item_id] = sim + itemSimility.Value; // 相似度叠加
						} else {
							recommend[item_id] = itemSimility.Value;
						}
					}
				}
				List<KeyValuePair<int, double>> tmp = recommend.ToList();

				// 将大于给定个数的时候,删除感兴趣值程度小的
				if (tmp.Count > recommendNumber) {
					tmp.Sort((x1, x2) => {  // 从大到小排序
						if (x1.Value < x2.Value) return 1;
						else return 0;
					});
					while (tmp.Count > recommendNumber) tmp.RemoveAt(tmp.Count - 1);
				}
				// 保存到数据库
				Respository.Recommend.UpdateRecommend(userPair.Key,tmp);

				recommend.Clear();
				tmp.Clear(); // 清空
			}
		}

		// 定义一个阈值, 如果物品相似度小于这个值, 就表示两个物品无关, 就不加入相似度矩阵
		private const double similityThreshold = 0.001; 

		// 计算相似度矩阵
		private static List<Dictionary<int,double>> GetSimilityMatrix(List<HashSet<string>> itemsOfusers) {
			List<Dictionary<int, double>> result = new List<Dictionary<int, double>>();
			for(int i = 0; i < itemsOfusers.Count; ++i) {
				result.Add(new Dictionary<int, double>());
				for (int j = i + 1; j < itemsOfusers.Count; ++j) { // 计算两两相似度
					int Nj = itemsOfusers[j].Count();
					int Ni = itemsOfusers[i].Count();
					if (Ni == 0 || Nj == 0) continue;
					int Nij = 0;
					foreach(var user_i in itemsOfusers[i]) { // 计算交集个数
						if (itemsOfusers[j].Contains(user_i))++ Nij;
					}
					double simility = Convert.ToDouble(Nij) / Math.Sqrt(Ni * Nj);
					if(simility > similityThreshold) {// 如果相似度大于这个阈值,就加进去
						result[i].Add(j, simility);
					}
				}
			}
			return result;
		}
	}
}