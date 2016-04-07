﻿using System.Web.Mvc;
using Gear.Infrastructure.Web.Controllers;
using ReportMS.DataTransferObjects.Dtos;
using ReportMS.Web.Client.Tenancy;
using ReportMS.Web.Client.Membership;
using ReportMS.Web.Client.Roles;

namespace ReportMS.Web
{
    /// <summary>
    /// Controller 基类
    /// </summary>
    public abstract class BaseController : GearController
    {
        #region Private fields

        private TenantDto _tenant;
        private RoleDto _roleOfTenant;
        private bool? _isAdmin;

        #endregion

        #region Public Properties

        /// <summary>
        /// 用户是否是管理员（并非系统管理者）.
        /// </summary>
        public bool IsAdmin
        {
            get
            {
                if (this._isAdmin.HasValue)
                    return this._isAdmin.Value;

                this._isAdmin = this.IsAdminOfCurrentUser();
                return this._isAdmin.Value;
            }
        }

        /// <summary>
        /// 获取当前租户信息.
        /// 若不存在租户或租户信息没有，返回 null
        /// </summary>
        public TenantDto Tenant
        {
            get { return this._tenant ?? (this._tenant = this.GetTenant()); }
        }

        /// <summary>
        /// 获取登录的用户在当前租户中的角色的详细信息（多租户）。
        /// 若不存在租户或租户信息没有，返回 null
        /// </summary>
        public RoleDto RoleOfTenant
        {
            get { return this._roleOfTenant ?? (this._roleOfTenant = this.GetRoleOfTenant()); }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 跳转到主页
        /// </summary>
        /// <returns>ActionResult</returns>
        public ActionResult RedirectToHome()
        {
            return RedirectToAction("Index", "Home", new {area = string.Empty});
        }

        /// <summary>
        /// Json 序列化，基于 Newtonsoft.Json 框架
        /// </summary>
        /// <param name="isSuccess">要传递的状态 status: success/fail</param>
        /// <param name="message">要传递的消息 message</param>
        /// <returns>ActionResult</returns>
        public ActionResult Json(bool isSuccess, string message = null)
        {
            return this.Json(isSuccess, message, JsonRequestBehavior.DenyGet);
        }

        /// <summary>
        /// Json 序列化，基于 Newtonsoft.Json 框架
        /// </summary>
        /// <param name="isSuccess">要传递的状态 status: success/fail</param>
        /// <param name="message">要传递的消息 message</param>
        /// <param name="behavior">请求 Json 序列化的行为</param>
        /// <returns>ActionResult</returns>
        public ActionResult Json(bool isSuccess, string message, JsonRequestBehavior behavior)
        {
            return this.Json(isSuccess, null, message, behavior);
        }

        /// <summary>
        /// Json 序列化，基于 Newtonsoft.Json 框架
        /// </summary>
        /// <param name="isSuccess">要传递的状态 status: success/fail</param>
        /// <param name="data">要序列化的数据 data</param>
        /// <param name="message">要传递的消息 message</param>
        /// <returns>ActionResult</returns>
        public ActionResult Json(bool isSuccess, object data, string message = null)
        {
            return this.Json(isSuccess, data, message, JsonRequestBehavior.DenyGet);
        }

        /// <summary>
        /// Json 序列化，基于 Newtonsoft.Json 框架
        /// </summary>
        /// <param name="isSuccess">要传递的状态 status: success/fail</param>
        /// <param name="data">要序列化的数据 data</param>
        /// <param name="message">要传递的消息 message</param>
        /// <param name="behavior">请求 Json 序列化的行为</param>
        /// <returns>ActionResult</returns>
        public ActionResult Json(bool isSuccess, object data, string message, JsonRequestBehavior behavior)
        {
            var status = isSuccess ? "success" : "fail";
            return this.NewtonsoftJson(new {status, data, message}, behavior);
        }

        #endregion

        #region protected override events

        // 重写, 登录验证
        protected override void OnAuthorization(AuthorizationContext filterContext)
        {
            // 1, Authentication
            if (!this.Authenticate(filterContext))
            {
                filterContext.Result = new HttpUnauthorizedResult();
                return;
            }

            // 2, Manager Or Administrator 
            // allow the system administrators to visit all.
            if (this.IsAdmin || this.IsAdministrator)
                return;

            // 3, AllowAuthenticated
            if (this.IsAllowAuthenticated(filterContext.ActionDescriptor))
                return;

            // 4, Authorization
            //  a, no tenant or not role in current tenant
            if (this.RoleOfTenant == null)
            {
                filterContext.Result = this.RedirectToPermission();
                return;
            }

            //  b, exist valid tenant
            //      b1, the role of the user in current tenant. (Only one role in a tenant)
            //      b2, the permissions of the role
            //      b3, Is limit action ?
            //var action = this.RouteData.Values["action"] as string;
            //var controller = this.RouteData.Values["controller"] as string;
            //var area = this.RouteData.DataTokens["area"] as string;

            //var hasPermission = RoleManager.Instance.HasPermissionOfRole(this.RoleOfTenant.ID, area, controller, action);
            //if (!hasPermission)
            //{
            //    filterContext.Result = this.RedirectToPermission();
            //}
        }

        // 重写，登录 Log 记录
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // async log
        }

        // 重写，异常处理
        protected override void OnException(ExceptionContext filterContext)
        {
            // log exception
        }

        #endregion

        #region Private Methods

        private TenantDto GetTenant()
        {
            var tenantOwer = new TenantOwner(this.RouteData);
            return tenantOwer.GetCurrentTenant();
        }

        private RoleDto GetRoleOfTenant()
        {
            if (this.Tenant == null)
                return null;

            var userId = this.LoginUser.NameIdentifier;
            if (!userId.HasValue)
                return null;

            var tenantId = this.Tenant.ID;
            return UserManager.Instance.GetRoleOfTenant(userId.Value, tenantId);
        }

        private bool IsAdminOfCurrentUser()
        {
            var userId = this.LoginUser.NameIdentifier;
            if (!userId.HasValue)
                return false;

            return UserManager.Instance.IsAdmin(userId.Value);
        }

        private ActionResult RedirectToPermission()
        {
            if (this.ControllerContext.HttpContext.Request.IsAjaxRequest())
                return Json(false, "You have not permission to access this page.");
            if (this.ControllerContext.IsChildAction)
                return PartialView("Permission");
            return View("Permission");
        }

        #endregion
    }
}