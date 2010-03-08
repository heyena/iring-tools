// ModuleLibrary.Desktop and ModuleLibrary.Silverlight are linked files
// (The .Silverlight project contains the actual code).  We will resolve
// incompatibilities between the two frameworks in this file to ensure
// we can compile without having to litter conditional statements throughout
// the framework

// See the following for information on Project Linking (Multi-Targeting)
// http://www.global-webnet.net/blogengine/post/2009/01/10/Project-Linker-sharing-single-code-base-between-Silverlight-and-Desktop-applications.aspx

using System;
using System.Collections.Generic;
namespace System.Web { }
namespace System.ServiceModel.Web { }

#if SILVERLIGHT

public class WebGetAttribute : Attribute
{
  public WebGetAttribute() { }
  public WebGetAttribute(string uriTemplate) { this.UriTemplate = uriTemplate; }
  public string UriTemplate { get; set; }
}

public class WebInvokeAttribute : Attribute
{
  public WebInvokeAttribute() { }
  public WebInvokeAttribute(string method, string uriTemplate)
  {

  }
  public string Method { get; set; }
  public string UriTemplate { get; set; }

}
namespace System.Collections.Generic
{
  public class SortedList<TKey, TValue> : 
    IDictionary<TKey, TValue>,    
    ICollection<KeyValuePair<TKey, TValue>>, 
    IEnumerable<KeyValuePair<TKey, TValue>>, 
    IEnumerable
  {
    public IDictionary<TKey, TValue> List = new Dictionary<TKey, TValue>();

    public void Add(TKey key, TValue value)
    {
      List.Add(key, value);
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
      return List.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return List.GetEnumerator();
    }

    public void Add(KeyValuePair<TKey, TValue> item)
    {
      List.Add(item.Key, item.Value);
    }

    public void Clear()
    {
      List.Clear();
    }

    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
      throw new NotImplementedException();
    }

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
      throw new NotImplementedException();
    }

    public int Count
    {
      get
      {
        return List.Count;
      }
    }

    public bool IsReadOnly
    {
      get { throw new NotImplementedException(); }
    }

    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
      throw new NotImplementedException();
    }


    public bool ContainsKey(TKey key)
    {
      throw new NotImplementedException();
    }

    public ICollection<TKey> Keys
    {
      get { throw new NotImplementedException(); }
    }

    public bool Remove(TKey key)
    {
      throw new NotImplementedException();
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
      throw new NotImplementedException();
    }

    public ICollection<TValue> Values
    {
      get { throw new NotImplementedException(); }
    }

    public TValue this[TKey key]
    {
      get
      {
        return List[key];
      }
      set
      {
        List[key] = value; 
      }
    }
  }
}



#else





#endif