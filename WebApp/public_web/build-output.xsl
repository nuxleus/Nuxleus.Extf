<?xml version="1.0"?>
<xsl:stylesheet version="1.0" 
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
  xmlns:my="http://xameleon.org/service/session"
  xmlns="http://www.w3.org/1999/xhtml" 
  xmlns:html="http://www.w3.org/1999/xhtml" 
  xmlns:bVend="http://xameleon.org/service/browservendors" 
  xmlns:page="http://xameleon.org/service/page/output" 
  exclude-result-prefixes="my html page bVend">
  
  <xsl:variable name="vendor" select="system-property('xsl:vendor')" />
  
  <xsl:output 
    doctype-system="/resources/dtd/xhtml1-strict.dtd" 
    doctype-public="-//W3C//DTD XHTML 1.0 Strict//EN" 
    cdata-section-elements="script"
    method="xml" 
    omit-xml-declaration="yes"/>
    
  <xsl:template match="my:session">
    <xsl:apply-templates/>
  </xsl:template>
  
  <xsl:template match="my:page">
    <html>    
      <head>
        <xsl:apply-templates select="page:output/page:head/page:title"/>
        <style type="text/css">
        <xsl:apply-templates select="page:output/page:head/page:head.include[@fileType = 'css']"/>
        <xsl:apply-templates select="page:config/bVend:bVendors/bVend:vBrowser[@vendor = $vendor]/page:head.include[@fileType = 'css']"/>
        </style>
        <xsl:apply-templates select="page:output/page:head/page:head.include[@fileType = 'javascript']"/>
        <xsl:apply-templates select="page:config/bVend:bVendors/bVend:vBrowser[@vendor = $vendor]/page:head.include[@fileType = 'javascript']"/>
      </head>
      <body onload="javascript: hello(); return true;">
        <xsl:apply-templates select="page:output/page:body"/>
      </body>
    </html>
  </xsl:template>

  <xsl:template match="page:head.include[@fileType = 'css']">
    <xsl:text>@import "</xsl:text>
    <xsl:value-of select="@href"/>
    <xsl:text>";</xsl:text>
    <xsl:text>
</xsl:text>
  </xsl:template>
  
  <xsl:template match="page:head.include[@fileType = 'javascript']">
    <script type="text/javascript" src="{@href}">
    <xsl:comment>/* hack to ensure browser compatibility */</xsl:comment>
    </script>
  </xsl:template>
  
  <xsl:template match="page:title">
    <title>
      <xsl:apply-templates/>
    </title>
  </xsl:template>
  
  <xsl:template match="page:body">
      <xsl:apply-templates/>
  </xsl:template>
  
  <xsl:template match="page:content">
    <xsl:apply-templates/>
  </xsl:template>
  
  <xsl:template match="page:heading">
    <xsl:element name="{@size}">
      <xsl:apply-templates/>
    </xsl:element>
  </xsl:template>
  
  <xsl:template match="html:*">
    <xsl:element name="{local-name()}">
    <xsl:copy-of select="@*"/>
      <xsl:apply-templates/>
    </xsl:element>
  </xsl:template>
  
  <xsl:template match="text()">
    <xsl:value-of select="."/>
  </xsl:template>
  
</xsl:stylesheet>


