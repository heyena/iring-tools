using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleApplication3
{
  class Program
  {
    static void Main(string[] args)
    {
      foreach (System.Reflection.Assembly asm in System.AppDomain.CurrentDomain.GetAssemblies())
      {
          foreach (System.Type t in asm.GetTypes())
          {
            string name = asm.FullName.Split(',')[0];
            string assembly = string.Format("{0}, {1}", t.FullName, name);
            Console.WriteLine(name + " " + assembly);
          }
      }

      Console.ReadKey();
    }
  }
}
