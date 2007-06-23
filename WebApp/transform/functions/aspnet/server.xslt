<?xml version="1.0"?>
<xsl:transform version="2.0"
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:xs="http://www.w3.org/2001/XMLSchema"
    xmlns:func="http://atomictalk.org/function"
    xmlns:saxon="http://saxon.sf.net/"
    xmlns:aspnet-server="clitype:System.Web.HttpServerUtility?from=file:///usr/lib/mono/2.0/System.Web.dll"
    xmlns:clitype="http://saxon.sf.net/clitype"
    exclude-result-prefixes="xs func clitype saxon aspnet-server">

  <xsl:function name="func:placeholder">
    <xsl:param name="request"/>
  </xsl:function>
  
</xsl:transform>
