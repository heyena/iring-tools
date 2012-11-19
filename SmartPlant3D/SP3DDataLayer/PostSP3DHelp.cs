using System;
using System.Collections;
using System.Collections.Generic;
using log4net;
using org.iringtools.library;
using System.Linq;
using NHibernate;
using org.iringtools.adapter;

namespace iringtools.sdk.sp3ddatalayer
{
  public class PostSP3DHelp
  {
    public List<string> updateIdentifiers = new List<string>();
    public Dictionary<string, IDataObject> identiferNewDataObjectPairs = new Dictionary<string, IDataObject>();
    public Dictionary<string, IDataObject> identiferUpdateDataObjectPairs = new Dictionary<string, IDataObject>();
    public IList<IDataObject> updateDataObjects = new List<IDataObject>();
    public string objectName = string.Empty;
    public DataObject dataObject = null;
    public BusinessObject businessObject = null;
    private List<PostRelatedObjectIdentifier> postRelatedObjectHelps = null;
    public static readonly ILog _logger = LogManager.GetLogger(typeof(PostSP3DHelp));

    public void CreateDataObjectDictionary()
    {
      string identifier = string.Empty;

      foreach (IDataObject ido in updateDataObjects)
      {
        objectName = ido.GetType().Name;
        identifier = ido.GetPropertyValue("Id").ToString();
        identiferUpdateDataObjectPairs.Add(identifier, ido);
      }
    }

    public void CleanIdentifier(string identifier)
    {
      updateIdentifiers.Remove(identifier);
    }

    private PostRelatedObjectIdentifier GetPostROHelp(string postROIdentity)
    {
      foreach (PostRelatedObjectIdentifier postRO in postRelatedObjectHelps)
        if (postROIdentity == postRO.identifier)
          return postRO;

      return null;
    }
  }
}
