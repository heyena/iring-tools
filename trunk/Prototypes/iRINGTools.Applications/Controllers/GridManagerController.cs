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

namespace org.iringtools.web.controllers
{
	public class GridManagerController : BaseController
	{
		private static readonly ILog _logger = LogManager.GetLogger(typeof(GridManagerController));
		private IGridRepository _repository { get; set; }
		private Grid pageDtoGrid;		

		public GridManagerController() : this(new GridRepository()) { }

		public GridManagerController(IGridRepository repository)
		{
			_repository = repository;			
		}

		public ActionResult Pages(FormCollection form)
		{
			_logger.Error("GridManagerController -- Pages");
			JsonContainer<Grid> container = new JsonContainer<Grid>();
			pageDtoGrid = _repository.getGrid(form["scope"], form["app"], form["graph"], form["filter"], form["sort"], form["dir"], form["start"], form["limit"]);

			string response = _repository.getResponse();
			if (response != "")
			{
				return Json(new { success = false } + response, JsonRequestBehavior.AllowGet);
			}

			return Json(pageDtoGrid, JsonRequestBehavior.AllowGet);
		}		
	}
}
