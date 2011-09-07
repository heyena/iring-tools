using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DynamicGridDemo.Models
{
  public class GridData
  {
    public GridData()
    {
      rows = new List<IDictionary<string, string>>();
    }

    public string total { get; set; }
    public IList<IDictionary<string, string>> rows { get; set; }
  }
}