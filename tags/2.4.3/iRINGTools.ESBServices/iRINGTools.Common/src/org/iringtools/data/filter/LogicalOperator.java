
package org.iringtools.data.filter;

import javax.xml.bind.annotation.XmlEnum;
import javax.xml.bind.annotation.XmlEnumValue;
import javax.xml.bind.annotation.XmlType;


/**
 * <p>Java class for LogicalOperator.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * <p>
 * <pre>
 * &lt;simpleType name="LogicalOperator">
 *   &lt;restriction base="{http://www.w3.org/2001/XMLSchema}string">
 *     &lt;enumeration value="None"/>
 *     &lt;enumeration value="And"/>
 *     &lt;enumeration value="Or"/>
 *     &lt;enumeration value="Not"/>
 *     &lt;enumeration value="AndNot"/>
 *     &lt;enumeration value="OrNot"/>
 *   &lt;/restriction>
 * &lt;/simpleType>
 * </pre>
 * 
 */
@XmlType(name = "LogicalOperator")
@XmlEnum
public enum LogicalOperator {

    @XmlEnumValue("None")
    NONE("None"),
    @XmlEnumValue("And")
    AND("And"),
    @XmlEnumValue("Or")
    OR("Or"),
    @XmlEnumValue("Not")
    NOT("Not"),
    @XmlEnumValue("AndNot")
    AND_NOT("AndNot"),
    @XmlEnumValue("OrNot")
    OR_NOT("OrNot");
    private final String value;

    LogicalOperator(String v) {
        value = v;
    }

    public String value() {
        return value;
    }

    public static LogicalOperator fromValue(String v) {
        for (LogicalOperator c: LogicalOperator.values()) {
            if (c.value.equals(v)) {
                return c;
            }
        }
        throw new IllegalArgumentException(v);
    }

}
