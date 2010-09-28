using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace org.iringtools.client.Models
{
  public class JsonArrayItem<T1, T2>
  {
    public T1 Key { get; set; }
    public T2 Value { get; set; }
  }
  
  public class JsonArray : List<JsonArrayItem<string, string>>
  {
    public void Add(string key, string value)
    {
      this.Add(new JsonArrayItem<string, string>{ Key = key, Value = value });
    }
  }
}