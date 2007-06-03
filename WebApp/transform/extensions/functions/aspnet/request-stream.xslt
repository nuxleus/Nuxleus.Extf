<?xml version="1.0"?>
<xsl:transform version="2.0"
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:xs="http://www.w3.org/2001/XMLSchema"
    xmlns:func="http://atomictalk.org/function"
    xmlns:saxon="http://saxon.sf.net/"
    xmlns:request-stream="clitype:System.Web.HttpRequest?from=file:///usr/lib/mono/2.0/System.Web.dll"
    xmlns:clitype="http://saxon.sf.net/clitype"
    exclude-result-prefixes="xs func clitype saxon request-stream">

  <xsl:function name="func:request.get-content-type">
    <xsl:param name="request"/>
    <xsl:value-of select="request-stream:ContentType($request)"/>
  </xsl:function>
  
</xsl:transform>
