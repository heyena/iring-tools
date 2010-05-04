<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.1"
                xmlns="http://ns.ids-adi.org/qxf/schema#"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:dto="http://dto.iringtools.org"
>
  <xsl:output method="xml" encoding="utf-8" indent="yes"/>

  <xsl:param name="dtoFilePath"/>
  <xsl:param name="graphName"/>
  <!--<xsl:variable name="dtoFilePath" select="'C:\iring\Adapter\AdapterService\XML\DTO.12345_000.PSPID.Lines.xml'"/>
  <xsl:variable name="graphName" select="'Lines'"/>-->
  <xsl:variable name="lowercase" select="'abcdefghijklmnopqrstuvwxyz'" />
  <xsl:variable name="uppercase" select="'ABCDEFGHIJKLMNOPQRSTUVWXYZ'" />
  <xsl:variable name="dtoList" select="document($dtoFilePath)/*/*"/>
  <xsl:variable name="xsd" select="'http://www.w3.org/2001/XMLSchema#'"/>
  <xsl:variable name="rdl" select="'http://rdl.rdlfacade.org/data#'"/>
  <xsl:variable name="tpl" select="'http://tpl.rdlfacade.org/data#'"/>
  <xsl:variable name="owl" select="'http://www.w3.org/2002/07/owl#'"/>
  <xsl:variable name="eg"  select="'http://www.example.com/data#'"/>
  <xsl:variable name="rdf" select="'http://www.w3.org/1999/02/22-rdf-syntax-ns#'"/>
  <xsl:variable name="valueMapsNode" select="//ValueMaps"/>
  
  <xsl:template match="/Mapping">
    <xsl:element name="qxf">
      <!-- process graphMaps -->
      <xsl:apply-templates select="GraphMaps/GraphMap"/>
    </xsl:element>
  </xsl:template>

  <xsl:template match="GraphMaps/GraphMap">
    <xsl:if test="@name=$graphName">
      <xsl:variable name="classId" select="@classId"/>
      <xsl:variable name="templateMaps" select="TemplateMaps"/>

      <xsl:for-each select="$dtoList">
        <xsl:call-template name="Classification">
          <xsl:with-param name="classId" select="$classId"/>
          <xsl:with-param name="classInstance" select="@id"/>
        </xsl:call-template>

        <!-- process templateMaps -->
        <xsl:apply-templates select="$templateMaps/TemplateMap">
          <xsl:with-param name="xPath" select="''"/>
          <xsl:with-param name="xPath2" select="''"/>
          <xsl:with-param name="classInstance" select="@id"/>
          <xsl:with-param name="dto" select="."/>
        </xsl:apply-templates>
      </xsl:for-each>
    </xsl:if>
  </xsl:template>

  <xsl:template match="TemplateMaps/TemplateMap">
    <xsl:param name="xPath"/>
    <xsl:param name="xPath2"/>
    <xsl:param name="classInstance"/>
    <xsl:param name="dto"/>
    <xsl:variable name="templateXPath">
      <xsl:choose>
        <xsl:when test="$xPath=''">
          <xsl:value-of select="concat('tpl:', @name)"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="concat($xPath, '.tpl:', @name)"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="templateXPath2">
      <xsl:choose>
        <xsl:when test="$xPath2=''">
          <xsl:value-of select="substring-after(@templateId, 'tpl:')"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="concat($xPath2, '_', substring-after(@templateId, 'tpl:'))"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <!-- create qxf relationship -->
    <xsl:element name="relationship">
      <xsl:attribute name="instance-of">
        <xsl:value-of select="concat($owl, 'Thing')"/>
      </xsl:attribute>

      <!-- create qxf property that connects graphMap/classMap with template -->
      <xsl:element name="property">
        <xsl:attribute name="instance-of">
          <xsl:value-of select="concat($rdf, 'type')"/>
        </xsl:attribute>
        <xsl:attribute name="reference">
          <xsl:value-of select="concat($tpl, substring-after(@templateId, 'tpl:'))"/>
        </xsl:attribute>
      </xsl:element>

      <!-- create qxf property for classRole -->
      <xsl:element name="property">
        <xsl:attribute name="instance-of">
          <xsl:value-of select="concat($tpl, substring-after(@classRole, 'tpl:'))"/>
        </xsl:attribute>
        <xsl:attribute name="reference">
          <xsl:value-of select="concat($eg, $classInstance)"/>
        </xsl:attribute>
      </xsl:element>

      <!-- process role maps -->
      <xsl:apply-templates select="RoleMaps/RoleMap">
        <xsl:with-param name="xPath" select="$templateXPath"/>
        <xsl:with-param name="xPath2" select="$templateXPath2"/>
        <xsl:with-param name="classInstance" select="$classInstance"/>
        <xsl:with-param name="dto" select="$dto"/>
      </xsl:apply-templates>
    </xsl:element>

    <!-- process next (ClassMap) level -->
    <xsl:for-each select="RoleMaps/RoleMap">
      <xsl:if test="child::node()">
        <xsl:apply-templates select="ClassMap">
          <xsl:with-param name="xPath" select="concat($templateXPath, '.tpl:', @name)"/>
          <xsl:with-param name="xPath2" select="concat($templateXPath2, '_', substring-after(@roleId, 'tpl:'))"/>
          <xsl:with-param name="classInstance" select="$classInstance"/>
          <xsl:with-param name="dto" select="$dto"/>
        </xsl:apply-templates>
      </xsl:if>
    </xsl:for-each>
  </xsl:template>

  <xsl:template match="RoleMaps/RoleMap">
    <xsl:param name="xPath"/>
    <xsl:param name="xPath2"/>
    <xsl:param name="classInstance"/>
    <xsl:param name="dto"/>
    <xsl:variable name="roleXPath" select="concat($xPath, '.tpl:', @name)"/>
    <xsl:variable name="roleXPath2" select="concat($xPath2, '_', substring-after(@roleId, 'tpl:'))"/>
    
    <xsl:choose>
      <!-- process reference roles -->
      <xsl:when test="@reference!=''">
        <xsl:element name="property">
          <xsl:attribute name="instance-of">
            <xsl:value-of select="concat($tpl, substring-after(@roleId, 'tpl:'))"/>
          </xsl:attribute>
          <xsl:attribute name="reference">
            <xsl:value-of select="concat($rdl, substring-after(@reference, 'rdl:'))"/>
          </xsl:attribute>
        </xsl:element>
      </xsl:when>

      <!-- process fixed value roles -->
      <xsl:when test="@value!=''">
        <xsl:element name="property">
          <xsl:attribute name="instance-of">
            <xsl:value-of select="concat($tpl, substring-after(@roleId, 'tpl:'))"/>
          </xsl:attribute>
          <xsl:attribute name="as">
            <xsl:value-of select="concat($xsd, substring-after(@dataType, 'xsd:'))"/>
          </xsl:attribute>
          <xsl:value-of select="@value"/>
        </xsl:element>
      </xsl:when>

      <!-- process property (mapped) roles -->
      <xsl:when test="@propertyName!=''">
        <xsl:variable name="roleId" select="@roleId"/>
        <xsl:variable name="valueList" select="@valueList"/>
        <xsl:variable name="dataType" select="@dataType"/>

        <xsl:for-each select="$dto/dto:Properties/dto:Property">
          <xsl:if test="@name=$roleXPath">
            <xsl:element name="property">
              <xsl:attribute name="instance-of">
                <xsl:value-of select="concat($tpl, substring-after($roleId, 'tpl:'))"/>
              </xsl:attribute>
              <xsl:choose>
                <xsl:when test="$valueList!=''">
                  <xsl:variable name="value" select="translate(@value, $lowercase, $uppercase)"/>
                  <xsl:variable name="valueMaps" select="$valueMapsNode/ValueMap[@valueList=$valueList]"/>
                  <xsl:variable name="valueMap" select="$valueMaps[@internalValue=$value]"/>
                  <xsl:variable name="modelURI" select="$valueMap/@modelURI"/>
                  <xsl:attribute name="as">
                    <xsl:value-of select="concat($rdl, substring-after($modelURI, 'rdl:'))"/>
                  </xsl:attribute>
                  <xsl:value-of select="$value"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:attribute name="as">
                    <xsl:value-of select="concat($xsd, substring-after($dataType, 'xsd:'))"/>
                  </xsl:attribute>
                  <xsl:value-of select="@value"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:element>
          </xsl:if>
        </xsl:for-each>
      </xsl:when>

      <!-- process classMap roles -->
      <xsl:when test="child::node()">
        <xsl:element name="property">
          <xsl:attribute name="instance-of">
            <xsl:value-of select="concat($tpl, substring-after(@roleId, 'tpl:'))"/>
          </xsl:attribute>
          <xsl:attribute name="reference">
            <xsl:value-of select="concat($eg, $roleXPath2, '_', substring-after(ClassMap/@classId, 'rdl:'), '_', $classInstance)"/>
          </xsl:attribute>
        </xsl:element>
      </xsl:when>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="ClassMap">
    <xsl:param name="xPath"/>
    <xsl:param name="xPath2"/>
    <xsl:param name="classInstance"/>
    <xsl:param name="dto"/>
    <xsl:variable name="classXPath" select="concat($xPath, '.rdl:', @name)"/>
    <xsl:variable name="classXPath2" select="concat($xPath2, '_', substring-after(@classId, 'rdl:'))"/>
    
    <xsl:call-template name="Classification">
      <xsl:with-param name="classId" select="@classId"/>
      <xsl:with-param name="classInstance" select="concat($classXPath2, '_', $classInstance)"/>
    </xsl:call-template>
    

    
    <!-- process templateMaps -->
    <xsl:apply-templates select="TemplateMaps/TemplateMap">
      <xsl:with-param name="xPath" select="$classXPath"/>
      <xsl:with-param name="xPath2" select="$classXPath2"/>
      <xsl:with-param name="classInstance" select="concat($classXPath2, '_', $classInstance)"/>
      <xsl:with-param name="dto" select="$dto"/>
    </xsl:apply-templates>
  </xsl:template>
  
  <xsl:template name="Classification">
    <xsl:param name="classId"/>
    <xsl:param name="classInstance"/>
    <xsl:element name="relationship">
      <xsl:attribute name="instance-of">
        <xsl:value-of select="concat($owl, 'Thing')"/>
      </xsl:attribute>
      <xsl:element name="property">
        <xsl:attribute name="instance-of">
          <xsl:value-of select="concat($rdf, 'type')"/>
        </xsl:attribute>
        <xsl:attribute name="reference">
          <xsl:value-of select="concat($tpl, 'R63638239485')"/>
        </xsl:attribute>
      </xsl:element>
      <xsl:element name="property">
        <xsl:attribute name="instance-of">
          <xsl:value-of select="concat($tpl, 'R55055340393')"/>
        </xsl:attribute>
        <xsl:attribute name="reference">
          <xsl:value-of select="concat($rdl, substring-after($classId, 'rdl:'))"/>
        </xsl:attribute>
      </xsl:element>
      <xsl:element name="property">
        <xsl:attribute name="instance-of">
          <xsl:value-of select="concat($tpl, 'R99011248051')"/>
        </xsl:attribute>
        <xsl:attribute name="reference">
          <xsl:value-of select="concat($eg, $classInstance)"/>
        </xsl:attribute>
      </xsl:element>
    </xsl:element>
  </xsl:template>
</xsl:stylesheet>
