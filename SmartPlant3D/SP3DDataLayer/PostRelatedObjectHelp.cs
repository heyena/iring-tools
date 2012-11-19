using System;
using System.Collections;
using System.Collections.Generic;
using log4net;
using org.iringtools.library;
using System.Linq;

namespace iringtools.sdk.sp3ddatalayer
{
  public class PostRelatedObjectHelp
  {
    public string relatedObjectName = string.Empty;
    public Dictionary<string, string> propertyList = new Dictionary<string,string>();    
  }

  public class PostRelatedObjectIdentifier
  {
    public string identifier = string.Empty;
    public List<PostRelatedObjectHelp> postRelatedObjectHelpList = new List<PostRelatedObjectHelp>();

    public PostRelatedObjectHelp GetPostROHelp(string relatedOBJName)
    {
      foreach (PostRelatedObjectHelp postRO in postRelatedObjectHelpList)
        if (relatedOBJName == postRO.relatedObjectName)
          return postRO;
      return null;
    }
  }
}
