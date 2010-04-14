<xsl:stylesheet version="1.1" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:i="http://www.w3.org/2001/XMLSchema-instance" xmlns:edm="http://schemas.microsoft.com/ado/2006/04/edm" xmlns:md="org.ids_adi.camelot.dataLayer.metadata" xmlns="http://ns.iringtools.org/library">
  <xsl:output method="xml" encoding="UTF-8" indent="yes"/>  
  <xsl:variable name="namespace" select="edm:Schema/@Namespace"/>
  <xsl:template match="/">
    <DataDictionary>
      <dataObjects>
        <xsl:for-each select="edm:Schema/edm:EntityType">          
          <DataObject>             
            <dataProperties>
              <xsl:for-each select="edm:Property">
                <xsl:variable name="PropertyName">
                  <xsl:value-of select="@Name"/>
                </xsl:variable>
                <DataProperty>
                  <dataType>
                    <xsl:value-of select="@Type"/>
                  </dataType>
                  <isPropertyKey>
                    <xsl:variable name="PropertyKey">
                      <xsl:for-each select="../edm:Key/edm:PropertyRef">
                        <xsl:if test="@Name = $PropertyName">true</xsl:if>
                      </xsl:for-each>
                    </xsl:variable>
                    <xsl:choose>
                      <xsl:when test="$PropertyKey != ''">true</xsl:when>
                      <xsl:otherwise>false</xsl:otherwise>
                    </xsl:choose>
                  </isPropertyKey>
                  <isRequired>
                    <xsl:choose>
                      <xsl:when test="@Nullable='false'">true</xsl:when>
                      <xsl:otherwise>false</xsl:otherwise>
                    </xsl:choose>
                  </isRequired>
                  <propertyName>
                    <xsl:value-of select="@Name"/>
                  </propertyName>                                 
                </DataProperty>                
              </xsl:for-each>
            </dataProperties>
            <dataRelationships>
              <xsl:for-each select="edm:NavigationProperty">
                <DataRelationship>
                  <cardinality>                    
                    <xsl:call-template name="AssociationCardinalityForRole">
                      <xsl:with-param name="FromRole" select="@FromRole"/>
                      <xsl:with-param name="ToRole" select="@ToRole"/>                     
                      <xsl:with-param name="RelationshipName" select="substring-after(@Relationship, concat($namespace, '.'))"/>
                    </xsl:call-template>
                  </cardinality>
                  <graphProperty>
                    <xsl:value-of select="@Name"/>
                  </graphProperty>
                  <relatedObject>
                    <xsl:call-template name="AssociationEntityForRole">
                      <xsl:with-param name="ToRole" select="@ToRole"/>
                      <xsl:with-param name="RelationshipName" select="substring-after(@Relationship, concat($namespace, '.'))"/>
                    </xsl:call-template>
                  </relatedObject> 
                </DataRelationship>
              </xsl:for-each>
            </dataRelationships>
            <objectName>
              <xsl:value-of select="@Name"/>
            </objectName>
            <objectNamespace>
              <xsl:value-of select="$namespace"/>
            </objectNamespace>
          </DataObject>          
        </xsl:for-each>
      </dataObjects>
    </DataDictionary>
  </xsl:template>
  <xsl:template name="AssociationEntityForRole">
    <xsl:param name="ToRole"/>
    <xsl:param name="RelationshipName"/>
    <xsl:for-each select="/edm:Schema/edm:Association[@Name = $RelationshipName]/edm:End">
      <xsl:if test="@Role=$ToRole">
        <xsl:value-of select="substring-after(@Type, concat($namespace, '.'))"/>
      </xsl:if>
    </xsl:for-each>
  </xsl:template> 
  <xsl:template name="AssociationCardinalityForRole">
    <xsl:param name="FromRole"/>
    <xsl:param name="ToRole"/>
    <xsl:param name="RelationshipName"/>    
    <xsl:variable name="Multiplicity1"
      select="/edm:Schema/edm:Association[@Name=$RelationshipName]/edm:End[@Role=$FromRole]/@Multiplicity"/>      
    <xsl:variable name="Multiplicity2"
      select="/edm:Schema/edm:Association[@Name=$RelationshipName]/edm:End[@Role=$ToRole]/@Multiplicity"/>
    <xsl:value-of select="@Multiplicity1"/>
    <xsl:choose>
      <xsl:when test="contains($Multiplicity1, '1') and contains($Multiplicity2, '*')">OneToMany</xsl:when>
      <xsl:when test="contains($Multiplicity1, '*') and contains($Multiplicity2, '1')">ManyToOne</xsl:when>
      <xsl:when test="contains($Multiplicity1, '1') and contains($Multiplicity2, '1')">OneToOne</xsl:when>
      <xsl:when test="contains($Multiplicity1, '*') and contains($Multiplicity2, '*')">ManyToMany</xsl:when>
    </xsl:choose>
  </xsl:template>  
</xsl:stylesheet>
