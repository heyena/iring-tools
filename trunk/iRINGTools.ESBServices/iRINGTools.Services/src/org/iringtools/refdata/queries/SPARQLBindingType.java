
package org.iringtools.refdata.queries;

import javax.xml.bind.annotation.XmlEnum;
import javax.xml.bind.annotation.XmlEnumValue;
import javax.xml.bind.annotation.XmlType;


/**
 * <p>Java class for SPARQLBindingType.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * <p>
 * <pre>
 * &lt;simpleType name="SPARQLBindingType">
 *   &lt;restriction base="{http://www.w3.org/2001/XMLSchema}string">
 *     &lt;enumeration value="Uri"/>
 *     &lt;enumeration value="Literal"/>
 *   &lt;/restriction>
 * &lt;/simpleType>
 * </pre>
 * 
 */
@XmlType(name = "SPARQLBindingType")
@XmlEnum
public enum SPARQLBindingType {

    @XmlEnumValue("Uri")
    URI("Uri"),
    @XmlEnumValue("Literal")
    LITERAL("Literal");
    private final String value;

    SPARQLBindingType(String v) {
        value = v;
    }

    public String value() {
        return value;
    }

    public static SPARQLBindingType fromValue(String v) {
        for (SPARQLBindingType c: SPARQLBindingType.values()) {
            if (c.value.equals(v)) {
                return c;
            }
        }
        throw new IllegalArgumentException(v);
    }

}
