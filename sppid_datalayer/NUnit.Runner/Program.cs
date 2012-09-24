using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Gui;

namespace org.iringtools.test
{
  class Program
  {
    [STAThread]
    static void Main()
    {
      string commandLine = @"/run ../../../SPPIDDataLayer.Tests/SPPIDDataLayer.Tests.csproj";

      string[] args = commandLine.Split(' ');
      AppEntry.Main(args);
    }
  }
}
