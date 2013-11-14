using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using org.iringtools.adapter;
using System.Configuration;

namespace TipMapGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            AbstractProvider abstractProvider = new AbstractProvider(ConfigurationManager.AppSettings);
            abstractProvider.GenerateAllTips();
        }
    }
}
