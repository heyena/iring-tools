﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iRINGTools.SDK.SPPIDDataLayer
{
  class MockLMADataSource :ILMADataSource
  {
    private string _siteNode = String.Empty;

    public string ProjectNumber { get; set; }

    public void set_SiteNode(string siteNode)
    {
      _siteNode = siteNode;
    }
  }
}
