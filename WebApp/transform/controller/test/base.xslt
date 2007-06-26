<?xml version="1.0" encoding="UTF-8"?>
<xsl:transform version="2.0"
    xmlns:xs="http://www.w3.org/2001/XMLSchema"
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
    xmlns:fn="http://www.w3.org/2005/xpath-functions"
    xmlns:saxon="http://saxon.sf.net/"
    xmlns:clitype="http://saxon.sf.net/clitype"
    xmlns:at="http://atomictalk.org"
    xmlns:func="http://atomictalk.org/function"
    xmlns:test="http://xameleon.org/controller/test"
    xmlns:http-sgml-to-xml="clitype:Xameleon.Function.HttpSgmlToXml?from=file:///srv/wwwroot/webapp/bin/Xameleon.dll"
    xmlns:html="http://www.w3.org/1999/xhtml"
    exclude-result-prefixes="http-sgml-to-xml html xs xsi fn clitype">


  <xsl:variable name="entries" select="document('compare.xml')"/>

  <xsl:template match="test:entries">
    <result>
      <xsl:apply-templates select="test:entry"/>
    </result>
  </xsl:template>

  <xsl:template match="test:entry">
    <xsl:variable name="title" select="http-sgml-to-xml:GetDocXml(@href, '/html/head/title', false())"/>
    <entry href="{@href}">
      <expected-title>
        <xsl:value-of select="."/>
      </expected-title>
      <actual-title>
        <xsl:value-of select="$title"/>
      </actual-title>
      <pass>
        <xsl:value-of select="if ($title = .) then 'True' else 'False'"/>
      </pass>
    </entry>
  </xsl:template>

</xsl:transform>
