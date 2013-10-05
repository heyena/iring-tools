using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
//using System.Workflow.Runtime;

namespace Commerce.MVC.Web.Controllers
{
  public static class ControllerExtensions
  {

    public static void SetErrorMessage(this Controller controller, string errorMessage)
    {
      controller.ViewBag.ErrorMessage = errorMessage;
    }

    public static string GetUserName(this Controller controller)
    {

      return GetUserName(controller.HttpContext);
    }

    public static string GetUserName(HttpContextBase context)
    {
      string result = Guid.NewGuid().ToString();

      if (context != null)
      {

        //if not, do we know who they are?
        if (context.User.Identity.IsAuthenticated)
        {
          result = context.User.Identity.Name;

          //do they have a cookie?
        }
        else if (context.Request.Cookies["viewer"] != null)
        {
          result = context.Request.Cookies["viewer"].Value;

        }

        //hate to do it this way, but Response.Cookies isn't implemented
        //in Controller.HttpContextBase.Response
        if (HttpContext.Current != null)
        {
          HttpContext.Current.Response.Cookies["viewer"].Value = result;
          HttpContext.Current.Response.Cookies["viewer"].Expires = DateTime.Now.AddMonths(12);
        }
      }


      return result;

    }

    //static WorkflowRuntime wfRuntime;
    //static object padlock;
    //public static WorkflowRuntime GetWorkflowRuntime(this Controller controller)
    //{
    //  if (wfRuntime == null)
    //  {
    //    lock (padlock)
    //    {
    //      wfRuntime = new WorkflowRuntime();
    //    }
    //  }
    //  return wfRuntime;
    //}

  }
}
