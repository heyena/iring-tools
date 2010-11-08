using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Linq;
using org.iringtools.utility;
using System.Collections.Generic;
using org.iringtools.library;

namespace org.iringtools.modulelibrary.extensions
{
  public static class SPARQLExtensions
  {
    static private List<SPARQLPrefix> prefixes = new List<SPARQLPrefix>
    {
      new SPARQLPrefix() { Label = @"dm",  Uri = @"http://dm.rdlfacade.org/data#", isMappable=false, objectType= SPARQLPrefix.ObjectType.Unknown  },
      new SPARQLPrefix() { Label = @"rdl", Uri = @"http://rdl.rdlfacade.org/data#", isMappable=false, objectType= SPARQLPrefix.ObjectType.Class  },
      new SPARQLPrefix() { Label = @"tpl", Uri = @"http://tpl.rdlfacade.org/data#", isMappable=false, objectType= SPARQLPrefix.ObjectType.Template  },
      new SPARQLPrefix() { Label = @"xsd", Uri = @"http://www.w3.org/2001/XMLSchema#" , isMappable=true, objectType= SPARQLPrefix.ObjectType.Unknown },
    };

    public static SPARQLPrefix.ObjectType GetObjectTypeFromUri(this string uri)
    {
      if (uri == null)
        return SPARQLPrefix.ObjectType.Unknown;

      if (uri.ToLower().StartsWith("file"))
          return SPARQLPrefix.ObjectType.Unknown;
        //Split up the URI
        string[] uriParts = uri.Split('#');
        string @namespace = uriParts[0] + "#";
        SPARQLPrefix.ObjectType objectType = SPARQLPrefix.ObjectType.Unknown;

        //Get the Alias
        var results = from prefix in prefixes
                      where prefix.Uri == @namespace
                      select prefix;

        if (results.Count() > 0)
        {
            SPARQLPrefix prefix = results.First<SPARQLPrefix>();
            objectType = prefix.objectType;
        }
        return objectType;
    }

    public static string GetAliasFromUri(this string uri)
    {
      if (uri == null)
        return String.Empty;

        //Split up the URI
        string[] uriParts = uri.Split('#');
        string @namespace = uriParts[0] + "#";        
        string alias = String.Empty;

        //Get the Alias
        var results = from prefix in prefixes
                      where prefix.Uri == @namespace
                      select prefix;

        if (results.Count() > 0)
        {
            SPARQLPrefix prefix = results.First<SPARQLPrefix>();
            alias = prefix.Label;
        }
        return alias;
    }

    public static string GetNamespaceFromUri(this string uri)
    {
      if (uri == null)
        return String.Empty;

        //Split up the URI
        string[] uriParts = uri.Split('#');
        string @namespace = uriParts[0] + "#";
        return @namespace;
    }

    public static bool IsMappable(this string uri)
    {
      bool isMappable = false;

      if (uri != null)
      {
        //Split up the URI
        string[] uriParts = uri.Split('#');
        string @namespace = uriParts[0] + "#";
        string classId = uriParts[1];


        //Get the Alias
        var results = from prefix in prefixes
                      where prefix.Uri == @namespace
                      select prefix;

        if (results.Count() > 0)
        {
          SPARQLPrefix prefix = results.First<SPARQLPrefix>();
          isMappable = prefix.isMappable;
        }
      }
        return isMappable;

    }

    public static string GetIdFromUri(this string uri)
    {
      if (uri == null)
        return String.Empty;

      if (uri.ToLower().StartsWith("file"))
          return string.Empty;

      //Split up the URI
      string[] uriParts = uri.Split('#');
      string @namespace = uriParts[0] + "#";
      string classId = uriParts[1];

      return classId;
    }

    public static string GetIdWithAliasFromUri(this string uri)
    {
      if (uri == null)
        return String.Empty;

      //Split up the URI
      string[] uriParts = uri.Split('#');
      string @namespace = uriParts[0] + "#";
      string classId = uriParts[1];
      string alias = String.Empty;

      //Get the Alias
      var results = from prefix in prefixes
                    where prefix.Uri == @namespace
                    select prefix;

      if (results.Count() > 0)
      {
        SPARQLPrefix prefix = results.First<SPARQLPrefix>();
        alias = prefix.Label;
      }

      //form the id
      string id = String.Format("{0}:{1}", alias, classId);

      return id;
    }
  }
}
