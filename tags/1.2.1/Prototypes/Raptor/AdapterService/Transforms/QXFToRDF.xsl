<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="xsl" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:rdf="http://www.w3.org/1999/02/22-rdf-syntax-ns#" xmlns:rdfs="http://www.w3.org/2000/01/rdf-schema#" xmlns:owl="http://www.w3.org/2002/07/owl#" xmlns:xsd="http://www.w3.org/2001/XMLSchema#" xmlns:dm="http://dm.rdlfacade.org/data#" xmlns:rdl="http://rdl.rdlfacade.org/data#" xmlns:tpl="http://tpl.rdlfacade.org/data#" xmlns:oim="http://oim.rdlfacade.org/data#" xmlns:qxl="http://ns.ids-adi.org/qxf/literal#" xmlns:qxf="http://ns.ids-adi.org/qxf/schema#">
	<xsl:output method="xml" indent="yes" encoding="UTF-8" omit-xml-declaration="no"/>
	<xsl:param name="generate-template-id-prefix" select="''"/>
	<!--<xsl:strip-space elements="*"/>-->
	<xsl:template name="extract-ns">
		<xsl:param name="uri"/>
		<xsl:value-of select="concat(substring-before($uri, '#'), '#')"/>
	</xsl:template>
	<xsl:template name="extract-qname">
		<xsl:param name="uri"/>
		<xsl:variable name="ns">
			<xsl:call-template name="extract-ns">
				<xsl:with-param name="uri" select="$uri"/>
			</xsl:call-template>
		</xsl:variable>
		<xsl:variable name="name" select="substring-after($uri, '#')"/>
		<xsl:choose>
			<!-- 
				@todo maybe load these mappings from
				an external source.
			

  -->
			<xsl:when test="$ns = 'http://www.w3.org/2001/XMLSchema#'">
				<xsl:value-of select="concat('xsd:', $name)"/>
			</xsl:when>
			<xsl:when test="$ns = 'http://www.w3.org/1999/02/22-rdf-syntax-ns#'">
				<xsl:value-of select="concat('rdf:', $name)"/>
			</xsl:when>
			<xsl:when test="$ns = 'http://www.w3.org/2000/01/rdf-schema#'">
				<xsl:value-of select="concat('rdfs:', $name)"/>
			</xsl:when>
			<xsl:when test="$ns = 'http://www.w3.org/2002/07/owl#'">
				<xsl:value-of select="concat('owl:', $name)"/>
			</xsl:when>
			<xsl:when test="$ns = 'http://dm.rdlfacade.org/data#'">
				<xsl:value-of select="concat('dm:', $name)"/>
			</xsl:when>
			<xsl:when test="$ns = 'http://rdl.rdlfacade.org/data#'">
				<xsl:value-of select="concat('rdl:', $name)"/>
			</xsl:when>
			<xsl:when test="$ns = 'http://tpl.rdlfacade.org/data#'">
				<xsl:value-of select="concat('tpl:', $name)"/>
			</xsl:when>
			<xsl:when test="$ns = 'http://oim.rdlfacade.org/data#'">
				<xsl:value-of select="concat('oim:', $name)"/>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="$name"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
	<xsl:template match="qxl:item">
		<xsl:variable name="next" select="(following-sibling::*)[position() = 1]"/>
		<rdf:List>
			<rdf:first>
				<xsl:choose>
					<xsl:when test="count(@reference) &gt; 0">
						<xsl:attribute namespace="http://www.w3.org/1999/02/22-rdf-syntax-ns#" name="rdf:resource">
							<xsl:value-of select="@reference"/>
						</xsl:attribute>
					</xsl:when>
					<xsl:otherwise>
						<xsl:value-of select="string(.)"/>
					</xsl:otherwise>
				</xsl:choose>
			</rdf:first>
			<rdf:rest>
				<xsl:choose>
					<xsl:when test="$next">
						<xsl:apply-templates select="$next"/>
					</xsl:when>
					<xsl:otherwise>
						<xsl:attribute namespace="http://www.w3.org/1999/02/22-rdf-syntax-ns#" name="rdf:resource">http://www.w3.org/1999/02/22-rdf-syntax-ns#nil</xsl:attribute>
					</xsl:otherwise>
				</xsl:choose>
			</rdf:rest>
		</rdf:List>
	</xsl:template>
	<xsl:template match="qxl:list">
		<xsl:variable name="first" select="(qxl:list|qxl:item)[position() = 1]"/>
		<xsl:apply-templates select="$first"/>
	</xsl:template>
	<xsl:template match="qxf:property">
		<xsl:variable name="type-ns">
			<xsl:call-template name="extract-ns">
				<xsl:with-param name="uri" select="@instance-of"/>
			</xsl:call-template>
		</xsl:variable>
		<xsl:variable name="type-qname">
			<xsl:call-template name="extract-qname">
				<xsl:with-param name="uri" select="@instance-of"/>
			</xsl:call-template>
		</xsl:variable>
		<xsl:element namespace="{$type-ns}" name="{$type-qname}">
			<xsl:choose>
				<xsl:when test="@as = 'http://ns.ids-adi.org/qxf/literal#literal'">
					<xsl:apply-templates select="(child::qxl:list|qxl:item)[position() = 1]"/>
				</xsl:when>
				<xsl:when test="@reference">
					<xsl:attribute name="rdf:resource">
						<xsl:value-of select="@reference"/>
					</xsl:attribute>
				</xsl:when>
				<xsl:otherwise>
					<xsl:if test="@lang">
						<xsl:attribute namespace="http://www.w3.org/2001/XMLSchema#" name="xml:lang">
							<xsl:value-of select="@lang"/>
						</xsl:attribute>
					</xsl:if>
					<xsl:if test="@as">
						<xsl:attribute namespace="http://www.w3.org/1999/02/22-rdf-syntax-ns#" name="rdf:datatype">
							<xsl:value-of select="@as"/>
						</xsl:attribute>
					</xsl:if>
					<xsl:value-of select="string(.)"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:element>
	</xsl:template>
	<!-- 
		Matches all non-specific relationships (for specific
		relationships, see below.
	

  -->
	<xsl:template match="qxf:relationship">
		<xsl:variable name="type-ns">
			<xsl:call-template name="extract-ns">
				<xsl:with-param name="uri" select="@instance-of"/>
			</xsl:call-template>
		</xsl:variable>
		<xsl:variable name="type-qname">
			<xsl:call-template name="extract-qname">
				<xsl:with-param name="uri" select="@instance-of"/>
			</xsl:call-template>
		</xsl:variable>
		<xsl:choose>
			<xsl:when test="@instance-of = 'http://tpl.rdlfacade.org/data#R99514676016'">
				<xsl:variable name="property" select="qxf:property[@instance-of = 'http://tpl.rdlfacade.org/data#R25980496885']/@reference"/>
				<xsl:variable name="annotated" select="qxf:property[@instance-of = 'http://tpl.rdlfacade.org/data#R71961093547']/@reference"/>
				<xsl:variable name="value" select="string(qxf:property[@instance-of = 'http://tpl.rdlfacade.org/data#R49608530151'])"/>
				<xsl:variable name="lang" select="string(qxf:property[@instance-of = 'http://tpl.rdlfacade.org/data#R49608530151']/@xml:lang)"/>
				<xsl:variable name="type" select="string(qxf:property[@instance-of = 'http://tpl.rdlfacade.org/data#R49608530151']/@as)"/>
				<xsl:variable name="property-ns">
					<xsl:call-template name="extract-ns">
						<xsl:with-param name="uri" select="$property"/>
					</xsl:call-template>
				</xsl:variable>
				<xsl:variable name="property-qname">
					<xsl:call-template name="extract-qname">
						<xsl:with-param name="uri" select="$property"/>
					</xsl:call-template>
				</xsl:variable>
				<rdf:Description rdf:about="{$annotated}">
					<xsl:element namespace="{$property-ns}" name="{$property-qname}">
						<xsl:if test="$type and not($lang)">
							<xsl:attribute namespace="http://www.w3.org/1999/02/22-rdf-syntax-ns#" name="datatype">
								<xsl:value-of select="$type"/>
							</xsl:attribute>
						</xsl:if>
						<xsl:if test="$lang">
							<xsl:attribute namespace="http://www.w3.org/XML/1998/namespace#" name="xml:lang">
								<xsl:value-of select="$lang"/>
							</xsl:attribute>
						</xsl:if>
						<xsl:value-of select="$value"/>
					</xsl:element>
				</rdf:Description>
			</xsl:when>
			<xsl:otherwise>
				<xsl:element namespace="{$type-ns}" name="{$type-qname}">
					<xsl:variable name="id">
						<xsl:choose>
							<xsl:when test="@id">
								<xsl:value-of select="string(@id)"/>
							</xsl:when>
							<xsl:when test="$generate-template-id-prefix != ''">
								<xsl:value-of select="concat($generate-template-id-prefix, generate-id(.))"/>
							</xsl:when>
							<xsl:otherwise/>
						</xsl:choose>
					</xsl:variable>
					<xsl:choose>
						<xsl:when test="$id != ''">
							<xsl:attribute name="rdf:about">
								<xsl:value-of select="$id"/>
							</xsl:attribute>
						</xsl:when>
						<xsl:otherwise>
							<!--  do nothing 
  -->
						</xsl:otherwise>
					</xsl:choose>
					<xsl:apply-templates/>
				</xsl:element>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
	<!-- 
		Specifically matches relationships expressing RDF triples
	

  -->
	<xsl:template match="qxf:relationship[@instance-of='http://www.w3.org/1999/02/22-rdf-syntax-ns#Statement']">
		<xsl:variable name="predicate-uri" select="qxf:property[@instance-of='http://www.w3.org/1999/02/22-rdf-syntax-ns#predicate']/ @reference"/>
		<xsl:variable name="predicate-ns">
			<xsl:call-template name="extract-ns">
				<xsl:with-param name="uri" select="$predicate-uri"/>
			</xsl:call-template>
		</xsl:variable>
		<xsl:variable name="predicate-qname">
			<xsl:call-template name="extract-qname">
				<xsl:with-param name="uri" select="$predicate-uri"/>
			</xsl:call-template>
		</xsl:variable>
		<rdf:Description rdf:about="{qxf:property[ @instance-of='http://www.w3.org/1999/02/22-rdf-syntax-ns#subject']/@reference}">
			<xsl:element namespace="{$predicate-ns}" name="{$predicate-qname}">
				<xsl:attribute namespace="http://www.w3.org/1999/02/22-rdf-syntax-ns#" name="resource">
					<xsl:value-of select="qxf:property[ @instance-of='http://www.w3.org/1999/02/22-rdf-syntax-ns#object']/ @reference"/>
				</xsl:attribute>
			</xsl:element>
		</rdf:Description>
	</xsl:template>
	<!-- 
		Specifically matches rdf:type expressed as a relationship.
	

  -->
	<xsl:template match="qxf:relationship[@instance-of='http://www.w3.org/1999/02/22-rdf-syntax-ns#Statement' and (string(qxf:property[@instance-of='http://www.w3.org/1999/02/22-rdf-syntax-ns#predicate']/@reference) = 'http://www.w3.org/1999/02/22-rdf-syntax-ns#type' )]">
		<xsl:variable name="object-uri" select="qxf:property[@instance-of='http://www.w3.org/1999/02/22-rdf-syntax-ns#object']/ @reference"/>
		<xsl:if test="not($object-uri)">
			<xsl:message terminate="yes">ERROR: no object qxf:property/@reference</xsl:message>
		</xsl:if>
		<xsl:if test="not(qxf:property[@instance-of='http://www.w3.org/1999/02/22-rdf-syntax-ns#subject']/@reference)">
			<xsl:message terminate="yes">ERROR: no subject qxf:property/@reference</xsl:message>
		</xsl:if>
		<xsl:variable name="object-ns">
			<xsl:call-template name="extract-ns">
				<xsl:with-param name="uri" select="$object-uri"/>
			</xsl:call-template>
		</xsl:variable>
		<xsl:variable name="object-qname">
			<xsl:call-template name="extract-qname">
				<xsl:with-param name="uri" select="$object-uri"/>
			</xsl:call-template>
		</xsl:variable>
		<xsl:element namespace="{$object-ns}" name="{$object-qname}">
			<xsl:attribute namespace="http://www.w3.org/1999/02/22-rdf-syntax-ns#" name="rdf:about">
				<xsl:value-of select="qxf:property[ @instance-of='http://www.w3.org/1999/02/22-rdf-syntax-ns#subject']/@reference"/>
			</xsl:attribute>
		</xsl:element>
	</xsl:template>
	<xsl:template match="/qxf:qxf">
		<rdf:RDF>
			<xsl:if test="count(@xml:base) &gt; 0">
				<xsl:attribute namespace="http://www.w3.org/XML/1998/namespace#" name="xml:base">
					<xsl:value-of select="@xml:base"/>
				</xsl:attribute>
			</xsl:if>
			<xsl:apply-templates/>
		</rdf:RDF>
	</xsl:template>
	<xsl:template match="/">
		<xsl:apply-templates/>
	</xsl:template>
</xsl:stylesheet>