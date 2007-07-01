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
    xmlns:http-sgml-to-xml="clitype:Xameleon.Function.HttpSgmlToXml?partialname=Xameleon"
    xmlns:proxy="http://xameleon.org/service/proxy"
    xmlns:html="http://www.w3.org/1999/xhtml"
    exclude-result-prefixes="http-sgml-to-xml html xs xsi fn clitype proxy">
    

  <xsl:import href="../../functions/funcset-Util.xslt"/>

  <xsl:template match="proxy:return-xml-from-html">
    <xsl:sequence select="func:return-xml-from-html(func:resolve-variable(@uri), func:resolve-variable(@xpath))"/>
  </xsl:template>

  <xsl:function name="func:return-xml-from-html">
    <xsl:param name="uri"/>
    <xsl:param name="xpath"/>
    <xsl:variable name="html-to-xml" select="http-sgml-to-xml:GetDocXml($uri, $xpath, false())"/>
    <external-html>
      <xsl:sequence select="saxon:parse($html-to-xml)"/>
    </external-html>
  </xsl:function>

</xsl:transform>
