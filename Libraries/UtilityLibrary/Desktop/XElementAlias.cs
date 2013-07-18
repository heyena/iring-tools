// Since the Xelement is not serializable so to put xelement objects in Ldap we needed a wrapper
// class which help us to keep our xelement objects in LDAP.  
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace org.iringtools.utility
{
    [Serializable]
    public class XElementClone
    {
        public string fileName;
        [NonSerialized]
        public XElement data;
        public string xElementContent;
    }
}
