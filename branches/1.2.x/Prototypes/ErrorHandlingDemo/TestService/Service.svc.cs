using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace TestService
{
  public class Service : IService
  {
    public string SayHello()
    {
      return string.Format("Hello");
    }

    public string GetFault()
    {
      try
      {
        int a = 5;
        int b = 0;

        return string.Format("Error: " + (a / b));
      }
      catch (Exception ex)
      {
        ArithmeticFault fault = new ArithmeticFault();
        fault.Operation = Operation.Divide;
        fault.Description = ex.Message;
        throw new FaultException<ArithmeticFault>(fault, new FaultReason("divide by 0"));
      }
    }
  }
}
