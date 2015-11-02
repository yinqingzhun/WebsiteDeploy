using System.Web;
using System.Web.Mvc;
using WebDeploy.Utils;

namespace WebDeploy
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new GlobalHandleErrorAttribute());
        }
    }

    public class GlobalHandleErrorAttribute : HandleErrorAttribute
    {
        public override void OnException(ExceptionContext filterContext)
        {
            LogHelper.Error("系统捕捉到异常。",filterContext.Exception);
            base.OnException(filterContext);
        }
    }
}