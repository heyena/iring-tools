using System;
using VDS.RDF;
using org.iringtools.utility;
namespace DecryptCredentials
{
  class Program
  {
    static void Main(string[] args)
    {


      string cred = "mrnrseqwtqJmwmNBif4sicCcdUI0BSaw2uRP+q2Y2NdxLO48HqeeQSWH7n01ickYZ3TbDFLFbLgy7ugY+1T0XQ==";
      WebCredentials wcred = new WebCredentials(cred);
      wcred.Decrypt();

    }
  }
}
