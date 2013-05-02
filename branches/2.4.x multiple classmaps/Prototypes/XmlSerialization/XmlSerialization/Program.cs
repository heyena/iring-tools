using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using org.iringtools.utility;
using System.Runtime.Serialization;

namespace XmlSerialization
{
  class Program
  {
    static void Main(string[] args)
    {
      Dto dto = new Dto
      {
        identifier = "dto1",
        clsObj = new List<ClsObj>
        {
          new ClsObj
          {
            classId = "clsId1",
            name = "clsName1",
            identifier = "clsIdentifier1"
          },          
          new ClsObj
          {
            classId = "clsId2",
            name = "clsName2",
            identifier = "clsIdentifier2"
          }
        }
      };

      //Utility.Write<Dto>(dto, "c:\\temp\\cs_dto.xml");

      Console.WriteLine(Utility.SerializeDataContract<Dto>(dto));
      Console.ReadKey();
    }
  }

  [DataContract(Namespace = "http://www.iringtools.org/test", Name="dto")]
  public class Dto
  {
    [DataMember(Order=0)]
    public string identifier { get; set; }
    [DataMember(Order=1)]
    public List<ClsObj> clsObj { get; set; }
  }

  [DataContract(Namespace = "http://www.iringtools.org/test", Name="clsObj")]
  public class ClsObj
  {
    [DataMember(Order=0)]
    public string classId { get; set; }
    [DataMember(Order = 1)]
    public string name { get; set; }
    [DataMember(Order = 2)]
    public string identifier { get; set; }
  }
}
