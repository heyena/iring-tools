using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Gui;

namespace NUnit
{
    class Program
    {
        [STAThread]
        static void Main()
        {

            
            string[] args = {
                "/run",
                "../../../MSSQLLibrary.Tests/MSSQLLibrary.Tests.csproj"
              //  "../../../../MSSQLLibrary.Tests/MSSQLLibrary.Tests.csproj"
              };

            AppEntry.Main(args);
        }
    }
}
