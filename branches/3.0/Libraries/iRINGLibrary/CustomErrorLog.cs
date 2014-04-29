using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using log4net;
using System.Text;
using System.Collections;
using System.Runtime.Serialization;
using System.Xml.Linq;
using System.ServiceModel.Web;
using org.iringtools.utility;
using System.Net;

namespace org.iringtools.library
{
    public class CustomErrorLog
    {

        public string getJsonErrsrtring(List<CustomError> objList)
        {
            System.Web.Script.Serialization.JavaScriptSerializer jsonMsg = new System.Web.Script.Serialization.JavaScriptSerializer();
            return jsonMsg.Serialize(objList).ToString();
            //return strMsg;
        }
        

       
        public CustomError customErrorLogger(string stringMsg, Exception ex, ILog errorLogger)
        {

            try
            {
                CustomError objCustomError = new CustomError();
                Guid msgId = Guid.NewGuid();
                string logErrorMsg = string.Empty;
               // logErrorMsg = stringMsg + Environment.NewLine + "Error Reason :" + ex.Message.ToString();
                logErrorMsg = stringMsg;// +Environment.NewLine + "Error Reason :" + ex.Message.ToString();
                objCustomError.msgId = msgId;
                objCustomError.errMessage = logErrorMsg;

                StringBuilder sb = new StringBuilder();
                CreateExceptionString(sb, ex, string.Empty);
                objCustomError.stackTraceDescription = sb.ToString();

                //Logging
                log4net.GlobalContext.Properties["fname"] = msgId.ToString();
                log4net.Config.XmlConfigurator.Configure();
                errorLogger.Error(logErrorMsg);
                errorLogger.Error(objCustomError.stackTraceDescription);
                //return objList;
                return objCustomError;
            }
            catch (Exception e)
            {
                throw;
            }
            finally
            {
                errorLogger = null;
            }


        }

        public string WriteExceptionDetails(Exception exception)
        {

            try
            {
                StringBuilder sbError = new StringBuilder();
                sbError.Clear();
                var properties = exception.GetType().GetProperties();

                var fields = properties.Select(property => new
                                 {
                                     Name = property.Name,
                                     Value = property.GetValue(exception, null)
                                 })
                                 .Select(x => String.Format(
                                     "{0} = {1}",
                                     x.Name,
                                     x.Value != null ? x.Value.ToString() : string.Empty

                                 ));

                foreach (var s in fields)
                {
                    sbError.Append(s.ToString());
                }


                ////Base exception

                //var m = exception.GetBaseException().GetType().GetProperties();
                //var baseField = properties.Select(baseProperty => new
                //{
                //    Name = baseProperty.Name,
                //    Value = baseProperty.GetValue(exception, null)
                //})
                //                  .Select(x => String.Format(
                //                      "{0} = {1}",
                //                      x.Name,
                //                      x.Value != null ? x.Value.ToString() : "null"
                //                         ));
                //foreach (var s in baseField)
                //{
                //    sbError.Append(s.ToString());
                //}


                // return String.Join(" ", fields);
                //return  fields.ToString();

                return sbError.ToString();
            }
            catch (Exception e)
            {
                return null;
            }



        }


        private void CreateExceptionString(StringBuilder sb, Exception e, string indent)
        {
            if (indent == null)
            {
                indent = string.Empty;
            }
            else if (indent.Length > 0)
            {
                sb.AppendFormat("{0}Inner ", indent);
            }

            sb.AppendFormat("Exception Found:\n{0}Type: {1}", indent, e.GetType().FullName);
            sb.AppendFormat("\n{0}Message: {1}", indent, e.Message);
            sb.AppendFormat("\n{0}Source: {1}", indent, e.Source);
            sb.AppendFormat("\n{0}Stacktrace: {1}", indent, e.StackTrace);

            if (e.InnerException != null)
            {
                sb.Append("\n");
                CreateExceptionString(sb, e.InnerException, indent + "  ");
            }

          
        }

        public CustomError getErrorResponse(string response)
        {
            CustomError objCustomError = new CustomError();
            string errMessage = string.Empty;
            string stackTrace = string.Empty;
            XDocument xd = XDocument.Parse(response);

            IEnumerable<XElement> childElements = from el in xd.Root.Elements()
                                                  select el;
            foreach (var s in childElements)
            {
                if (s.Name.LocalName.Equals("messages"))
                    objCustomError.errMessage = s.Value;
                if (s.Name.LocalName.Equals("status_text"))
                    objCustomError.stackTraceDescription = s.Value;
            }

            return objCustomError;
        }


        public void throwJsonResponse(CustomError response)
        {
            string jsonMsg = Utility.SerializeJson(response, true);
            OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
            context.StatusCode = HttpStatusCode.InternalServerError;
            context.StatusDescription = jsonMsg;
            HttpContext.Current.Response.Write(context.StatusDescription.ToString());

        }


       
    }

 


}