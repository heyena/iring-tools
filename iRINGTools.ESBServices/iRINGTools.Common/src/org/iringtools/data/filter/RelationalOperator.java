
package org.iringtools.data.filter;

import javax.xml.bind.annotation.XmlEnum;
import javax.xml.bind.annotation.XmlEnumValue;
import javax.xml.bind.annotation.XmlType;


/**
 * <p>Java class for RelationalOperator.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * <p>
 * <pre>
 * &lt;simpleType name="RelationalOperator">
 *   &lt;restriction base="{http://www.w3.org/2001/XMLSchema}string">
 *     &lt;enumeration value="EqualTo"/>
 *     &lt;enumeration value="NotEqualTo"/>
 *     &lt;enumeration value="StartsWith"/>
 *     &lt;enumeration value="EndsWith"/>
 *     &lt;enumeration value="Contains"/>
 *     &lt;enumeration value="In"/>
 *     &lt;enumeration value="GreaterThan"/>
 *     &lt;enumeration value="GreaterThanOrEqual"/>
 *     &lt;enumeration value="LesserThan"/>
 *     &lt;enumeration value="LesserThanOrEqual"/>
 *   &lt;/restriction>
 * &lt;/simpleType>
 * </pre>
 * 
 */
@XmlType(name = "RelationalOperator")
@XmlEnum
public enum RelationalOperator {

    @XmlEnumValue("EqualTo")
    EQUAL_TO("EqualTo"),
    @XmlEnumValue("NotEqualTo")
    NOT_EQUAL_TO("NotEqualTo"),
    @XmlEnumValue("StartsWith")
    STARTS_WITH("StartsWith"),
    @XmlEnumValue("EndsWith")
    ENDS_WITH("EndsWith"),
    @XmlEnumValue("Contains")
    CONTAINS("Contains"),
    @XmlEnumValue("In")
    IN("In"),
    @XmlEnumValue("GreaterThan")
    GREATER_THAN("GreaterThan"),
    @XmlEnumValue("GreaterThanOrEqual")
    GREATER_THAN_OR_EQUAL("GreaterThanOrEqual"),
    @XmlEnumValue("LesserThan")
    LESSER_THAN("LesserThan"),
    @XmlEnumValue("LesserThanOrEqual")
    LESSER_THAN_OR_EQUAL("LesserThanOrEqual");
    private final String value;

    RelationalOperator(String v) {
        value = v;
    }

    public String value() {
        return value;
    }

    public static RelationalOperator fromValue(String v) {
        for (RelationalOperator c: RelationalOperator.values()) {
            if (c.value.equals(v)) {
                return c;
            }
        }
        throw new IllegalArgumentException(v);
    }

}
