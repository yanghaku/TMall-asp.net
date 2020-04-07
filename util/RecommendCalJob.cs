using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TMall.util {
	public class RecommendCalJob: FluentScheduler.IJob {
		public void Execute() {
			RecommendCal.Run();
		}
	}
}