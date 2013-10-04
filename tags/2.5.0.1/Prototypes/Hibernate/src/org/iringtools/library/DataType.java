
package org.iringtools.library;

import javax.xml.bind.annotation.XmlEnum;
import javax.xml.bind.annotation.XmlEnumValue;
import javax.xml.bind.annotation.XmlType;


/**
 * <p>Java class for DataType.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * <p>
 * <pre>
 * &lt;simpleType name="DataType">
 *   &lt;restriction base="{http://www.w3.org/2001/XMLSchema}string">
 *     &lt;enumeration value="@Boolean"/>
 *     &lt;enumeration value="@Byte"/>
 *     &lt;enumeration value="@Char"/>
 *     &lt;enumeration value="@DateTime"/>
 *     &lt;enumeration value="@Decimal"/>
 *     &lt;enumeration value="@Double"/>
 *     &lt;enumeration value="@Int16"/>
 *     &lt;enumeration value="@Int32"/>
 *     &lt;enumeration value="@Int64"/>
 *     &lt;enumeration value="@Single"/>
 *     &lt;enumeration value="@String"/>
 *     &lt;enumeration value="TimeSpan"/>
 *   &lt;/restriction>
 * &lt;/simpleType>
 * </pre>
 * 
 */
@XmlType(name = "DataType")
@XmlEnum
public enum DataType {

    @XmlEnumValue("@Boolean")
    BOOLEAN("@Boolean"),
    @XmlEnumValue("@Byte")
    BYTE("@Byte"),
    @XmlEnumValue("@Char")
    CHAR("@Char"),
    @XmlEnumValue("@DateTime")
    DATE_TIME("@DateTime"),
    @XmlEnumValue("@Decimal")
    DECIMAL("@Decimal"),
    @XmlEnumValue("@Double")
    DOUBLE("@Double"),
    @XmlEnumValue("@Int16")
    INT_16("@Int16"),
    @XmlEnumValue("@Int32")
    INT_32("@Int32"),
    @XmlEnumValue("@Int64")
    INT_64("@Int64"),
    @XmlEnumValue("@Single")
    SINGLE("@Single"),
    @XmlEnumValue("@String")
    STRING("@String"),
    @XmlEnumValue("TimeSpan")
    TIME_SPAN("TimeSpan");
    private final String value;

    DataType(String v) {
        value = v;
    }

    public String value() {
        return value;
    }

    public static DataType fromValue(String v) {
        for (DataType c: DataType.values()) {
            if (c.value.equals(v)) {
                return c;
            }
        }
        throw new IllegalArgumentException(v);
    }

}
