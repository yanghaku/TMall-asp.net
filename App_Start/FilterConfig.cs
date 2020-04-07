using System.Web;
using System;
using System.Web.Mvc;

namespace TMall {
	public class FilterConfig {
		public static void RegisterGlobalFilters(GlobalFilterCollection filters) {
			filters.Add(new HandleErrorAttribute());
		}
	}


	// 自定义权限过滤器, 用于检查是否是登录用户, 如果不是登录用户,则跳转到登录页面
	public class MyUserFilter : AuthorizeAttribute {
		// 重载验证函数, 判断是否是登录用户
		protected override bool AuthorizeCore(HttpContextBase httpContext) {
			if (!string.IsNullOrEmpty((string)httpContext.Session["username"])) return true; //已经登录返回true, 如果session没有检查到, 也就是没有登录, 就检查cookie有没有有效的登录信息
			HttpCookie cookie = httpContext.Request.Cookies["authorizeUser"];
			if (cookie != null) {
				string username = cookie["username"];
				string passwd = cookie["passwd"];
				if (TMall.Respository.Users.login(username, passwd)) {// 如果cookie保存了有效值, 就直接登录 , 并且返回true
					httpContext.Session.Add("username", username);
					return true;
				}
			}
			return false;
		}

		protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext) {
			base.HandleUnauthorizedRequest(filterContext);
			if (filterContext.HttpContext.Request.IsAjaxRequest()) {// 如果是ajax异步请求, 就返回错误信息, 否则重定向
				filterContext.HttpContext.Response.StatusCode = 401;// 返回未授权状态码
			} else filterContext.Result = new RedirectResult("/Users/Login?redirectURL="+filterContext.HttpContext.Request.Url);
		}
	}

	// 自定义权限过滤器, 用于检查是不是管理员用户, 如果不是, 跳转到错误页面
	public class MyAdminFilter: AuthorizeAttribute {
		protected override bool AuthorizeCore(HttpContextBase httpContext) {
			if (!string.IsNullOrEmpty((string)httpContext.Session["username"])) {
				//已经登录就判断是不是管理员, 如果session没有检查到, 也就是没有登录, 就检查cookie有没有有效的登录信息
				return TMall.Respository.Users.isAdmin((string)httpContext.Session["username"]);
			}
			HttpCookie cookie = httpContext.Request.Cookies["authorizeUser"];
			if (cookie != null) {
				string username = cookie["username"];
				string passwd = cookie["passwd"];
				if (TMall.Respository.Users.login(username, passwd)) {// 如果cookie保存了有效值, 就直接登录 , 并且返回true
					httpContext.Session.Add("username", username);
					return TMall.Respository.Users.isAdmin(username);
				}
			}
			return false;
		}
		// 如果不是管理员, 就返回到主页
		protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext) {
			base.HandleUnauthorizedRequest(filterContext);
			if (filterContext.HttpContext.Request.IsAjaxRequest()) {// 如果是ajax异步请求, 就返回错误信息, 否则重定向到主页
				filterContext.HttpContext.Response.StatusCode = 401;// 返回未授权状态码
			} else filterContext.Result = new RedirectResult("/Home/Index");
		}
	}
}
