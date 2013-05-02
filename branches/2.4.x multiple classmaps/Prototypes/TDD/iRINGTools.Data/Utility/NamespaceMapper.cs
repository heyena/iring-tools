using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iRINGTools.Data
{
  public class NamespaceMapper : INamespaceMapper
  {
    /// <summary>
    /// Constant Uri for the RDF Namespace
    /// </summary>
    public const string RDF = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";
    /// <summary>
    /// Constant Uri for the RDF Scheme Namespace
    /// </summary>
    public const string RDFS = "http://www.w3.org/2000/01/rdf-schema#";
    /// <summary>
    /// Constant Uri for the XML Scheme Namespace
    /// </summary>
    public const string XSD = "http://www.w3.org/2001/XMLSchema#";
    /// <summary>
    /// Constant Uri for the OWL Namespace
    /// </summary>
    public const string OWL = "http://www.w3.org/2002/07/owl#";
    /// <summary>
    /// Constant Uri for the example Namespace
    /// </summary>
    public const string EG = "http://example.org/data#";
    /// <summary>
    /// Constant Uri for the RDL Namespace
    /// </summary>
    public const string RDL = "http://rdl.rdlfacade.org/data#";
    /// <summary>
    /// Constant Uri for the TPL Namespace
    /// </summary>
    public const string TPL = "http://tpl.rdlfacade.org/data#";
    /// <summary>
    /// Constant Uri for the DM Namespace
    /// </summary>
    public const string DM = "http://dm.rdlfacade.org/data#";
    /// <summary>
    /// Constant Uri for the P8DM Namespace
    /// </summary>
    public const string P8DM = "http://standards.tc184-sc4.org/iso/15926/-8/data-model#";
    /// <summary>
    /// Constant Uri for the OWL2XML Namespace
    /// </summary>
    public const string OWL2XML = "http://www.w3.org/2006/12/owl2-xml#";
    /// <summary>
    /// Constant Uri for the P8 Namespace
    /// </summary>
    public const string P8 = "http://standards.tc184-sc4.org/iso/15926/-8/template-model#";
    /// <summary>
    /// Constant Uri for the TEMPLATES Namespace
    /// </summary>
    public const string TEMPLATES = "http://standards.tc184-sc4.org/iso/15926/-8/templates#";

    /// <summary>
    /// Mapping of Prefixes to URIs
    /// </summary>
    protected Dictionary<String, Uri> _uris;
    /// <summary>
    /// Mapping of URIs to Prefixes
    /// </summary>
    protected Dictionary<int, String> _prefixes;

    /// <summary>
    /// Constructs a new Namespace Map
    /// </summary>
    /// <remarks>The Prefixes rdf, rdfs and xsd are automatically defined</remarks>
    public NamespaceMapper()
      : this(false) { }

    /// <summary>
    /// Constructs a new Namespace Map which is optionally empty
    /// </summary>
    /// <param name="empty">Whether the Namespace Map should be empty, if set to false the Prefixes rdf, rdfs and xsd are automatically defined</param>
    public NamespaceMapper(bool empty)
    {
      this._uris = new Dictionary<string, Uri>();
      this._prefixes = new Dictionary<int, string>();

      if (!empty)
      {
        //Add Standard Namespaces
        this.AddNamespace("rdf", new Uri(RDF));
        this.AddNamespace("rdfs", new Uri(RDFS));
        this.AddNamespace("xsd", new Uri(XSD));
        this.AddNamespace("owl", new Uri(OWL));
        this.AddNamespace("owl2xml", new Uri(OWL2XML));
        this.AddNamespace("rdl", new Uri(RDL));
        this.AddNamespace("tpl", new Uri(TPL));
        this.AddNamespace("eg", new Uri(EG));
        this.AddNamespace("dm", new Uri(DM));
        this.AddNamespace("p8dm", new Uri(P8DM));
        this.AddNamespace("p8", new Uri(P8));
        this.AddNamespace("templates", new Uri(TEMPLATES));
      }
    }

    /// <summary>
    /// Constructs a new Namespace Map which is based on an existing map
    /// </summary>
    /// <param name="nsmapper"></param>
    protected internal NamespaceMapper(INamespaceMapper nsmapper)
      : this(true)
    {
      this.Import(nsmapper);
    }

    /// <summary>
    /// Returns the Prefix associated with the given Namespace URI
    /// </summary>
    /// <param name="uri">The Namespace URI to lookup the Prefix for</param>
    /// <returns>String prefix for the Namespace</returns>
    public virtual String GetPrefix(Uri uri)
    {
      int hash = uri.GetEnhancedHashCode();
      if (this._prefixes.ContainsKey(hash))
      {
        return this._prefixes[hash];
      }
      else
      {
        throw new Exception("The Prefix for the given URI '" + uri.ToString() + "' is not known by the in-scope NamespaceMapper");
      }
    }

    /// <summary>
    /// Returns the Namespace URI associated with the given Prefix
    /// </summary>
    /// <param name="prefix">The Prefix to lookup the Namespace URI for</param>
    /// <returns>URI for the Namespace</returns>
    public virtual Uri GetNamespaceUri(String prefix)
    {
      if (this._uris.ContainsKey(prefix))
      {
        return this._uris[prefix];
      }
      else
      {
        throw new Exception("The Namespace URI for the given Prefix '" + prefix + "' is not known by the in-scope NamespaceMapper");
      }
    }

    /// <summary>
    /// Adds a Namespace to the Namespace Map
    /// </summary>
    /// <param name="prefix">Namespace Prefix</param>
    /// <param name="uri">Namespace Uri</param>
    public virtual void AddNamespace(String prefix, Uri uri)
    {
      int hash = uri.GetEnhancedHashCode();
      if (!this._uris.ContainsKey(prefix))
      {
        //Add a New Prefix
        this._uris.Add(prefix, uri);

        if (!this._prefixes.ContainsKey(hash))
        {
          //Add a New Uri
          this._prefixes.Add(hash, prefix);
          this.OnNamespaceAdded(prefix, uri);
        }
        else
        {
          //Check whether the Namespace Uri is actually being changed
          //If the existing Uri is the same as the old one then we change the prefix
          //but we don't raise the OnNamespaceModified event
          this._prefixes[hash] = prefix;
          if (!this._uris[prefix].ToString().Equals(uri.ToString(), StringComparison.Ordinal))
          {
            //Raise modified event
            this.OnNamespaceModified(prefix, uri);
          }
        }
      }
      else
      {
        //Check whether the Namespace is actually being changed
        //If the existing Uri is the same as the old one no change is needed
        if (!this._uris[prefix].ToString().Equals(uri.ToString(), StringComparison.Ordinal))
        {
          //Update the existing Prefix
          this._uris[prefix] = uri;
          this._prefixes[hash] = prefix;
          this.OnNamespaceModified(prefix, uri);
        }
      }
    }

    /// <summary>
    /// Removes a Namespace from the NamespaceMapper
    /// </summary>
    /// <param name="prefix">Namespace Prefix of the Namespace to remove</param>
    public virtual void RemoveNamespace(String prefix)
    {
      //Check the Namespace is defined
      if (this._uris.ContainsKey(prefix))
      {
        Uri u = this._uris[prefix];

        //Remove the Prefix to Uri Mapping
        this._uris.Remove(prefix);

        //Remove the corresponding Uri to Prefix Mapping
        int hash = u.GetEnhancedHashCode();
        if (this._prefixes.ContainsKey(hash))
        {
          this._prefixes.Remove(hash);
        }

        //Raise the Event
        this.OnNamespaceRemoved(prefix, u);
      }
    }

    /// <summary>
    /// Method which checks whether a given Namespace Prefix is defined
    /// </summary>
    /// <param name="prefix">Prefix to test</param>
    /// <returns></returns>
    public virtual bool HasNamespace(String prefix)
    {
      return this._uris.ContainsKey(prefix);
    }

    /// <summary>
    /// Clears the Namespace Map
    /// </summary>
    public void Clear()
    {
      this._prefixes.Clear();
      this._uris.Clear();
    }

    /// <summary>
    /// Gets a Enumerator of all the Prefixes
    /// </summary>
    public IEnumerable<String> Prefixes
    {
      get
      {
        return this._uris.Keys;
      }
    }

    /// <summary>
    /// A Function which attempts to reduce a Uri to a QName
    /// </summary>
    /// <param name="uri">The Uri to attempt to reduce</param>
    /// <param name="qname">The value to output the QName to if possible</param>
    /// <returns></returns>
    /// <remarks>This function will return a Boolean indicated whether it succeeded in reducing the Uri to a QName.  If it did then the out parameter qname will contain the reduction, otherwise it will be the empty string.</remarks>
    public virtual bool ReduceToQName(String uri, out String qname)
    {
      foreach (Uri u in this._uris.Values)
      {
        String baseuri = u.ToString();

        //Does the Uri start with the Base Uri
        if (uri.StartsWith(baseuri))
        {
          //Remove the Base Uri from the front of the Uri
          qname = uri.Substring(baseuri.Length);
          //Add the Prefix back onto the front plus the colon to give a QName
          qname = this._prefixes[u.GetEnhancedHashCode()] + ":" + qname;
          if (qname.Equals(":")) continue;
          if (qname.Contains("/") || qname.Contains("#")) continue;
          return true;
        }
      }

      //Failed to find a Reduction
      qname = String.Empty;
      return false;
    }

    /// <summary>
    /// Imports the contents of another Namespace Map into this Namespace Map
    /// </summary>
    /// <param name="nsmap">Namespace Map to import</param>
    /// <remarks>
    /// Prefixes in the imported Map which are already defined in this Map are ignored, this may change in future releases.
    /// </remarks>
    public virtual void Import(INamespaceMapper nsmap)
    {
      String tempPrefix = "ns0";
      int tempPrefixID = 0;
      foreach (String prefix in nsmap.Prefixes)
      {
        if (!this._uris.ContainsKey(prefix))
        {
          //Non-colliding Namespaces get copied across
          this.AddNamespace(prefix, nsmap.GetNamespaceUri(prefix));
        }
        else
        {
          //Colliding Namespaces get remapped to new prefixes
          //Assuming the prefixes aren't already used for the same Uri
          if (!this._uris[prefix].ToString().Equals(nsmap.GetNamespaceUri(prefix).ToString(), StringComparison.Ordinal))
          {
            while (this._uris.ContainsKey(tempPrefix))
            {
              tempPrefixID++;
              tempPrefix = "ns" + tempPrefixID;
            }
            this.AddNamespace(tempPrefix, nsmap.GetNamespaceUri(prefix));
          }
        }
      }
    }

    /// <summary>
    /// Event which is raised when a Namespace is Added
    /// </summary>
    public event NamespaceChanged NamespaceAdded;

    /// <summary>
    /// Event which is raised when a Namespace is Modified
    /// </summary>
    public event NamespaceChanged NamespaceModified;

    /// <summary>
    /// Event which is raised when a Namespace is Removed
    /// </summary>
    public event NamespaceChanged NamespaceRemoved;

    /// <summary>
    /// Internal Helper for the NamespaceAdded Event which raises it only when a Handler is registered
    /// </summary>
    /// <param name="prefix">Namespace Prefix</param>
    /// <param name="uri">Namespace Uri</param>
    protected virtual void OnNamespaceAdded(String prefix, Uri uri)
    {
      NamespaceChanged handler = this.NamespaceAdded;
      if (handler != null)
      {
        handler(prefix, uri);
      }
    }

    /// <summary>
    /// Internal Helper for the NamespaceModified Event which raises it only when a Handler is registered
    /// </summary>
    /// <param name="prefix">Namespace Prefix</param>
    /// <param name="uri">Namespace Uri</param>
    protected virtual void OnNamespaceModified(String prefix, Uri uri)
    {
      NamespaceChanged handler = this.NamespaceModified;
      if (handler != null)
      {
        handler(prefix, uri);
      }
    }

    /// <summary>
    /// Internal Helper for the NamespaceRemoved Event which raises it only when a Handler is registered
    /// </summary>
    /// <param name="prefix">Namespace Prefix</param>
    /// <param name="uri">Namespace Uri</param>
    protected virtual void OnNamespaceRemoved(String prefix, Uri uri)
    {
      NamespaceChanged handler = this.NamespaceRemoved;
      if (handler != null)
      {
        handler(prefix, uri);
      }
    }

    #region IDisposable Members

    /// <summary>
    /// Disposes of a Namespace Map
    /// </summary>
    public void Dispose()
    {
      this._prefixes.Clear();
      this._uris.Clear();
    }
    public String ResolveQName(String qname, NamespaceMapper nsmap, Uri baseUri)
    {
      String output;

      if (qname.StartsWith(":"))
      {
        //QName in Default Namespace
        if (nsmap.HasNamespace(String.Empty))
        {
          //Default Namespace Defined
          output = nsmap.GetNamespaceUri(String.Empty).ToString() + qname.Substring(1);
        }
        else
        {

          if (baseUri != null)
          {
            output = baseUri.ToString();
            if (output.EndsWith("#"))
            {
              output += qname.Substring(1);
            }
            else
            {
              output += "#" + qname.Substring(1);
            }
          }
          else
          {
            throw new Exception("Cannot resolve a QName in the Default Namespace when there is no in-scope Base URI and no Default Namespace defined");
          }
        }
      }
      else
      {
        //QName in some other Namespace
        String[] parts = qname.Split(':');
        if (parts.Length == 1)
        {
          output = nsmap.GetNamespaceUri(String.Empty).ToString() + parts[0];
        }
        else
        {
          output = nsmap.GetNamespaceUri(parts[0]).ToString() + parts[1];
        }
      }

      return output;
    }
    #endregion

    public string PrefixString()
    {
      StringBuilder prefixes = new StringBuilder();
      foreach (KeyValuePair<int, string> pref in this._prefixes)
      {
        prefixes.AppendLine("PREFIX " + pref.Value + ": <" + this.GetNamespaceUri(pref.Value) + ">");
      }
      return prefixes.ToString();
    }
  }


  public delegate void NamespaceChanged(String prefix, Uri uri);

  public interface INamespaceMapper : IDisposable
  {
    /// <summary>
    /// Adds a Namespace to the Namespace Map
    /// </summary>
    /// <param name="prefix">Namespace Prefix</param>
    /// <param name="uri">Namespace Uri</param>
    void AddNamespace(string prefix, Uri uri);

    /// <summary>
    /// Clears the Namespace Map
    /// </summary>
    void Clear();

    /// <summary>
    /// Returns the Namespace URI associated with the given Prefix
    /// </summary>
    /// <param name="prefix">The Prefix to lookup the Namespace URI for</param>
    /// <returns>URI for the Namespace</returns>
    Uri GetNamespaceUri(string prefix);

    /// <summary>
    /// Returns the Prefix associated with the given Namespace URI
    /// </summary>
    /// <param name="uri">The Namespace URI to lookup the Prefix for</param>
    /// <returns>String prefix for the Namespace</returns>
    String GetPrefix(Uri uri);

    /// <summary>
    /// Method which checks whether a given Namespace Prefix is defined
    /// </summary>
    /// <param name="prefix">Prefix to test</param>
    /// <returns></returns>
    bool HasNamespace(string prefix);

    /// <summary>
    /// Imports the contents of another Namespace Map into this Namespace Map
    /// </summary>
    /// <param name="nsmap">Namespace Map to import</param>
    /// <remarks>
    /// Prefixes in the imported Map which are already defined in this Map are ignored, this may change in future releases.
    /// </remarks>
    void Import(INamespaceMapper nsmap);

    /// <summary>
    /// Event which is raised when a Namespace is Added
    /// </summary>
    event NamespaceChanged NamespaceAdded;

    /// <summary>
    /// Event which is raised when a Namespace is Modified
    /// </summary>
    event NamespaceChanged NamespaceModified;

    /// <summary>
    /// Event which is raised when a Namespace is Removed
    /// </summary>
    event NamespaceChanged NamespaceRemoved;

    /// <summary>
    /// Gets a Enumeratorion of all the Prefixes
    /// </summary>
    IEnumerable<string> Prefixes
    {
      get;
    }

    /// <summary>
    /// A Function which attempts to reduce a Uri to a QName
    /// </summary>
    /// <param name="uri">The Uri to attempt to reduce</param>
    /// <param name="qname">The value to output the QName to if possible</param>
    /// <returns></returns>
    /// <remarks>
    /// This function will return a Boolean indicated whether it succeeded in reducing the Uri to a QName.  If it did then the out parameter qname will contain the reduction, otherwise it will be the empty string.
    /// </remarks>
    bool ReduceToQName(string uri, out string qname);

    /// <summary>
    /// Removes a Namespace from the Namespace Map
    /// </summary>
    /// <param name="prefix">Namespace Prefix of the Namespace to remove</param>
    void RemoveNamespace(string prefix);

    String ResolveQName(String qname, NamespaceMapper nsmap, Uri baseUri);
  }

  public static class Extensions
  {
    #region Enumerable Extensions

    /// <summary>
    /// Takes a single item and generates an IEnumerable containing only it
    /// </summary>
    /// <typeparam name="T">Type of the enumerable</typeparam>
    /// <param name="item">Item to wrap in an IEnumerable</param>
    /// <returns></returns>
    /// <remarks>
    /// This method taken from Stack Overflow - see <a href="http://stackoverflow.com/questions/1577822/passing-a-single-item-as-ienumerablet">here</a>
    /// </remarks>
    public static IEnumerable<T> AsEnumerable<T>(this T item)
    {
      yield return item;
    }


    public static bool IsDisjoint<T>(this IEnumerable<T> x, IEnumerable<T> y)
    {
      return x.All(item => !y.Contains(item));
    }

    #endregion


    #region Hash Code Extensions

    public static int GetEnhancedHashCode(this Uri u)
    {
      if (u == null) throw new ArgumentNullException("Cannot calculate an Enhanced Hash Code for a null URI");
      return u.ToString().GetHashCode();
    }

    #endregion

    #region ToString() Extensions

    /// <summary>
    /// Gets either the String representation of the Object or the Empty String if the object is null
    /// </summary>
    /// <param name="obj">Object</param>
    /// <returns></returns>
    internal static String ToSafeString(this Object obj)
    {
      return (obj == null) ? String.Empty : obj.ToString();
    }


    #endregion
  }

}
