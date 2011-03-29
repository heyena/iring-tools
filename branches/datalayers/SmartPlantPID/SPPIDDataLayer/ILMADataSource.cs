using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iRINGTools.SDK.SPPIDDataLayer
{
  interface ILMADataSource
  {
    string ProjectNumber { get; set; }

    void set_SiteNode(string siteNode);
  }
}
