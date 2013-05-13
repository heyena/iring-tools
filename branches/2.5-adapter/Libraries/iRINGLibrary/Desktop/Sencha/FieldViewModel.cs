using System.ComponentModel;
using System.Runtime.Serialization;

namespace org.iringtools.library
{
  [DataContract]
  public class FieldViewModel
  {    

    
    [DataMember(Name = "dateFormat", EmitDefaultValue = true)]
      public string dateFormat { get; set; }

    [DataMember(Name = "dateValue", EmitDefaultValue = true)]
    [DefaultValue("")]
    public string dateValue { get; set; }

    [DataMember(Name = "mapping", EmitDefaultValue = true)]
    public string mapping { get; set; }

    [DataMember(Name = "name", EmitDefaultValue = true)]
    public string name { get; set; }

    [DataMember(Name = "persist", EmitDefaultValue = true)]
    [DefaultValue(true)]
    public bool persist { get; set; }


    [DataMember(Name = "sortDir", EmitDefaultValue = true)]
    [DefaultValue("ASC")]
    public string sortDir { get; set; }


    [DataMember(Name = "type", EmitDefaultValue = true)]
    [DefaultValue("auto")]
    public string type { get; set; }

    [DataMember(Name = "useNull", EmitDefaultValue = true)]
    [DefaultValue("false")]
    public bool useNull { get; set; }

    public FieldViewModel()
    {
      dateValue = "";
      persist = true;
      sortDir = "ASC";
      type = "auto";
      useNull = false;
    }
  }
}