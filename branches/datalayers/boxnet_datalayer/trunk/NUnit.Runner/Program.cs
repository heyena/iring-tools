using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Gui;

//namespace org.iringtools.nunit
namespace NUnit
{
  class Program
  {
    static void Main()
    {
        string commandLine = @"/run ../../../BoxDotNet.Tests/BoxDotNetDataLayer.Tests.csproj";

      string[] args = commandLine.Split(' ');
      AppEntry.Main(args);
    }
  }
}
