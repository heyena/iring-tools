using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Xml.Linq;
using System.Web;
using org.iringtools.adapter;
using org.iringtools.library;
using org.iringtools.utility;
using System.Text;
using System.Collections;
using System.Net;
using System.IO;

namespace org.iringtools.datadictionarysample
{
    class Program
    {
        static void Main(string[] args)
        {
            DataFilter dataFilter = new DataFilter
            {
                Expressions = new List<Expression>
          {
              new Expression
              {
                  OpenGroupCount = 1,
                  PropertyName = "color",
                  RelationalOperator = RelationalOperator.In,
                  Values = new Values
                  {
                      "red",
                      "blue",
                      "green",
                  },
                  IsCaseSensitive = false,
                  LogicalOperator = LogicalOperator.And,
                  CloseGroupCount = 0
              },
              new Expression
              {
                  OpenGroupCount = 0,
                  PropertyName = "material",
                  RelationalOperator = RelationalOperator.EqualTo,
                  Values = new Values
                  {
                      "Al",
                  },
                  IsCaseSensitive = true,
                  LogicalOperator = LogicalOperator.None,
                  CloseGroupCount = 1
              }
          },
                OrderExpressions = new List<OrderExpression>
                {
                    new OrderExpression
                    {
                        PropertyName = "name",
                        SortOrder = SortOrder.Asc
                    }
                }
            };

            string json = Utility.SerializeJson<DataFilter>(dataFilter, false);

            Utility.WriteString(json, @"C:\iring-tools\branches\2.4.x\Prototypes\DataFilterJSON\ConsoleApplication1\ConsoleApplication1\DataFilter.json");
        }
    }
}
