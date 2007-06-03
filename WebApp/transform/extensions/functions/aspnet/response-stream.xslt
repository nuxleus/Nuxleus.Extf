<?xml version="1.0"?>
<xsl:transform version="2.0"
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:xs="http://www.w3.org/2001/XMLSchema"
    xmlns:func="http://atomictalk.org/function"
    xmlns:saxon="http://saxon.sf.net/"
    xmlns:response-stream="clitype:System.Web.HttpResponse?from=file:///usr/lib/mono/2.0/System.Web.dll"
    xmlns:clitype="http://saxon.sf.net/clitype"
    exclude-result-prefixes="xs func clitype saxon response-stream">

  <xsl:function name="func:response.get-buffer-output">
    <xsl:param name="response"/>
    <xsl:value-of select="response-stream:BufferOutput($response)"/>
  </xsl:function>

  <xsl:function name="func:response.set-buffer-output">
    <xsl:param name="response"/>
    <xsl:param name="buffer-output" as="xs:boolean"/>
    <xsl:value-of select="response-stream:set_BufferOutput($response, $buffer-output)"/>
  </xsl:function>

  <xsl:function name="func:response.get-content-encoding">
    <xsl:param name="response"/>
    <xsl:value-of select="response-stream:ContentEncoding($response)"/>
  </xsl:function>

  <xsl:function name="func:response.set-content-encoding">
    <xsl:param name="response"/>
    <xsl:param name="content-encoding" as="xs:string"/>
    <xsl:value-of select="response-stream:set_ContentEncoding($response, $content-encoding)"/>
  </xsl:function>

  <xsl:function name="func:response.get-content-type">
    <xsl:param name="response"/>
    <xsl:value-of select="response-stream:ContentType($response)"/>
  </xsl:function>

  <xsl:function name="func:response.set-content-type">
    <xsl:param name="response"/>
    <xsl:param name="content-type" as="xs:string"/>
    <xsl:value-of select="response-stream:set_ContentType($response, $content-type)"/>
  </xsl:function>

  <xsl:function name="func:response.get-charset">
    <xsl:param name="response"/>
    <xsl:value-of select="response-stream:Charset($response)"/>
  </xsl:function>

  <xsl:function name="func:response.set-charset">
    <xsl:param name="response"/>
    <xsl:param name="charset" as="xs:string"/>
    <xsl:value-of select="response-stream:set_Charset($response, $charset)"/>
  </xsl:function>

  <xsl:function name="func:response.get-cookies">
    <xsl:param name="response"/>
    <xsl:value-of select="response-stream:Cookies($response)"/>
  </xsl:function>

  <xsl:function name="func:response.transmit-file">
    <xsl:param name="response"/>
    <xsl:param name="file" as="xs:string"/>
    <xsl:value-of select="response-stream:TransmitFile($response, $file)"/>
  </xsl:function>

</xsl:transform>
