<xsl:stylesheet version="1.1" 
                xmlns="http://ns.iringtools.org/library" 
                xmlns:edm="http://schemas.microsoft.com/ado/2006/04/edm/ssdl"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
                xmlns:i="http://www.w3.org/2001/XMLSchema-instance">
  <xsl:output method="xml" encoding="UTF-8" indent="yes"/>  
  <xsl:template match="/">
    <DatabaseDictionary>
      <connectionString>Data Source=;Initial Catalog=;User Id=;Password=</connectionString>
      <provider>MsSql<xsl:value-of select="edm:Schema/@ProviderManifestToken"/></provider>
      <tables>
        <xsl:for-each select="edm:Schema/edm:EntityType">
        <Table>
          <associations/>
          <columns>
          <xsl:for-each select="edm:Property">
            <Column>
              <columnName>
                <xsl:value-of select="@Name"/>
              </columnName>
              <columnType>
                <xsl:call-template name="SqlTypeToCSharpType">
                  <xsl:with-param name="SqlType" select="@Type"/>
                </xsl:call-template>
              </columnType>
              <propertyName>
                <xsl:value-of select="@Name"/>
              </propertyName>
            </Column>
          </xsl:for-each>
          </columns>
          <entityName>
            <xsl:value-of select="@Name"/>
          </entityName>
          <keys>
            <xsl:for-each select="edm:Key">
              <xsl:variable name="columnName">
                <xsl:value-of select="edm:PropertyRef/@Name"/>
              </xsl:variable>
              <Key>
                <columnName>
                  <xsl:value-of select="$columnName"/>
                </columnName>
                <columnType>
                  <xsl:for-each select="../edm:Property">
                    <xsl:if test="@Name = $columnName">
                      <xsl:call-template name="SqlTypeToCSharpType">
                        <xsl:with-param name="SqlType" select="@Type"/>
                      </xsl:call-template>
                    </xsl:if>
                  </xsl:for-each>
                </columnType>
                <propertyName>
                  <xsl:value-of select="$columnName"/>
                </propertyName>
                <keyType>assigned</keyType>
              </Key>
            </xsl:for-each>
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
  
  <xsl:template name="SqlTypeToCSharpType">
    <xsl:param name="SqlType"/>
    <xsl:choose>
      <xsl:when test="$SqlType = 'bit'">Boolean</xsl:when>
      <xsl:when test="$SqlType = 'date'">DateTime</xsl:when>
      <xsl:when test="$SqlType = 'datetime'">DateTime</xsl:when>
      <xsl:when test="$SqlType = 'int'">Int32</xsl:when>
      <xsl:when test="$SqlType = 'float'">Double</xsl:when>
      <xsl:when test="$SqlType = 'varchar'">String</xsl:when>
    </xsl:choose>
  </xsl:template>
</xsl:stylesheet>
