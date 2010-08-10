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

using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using org.ids_adi.iring;
using org.ids_adi.qmxf;
using org.w3.sparql_results;
using org.iringtools.library;

namespace org.ids_adi.iring.referenceData
{
  [ServiceContract]
  public interface  IService
  {    
    /// <summary>
    ///get configured repositories
    /// </summary>
    [OperationContract]
    [WebGet(UriTemplate = "/repositories")]
    List<Repository> GetRepositories();

    #region Prototype Part8

    /// <summary>
    ///do a search
    /// </summary>
    [OperationContract]
    [WebGet(UriTemplate = "/search/{query}")]
    RefDataEntities Search(string query);

    /// <summary>
    ///do a search, ignoring cached results
    /// </summary>
    [OperationContract]
    [WebGet(UriTemplate = "/search/{query}/reset")]
    RefDataEntities SearchReset(string query);

    /// <summary>
    ///do a search and return a specific page
    /// </summary>
    [OperationContract]
    [WebGet(UriTemplate = "/search/{query}/{page}")]
    RefDataEntities SearchPage(string query, string page);

    /// <summary>
    ///do a search, ignoring cached results, and return a specific page
    /// </summary>
    [OperationContract]
    [WebGet(UriTemplate = "/search/{query}/{page}/reset")]
    RefDataEntities SearchPageReset(string query, string page);

    /// <summary>
    ///find a sepcific item by label
    /// </summary>
    [OperationContract]
    [WebGet(UriTemplate = "/find/{query}")]
    List<Entity> Find(string query);

    ///// <summary>
    /////return class details
    ///// </summary>
   // [XmlSerializerFormat]
    [OperationContract]
    [WebGet(UriTemplate = "/classes/{id}")]
    QMXF GetClass(string id);

    ///// <summary>
    /////return class details
    ///// </summary>
   // [XmlSerializerFormat]
    [OperationContract]
    [WebGet(UriTemplate = "/classes/{id}/label")]
    string GetClassLabel(string id);

    ///// <summary>
    /////return class details
    ///// </summary>
    [OperationContract]
    [WebGet(UriTemplate = "/classes/{id}/superclasses")]
    List<Entity> GetSuperClasses(string id);

    ///// <summary>
    /////return class details
    ///// </summary>
    [OperationContract]
    [WebGet(UriTemplate = "/classes/{id}/allsuperclasses")]
    List<Entity> GetAllSuperClasses(string id);

    ///// <summary>
    /////return class details
    ///// </summary>
    [OperationContract]
    [WebGet(UriTemplate = "/classes/{id}/subclasses")]
    List<Entity> GetSubClasses(string id);

    ///// <summary>
    ///// templates on a class
    ///// </summary>
    [OperationContract]
    [WebGet(UriTemplate = "/classes/{id}/templates")]
    List<Entity> GetClassTemplates(string id);

    ///// <summary>
    ///// get roles on a template
    ///// </summary>
  //  [XmlSerializerFormat]
    [OperationContract]
    [WebGet(UriTemplate = "/templates/{id}")]
    QMXF GetTemplate(string id);

    ///// <summary>
    /////Insert a Template into the Sandbox
    ///// </summary>
   // [XmlSerializerFormat]
    [OperationContract]
    [WebInvoke(UriTemplate = "/templates")]
    Response PostTemplate(QMXF template);

    ///// <summary>
    /////Insert a class into the Sandbox
    ///// </summary>
   // [XmlSerializerFormat]
    [OperationContract]
    [WebInvoke(UriTemplate = "/classes")]
    Response PostClass(QMXF @class);

    #endregion Prototype Part8

    #region Part8
    
    /// <summary>
    ///get part 8 template
    /// </summary>
    [OperationContract]
    [WebGet(UriTemplate = "/part8/template/{id}")]
    QMXF GetPart8Template(string id);

   /// <summary>
    ///get part 8 template Specialization
    /// </summary>
    [OperationContract]
    [WebGet(UriTemplate = "/part8/templatespec/{id}")]
    List<Specialization> GetPart8TemplateSpec(string id);
      
    /// <summary>
    ///get part 8 template Classification
    /// </summary>
    [OperationContract]
    [WebGet(UriTemplate = "/part8/templateclasif/{id}")]
    List<Classification> GetPart8TemplateClassif(string id);

    ///// <summary>
    /////Insert a Template into the Sandbox
    ///// </summary>
    // [XmlSerializerFormat]
    [OperationContract]
    [WebInvoke(UriTemplate = "/part8/templates")]
    Response PostPart8Template(QMXF template);

    /// <summary>
    ///get part 8 class
    /// </summary>
    [OperationContract]
    [WebGet(UriTemplate = "/part8/class/{id}")]
    QMXF GetPart8Class(string id);

    /// <summary>
    ///do a search
    /// </summary>
    [OperationContract]
    [WebGet(UriTemplate = "/part8/search/{query}")]
    RefDataEntities Part8Search(string query);

    [OperationContract]
    [WebInvoke(UriTemplate = "/part8/classes")]
    Response PostPart8Class(QMXF @class);

    #endregion
  }

}