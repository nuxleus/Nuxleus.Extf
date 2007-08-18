<?xml version="1.0" encoding="UTF-8"?>
<xsl:transform
    version="2.0"
    xmlns:xs="http://www.w3.org/2001/XMLSchema"
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:saxon="http://saxon.sf.net/"
    xmlns:clitype="http://saxon.sf.net/clitype"
    xmlns:aspnet-context="clitype:System.Web.HttpContext?partialname=System.Web"
    xmlns:aspnet-request="clitype:System.Web.HttpRequest?partialname=System.Web"
    xmlns:request-collection="clitype:Xameleon.Function.HttpRequestCollection?partialname=Xameleon"
    xmlns:browser="clitype:System.Web.HttpBrowserCapabilities?partialname=System.Web"
    xmlns:timestamp="clitype:System.DateTime"
    xmlns:uri="clitype:System.Uri?partialname=System"
    xmlns:doc="urn:schemas-cagle-com:document"
    exclude-result-prefixes="doc aspnet-context aspnet-request request-collection browser timestamp uri saxon xs clitype">

  <xsl:import href="./controller/atomicxml/base.xslt"/>
  <xsl:import href="./controller/aws/s3/base.xslt"/>
  <xsl:import href="./controller/proxy/base.xslt"/>
  <xsl:import href="./functions/amazonaws/funcset-s3.xslt"/>
  <xsl:import href="./functions/funcset-dateTime.xslt"/>
  <xsl:import href="./functions/funcset-Util.xslt"/>
  <xsl:import href="./functions/aspnet/session.xslt"/>
  <xsl:import href="./functions/aspnet/server.xslt"/>
  <xsl:import href="./functions/aspnet/request-stream.xslt"/>
  <xsl:import href="./functions/aspnet/response-stream.xslt"/>
  <xsl:import href="./functions/aspnet/timestamp.xslt"/>

  <xsl:param name="current-context" />
  <xsl:param name="response" select="aspnet-context:Response($current-context)" />
  <xsl:param name="request" select="aspnet-context:Request($current-context)" />
  <xsl:param name="server" select="aspnet-context:Server($current-context)"/>
  <xsl:param name="session" select="aspnet-context:Session($current-context)"/>
  <xsl:param name="timestamp" select="aspnet-context:Timestamp($current-context)"/>
  <xsl:variable name="debug" select="if (request-collection:GetValue($request, 'query-string', 'debug') = 'true') then true() else false()" as="xs:boolean" />
  <xsl:variable name="request-uri" select="aspnet-request:Url($request)"/>
  <xsl:variable name="browser" select="aspnet-request:Browser($request)"/>

  <xsl:strip-space elements="*"/>

  <doc:summary>
    <doc:title>Documentation Stripper</doc:title>
    <doc:filename>stripDocumentation.xsl</doc:filename>
    <doc:version>0.7</doc:version>
    <doc:dateCreated>2000-01-02</doc:dateCreated>
    <doc:lastModified>2001-01-03</doc:lastModified>
    <doc:description>
      <span style="color:blue;font-size:14pt;font-family:Impact;">Documentation Stripper</span> 
        removes all doc: space documentation from a stylesheet to create a lighter weight
        version of the stylesheet for production use. Save the resulting file under a separate
        name from the documented file.  The details of this and Documentation Maker are included 
        in the article <b>Document Your XSLT</b> by Kurt Cagle. Documentation Stripper is copyright 2000 by 
        <a href="mailto:cagle@olywa.net">Kurt Cagle</a>. For use in 
        commercial products, please contact the author for permissions.
        </doc:description>
  </doc:summary>

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
