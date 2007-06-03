<?xml version="1.0"?>
<xsl:transform version="2.0"
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:xs="http://www.w3.org/2001/XMLSchema"
    xmlns:func="http://atomictalk.org/function"
    xmlns:saxon="http://saxon.sf.net/"
    xmlns:timestamp="clitype:System.DateTime?from=file:///usr/lib/mono/2.0/mscorlib.dll"
    xmlns:clitype="http://saxon.sf.net/clitype"
    exclude-result-prefixes="xs func clitype saxon timestamp">

  <xsl:function name="func:get-timestamp">
    <xsl:param name="timestamp"/>
    <xsl:param name="format" as="xs:string"/>
    <xsl:choose>
      <xsl:when test="$format = 'short-date'">
        <xsl:value-of select="timestamp:ToShortDateString($timestamp)"/>
      </xsl:when>
      <xsl:when test="$format = 'long-date'">
        <xsl:value-of select="timestamp:ToLongDateString($timestamp)"/>
      </xsl:when>
      <xsl:when test="$format = 'short-time'">
        <xsl:value-of select="timestamp:ToShortTimeString($timestamp)"/>
      </xsl:when>
      <xsl:when test="$format = 'long-time'">
        <xsl:value-of select="timestamp:ToLongTimeString($timestamp)"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="timestamp:ToLongTimeString($timestamp)"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:function>

</xsl:transform>
