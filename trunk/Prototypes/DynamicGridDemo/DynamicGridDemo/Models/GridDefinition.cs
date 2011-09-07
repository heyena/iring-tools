using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace DynamicGridDemo.Models
{
  public class GridDefinition
  {
    public GridDefinition()
    {
      columns = new List<ColumnDefinition>();
      fields = new List<IDictionary<string, string>>();
      filters = new List<IDictionary<string, string>>(); 
    }

    public string idProperty { get; set; }
    public IList<ColumnDefinition> columns { get; set; }
    public IList<IDictionary<string, string>> fields { get; set; }
    public IList<IDictionary<string, string>> filters { get; set; }
  }

  public class ColumnDefinition
  {
    public ColumnDefinition()
    {
      width = 100;
      sortable = true;
      filterable = true;
    }

    public string id { get; set; }
    public string header { get; set; }
    public string dataIndex { get; set; }
    public int width { get; set; }
    public string align { get; set; }
    public bool sortable { get; set; }
    public bool filterable { get; set; }
    public string renderer { get; set; }
  }
}