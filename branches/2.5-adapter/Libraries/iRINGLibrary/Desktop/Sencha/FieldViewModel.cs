using System.ComponentModel;
using System.Runtime.Serialization;

namespace org.iringtools.library
{
  [DataContract]
  public class FieldViewModel
  {    

    
    [DataMember(Name = "dateFormat", EmitDefaultValue = true)]
    public string DateFormat { get; set; }

    [DataMember(Name = "dateValue", EmitDefaultValue = true)]
    [DefaultValue("")]
    public string DateValue { get; set; }

    [DataMember(Name = "mapping", EmitDefaultValue = true)]
    public string Mapping { get; set; }

    [DataMember(Name = "name", EmitDefaultValue = true)]
    public string Name { get; set; }

    [DataMember(Name = "persist", EmitDefaultValue = true)]
    [DefaultValue(true)]
    public bool Persist { get; set; }


    [DataMember(Name = "sortDir", EmitDefaultValue = true)]
    [DefaultValue("ASC")]
    public string SortDir { get; set; }


    [DataMember(Name = "type", EmitDefaultValue = true)]
    [DefaultValue("auto")]
    public string type { get; set; }

    [DataMember(Name = "useNull", EmitDefaultValue = true)]
    [DefaultValue("false")]
    public bool UseNull { get; set; }

    public FieldViewModel()
    {
      DateValue = "";
      Persist = true;
      SortDir = "ASC";
      type = "auto";
      UseNull = false;
    }
  }
}