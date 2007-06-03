<?xml version="1.0" encoding="UTF-8"?>
<xsl:transform version="2.0" xmlns:xs="http://www.w3.org/2001/XMLSchema"
    xmlns="http://www.w3.org/1999/xhtml" xmlns:atom="http://www.w3.org/2005/Atom"
    xmlns:omx="http://x2x2x.org/atomicxml/system" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:search="http://xameleon.org/search" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
    xmlns:fn="http://www.w3.org/2005/xpath-functions"
    xmlns:func="http://atomictalk.org/function"
    xmlns:s3="clitype:Extf.Net.S3.AWSAuthConnection?from=file:///srv/wwwroot/webapp/bin/Extf.Net.dll"
    xmlns:sortedlist="clitype:System.Collections.SortedList?from=file:///usr/lib/mono/2.0/mscorlib.dll"
    xmlns:s3response="clitype:Extf.Net.S3.Response?from=file:///srv/wwwroot/webapp/bin/Extf.Net.dll"
    xmlns:session="http://xameleon.org/service/session"
    xmlns:clitype="http://saxon.sf.net/clitype" exclude-result-prefixes="atom xs omx xsi fn">

  <xsl:import href="file:///srv/wwwroot/sonicradar.com/transform/atomicxml.xslt"/>
  <xsl:import href="file:///srv/wwwroot/sonicradar.com/transform/s3.xslt"/>
  <xsl:import href="file:///srv/wwwroot/sonicradar.com/transform/functions/funcset-dateTime.xslt"/>
  <xsl:import href="file:///srv/wwwroot/sonicradar.com/transform/functions/amazonaws/funcset-s3.xslt"/>
  <xsl:import href="file:///srv/wwwroot/sonicradar.com/transform/functions/aspnet/session.xslt"/>
  <xsl:import href="file:///srv/wwwroot/sonicradar.com/transform/functions/aspnet/server.xslt"/>
  <xsl:import href="file:///srv/wwwroot/sonicradar.com/transform/functions/aspnet/request-stream.xslt"/>
  <xsl:import href="file:///srv/wwwroot/sonicradar.com/transform/functions/aspnet/response-stream.xslt"/>
  <xsl:import href="file:///srv/wwwroot/sonicradar.com/transform/functions/aspnet/timestamp.xslt"/>

  <xsl:param name="xml.base" select="/atom:feed/@xml:base" as="xs:string" />
  <xsl:param name="google.maps.key" as="xs:string" />
  <xsl:param name="request.ip" as="xs:string" />
  <xsl:param name="return_uri" select="'http://sonicradar.com/'" as="xs:string" />

  <xsl:variable name="css-base-class" select="'base'" as="xs:string"/>

  <xsl:variable name="geoip-data"
      select="document(concat('http://sonicradar.com/service/ipgeolocator/geocode?debug=true&amp;ip=', $request.ip))/location"/>
  <xsl:param name="lat" select="substring-before($geoip-data/point, '&#32;')" as="xs:string"/>
  <xsl:param name="long" select="substring-after($geoip-data/point, '&#32;')" as="xs:string"/>
  <xsl:param name="map-depth" select="'8'" as="xs:string"/>
  <xsl:param name="city" select="if ($geoip-data/city) then ($geoip-data/city) else 'unknown'" as="xs:string"/>
  <xsl:param name="country" select="if ($geoip-data/country) then ($geoip-data/country) else 'unknown'" as="xs:string"/>
  <xsl:param name="ip" select="if ($geoip-data/ip) then ($geoip-data/ip) else 'unknown'" as="xs:string"/>
  <xsl:param name="response"/>
  <xsl:param name="search"/>
  <xsl:variable name="rights" select="/atom:feed/atom:rights/*"/>
  <xsl:variable name="author" select="/atom:feed/atom:author/atom:name"/>

  <xsl:strip-space elements="*"/>

  <!-- <xsl:output doctype-public="-//W3C//DTD XHTML 1.1//EN"
      doctype-system="http://www.w3.org/TR/xhtml1/DTD/xhtml1.1.dtd" include-content-type="yes"
      indent="yes" media-type="text/html" method="xhtml" /> -->

  <xsl:output method="xml" indent="yes"/>

  <xsl:template match="/">
    
  <!-- 
  <xsl:value-of
    select="file:SaveExternalFile('http://s3.amazonaws.com/m.david/xslt/eveel_plan/XSLnT.png', '/srv/wwwroot/sonicradar.com/foo.png')" 
    xmlns:file="clitype:Xameleon.Function.HttpFileStream?asm=Xameleon;ver=1.0.0.0;sn=8974f7aaf62d3d0f;from=/srv/wwwroot/webapp/bin/Xameleon.dll"/>
   -->
    <xsl:apply-templates/>
  </xsl:template>

  <xsl:template match="atom:feed">
    <xsl:processing-instruction name="xml-stylesheet">
      <xsl:text>type="text/xsl" href="/transform/build-output.xsl"</xsl:text>
    </xsl:processing-instruction>
  </xsl:template>

  <xsl:template match="atom:link[@type = 'text/css']">
    <xsl:text></xsl:text>
    <xsl:text>@import "</xsl:text>
    <xsl:value-of
        select="if (@rel = 'current') then concat($xml.base, @href) else concat(@rel, @href)"/>
    <xsl:text>";</xsl:text>
  </xsl:template>

  <xsl:template match="atom:link[@type = 'text/javascript']">
    <xsl:text></xsl:text>
    <script src="{if (@rel = 'current') then concat($xml.base, @href) else concat(@rel, @href)}" type="text/javascript">
      <xsl:text>/* */</xsl:text>
    </script>
  </xsl:template>

  <xsl:template match="atom:content[@src]">
    <xsl:apply-templates select="document(@src)/atom:entry/atom:content/omx:module"/>
  </xsl:template>

  <xsl:template match="atom:content[not(@src)]">
    <xsl:apply-templates />
  </xsl:template>

  <xsl:template match="text()">
    <xsl:value-of select="normalize-space(.)"/>
  </xsl:template>

</xsl:transform>
