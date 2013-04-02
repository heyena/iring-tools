using System.Collections.Generic;
using System.Runtime.Serialization;

namespace org.iringtools.library
{
  [DataContract]
  public class MetaDataViewModel
  {
    [DataMember(Name = "columns", EmitDefaultValue = true)]
    public IEnumerable<ColumnViewModel> Columns { get; set; }

    [DataMember(Name = "fields", EmitDefaultValue = true)]
    public IEnumerable<FieldViewModel> Fields { get; set; }
  }
}