using System;
using System.Web.Mvc;
using org.iringtools.web.Models;
using org.iringtools.web.controllers;
using org.iringtools.library;

namespace org.iringtools.web.Controllers
{
    public class FacadeController : Controller
    {
        private IFacadeRepository _facadeRepository = null;

        public FacadeController() : this(new FacadeRepository()) { }

        public FacadeController(IFacadeRepository repository)
        {
            _facadeRepository = repository;
        }

        public JsonResult RefreshFacade(FormCollection form)
        {
            var result = new JsonResult();
            try
            {
                string[] vars = form["scope"].Split('/');
                string scope = vars[0];
                string app = vars[1];
                string graph = vars[vars.Length -1];
                Response resp = _facadeRepository.RefreshGraph(scope, app, graph);
            }
            catch
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

    }
}
