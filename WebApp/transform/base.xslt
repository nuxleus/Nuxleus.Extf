<?xml version="1.0" encoding="UTF-8"?>
<xsl:transform version="2.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

  <xsl:import href="./view/atomicxml.xslt"/>
  <xsl:import href="./view/s3.xslt"/>
  <xsl:import href="./functions/funcset-dateTime.xslt"/>
  <xsl:import href="./functions/amazonaws/funcset-s3.xslt"/>
  <xsl:import href="./functions/funcset-Util.xslt"/>
  <xsl:import href="./functions/aspnet/session.xslt"/>
  <xsl:import href="./functions/aspnet/server.xslt"/>
  <xsl:import href="./functions/aspnet/request-stream.xslt"/>
  <xsl:import href="./functions/aspnet/response-stream.xslt"/>
  <xsl:import href="./functions/aspnet/timestamp.xslt"/>

  <xsl:strip-space elements="*"/>

  <xsl:character-map name="xml">
    <xsl:output-character character="&lt;" string="&lt;"/>
    <xsl:output-character character="&gt;" string="&gt;"/>
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
