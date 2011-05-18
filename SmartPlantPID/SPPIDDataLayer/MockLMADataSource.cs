using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Llama;

namespace iRINGTools.SDK.SPPIDDataLayer
{
  public class MockLMADataSource : ILMADataSource
  {
    private string _siteNode = String.Empty;

    public MockLMADataSource()
    {
    }
   
    public string ProjectNumber { get; set; }
    public LMAFilter _lmFilters { get; set; }
    public LMACriterion _lmCriterion { get; set; }

    public void set_SiteNode(string siteNode)
    {
      _siteNode = siteNode;
    }
  }
}
