using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.Linq;
using System.Text;
using org.iringtools.library;

namespace Hatch.DataLayers.iPasXL
{
  [DataContract(Name = "workbook")]
  public class iPasXLConfiguration
  {
    [DataMember(Name = "location", Order = 0)]
    public string Location { get; set; }

    [DataMember(Name = "worksheets", Order = 1)]
    public List<iPasXLWorksheet> Worksheets { get; set; }  
  }

  [DataContract(Name = "worksheet")]
  public class iPasXLWorksheet
  {
    [DataMember(Name = "name", Order = 0)]
    public string Name { get; set; }

    [DataMember(Name = "identifier", Order = 1)]
    public string Identifier { get; set; }

    [DataMember(Name = "columns", Order = 2)]
    public List<iPasXLColumn> Columns { get; set; }
  }

  [DataContract(Name = "column")]
  public class iPasXLColumn
  {
    [DataMember(Name = "name", Order = 0)]
    public string Name { get; set; }

    [DataMember(Name = "datatype", Order = 1)]
    public DataType DataType { get; set; }

    [DataMember(Name = "index", Order = 2)]
    public int Index { get; set; }
  }

}
