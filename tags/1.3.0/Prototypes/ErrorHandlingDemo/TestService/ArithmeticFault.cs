using System;
using System.Collections.Generic;
using System.Linq;

namespace TestService
{
  public class ArithmeticFault
  {
    public Operation Operation { get; set; }
    public string Description { get; set; }
  }
  
  public enum Operation
  {
    Add,
    Subtract,
    Multiply,
    Divide
  }
}