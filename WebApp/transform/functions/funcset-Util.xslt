<?xml version="1.0"?>
<xsl:transform version="2.0"
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:xs="http://www.w3.org/2001/XMLSchema"
    xmlns:func="http://atomictalk.org/function"
    xmlns:saxon="http://saxon.sf.net/"
    xmlns:enc="clitype:System.Text.UTF8Encoding"
    xmlns:string="clitype:System.String"
    xmlns:hmacsha1="clitype:System.Security.Cryptography.HMACSHA1"
    xmlns:convert="clitype:System.Convert"
    xmlns:s3="clitype:Xameleon.Utility.S3.AWSAuthConnection?partialname=Xameleon"
    xmlns:s3response="clitype:Xameleon.Utility.S3.Response?partialname=Xameleon"
    xmlns:awsAuth="clitype:Xameleon.Utility.S3.AWSAuthConnection?partialname=Xameleon"
    xmlns:auth="clitype:Xameleon.Utility.S3.QueryStringAuthGenerator?partialname=Xameleon"
    xmlns:s3object="clitype:Xameleon.Utility.S3.S3Object?partialname=Xameleon"
    xmlns:sortedlist="clitype:System.Collections.SortedList"
    xmlns:request-collection="clitype:Xameleon.Function.HttpRequestCollection?partialname=Xameleon"
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

  
