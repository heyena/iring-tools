using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Ninject;

namespace NinjectMVC.DataLayer
{  
  [HandleError]
  public class DataController: Controller
  {
    private IDataRepository _repository = null;

    public DataController()
      : this(new DataRepository())
    {
    }

    public DataController(IDataRepository repository)
    {
      _repository = repository;
    }

    public JsonResult Index()
    {
      return Json(new { 
        Sucess = true, 
        Message = _repository.GetMessage() 
      }, JsonRequestBehavior.AllowGet);
    }
  }
}
