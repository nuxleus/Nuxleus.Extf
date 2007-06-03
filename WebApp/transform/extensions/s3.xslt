<?xml version="1.0" encoding="UTF-8"?>
<xsl:transform version="2.0"
    xmlns:xs="http://www.w3.org/2001/XMLSchema"
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
    xmlns:fn="http://www.w3.org/2005/xpath-functions"
    xmlns:at="http://atomictalk.org"
    xmlns:func="http://atomictalk.org/function"
    xmlns:s3="clitype:Extf.Net.S3.AWSAuthConnection?from=file:///srv/wwwroot/webapp/bin/Extf.Net.dll"
    xmlns:auth="clitype:Extf.Net.S3.QueryStringAuthGenerator?from=file:///srv/wwwroot/webapp/bin/Extf.Net.dll"
    xmlns:sortedlist="clitype:System.Collections.SortedList?from=file:///usr/lib/mono/2.0/mscorlib.dll"
    xmlns:s3response="clitype:Extf.Net.S3.Response?from=file:///srv/wwwroot/webapp/bin/Extf.Net.dll"
    xmlns:aspnet-session="clitype:System.Web.SessionState.HttpSessionState?from=file:///usr/lib/mono/2.0/System.Web.dll"
    xmlns:aspnet-server="clitype:System.Web.HttpServerUtility?from=file:///usr/lib/mono/2.0/System.Web.dll"
    xmlns:request-stream="clitype:System.Web.HttpRequest?from=file:///usr/lib/mono/2.0/System.Web.dll"
    xmlns:response-stream="clitype:System.Web.HttpResponse?from=file:///usr/lib/mono/2.0/System.Web.dll"
    xmlns:timestamp="clitype:System.DateTime?from=file:///usr/lib/mono/2.0/mscorlib.dll"
    xmlns:session="http://xameleon.org/service/session"
    xmlns:param="http://xameleon.org/service/session/param"
    xmlns:aws="http://xameleon.org/service/aws"
    xmlns:header="http://xameleon.org/service/http/header"
    xmlns:metadata="http://xameleon.org/service/metadata"
    xmlns:so="clitype:System.Object"
    xmlns:saxon="http://saxon.sf.net/"
    xmlns:clitype="http://saxon.sf.net/clitype" exclude-result-prefixes="at auth aspnet-server aspnet-session request-stream response-stream saxon metadata header sortedlist param session s3response aws s3 func xs xsi fn clitype">

  <xsl:import href="file:///srv/wwwroot/sonicradar.com/transform/functions/amazonaws/funcset-s3.xslt"/>

  <xsl:param name="aws-public-key" select="'not-set'" as="xs:string"/>
  <xsl:param name="aws-private-key" select="'not-set'" as="xs:string"/>
  <xsl:param name="cookie_openid" select="'not-set'" as="xs:string" />
  <xsl:param name="cookie_guid" select="'not-set'" as="xs:string" />
  <xsl:param name="qs_return_uri" select="'http://sonicradar.com/'" as="xs:string" />
  <xsl:param name="response" />
  <xsl:param name="request"/>
  <xsl:param name="server"/>
  <xsl:param name="session"/>
  <xsl:param name="timestamp"/>
  <xsl:variable name="file-name" select="concat(substring-before(substring-after($cookie_openid, 'http%3A%2F%2F'), '%2F'), '/session')"/>

  <xsl:variable name="session-params" select="/session:session/param:*"/>
  <xsl:variable name="s3-bucket-name" select="$session-params[local-name() = 's3-bucket-name']" />

  <xsl:strip-space elements="*"/>

  <xsl:output method="xml" indent="no"/>

  <xsl:template match="header:*">
    <xsl:param name="sorted-list" as="clitype:System.Collections.SortedList"/>
    <xsl:variable name="key" select="local-name() cast as xs:untypedAtomic"/>
    <xsl:variable name="value" select=". cast as xs:untypedAtomic"/>
    <xsl:value-of select="sortedlist:Add($sorted-list, $key, $value)"/>
  </xsl:template>

  <xsl:template match="session:session">
    <xsl:apply-templates select="aws:s3"/>
  </xsl:template>

  <xsl:template match="aws:s3">
    <xsl:apply-templates />
  </xsl:template>

  <xsl:template match="aws:sequence">
    <xsl:apply-templates />
  </xsl:template>

  <xsl:template match="aws:check-for-existing-file">
    <xsl:apply-templates select="at:condition"/>
  </xsl:template>

  <xsl:template match="at:condition">
    <xsl:variable name="issecure" select="false()" as="xs:boolean"/>
    <xsl:variable name="content-type" select="func:response.set-content-type($response, 'text/xml')"/>
    <xsl:processing-instruction name="xml-stylesheet">
      <xsl:text>type="text/xsl" href="/transform/openid-redirect.xsl"</xsl:text>
    </xsl:processing-instruction>
    <auth status="session">
      <url>
        <xsl:value-of select="$qs_return_uri"/>
      </url>
      <message>
        <xsl:value-of select="$content-type"/>
        <xsl:value-of select="func:get-timestamp($timestamp, 'short-time')"/>
        <xsl:value-of
            select="func:put-s3-object($s3-bucket-name, $file-name, $cookie_guid, $aws-public-key, $aws-private-key, $issecure) cast as xs:string"/>
       <!-- <xsl:value-of
            select="func:get-s3-signature($s3-bucket-name, $file-name, $cookie_guid, $aws-public-key, $aws-private-key, $issecure, 'get') cast as xs:string"/> -->
      </message>
    </auth>
  </xsl:template>

  <xsl:template match="text()">
    <xsl:value-of select="normalize-space(.)"/>
  </xsl:template>

</xsl:transform>
