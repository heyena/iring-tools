using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Gui;

namespace NUnit
{
    class Program
    {
        static void Main()
        {
          string commandLine = @"/run ../../../../../CSVDataLayer.NUnit/CSVDataLayer.NUnit.csproj ../../../../../../MSSQLLibrary/MSSQLLibrary.Tests.csproj";

            string[] args = commandLine.Split(' ');
            AppEntry.Main(args);
        }
    }
}
