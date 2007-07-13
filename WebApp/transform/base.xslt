<?xml version="1.0" encoding="UTF-8"?>
<xsl:transform 
  version="2.0" 
  xmlns:xs="http://www.w3.org/2001/XMLSchema"
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
  xmlns:fn="http://www.w3.org/2005/xpath-functions"
  xmlns:saxon="http://saxon.sf.net/"
  xmlns:clitype="http://saxon.sf.net/clitype"
  xmlns:at="http://atomictalk.org"
  xmlns:func="http://atomictalk.org/function"
  xmlns:aspnet="http://atomictalk.org/function/aspnet"
  xmlns:service="http://xameleon.org/service"
  xmlns:operation="http://xameleon.org/service/operation"
  xmlns:proxy="http://xameleon.org/service/proxy"
  xmlns:session="http://xameleon.org/service/session"
  xmlns:param="http://xameleon.org/service/session/param"
  xmlns:aws="http://xameleon.org/function/aws"
  xmlns:s3="http://xameleon.org/function/aws/s3"
  xmlns:header="http://xameleon.org/service/http/header"
  xmlns:metadata="http://xameleon.org/service/metadata"
  xmlns:test="http://xameleon.org/controller/test"
  xmlns:stream="clitype:System.IO.Stream"
  xmlns:sortedlist="clitype:System.Collections.SortedList"
  xmlns:uri="clitype:System.Uri?partialname=System"
  xmlns:http-util="clitype:System.Web.HttpUtility?partialname=System.Web"
  xmlns:web-response="clitype:System.Net.WebResponse?partialname=System"
  xmlns:browser="clitype:System.Web.HttpBrowserCapabilities?partialname=System.Web"
  xmlns:aspnet-server="clitype:System.Web.HttpServerUtility?partialname=System.Web"
  xmlns:aspnet-request="clitype:System.Web.HttpRequest?partialname=System.Web"
  xmlns:aspnet-response="clitype:System.Web.HttpResponse?partialname=System.Web"
  xmlns:aspnet-session="clitype:System.Web.SessionState.HttpSessionState?partialname=System.Web"
  xmlns:aspnet-context="clitype:System.Web.HttpContext?partialname=System.Web"
  xmlns:aspnet-timestamp="clitype:System.DateTime"
  xmlns:request-collection="clitype:Xameleon.Function.HttpRequestCollection?partialname=Xameleon"
  xmlns:response-collection="clitype:Xameleon.Function.HttpResponseCollection?partialname=Xameleon"
  xmlns:web-request="clitype:Xameleon.Function.HttpWebRequestStream?partialname=Xameleon"
  xmlns:http-response-stream="clitype:Xameleon.Function.HttpWebResponseStream?partialname=Xameleon"
  xmlns:s3-object-compare="clitype:Xameleon.Function.S3ObjectCompare?partialname=Xameleon"
  xmlns:http-sgml-to-xml="clitype:Xameleon.Function.HttpSgmlToXml?partialname=Xameleon"
  xmlns:html="http://www.w3.org/1999/xhtml"
  xmlns:timestamp="clitype:System.DateTime"
  exclude-result-prefixes="aspnet-context test http-sgml-to-xml html web-response web-request stream http-response-stream browser http-util uri at aspnet aspnet-timestamp aspnet-server aspnet-session aspnet-request aspnet-response saxon metadata header sortedlist param service operation session func xs xsi fn clitype response-collection request-collection">

  <!-- <xsl:import href="./controller/atomicxml/base.xslt"/>
  <xsl:import href="./controller/aws/s3/base.xslt"/>
  <xsl:import href="./controller/proxy/base.xslt"/>
  <xsl:import href="./functions/funcset-dateTime.xslt"/>
  <xsl:import href="./functions/funcset-Util.xslt"/>
  <xsl:import href="./functions/amazonaws/funcset-s3.xslt"/>
  <xsl:import href="./functions/aspnet/session.xslt"/>
  <xsl:import href="./functions/aspnet/server.xslt"/>
  <xsl:import href="./functions/aspnet/request-stream.xslt"/>
  <xsl:import href="./functions/aspnet/response-stream.xslt"/>
  <xsl:import href="./functions/aspnet/timestamp.xslt"/> -->

  <xsl:param name="current-context" select="aspnet-context:Current()" />
  <xsl:param name="response" select="aspnet-context:Response($current-context)" />
  <xsl:param name="request" select="aspnet-context:Request($current-context)" />
  <xsl:param name="server" select="aspnet-context:Server($current-context)"/>
  <xsl:param name="session" select="aspnet-context:Session($current-context)"/>
  <xsl:param name="timestamp" select="aspnet-context:Timestamp($current-context)"/>
  <xsl:variable name="debug" select="if (request-collection:GetValue($request, 'query-string', 'debug') = 'true') then true() else false()" as="xs:boolean" />
  <xsl:variable name="request-uri" select="aspnet-request:Url($request)"/>
  <xsl:variable name="browser" select="aspnet-request:Browser($request)"/>

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
    <!-- <xsl:apply-templates/> -->
    <xsl:value-of select="timestamp:ToShortDateString($timestamp)"/>
    <xsl:value-of select="uri:ToString($request-uri)"/>
    <xsl:value-of select="browser:Browser($browser)"/>
  </xsl:template>

</xsl:transform>
