using System.ComponentModel;
using System.Runtime.Serialization;

namespace org.iringtools.library
{
  [DataContract]
  public class ColumnViewModel
  {
    //public IEnumerable<ColumnViewModel> columns { get; set; }

    [DataMember(Name = "dataIndex")]
    public string DataIndex { get; set; }

    [DataMember(Name = "groupable")]
    [DefaultValue(false)]
    public bool Groupable { get; set; }

    [DataMember(Name = "hidden")]
    [DefaultValue(false)]
    public bool Hidden { get; set; }

    [DataMember(Name = "text")]
    public string Text { get; set; }

    [DataMember(Name = "xtype")]
    public string XType { get; set; }

    [DataMember(Name = "format")]
    public string Format { get; set; }

    [DataMember(Name = "flex")]
    [DefaultValue(0)]
    public int Flex { get; set; }



    public ColumnViewModel()
    {
      this.Flex = 0;
      this.XType = "gridcolumn";
    }
  }
}