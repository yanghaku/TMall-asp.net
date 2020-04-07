using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FluentScheduler;

namespace TMall.util {
	public class JobRegistry : Registry{
		public JobRegistry() {

			// 设置每天两点执行订单清理任务
			Schedule<OrdersClearJob>().ToRunEvery(1).Days().At(02, 00);

			// 设置每天凌晨1点执行推荐算法的计算
			Schedule<RecommendCalJob>().ToRunEvery(1).Days().At(01, 00);
		}
	}
}