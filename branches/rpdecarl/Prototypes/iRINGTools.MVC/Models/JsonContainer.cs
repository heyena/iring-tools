﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace org.iringtools.client.Models
{
  public class JsonContainer<T>
  {    
    public List<T> Items { get; set; }
    public string Message { get; set; }
    public Boolean Success { get; set; }
    public int Total { get; set; }
  }
}