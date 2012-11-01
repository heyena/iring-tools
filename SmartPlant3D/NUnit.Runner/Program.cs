using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Gui;

namespace iringtools.sdk.test
{
  class Program
  {
    [STAThread]
    static void Main()
    {
      string commandLine = @"/run ../../../SP3DDataLayer.Tests/SP3DDataLayer.Tests.csproj";

      string[] args = commandLine.Split(' ');
      AppEntry.Main(args);
    }
  }
}
