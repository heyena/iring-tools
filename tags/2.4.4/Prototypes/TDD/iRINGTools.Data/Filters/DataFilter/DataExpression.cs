using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iRINGTools.Data
{
  public class DataExpression
  {
    public int OpenGroupCount { get; set; }
    public string PropertyName { get; set; }
    public RelationalOperator RelationalOperator { get; set; }
    public IList<string> Values { get; set; }
    public LogicalOperator LogicalOperator { get; set; }
    public int CloseGroupCount { get; set; }
    public bool IsCaseSensitive { get; set; }
  }
}
