using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.Linq;
using System.Text;
using org.iringtools.library;

namespace org.iringtools.datalayer.excel
{
  [DataContract(Name = "workbook")]
  public class ExcelConfiguration
  {
    [DataMember(Name = "location", Order = 0)]
    public string Location { get; set; }

    [DataMember(Name = "worksheets", Order = 1)]
    public List<ExcelWorksheet> Worksheets { get; set; }  
  }

  [DataContract(Name = "worksheet")]
  public class ExcelWorksheet
  {
    [DataMember(Name = "name", Order = 0)]
    public string Name { get; set; }

    [DataMember(Name = "label", Order = 1)]
    public string Label { get; set; }

    [DataMember(Name = "identifier", Order = 2)]
    public string Identifier { get; set; }

    [DataMember(Name = "header", Order = 3)]
    public int HeaderIdx { get; set; }

    [DataMember(Name = "start", Order = 4)]
    public int DataIdx { get; set; }

    [DataMember(Name = "columns", Order = 5)]
    public List<ExcelColumn> Columns { get; set; }
  }

  [DataContract(Name = "column")]
  public class ExcelColumn
  {
    [DataMember(Name = "name", Order = 0)]
    public string Name { get; set; }

    [DataMember(Name = "label", Order = 1)]
    public string Label { get; set; }

    [DataMember(Name = "datatype", Order = 2)]
    public DataType DataType { get; set; }

    [DataMember(Name = "index", Order = 3)]
    public int Index { get; set; }
  }

}
