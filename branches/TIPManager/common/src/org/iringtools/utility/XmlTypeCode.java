package org.iringtools.utility;

public class XmlTypeCode
{
  /* private int id; */
  private TypeCode e;

  public enum TypeCode {
    None, Item, Node, Document, Element, Attribute, Namespace, ProcessingInstruction, Comment, Text, AnyAtomicType, UntypedAtomic, String, Boolean, Decimal, Float, Double, Duration, DateTime, Time, Date, GYearMonth, GYear, GMonthDay, GDay, GMonth, HexBinary, Base64Binary, AnyUri, QName, Notation, NormalizedString, Token, Language, NmToken, // NmTokens
    // is
    // not
    // primitive
    Name, NCName, Id, Idref, Entity, Integer, NonPositiveInteger, NegativeInteger, Long, Int, Short, Byte, NonNegativeInteger, UnsignedLong, UnsignedInt, UnsignedShort, UnsignedByte, PositiveInteger, YearMonthDuration, DayTimeDuration;

    public int getValue()
    {
      return this.ordinal();
    }

    public static TypeCode forValue(int value)
    {
      return values()[value];
    }
  }

  /*
   * public int getId() { return id; }
   * 
   * public void setId(int id) { this.id = id; }
   */

  public TypeCode getE()
  {
    return e;
  }

  public void setE(TypeCode e)
  {
    this.e = e;
  }
}
