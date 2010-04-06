<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.1"
                xmlns="http://ns.ids-adi.org/qxf/schema#"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:dto="http://dto.iringtools.org"
>
  <xsl:output method="xml" encoding="utf-8" indent="yes"/>

  <xsl:param name="dtoFilePath"/>
  <!--<xsl:variable name="dtoFilePath" select="'C:\iring-tools-1.2\Adapter\AdapterService\Transforms\DTO.12345_000.Inspec.LineList.xml'"/>-->
  <xsl:variable name="dtoList" select="document($dtoFilePath)/*/*"/>

  <xsl:param name="graphName"/>
  <!--<xsl:variable name="graphName" select="'LineList'"/>-->

  <xsl:template match="/Mapping">
    <xsl:element name="qxf">
      <xsl:apply-templates select="GraphMaps/GraphMap"/>
    </xsl:element>
  </xsl:template>

  <xsl:template match="GraphMaps/GraphMap">
    <xsl:if test="@name=$graphName">
      <xsl:variable name="classId" select="@classId"/>
      <xsl:for-each select="$dtoList">
        <xsl:element name="relationship">
          <xsl:attribute name="instance-of">
            <xsl:value-of select="'http://dm.rdlfacade.org/data#classification'"/>
          </xsl:attribute>
          <xsl:element name="property">
            <xsl:attribute name="instance-of">
              <xsl:value-of select="'http://dm.rdlfacade.org/data#class'"/>
            </xsl:attribute>
            <xsl:attribute name="reference">
              <xsl:value-of select="concat('http://rdl.rdlfacade.org/data#', substring-after($classId, 'rdl:'))"/>
            </xsl:attribute>
          </xsl:element>
          <xsl:element name="property">
            <xsl:attribute name="instance-of">
              <xsl:value-of select="'http://dm.rdlfacade.org/data#instance'"/>
            </xsl:attribute>
            <xsl:attribute name="reference">
              <xsl:value-of select="concat('http://www.example.com/data#', @id)"/>
            </xsl:attribute>
          </xsl:element>
        </xsl:element>
      </xsl:for-each>
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
        <!-- process reference and fixed roles -->
        <xsl:variable name="referenceRoles">
          <xsl:for-each select="RoleMaps/RoleMap">
            <xsl:choose>
              <xsl:when test="@reference!=''">
                <xsl:element name="property">
                  <xsl:attribute name="instance-of">
                    <xsl:value-of select="concat('http://tpl.rdlfacade.org/data#', substring-after(@roleId, 'tpl:'))"/>
                  </xsl:attribute>
                  <xsl:attribute name="reference">
                    <xsl:value-of select="concat('http://rdl.rdlfacade.org/data#', substring-after(@reference, 'rdl:'))"/>
                  </xsl:attribute>
                </xsl:element>
              </xsl:when>
              <xsl:when test="@value!=''">
                <xsl:element name="property">
                  <xsl:attribute name="instance-of">
                    <xsl:value-of select="concat('http://tpl.rdlfacade.org/data#', substring-after(@roleId, 'tpl:'))"/>
                  </xsl:attribute>
                  <xsl:attribute name="as">
                    <xsl:value-of select="concat('http://www.w3.org/2001/XMLSchema#', substring-after(@dataType, 'xsd:'))"/>
                  </xsl:attribute>
                  <xsl:value-of select="@value"/>
                </xsl:element>
              </xsl:when>
            </xsl:choose>
          </xsl:for-each>
        </xsl:variable>
        <!-- process property (mapped) roles -->
        <xsl:for-each select="RoleMaps/RoleMap">
          <xsl:if test="@propertyName!=''">
            <xsl:variable name="templateId" select="../../@templateId"/>
            <xsl:variable name="classRole" select="../../@classRole"/>
            <xsl:variable name="roleXPath" select="concat($templateXPath, '.tpl:', @name)"/>
            <xsl:variable name="roleId" select="@roleId"/>
            <xsl:variable name="dataType" select="@dataType"/>
            <xsl:variable name="valueList" select="@valueList"/>
            <xsl:variable name="valueMaps" select="/Mapping/ValueMaps/ValueMap[@valueList=$valueList]"/>
            <xsl:for-each select="$dtoList">
              <xsl:for-each select="dto:Properties/dto:Property">
                <xsl:if test="@name=$roleXPath">
                  <xsl:element name="relationship">
                    <xsl:attribute name="instance-of">
                      <xsl:value-of select="concat('http://tpl.rdlfacade.org/data#', substring-after($templateId, 'tpl:'))"/>
                    </xsl:attribute>
                    <xsl:copy-of select="$referenceRoles" />
                    <xsl:element name="property">
                      <xsl:attribute name="instance-of">
                        <xsl:value-of select="concat('http://tpl.rdlfacade.org/data#', substring-after($roleId, 'tpl:'))"/>
                      </xsl:attribute>
                      <xsl:choose>
                        <xsl:when test="$valueList!=''">
                          <xsl:variable name="roleMapValueList" select="@valueList"/>
                          <xsl:variable name="value" select="@value"/>
                          <xsl:variable name="valueMap" select="$valueMaps[@internalValue=$value]"/>
                          <xsl:variable name="modelURI" select="$valueMap/@modelURI"/>
                          <xsl:attribute name="as">
                            <xsl:value-of select="concat('http://rdl.rdlfacade.org/data#', substring-after($modelURI, 'rdl:'))"/>
                          </xsl:attribute>
                          <xsl:value-of select="$value"/>
                        </xsl:when>
                        <xsl:otherwise>
                          <xsl:attribute name="as">
                            <xsl:value-of select="concat('http://www.w3.org/2001/XMLSchema#', substring-after($dataType, 'xsd:'))"/>
                          </xsl:attribute>
                          <xsl:value-of select="@value"/>
                        </xsl:otherwise>
                      </xsl:choose>
                    </xsl:element>
                    <!-- process template class role -->
                    <xsl:element name="property">
                      <xsl:attribute name="instance-of">
                        <xsl:value-of select="concat('http://tpl.rdlfacade.org/data#', substring-after($classRole, 'tpl:'))"/>
                      </xsl:attribute>
                      <xsl:attribute name="reference">
                        <xsl:value-of select="concat('http://www.example.com/data#', 'id__', ../../@id)"/>
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
        <xsl:variable name="templateId" select="@templateId"/>
        <xsl:variable name="classRole" select="@classRole"/>
        <xsl:variable name="roleMaps" select="RoleMaps"/>
        <xsl:for-each select="$dtoList">
          <xsl:variable name="identifier" select="@id"/>
          <xsl:element name="relationship">
            <xsl:attribute name="instance-of">
              <xsl:value-of select="concat('http://tpl.rdlfacade.org/data#', substring-after($templateId, 'tpl:'))"/>
            </xsl:attribute>
            <!-- process reference and fixed roles that have no class map -->
            <xsl:for-each select="$roleMaps/RoleMap">
              <xsl:choose>
                <xsl:when test="child::node()">
                  <xsl:element name="property">
                    <xsl:attribute name="instance-of">
                      <xsl:value-of select="concat('http://tpl.rdlfacade.org/data#', substring-after(@roleId, 'tpl:'))"/>
                    </xsl:attribute>
                    <xsl:attribute name="reference">
                      <xsl:value-of select="concat('http://rdl.rdlfacade.org/data#', substring-after(ClassMap/@classId, 'rdl:'))"/>
                    </xsl:attribute>
                  </xsl:element>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:choose>
                    <xsl:when test="@reference!=''">
                      <xsl:element name="property">
                        <xsl:attribute name="instance-of">
                          <xsl:value-of select="concat('http://tpl.rdlfacade.org/data#', substring-after(@roleId, 'tpl:'))"/>
                        </xsl:attribute>
                        <xsl:attribute name="reference">
                          <xsl:value-of select="concat('http://rdl.rdlfacade.org/data#', substring-after(@reference, 'rdl:'))"/>
                        </xsl:attribute>
                      </xsl:element>
                    </xsl:when>
                    <xsl:when test="@value!=''">
                      <xsl:element name="property">
                        <xsl:attribute name="instance-of">
                          <xsl:value-of select="concat('http://tpl.rdlfacade.org/data#', substring-after(@roleId, 'tpl:'))"/>
                        </xsl:attribute>
                        <xsl:attribute name="as">
                          <xsl:value-of select="concat('http://www.w3.org/2001/XMLSchema#', substring-after(@dataType, 'xsd:'))"/>
                        </xsl:attribute>
                        <xsl:value-of select="@value"/>
                      </xsl:element>
                    </xsl:when>
                  </xsl:choose>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:for-each>
            <!-- process template class role -->
            <xsl:element name="property">
              <xsl:attribute name="instance-of">
                <xsl:value-of select="concat('http://tpl.rdlfacade.org/data#', substring-after($classRole, 'tpl:'))"/>
              </xsl:attribute>
              <xsl:attribute name="reference">
                <xsl:value-of select="concat('http://www.example.com/data#', 'id__', $identifier)"/>
              </xsl:attribute>
            </xsl:element>
          </xsl:element>
        </xsl:for-each>
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

