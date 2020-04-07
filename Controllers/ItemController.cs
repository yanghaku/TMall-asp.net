using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Webdiyer.WebControls.Mvc;


namespace TMall.Controllers {
    /*
	 * 
 /Item
  |-- /List      get:返回商品列表, 两个参数: category_id,page: 商品类型,页数
  |-- /Detail    get:返回商品的详细页面, 一个参数: item_id
  |-- /Edit      get:有参数时是修改物品, 无参数时是增加新物品(需要管理员权限)
                 post: 保存修改/增加的物品 (需要管理员权限)
  |
  |(以下功能都是在Detail页面中进行的js异步交互, 没有单独的页面)
  |-- /Category  get: 获取商品分类的列表
  |-- /Delete    get: 删除某个物品 (需要管理员权限)
  |-- /Recommend get: 获取推荐的物品列表(返回json)
  |-- /Comment   get: 获取评论列表(返回json), 一个参数: item_id
                 post: 增加评论, 两个参数: item_id, comment
	 */
    public class ItemController : Controller{
        public const int PageSize = 4; // 设置每页展示的大小

        // 获取商品列表
        [HttpGet]
        public ActionResult List(int? category_id, int? page) {
            int ca_id = category_id ?? 1;
            int pa_id = page ?? 1;
            int now = (pa_id - 1) * PageSize; // 当前页面的起始范围
            int totalCount;// 总个数
            PagedList<TMall.Models.ItemModel> pagedList = TMall.Respository.Item.
                GetItems(ca_id, now, PageSize, out totalCount).ToPagedList(now, PageSize);
            pagedList.TotalItemCount = totalCount;
            pagedList.CurrentPageIndex = pa_id;
            ViewData["totalCount"] = totalCount;
            return View(pagedList);
        }

        // 查看商品详细信息, 需要登录权限
        [HttpGet][MyUserFilter]
        public ActionResult Detail(int? item_id) {
            if (!item_id.HasValue) return new HttpStatusCodeResult(404);//参数不合法,跳转到404页面
            TMall.Models.ItemModel item = TMall.Respository.Item.GetItem(item_id.Value);
            if(item==null) return new HttpStatusCodeResult(404);
            return View(item);
        }

        // 编辑商品信息(或者增加新商品), 获取表单页面 , 需要管理员权限
        [HttpGet][MyAdminFilter]
        public ActionResult Edit(int? item_id) {
            if (item_id.HasValue) {
                TMall.Models.ItemModel item = TMall.Respository.Item.GetItem(item_id.Value);
                return View(item);
            }
            return View();
        }

        // 编辑商品信息(或者增加商品), 提交页面, 需要管理员权限
        [HttpPost][MyAdminFilter]
        public ActionResult Edit(TMall.Models.ItemModel itemModel) {
            if (ModelState.IsValid) {//查看表单是不是有效
                itemModel.LastUpdateTime = DateTime.Now;
                if (itemModel.ItemId < 0) {//新增
                    int id = TMall.Respository.Item.AddItem(itemModel);
                    if (id > 0) return Redirect($"/Item/Detail?item_id={id}");//成功后转移到详情页
                } else {// update
                    if (TMall.Respository.Item.UpdateItem(itemModel))
                        return Redirect($"/Item/Detail?item_id={itemModel.ItemId}");//成功后转移到详情页
                }
            }
            return View();
        }

        // 删除商品, 需要管理员权限
        [HttpGet][MyAdminFilter]
        public ActionResult Delete(int? item_id) {
            if (item_id.HasValue) {
                int id = item_id.Value;
                if (TMall.Respository.Item.DeleteItem(id)) {
                    return Json("Delete success!",JsonRequestBehavior.AllowGet);
                } else return Json("Delete fail!",JsonRequestBehavior.AllowGet);
            }
            return Json("404 Not Found!",JsonRequestBehavior.AllowGet);
        }

        // 获取目录
        [HttpGet]
        public ActionResult Category() {
            return Json(TMall.Respository.Item.GetItemCategories(),JsonRequestBehavior.AllowGet);
        }

        // 获取某个商品的所有评论
        [HttpGet][MyUserFilter]
        public ActionResult Comment(int? item_id) {
            if (item_id.HasValue) {
                return Json(TMall.Respository.Item.GetItemComments(item_id.Value), JsonRequestBehavior.AllowGet);
            }
            // 如果item_id不合法
            return Json("404 Item Not Found", JsonRequestBehavior.AllowGet);
        }

        
        // 评论某个商品
        [HttpPost][MyUserFilter]
        public ActionResult Comment(TMall.Models.ItemCommentModel itemCommentModel) {
            // 这个是使用json交互的, 所以返回一个固定格式的信息
            if (ModelState.IsValid) {
                itemCommentModel.Username = (string)Session["username"];
                if (TMall.Respository.Item.AddItemComment(itemCommentModel)) {
                    return Json("success");
                }
            }
            return Json("fail");// 返回失败的json信息
        }


        // 返回推荐的商品列表
        [HttpGet][MyUserFilter]
        public ActionResult Recommend() {
            string username = (string)Session["username"];
            return Json(Respository.Recommend.GetRecommend(username), JsonRequestBehavior.AllowGet);
        }

    }
}