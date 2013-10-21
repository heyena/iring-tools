using System;
using NUnit.Gui;

namespace NUnit
{
    class Program
    {
        [STAThread]
        static void Main()
        {
            string commandLine = @"/run ../../../LWDLSampleCode.Test/LWDLSampleCode.Test.csproj";

            string[] args = commandLine.Split(' ');
            AppEntry.Main(args);
        }
    }
}