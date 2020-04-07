using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using FluentScheduler;

namespace TMall.util {
	public class OrdersClearJob : IJob {
		public void Execute() {
			Respository.Users.OrdersClear();
		}
	}
}