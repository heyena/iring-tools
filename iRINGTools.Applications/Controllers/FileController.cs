using System;
using System.Collections.Generic;
using System.Web.Mvc;
using iRINGTools.Web.Helpers;
using iRINGTools.Web.Models;
using log4net;
using org.iringtools.library;
using System.Web;
using System.Collections.Specialized;
using System.Configuration;

namespace org.iringtools.web.controllers
{
    public class FileController : BaseController
    {


        private static readonly ILog _logger = LogManager.GetLogger(typeof(FileController));
        private FileRepository _repository;
        private ServiceSettings _settings = null;
        private CustomError _CustomError = null;
        private CustomErrorLog _CustomErrorLog = null;
        public FileController() : this(new FileRepository()) { }

        public FileController(FileRepository repository)
          : base()
        {

            NameValueCollection settings = ConfigurationManager.AppSettings;
            _settings = new ServiceSettings();
            _settings.AppendSettings(settings);
            _repository = repository;
          _repository.AuthHeaders = _authHeaders;
        }

        
        
        //
        // GET: /File/

        public ActionResult Index()
        {
            return View();
        }   

        ///Done by: Ganesh Bisht: Polaris 
        /// <summary>
        ///  Used for downloading the file
        /// </summary>
        /// <param name="scope"> scope name</param>
        /// <param name="application"> application name</param>
        /// <param name="file"> file to be downloaded</param>
        /// <returns>action result</returns>
        public ActionResult Export(string scope, string application, string file)
        {
            try
            {
                
                string filename = file.Substring(0,file.LastIndexOf('.'));
                string ext = file.Substring(file.LastIndexOf('.') + 1);

                byte[] bytes = _repository.getFile(scope, application, filename, ext);
                return File(bytes, "application/octet-stream", string.Format("{0}.{1}", filename, ext));
            }
            catch (Exception ioEx)
            {
                _logger.Error(ioEx.Message);
                _CustomErrorLog = new CustomErrorLog();
                _CustomError = _CustomErrorLog.customErrorLogger(ErrorMessages.errUIExportFile, ioEx, _logger);
                return Json(new { success = false, message = "[ Message Id " + _CustomError.msgId + "] - " + _CustomError.errMessage, stackTraceDescription = _CustomError.stackTraceDescription }, JsonRequestBehavior.AllowGet);

                //throw ioEx;
            }
        }

        ///Done by: Ganesh Bisht: Polaris 
        /// <summary>
        /// Used for upload any generic file for particular scope and application
        /// </summary>
        /// <param name="form"></param>
        /// <returns>json</returns>
        public JsonResult Upload(FormCollection form)
        {
            try
            {
                
               // string datalayer = "org.iringtools.adapter.datalayer.SpreadsheetDatalayer, SpreadsheetDatalayer";
                string savedFileName = string.Empty;

                HttpFileCollectionBase files = Request.Files;

                foreach (string file in files)
                {
                    string filename = string.Empty;
                    HttpPostedFileBase hpf = files[file] as HttpPostedFileBase;
                    if (hpf.ContentLength == 0)
                        continue;
                    filename = hpf.FileName;
                    if (filename.Contains("\\"))
                    {
                        filename = filename.Substring(filename.LastIndexOf('\\') + 1);
                    }
                    string fileLocation = string.Format(@"{0}{1}.{2}.{3}", _settings["AppDataPath"], form["Scope"], form["Application"], filename);
                    Response rs = _repository.PostFile(form["Scope"], form["Application"], hpf.InputStream, fileLocation);
                    
                    //break;
                }
            }
            catch (Exception ex)
            {

                //return new JsonResult()
                //{
                //    ContentType = "text/html",
                //    Data = PrepareErrorResponse(ex)
                //};

                _logger.Error(ex.Message);
                _CustomErrorLog = new CustomErrorLog();
                _CustomError = _CustomErrorLog.customErrorLogger(ErrorMessages.errUIUploadFile, ex, _logger);
                return Json(new { success = false, message = "[ Message Id " + _CustomError.msgId + "] - " + _CustomError.errMessage, stackTraceDescription = _CustomError.stackTraceDescription }, JsonRequestBehavior.AllowGet);

            }
            return new JsonResult()
            {
                ContentType = "text/html",
                Data = new { success = true }
            };
        }

        ///Done by: Ganesh Bisht: Polaris 
        /// <summary>
        /// This is used to get all the files available for download under specific scope and application
        /// </summary>
        /// <param name="scope">scope name</param>
        /// <param name="application">application name</param>
        /// <returns>json</returns>
        public JsonResult GetFiles(string scope, string application)
        {

            try
            {
                List<Files> files = _repository.getDownloadedList(scope, application);
                return Json(files, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ioEx)
            {
                _logger.Error(ioEx.Message);
                _CustomErrorLog = new CustomErrorLog();
                _CustomError = _CustomErrorLog.customErrorLogger(ErrorMessages.errUIGetFile, ioEx, _logger);
                return Json(new { success = false, message = "[ Message Id " + _CustomError.msgId + "] - " + _CustomError.errMessage, stackTraceDescription = _CustomError.stackTraceDescription }, JsonRequestBehavior.AllowGet);
               // throw ioEx;
            }                     
            
        }            


        /// <summary>
        /// Handles the error message
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        private Response PrepareErrorResponse(Exception ex)
        {
            Response response = new Response();
            response.Level = StatusLevel.Error;
            response.Messages = new Messages();
            response.Messages.Add(ex.Message);
            response.Messages.Add(ex.StackTrace);
            return response;
        }
    }
}
