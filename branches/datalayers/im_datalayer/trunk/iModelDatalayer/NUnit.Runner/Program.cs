using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using NUnit.Gui;

namespace org.iringtools.nunit
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            string commandLine = @"/run ../../../iModelDatalayer.Test/iModelDatalayer.Test.csproj";

            string[] args = commandLine.Split(' ');
            AppEntry.Main(args);
        }
    }
}
