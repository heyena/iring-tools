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
            string commandLine = @"/run ../../../RestDataLayer.Test/RestDataLayer.Test.csproj";

            string[] args = commandLine.Split(' ');
            AppEntry.Main(args);
        }
    }
}
