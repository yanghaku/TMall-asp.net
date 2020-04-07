using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TMall.Controllers {
	// 用户过滤器
	public class UsersController : Controller {

		// 验证码部分, 收到请求的时候刷新验证码
		[HttpGet]
		public ActionResult Captcha() {
			byte[] arr;
			string code;
			// 获取验证码和对应的的图片
			TMall.util.Captcha.GetCaptcha(out arr, out code);
			// 将新的验证码添加到Session里, (全变成小写)
			Session.Add("Captcha", code.ToLower());
			// 返回验证码图片
			return File(arr, "image/jpeg");
		}

		// 注册页面, 返回注册页面
		[HttpGet]
		public ActionResult Register(string redirectURL) {
			ViewData["redirectURL"] = redirectURL; // 将重定向的url传递到表单隐藏域, 实现POST请求之后能重定向回去
			return View();
		}

		// 注册提交页面
		[HttpPost]
		public ActionResult Register(TMall.Models.UserProfileModel profileModel,string redirectURL) {
			ViewData["redirectURL"] = redirectURL;  // 重定向的url传递到表单隐藏域, 以免下次提交的时候丢失
			if (ModelState.IsValid) { // 表单有效
				if (!profileModel.Captcha.ToLower().Equals((string)Session["Captcha"])) {// 如果验证码不相等
					ModelState.AddModelError("Captcha", "请填写正确的验证码");
					return View();
				}
				if (TMall.Respository.Users.hasRegistered(profileModel.Username)) {
					ModelState.AddModelError("Username", "这个用户名已经被注册过了, 请换一个试试");
					return View();
				}
				profileModel.RegisterTime = DateTime.Now;
				profileModel.Level = TMall.Respository.Users.NormalUser;//注册用户默认不是管理员权限
				if (TMall.Respository.Users.register(profileModel)) {// 注册成功
					Session.Add("username", profileModel.Username);
					if (string.IsNullOrEmpty(redirectURL)) return RedirectToAction("Index", "Home");
					else return Redirect(redirectURL);
				}
			}
			return View();
		}

		// 登录页面
		[HttpGet]
		public ActionResult Login(string redirectURL) {
			if (!string.IsNullOrEmpty((string)Session["username"]))
				if (string.IsNullOrEmpty(redirectURL)) return RedirectToAction("Index", "Home");
				else return Redirect(redirectURL);// 如果处于登录状态, 直接重定向回去(当然, 这个正常操作是不会发生的)
			
			HttpCookie cookie = Request.Cookies["authorizeUser"];// 先查cookie
			if (cookie != null) {
				string username = cookie["username"];
				string passwd = cookie["passwd"];
				if (TMall.Respository.Users.login(username, passwd)) {// 如果cookie保存了有效值, 就直接登录
					Session.Add("username", username);
					if (string.IsNullOrEmpty(redirectURL)) return RedirectToAction("Index", "Home");
					else return Redirect(redirectURL);   // 直接登录后重定向回去
				}
			}

			// 前面尝试都不可以之后, 就传回表单
			if(!string.IsNullOrEmpty(redirectURL))
				ViewData["redirectURL"] = redirectURL; // 将重定向的url传递到表单隐藏域, 实现POST请求之后能重定向回去
			return View();
		}

		// 登录提交页面
		[HttpPost]
		public ActionResult Login(TMall.Models.UserLoginModel loginModel,bool? remember,string redirectURL) {
			if(!string.IsNullOrEmpty(redirectURL))
				ViewData["redirectURL"] = redirectURL;// 将重定向的url传递到视图里
			if (ModelState.IsValid) { //看每个域是否都有效
				if (!loginModel.Captcha.ToLower().Equals((string)Session["Captcha"])) {// 如果验证码不相等
					ModelState.AddModelError("Captcha", "请填写正确的验证码");
					return View();
				}
				if (TMall.Respository.Users.login(loginModel.Username, loginModel.Passwd)) {// 登录成功
					// 将登录信息写到Session中
					Session.Add("username",loginModel.Username);

					bool re = remember ?? false;
					if (re) {// 如果勾选了记住密码, 就保存7天cookie
						HttpCookie cookie = new HttpCookie("authorizeUser");
						cookie.Expires = DateTime.Now.Add(new TimeSpan(7, 0, 0, 0));// 设置过期时间为7天后
						cookie.Values.Add("username", loginModel.Username);
						cookie.Values.Add("passwd", loginModel.Passwd);
						Response.AppendCookie(cookie);// 将cookie增加到response中
					}

					if (string.IsNullOrEmpty(redirectURL)) return RedirectToAction("Index", "Home");// 如果重定向的字符串不合法,就回主页
					return Redirect(redirectURL);// 重定向到那个字符串
				}
				ModelState.AddModelError("Passwd", "用户名密码错误");
				return View();//将已经填写的错误信息一并返回
			}
			return View();
		}

		// 登出
		[HttpGet]
		public ActionResult Logout() {
			Session.Remove("username");    // 清除登录状态
			HttpCookie cookie = new HttpCookie("authorizeUser");
			cookie.Expires = DateTime.Now.Add(new TimeSpan(-10));
			Response.AppendCookie(cookie); // 用过期的cookie覆盖掉原来的
			return RedirectToAction("Index", "Home");// 重定向到主页
		}


		// 获取个人中心的页面
		[HttpGet][MyUserFilter]
		public ActionResult Profiles() {
			string username = (string)Session["username"];
			Models.UserProfileModel user = Respository.Users.GetUsers(username);
			return View(user);
		}

		// 更新用户信息
		[HttpPost][MyUserFilter]
		public ActionResult Profiles(TMall.Models.UserProfileModel profileModel) {
			if (ModelState.IsValid) { //表单格式通过验证
				if (!profileModel.Captcha.ToLower().Equals((string)Session["Captcha"])) {//验证码不相等
					return Json(new { result = "fail", data = "验证码错误,请填写正确的验证码!" });
				}
				if (Respository.Users.UpdateUsers(profileModel)) {
					return Json(new { result = "success" });
				}
				else return Json(new { result="fail",data="数据库操作错误!"});
			} else {
				// 获取表中的错误信息, 返回
				List<string> errorList = new List<string>();
				List<string> keys = ModelState.Keys.ToList();
				foreach(var key in keys) {
					var errors = ModelState[key].Errors.ToList();
					foreach(var error in errors) {
						errorList.Add(error.ErrorMessage);
					}
				}
				// 更新失败将信息返回
				return Json(new { result = "fail", data = errorList });
			}
		}

		// 用户收藏的功能
		[HttpGet][MyUserFilter]
		public ActionResult Collect(int? item_id,string op) {
			string username = (string)Session["username"];
			if (string.IsNullOrEmpty(op) || !item_id.HasValue) {//如果参数不全,就是返回列表
				return Json(Respository.Users.GetCollects(username), JsonRequestBehavior.AllowGet);
			}
			if (op.Equals("add")) {
				return Json(Respository.Users.AddCollect(username, item_id.Value), JsonRequestBehavior.AllowGet);
			} else if (op.Equals("del")) {
				return Json(Respository.Users.DelCollect(username, item_id.Value), JsonRequestBehavior.AllowGet);
			} else {
				return Json(Respository.Users.JudgeCollect(username, item_id.Value), JsonRequestBehavior.AllowGet);
			}
		}

		// 购物车的功能
		[HttpGet][MyUserFilter]
		public ActionResult Cart(string op, int? item_id,int? num) {
			string username = (string)Session["username"];
			int id = item_id ?? -1;
			int number = num ?? 1;
			if (!string.IsNullOrEmpty(op)) {
				if (op.Equals("list")) { // list
					return Json(Respository.Users.GetCarts(username), JsonRequestBehavior.AllowGet);
				} else {// del, set,add
					if (id == -1) return Json("商品编号参数错误!", JsonRequestBehavior.AllowGet);
					if (op.Equals("del")) {
						return Json(Respository.Users.DelCart(username, id), JsonRequestBehavior.AllowGet);
					} else {//set or add
						if (number <= 0) return Json("商品个数不合法!", JsonRequestBehavior.AllowGet);
						if (Respository.Users.JudgeCart(username, id)) { // 数据库已经存在了
							if (op.Equals("add")) {
								return Json(Respository.Users.UpdateCartAdd(username,id,number),JsonRequestBehavior.AllowGet);
							} else { //set
								return Json(Respository.Users.UpdateCartSet(username, id, number), JsonRequestBehavior.AllowGet);
							}
						} else { // 需要在数据库新建
							return Json(Respository.Users.AddCart(username, id, number), JsonRequestBehavior.AllowGet);
						}
					}
				}
			}
			return Json("操作指令不合法!!", JsonRequestBehavior.AllowGet);
		}

		// 订单的功能
		[HttpGet][MyUserFilter]
		public ActionResult Orders(string op, int? item_id,int? order_id) {
			string username = (string)Session["username"];
			if (!string.IsNullOrEmpty(op)) {
				if (op.Equals("list")) {//获取订单列表
					return Json(Respository.Users.GetOrders(username), JsonRequestBehavior.AllowGet);
				}
				else if (op.Equals("add")) { // 新建订单
					if (!item_id.HasValue) return Json("参数错误!", JsonRequestBehavior.AllowGet);
					// 获取商品在购物车里的数量
					int number = Respository.Users.GetNumberInCart(username,item_id.Value);
					// 将这个商品在用户购物车删除
					Respository.Users.DelCart(username, item_id.Value);
					// 获取商品的价格
					double price = Respository.Users.GetItemPrice(item_id.Value);
					// 新建订单, 并且将这个订单返回
					return Json(Respository.Users.AddOrders(username, item_id.Value, number, price), JsonRequestBehavior.AllowGet);
				}
				else if (op.Equals("pay")) { // 支付
					if(!order_id.HasValue) return Json("参数错误!", JsonRequestBehavior.AllowGet);
					return Json(Respository.Users.ChangeOrderStatus(order_id.Value, "已支付"), JsonRequestBehavior.AllowGet);
				}
			}
			return Json("操作指令不合法!!", JsonRequestBehavior.AllowGet);
		}
	}
}