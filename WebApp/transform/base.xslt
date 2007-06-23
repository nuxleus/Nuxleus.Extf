<?xml version="1.0" encoding="UTF-8"?>
<xsl:transform version="2.0" 
    xmlns:xs="http://www.w3.org/2001/XMLSchema"
    xmlns:atom="http://www.w3.org/2005/Atom"
    xmlns:omx="http://x2x2x.org/atomicxml/system" 
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
    xmlns:fn="http://www.w3.org/2005/xpath-functions"
    xmlns="http://www.w3.org/1999/xhtml" 
    xmlns:clitype="http://saxon.sf.net/clitype" exclude-result-prefixes="atom xs omx xsi fn">

  <xsl:import href="file:///srv/wwwroot/sonicradar.com/transform/atomicxml.xslt"/>
  <xsl:import href="file:///srv/wwwroot/sonicradar.com/transform/s3.xslt"/>
  <xsl:import href="file:///srv/wwwroot/sonicradar.com/transform/functions/funcset-dateTime.xslt"/>
  <xsl:import href="file:///srv/wwwroot/sonicradar.com/transform/functions/amazonaws/funcset-s3.xslt"/>
  <xsl:import href="file:///srv/wwwroot/sonicradar.com/transform/functions/aspnet/funcset-Util.xslt"/>
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

  <xsl:character-map name="xml">
    <xsl:output-character character="&lt;" string="&lt;"/>
    <xsl:output-character character="&gt;" string="&gt;"/>
    <!--     
      <xsl:output-character character="&amp;" string="&amp;"/> 
    -->
    <xsl:output-character character="&#xD;" string="&#xD;"/>
  </xsl:character-map>

  <xsl:output name="xhtml" doctype-public="-//W3C//DTD XHTML 1.1//EN"
      doctype-system="http://www.w3.org/TR/xhtml1/DTD/xhtml1.1.dtd" include-content-type="yes"
      indent="yes" media-type="text/html" method="xhtml" />

  <xsl:output name="html" doctype-system="-//W3C//DTD HTML 4.01//EN"
      doctype-public="http://www.w3.org/TR/html4/strict.dtd" method="html"
      cdata-section-elements="script" indent="no" media-type="html" />

  <xsl:output method="xml" indent="yes" encoding="UTF-8" use-character-maps="xml"/>

  <xsl:template match="/">
    <xsl:apply-templates/>
  </xsl:template>

</xsl:transform>
