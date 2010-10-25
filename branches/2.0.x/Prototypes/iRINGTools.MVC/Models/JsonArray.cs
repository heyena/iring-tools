using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace org.iringtools.client.Models
{  
  public class JsonArrayItem : Dictionary<string, string>
  { 
  }

  public class JsonArray : List<JsonArrayItem>
  {    
  }
}