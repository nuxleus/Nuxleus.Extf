<?xml version="1.0"?>
<xsl:transform version="2.0"
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:xs="http://www.w3.org/2001/XMLSchema"
    xmlns:func="http://atomictalk.org/function"
    xmlns:saxon="http://saxon.sf.net/"
    xmlns:enc="clitype:System.Text.UTF8Encoding?from=file:///usr/lib/mono/2.0/mscorlib.dll"
    xmlns:string="clitype:System.String?from=file:///usr/lib/mono/2.0/mscorlib.dll"
    xmlns:hmacsha1="clitype:System.Security.Cryptography.HMACSHA1?from=file:///usr/lib/mono/2.0/mscorlib.dll"
    xmlns:convert="clitype:System.Convert?from=file:///usr/lib/mono/2.0/mscorlib.dll"
    xmlns:s3="clitype:Extf.Net.S3.AWSAuthConnection?from=file:///srv/wwwroot/webapp/bin/Extf.Net.dll"
    xmlns:s3response="clitype:Extf.Net.S3.Response?from=file:///srv/wwwroot/webapp/bin/Extf.Net.dll"
    xmlns:awsAuth="clitype:Extf.Net.S3.AWSAuthConnection?from=file:///srv/wwwroot/webapp/bin/Extf.Net.dll"
    xmlns:auth="clitype:Extf.Net.S3.QueryStringAuthGenerator?from=file:///srv/wwwroot/webapp/bin/Extf.Net.dll"
    xmlns:s3object="clitype:Extf.Net.S3.S3Object?from=file:///srv/wwwroot/webapp/bin/Extf.Net.dll"
    xmlns:sortedlist="clitype:System.Collections.SortedList?from=file:///usr/lib/mono/2.0/mscorlib.dll"
    xmlns:request-collection="clitype:Xameleon.Function.HttpRequestCollection?from=file:///srv/wwwroot/webapp/bin/Xameleon.dll"
    xmlns:clitype="http://saxon.sf.net/clitype"
    exclude-result-prefixes="xs func enc hmacsha1 clitype sortedlist saxon string convert s3response awsAuth s3object request-collection">

  <xsl:param name="request"/>

  <xsl:function name="func:resolve-variable">
    <xsl:param name="operator"/>
    <xsl:value-of select="if (contains($operator, '{')) then func:evaluate-collection(substring-before(substring-after($operator, '{'), '}')) else $operator"/>
  </xsl:function>

  <xsl:function name="func:evaluate-collection">
    <xsl:param name="operator"/>
    <xsl:value-of select="if (starts-with($operator, '$')) then $session-params[local-name() = substring-after($operator, '$')] else request-collection:GetValue($request, substring-before($operator, ':'), substring-after($operator, ':'))" />
  </xsl:function>

</xsl:transform>

  
