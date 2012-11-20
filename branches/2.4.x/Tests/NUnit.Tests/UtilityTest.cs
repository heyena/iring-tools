using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using org.iringtools.utility;
using System.Xml.Linq;
using NUnit.Framework;

namespace NUnit.Tests
{
    [TestFixture]
    public class UtilityTest
  {
      [Test]
      public void ConvertLocalDateTime()
      {
          DateTime dateTime = new DateTime(2012, 10, 11, 9, 8, 7, 6, DateTimeKind.Local);

          string xsdDateTime = Utility.ToXsdDateTime(dateTime);

          Assert.AreEqual("2012-10-11T13:08:07.006-00:00", xsdDateTime);
      }

      [Test]
      public void ConvertUtcDateTime()
      {
          DateTime dateTime = new DateTime(2012, 10, 11, 9, 8, 7, 6, DateTimeKind.Utc);

          string xsdDateTime = Utility.ToXsdDateTime(dateTime);

          Assert.AreEqual("2012-10-11T09:08:07.006-00:00", xsdDateTime);
      }


      [Test]
      public void ConvertUnspecifiedDateTime()
      {
          DateTime dateTime = new DateTime(2012, 10, 11);

          string xsdDateTime = Utility.ToXsdDateTime(dateTime);

          Assert.AreEqual("2012-10-11T00:00:00.000-00:00", xsdDateTime);
      }

      [Test]
      public void ConvertDate()
      {
          string date = "10/11/12";

          string xsdDateTime = Utility.ToXsdDateTime(date);

          Assert.AreEqual("2012-10-11T00:00:00.000-00:00", xsdDateTime);
      }
  }
}
