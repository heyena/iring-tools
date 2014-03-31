using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;
using System.Configuration;
using System.Collections.Specialized;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Xml;

using org.iringtools.library;
using org.iringtools.utility;
using org.iringtools.mapping;

using iRINGTools.Web.Helpers;
using iRINGTools.Web.Models;
using org.ids_adi.qmxf;
using VDS.RDF;
using System.Text;
using log4net;
using System.Runtime.Serialization.Json;

namespace org.iringtools.web.controllers
{
    public class GridManagerController : BaseController
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(GridManagerController));
        private GridRepository _repository { get; set; }
        //private Grid dataGrid;		
        private StoreViewModel dataGrid;
        private CustomError _CustomError = null;
        private CustomErrorLog _CustomErrorLog = null;
        public GridManagerController() : this(new GridRepository()) { }

        public GridManagerController(GridRepository repository)
        {
            _repository = repository;
            _repository.AuthHeaders = _authHeaders;
        }

        public ActionResult Pages(FormCollection form)
        {
            try
            {
                _repository.Session = Session;
                JsonContainer<Grid> container = new JsonContainer<Grid>();

                string keyName = string.Format("{0}.{1}.{2}", form["scope"], form["app"], form["graph"]);
                DataFilter filter = (DataFilter)Session[keyName];
                if (filter == null)
                {
                    _repository.GetFilterFile(ref filter, keyName);
                }

                if (filter == null)
                {
                    if (form["filter"] != null)
                    {
                        filter = new DataFilter();
                        Expression expression = new Expression();
                        string[] filterVal = form["filter"].Split(':');
                        expression.Values = new Values() { filterVal[1] };
                        expression.PropertyName = filterVal[0];
                        expression.RelationalOperator = (RelationalOperator)Enum.Parse(typeof(RelationalOperator), "EqualTo");
                        filter.Expressions.Add(expression);
                        dataGrid = _repository.GetGrid(form["scope"], form["app"], form["graph"], filter, form["start"], form["limit"]);
                    }
                    else
                        dataGrid = _repository.GetGrid(form["scope"], form["app"], form["graph"], form["filter"], form["sort"], form["dir"], form["start"], form["limit"]);
                }

                else if (filter != null && form["filter"] != null)
                {

                    string[] filterVal = form["filter"].Split(':');
                    filter.Expressions[0].Values[0] = filterVal[1];
                    filter.Expressions[0].PropertyName = filterVal[0];
                    filter.Expressions[0].RelationalOperator = (RelationalOperator)Enum.Parse(typeof(RelationalOperator), "EqualTo");
                    dataGrid = _repository.GetGrid(form["scope"], form["app"], form["graph"], filter, form["start"], form["limit"]);
                }
                else
                {
                    dataGrid = _repository.GetGrid(form["scope"], form["app"], form["graph"], filter, form["start"], form["limit"]);

                }

                string response = _repository.GetResponse();
                if (response != "")
                {
                    return Json(new { success = false, message = response }, JsonRequestBehavior.AllowGet);
                }

                return Json(dataGrid, JsonRequestBehavior.AllowGet);
            }

            catch (Exception e)
            {
                _CustomErrorLog = new CustomErrorLog();
                _CustomError = _CustomErrorLog.customErrorLogger(ErrorMessages.errUIGridPages, e, _logger);
                return Json(new { success = false, message = "[ Message Id " + _CustomError.msgId + "] - " + _CustomError.errMessage, stackTraceDescription = _CustomError.stackTraceDescription }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
