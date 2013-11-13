// Copyright (c) 2010, iringtools.org //////////////////////////////////////////
// All rights reserved.
//------------------------------------------------------------------------------
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//     * Redistributions of source code must retain the above copyright
//       notice, this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above copyright
//       notice, this list of conditions and the following disclaimer in the
//       documentation and/or other materials provided with the distribution.
//     * Neither the name of the ids-adi.org nor the
//       names of its contributors may be used to endorse or promote products
//       derived from this software without specific prior written permission.
//------------------------------------------------------------------------------
// THIS SOFTWARE IS PROVIDED BY ids-adi.org ''AS IS'' AND ANY
// EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL ids-adi.org BE LIABLE FOR ANY
// DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
////////////////////////////////////////////////////////////////////////////////

using System.ComponentModel;
using System.Configuration;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Xml.Linq;
using log4net;
using org.iringtools.library;
using org.iringtools.utility;
using org.iringtools.adapter;
using System.Collections.Specialized;
using System;
using System.IO;
using System.Net;
using System.Web.Script.Serialization;
using System.Web;
using System.ServiceModel.Channels;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Collections.Generic;
using net.java.dev.wadl;
using System.Collections;

namespace org.iringtools.services
{
    [ServiceContract(Namespace = "http://www.iringtools.org/service")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
    public class TipGeneratorService
    {
        private HMCommonDataService _hmCommonService = null;

        public TipGeneratorService()
        {
            _hmCommonService = new HMCommonDataService();
        }

        [Description("Create Tip mapping form mapping.")]
        [WebGet(UriTemplate = "/{app}/{project}/{resource}")]
        public void GetGenerateTip(string app, string project, string resource)
        {
            _hmCommonService.GenerateTIP(project, app, resource);
        }

        #region Private Methods
        private void ExceptionHandler(Exception ex)
        {
            OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
            string statusText = string.Empty;

            if (ex is FileNotFoundException)
            {
                context.StatusCode = HttpStatusCode.NotFound;
            }
            else if (ex is UnauthorizedAccessException)
            {
                context.StatusCode = HttpStatusCode.Unauthorized;
            }
            else
            {
                context.StatusCode = HttpStatusCode.InternalServerError;

                if (ex is WebFaultException && ex.Data != null)
                {
                    foreach (DictionaryEntry entry in ex.Data)
                    {
                        statusText += ex.Data[entry.Key].ToString();
                    }
                }
            }

            if (string.IsNullOrEmpty(statusText))
            {
                statusText = ex.Source + ": " + ex.ToString();
            }

            HttpContext.Current.Response.ContentType = "text/html";
            HttpContext.Current.Response.Write(statusText);
        }

        private void FormatOutgoingMessage<T>(T graph, string format, bool useDataContractSerializer)
        {
            if (format.ToUpper() == "JSON")
            {
                string json = Utility.SerializeJson<T>(graph, useDataContractSerializer);

                HttpContext.Current.Response.ContentType = "application/json; charset=utf-8";
                HttpContext.Current.Response.Write(json);
            }
            else
            {
                string xml = Utility.Serialize<T>(graph, useDataContractSerializer);

                HttpContext.Current.Response.ContentType = "application/xml";
                HttpContext.Current.Response.Write(xml);
            }
        }
        #endregion
    }
}