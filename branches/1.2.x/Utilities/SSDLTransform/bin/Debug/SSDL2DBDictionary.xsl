<xsl:stylesheet version="1.1" 
                xmlns="http://ns.iringtools.org/library" 
                xmlns:edm="http://schemas.microsoft.com/ado/2006/04/edm/ssdl"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
                xmlns:i="http://www.w3.org/2001/XMLSchema-instance">
  <xsl:output method="xml" encoding="UTF-8" indent="yes"/>  
  <xsl:template match="/">
    <DatabaseDictionary>
      <connectionString/>
      <provider>
        <xsl:value-of select="edm:Schema/edm:Provider"/>
      </provider>
      <tables>
        <xsl:for-each select="edm:Schema/edm:EntityType">
        <Table>
          <associations/>
          <columns>
          <xsl:for-each select="edm:Property">

          </xsl:for-each>
          </columns>
          <entityName>
            <xsl:value-of select="@Name"/>
          </entityName>
          <keys>
            <Key>              
              <!--<columnName>tag</columnName>
              <columnType>String</columnType>
              <propertyName>tag</propertyName>
              <keyType>assigned</keyType>-->
            </Key>
          </keys>
          <lazyLoading>true</lazyLoading>
          <tableName>
            <xsl:value-of select="@Name"/>
          </tableName>
        </Table>
        </xsl:for-each>
      </tables>
    </DatabaseDictionary>
  </xsl:template>
</xsl:stylesheet>
