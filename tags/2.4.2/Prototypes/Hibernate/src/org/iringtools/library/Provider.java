
package org.iringtools.library;

import javax.xml.bind.annotation.XmlEnum;
import javax.xml.bind.annotation.XmlEnumValue;
import javax.xml.bind.annotation.XmlType;


/**
 * <p>Java class for Provider.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * <p>
 * <pre>
 * &lt;simpleType name="Provider">
 *   &lt;restriction base="{http://www.w3.org/2001/XMLSchema}string">
 *     &lt;enumeration value="MsSql2000"/>
 *     &lt;enumeration value="MsSql2005"/>
 *     &lt;enumeration value="MsSql2008"/>
 *     &lt;enumeration value="MySql3"/>
 *     &lt;enumeration value="MySql4"/>
 *     &lt;enumeration value="MySql5"/>
 *     &lt;enumeration value="Oracle8i"/>
 *     &lt;enumeration value="Oracle9i"/>
 *     &lt;enumeration value="Oracle10g"/>
 *     &lt;enumeration value="OracleLite"/>
 *     &lt;enumeration value="PostgresSql81"/>
 *     &lt;enumeration value="PostgresSql82"/>
 *     &lt;enumeration value="SqLite"/>
 *   &lt;/restriction>
 * &lt;/simpleType>
 * </pre>
 * 
 */
@XmlType(name = "Provider")
@XmlEnum
public enum Provider {

    @XmlEnumValue("MsSql2000")
    MS_SQL_2000("MsSql2000"),
    @XmlEnumValue("MsSql2005")
    MS_SQL_2005("MsSql2005"),
    @XmlEnumValue("MsSql2008")
    MS_SQL_2008("MsSql2008"),
    @XmlEnumValue("MySql3")
    MY_SQL_3("MySql3"),
    @XmlEnumValue("MySql4")
    MY_SQL_4("MySql4"),
    @XmlEnumValue("MySql5")
    MY_SQL_5("MySql5"),
    @XmlEnumValue("Oracle8i")
    ORACLE_8_I("Oracle8i"),
    @XmlEnumValue("Oracle9i")
    ORACLE_9_I("Oracle9i"),
    @XmlEnumValue("Oracle10g")
    ORACLE_10_G("Oracle10g"),
    @XmlEnumValue("OracleLite")
    ORACLE_LITE("OracleLite"),
    @XmlEnumValue("PostgresSql81")
    POSTGRES_SQL_81("PostgresSql81"),
    @XmlEnumValue("PostgresSql82")
    POSTGRES_SQL_82("PostgresSql82"),
    @XmlEnumValue("SqLite")
    SQ_LITE("SqLite");
    private final String value;

    Provider(String v) {
        value = v;
    }

    public String value() {
        return value;
    }

    public static Provider fromValue(String v) {
        for (Provider c: Provider.values()) {
            if (c.value.equals(v)) {
                return c;
            }
        }
        throw new IllegalArgumentException(v);
    }

}
