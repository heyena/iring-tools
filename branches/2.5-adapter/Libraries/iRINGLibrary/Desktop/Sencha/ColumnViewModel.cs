using System.ComponentModel;
using System.Runtime.Serialization;

namespace org.iringtools.library
{
  [DataContract]
  public class ColumnViewModel
  {
    //public IEnumerable<ColumnViewModel> columns { get; set; }

    [DataMember(Name = "dataIndex")]
    public string dataIndex { get; set; }

    [DataMember(Name = "groupable")]
    [DefaultValue(false)]
    public bool groupable { get; set; }

    [DataMember(Name = "hidden")]
    [DefaultValue(false)]
    public bool hidden { get; set; }

    [DataMember(Name = "text")]
    public string text { get; set; }

    [DataMember(Name = "xtype")]
    public string xtype { get; set; }

    [DataMember(Name = "format")]
    public string format { get; set; }

    [DataMember(Name = "flex")]
    [DefaultValue(0)]
    public int flex { get; set; }



    public ColumnViewModel()
    {
      this.flex = 0;
      this.xtype = "gridcolumn";
    }
  }
}