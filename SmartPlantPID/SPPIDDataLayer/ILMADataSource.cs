using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Llama;

namespace iRINGTools.SDK.SPPIDDataLayer
{
  interface ILMADataSource
  {
    string ProjectNumber { get; set; }
    LMAFilter _lmFilters { get; set; }
    LMACriterion _lmCriterion { get; set; }
    
    void set_SiteNode(string siteNode);

    
  }
}
