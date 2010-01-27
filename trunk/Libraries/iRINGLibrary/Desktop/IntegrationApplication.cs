using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace org.iringtools.library
{
    public class IntegrationApplication
    {
        private string _name;

        public IntegrationApplication()
        {
            _name = string.Empty;
        }

        public IntegrationApplication(string Name)
        {
            _name = Name;
        }

        public string Name()
        {
            return _name;
        }

    }
}
