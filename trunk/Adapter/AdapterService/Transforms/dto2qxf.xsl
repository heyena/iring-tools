<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="2.0"
                xmlns="http://ns.ids-adi.org/qxf/schema#"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
>
  <xsl:output method="xml" encoding="utf-8" indent="yes"/>

  <!--<xsl:param name="dtoFilePath"/>-->
  <xsl:variable name="dtoFilePath" select="'C:\iring-tools-1.2\Adapter\AdapterService\Transforms\DTO.12345_000.Inspec.LineList.xml'"/>
  <xsl:variable name="dtoList" select="document($dtoFilePath)/Envelope/Payload/DataTransferObject"/>

  <!--<xsl:param name="graphName"/>-->
  <xsl:variable name="graphName" select="'LineList'"/>

  <xsl:template match="/Mapping">
    <xsl:element name="qxf">
      <xsl:apply-templates select="GraphMaps/GraphMap"/>
    </xsl:element>
  </xsl:template>

  <xsl:template match="GraphMaps/GraphMap">
    <xsl:if test="@name=$graphName">
      <xsl:apply-templates select="TemplateMaps/TemplateMap">
        <xsl:with-param name="xPath" select="''"/>
      </xsl:apply-templates>
    </xsl:if>
  </xsl:template>

  <xsl:template match="TemplateMaps/TemplateMap">
    <xsl:param name="xPath"/>
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
    <xsl:choose>
      <!-- process property template -->
      <xsl:when test="@type='Property'">
        <!-- process reference role maps -->
        <xsl:variable name="referenceRoles">
          <xsl:for-each select="RoleMaps/RoleMap">
            <xsl:if test="@reference!=''">
              <xsl:element name="property">
                <xsl:attribute name="instance-of">
                  <xsl:value-of select="concat('http://tpl.rdswip.org/data#', substring-after(@roleId, 'tpl:'))"/>
                </xsl:attribute>
                <xsl:attribute name="reference">
                  <xsl:value-of select="concat('http://rdl.rdswip.org/data#', substring-after(@reference, 'rdl:'))"/>
                </xsl:attribute>
              </xsl:element>
            </xsl:if>
          </xsl:for-each>
        </xsl:variable>
        
        <!-- process property role maps -->
        <xsl:for-each select="RoleMaps/RoleMap">
          <xsl:if test="@propertyName!=''">
            <xsl:variable name="templateId" select="../../@templateId"/>
            <xsl:variable name="classRole" select="../../@classRole"/>
            <xsl:variable name="roleXPath" select="concat($templateXPath, '.tpl:', @name)"/>
            <xsl:variable name="roleId" select="@roleId"/>
            <xsl:variable name="dataType" select="@dataType"/>
            <xsl:variable name="identifier" select="../../../../@identifier"/>
            <!-- TODO: handle multipart identifier -->
            <xsl:if test="@propertyName=$identifier">

            </xsl:if>
            <xsl:for-each select="$dtoList">
              <xsl:for-each select="Properties/Property">
                <xsl:if test="@name=$roleXPath">
                  <xsl:element name="relationship">
                    <xsl:attribute name="instance-of">
                      <xsl:value-of select="concat('http://tpl.rdswip.org/data#', substring-after($templateId, 'tpl:'))"/>
                    </xsl:attribute>
                    <xsl:copy-of select="$referenceRoles" />
                    <xsl:element name="property">
                      <xsl:attribute name="instance-of">
                        <xsl:value-of select="concat('http://tpl.rdswip.org/data#', substring-after($roleId, 'tpl:'))"/>
                      </xsl:attribute>
                      <xsl:attribute name="as">
                        <xsl:value-of select="concat('http://www.w3.org/2001/XMLSchema##', substring-after($dataType, 'xsd:'))"/>
                      </xsl:attribute>
                      <xsl:value-of select="@value"/>
                    </xsl:element>
                    <!-- process template class role -->
                    <xsl:element name="property">
                      <xsl:attribute name="instance-of">
                        <xsl:value-of select="concat('http://tpl.rdswip.org/data#', substring-after($classRole, 'tpl:'))"/>
                      </xsl:attribute>
                      <xsl:attribute name="reference">
                        <xsl:value-of select="concat('http://www.example.com/data#', 'id1__', $identifier)"/>
                      </xsl:attribute>
                    </xsl:element>
                  </xsl:element>
                </xsl:if>
              </xsl:for-each>
            </xsl:for-each>
          </xsl:if>
        </xsl:for-each>
      </xsl:when>
      <!-- process relationship template -->
      <xsl:otherwise>
        <xsl:element name="relationship">
          <xsl:attribute name="instance-of">
            <xsl:value-of select="concat('http://tpl.rdswip.org/data#', substring-after(@templateId, 'tpl:'))"/>
          </xsl:attribute>
          <!-- process reference roles that have no class map -->
          <xsl:for-each select="RoleMaps/RoleMap">
            <xsl:choose>
              <xsl:when test="child::node()">
                <xsl:element name="property">
                  <xsl:attribute name="instance-of">
                    <xsl:value-of select="concat('http://tpl.rdswip.org/data#', substring-after(@roleId, 'tpl:'))"/>
                  </xsl:attribute>
                  <xsl:attribute name="reference">
                    <xsl:value-of select="concat('http://rdl.rdswip.org/data#', substring-after(ClassMap/@classId, 'rdl:'))"/>
                  </xsl:attribute>
                </xsl:element>
              </xsl:when>
              <xsl:otherwise>
                <xsl:element name="property">
                  <xsl:attribute name="instance-of">
                    <xsl:value-of select="concat('http://tpl.rdswip.org/data#', substring-after(@roleId, 'tpl:'))"/>
                  </xsl:attribute>
                  <xsl:attribute name="reference">
                    <xsl:value-of select="concat('http://rdl.rdswip.org/data#', substring-after(@reference, 'rdl:'))"/>
                  </xsl:attribute>
                </xsl:element>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:for-each>
          <!-- process template class role -->
          <xsl:element name="property">
            <xsl:attribute name="instance-of">
              <xsl:value-of select="concat('http://tpl.rdswip.org/data#', substring-after(@classRole, 'tpl:'))"/>
            </xsl:attribute>
            <xsl:attribute name="reference">
              <xsl:value-of select="concat('http://www.example.com/data#', 'id__', ../../@identifier)"/>
            </xsl:attribute>
          </xsl:element>
        </xsl:element>
        <!-- process reference roles that have class map -->
        <xsl:for-each select="RoleMaps/RoleMap">
          <xsl:if test="child::node()">
            <xsl:apply-templates select="ClassMap">
              <xsl:with-param name="xPath" select="concat($templateXPath, '.tpl:', @name)"/>
            </xsl:apply-templates>
          </xsl:if>
        </xsl:for-each>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="ClassMap">
    <xsl:param name="xPath"/>
    <xsl:variable name="className">
      <xsl:call-template name="string-replace-all">
        <xsl:with-param name="text" select="@name" />
        <xsl:with-param name="replace" select="' '" />
        <xsl:with-param name="by" select="''" />
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="classMapXPath" select="concat($xPath, '.rdl:', $className)"/>
    <xsl:apply-templates select="TemplateMaps/TemplateMap">
      <xsl:with-param name="xPath" select="$classMapXPath"/>
    </xsl:apply-templates>
  </xsl:template>

  <xsl:template name="string-replace-all">
    <xsl:param name="text" />
    <xsl:param name="replace" />
    <xsl:param name="by" />
    <xsl:choose>
      <xsl:when test="contains($text, $replace)">
        <xsl:value-of select="substring-before($text,$replace)" />
        <xsl:value-of select="$by" />
        <xsl:call-template name="string-replace-all">
          <xsl:with-param name="text"
          select="substring-after($text,$replace)" />
          <xsl:with-param name="replace" select="$replace" />
          <xsl:with-param name="by" select="$by" />
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$text" />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
</xsl:stylesheet>

