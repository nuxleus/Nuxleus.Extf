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
    xmlns:clitype="http://saxon.sf.net/clitype"
    exclude-result-prefixes="xs func enc hmacsha1 clitype sortedlist saxon string convert s3response awsAuth s3object">
    
    
    <!-- <xsl:variable name="setContentType" select="responseHeader:Set($headers, 'ContentType', 'text/xml')"/> -->

  <xsl:function name="func:get-s3-signature">
    <xsl:param name="bucket" as="xs:string"/>
    <xsl:param name="key" as="xs:string"/>
    <xsl:param name="object"/>
    <xsl:param name="pubkey"/>
    <xsl:param name="privkey"/>
    <xsl:param name="issecure" as="xs:boolean"/>
    <xsl:param name="request" as="xs:string"/>
    <xsl:variable name="s3Object" select="s3object:new($object)"/>
    <xsl:variable name="awsauth" select="auth:new($pubkey, $privkey, $issecure)"/>
    <xsl:copy-of 
      select="auth:get($awsauth, $bucket, $key)"/>
  </xsl:function>
  
  <xsl:function name="func:put-s3-object">
    <xsl:param name="bucket" as="xs:string"/>
    <xsl:param name="key" as="xs:string"/>
    <xsl:param name="object"/>
    <xsl:param name="pubkey"/>
    <xsl:param name="privkey"/>
    <xsl:param name="issecure" as="xs:boolean"/>
    <xsl:variable name="s3Object" select="s3object:new($object)"/>
    <xsl:variable name="awsauth" select="awsAuth:new($pubkey, $privkey, $issecure)"/>
    <xsl:copy-of 
      select="s3response:getResponseMessage(awsAuth:put($awsauth, $bucket, $key, $s3Object))"/>
  </xsl:function>

  <xsl:function name="func:s3-create-bucket">
    <xsl:param name="publicKey" as="xs:string"/>
    <xsl:param name="privateKey" as="xs:string"/>
    <xsl:param name="issecure" as="xs:boolean"/>
    <xsl:param name="bucket" as="xs:string"/>
    <xsl:copy-of 
      select="s3response:getResponseMessage(s3:createBucket(s3:new($publicKey, $privateKey, $issecure), $bucket))"/>
  </xsl:function>

</xsl:transform>
