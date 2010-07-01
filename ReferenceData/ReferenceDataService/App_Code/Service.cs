// Copyright (c) 2009, ids-adi.org /////////////////////////////////////////////
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using org.iringtools.utility;
using org.iringtools.utility.Loggers;
using org.ids_adi.qmxf;
using org.w3.sparql_results;
using org.ids_adi.iring.referenceData;
using org.iringtools.library;
using log4net;
using System.Configuration;
using System.Collections.Specialized;
using System.ServiceModel.Activation;

namespace org.ids_adi.iring.referenceData
{
    // NOTE: If you change the class name "Service" here, you must also update the reference to "Service" in Web.config and in the associated .svc file.
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
    public class Service : IService
    {
      private static readonly log4net.ILog log = LogManager.GetLogger(typeof(Service));
      private ReferenceDataProvider _referenceDataServiceProvider = null;
       
        public Service()
        {
          NameValueCollection settings = ConfigurationManager.AppSettings;
          _referenceDataServiceProvider = new ReferenceDataProvider(settings);            
        }

        public List<Repository> GetRepositories()
        {
          return _referenceDataServiceProvider.GetRepositories();
        }

        public List<Entity> Find(string query)
        {
          return _referenceDataServiceProvider.Find(query);
        }

        public RefDataEntities Search(string query)
        {
          return _referenceDataServiceProvider.Search(query);
        }

        public RefDataEntities SearchPage(string query, string page)
        {
          return _referenceDataServiceProvider.SearchPage(query, page);
        }

        public RefDataEntities SearchReset(string query)
        {
          return _referenceDataServiceProvider.SearchReset(query);
        }

        public RefDataEntities SearchPageReset(string query, string page)
        {
          return _referenceDataServiceProvider.SearchPageReset(query, page);
        }       

        public string GetClassLabel(string id)
        {
          return _referenceDataServiceProvider.GetClassLabel(id);
        }

        public QMXF GetClass(string id)
        {
          return _referenceDataServiceProvider.GetClass(id);
        }

        public List<Entity> GetSuperClasses(string id)
        {
          return _referenceDataServiceProvider.GetSuperClasses(id);
        }

        public List<Entity> GetAllSuperClasses(string id)
        {
            return _referenceDataServiceProvider.GetAllSuperClasses(id);
        }

        public List<Entity> GetSubClasses(string id)
        {
          return _referenceDataServiceProvider.GetSubClasses(id);
        }

        public List<Entity> GetClassTemplates(string id)
        {
          return _referenceDataServiceProvider.GetClassTemplates(id);
        }
     
        public QMXF GetTemplate(string id)
        {
          return _referenceDataServiceProvider.GetTemplate(id);
        }
       
        public Response PostTemplate(QMXF qmxf)
        {
          return _referenceDataServiceProvider.PostTemplate(qmxf);                
        }

        public Response PostClass(QMXF qmxf)
        {
          return _referenceDataServiceProvider.PostClass(qmxf);
        }

    }
}
