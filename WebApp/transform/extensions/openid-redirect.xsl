<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns="http://www.w3.org/1999/xhtml" xmlns:html="http://www.w3.org/1999/xhtml" version="1.0"
    exclude-result-prefixes="html">

    <xsl:variable name="return-uri" select="/auth/return-location" />
    <xsl:variable name="vendor" select="system-property('xsl:vendor')" />

    <xsl:output doctype-system="-//W3C//DTD HTML 4.01//EN"
        doctype-public="http://www.w3.org/TR/html4/strict.dtd" cdata-section-elements="script"
        method="html" omit-xml-declaration="yes" />

    <xsl:template match="/">
        <xsl:apply-templates />
    </xsl:template>

    <xsl:template match="auth">
        <html xmlns="http://www.w3.org/1999/xhtml">
            <head>
                <title>SonicRadar OpenID Redirection...</title>
                <meta http-equiv="content-type" content="text/html; charset=utf-8" />
                <xsl:if test="@redirect-type = 'meta'">
                    <xsl:choose>
                        <xsl:when test="@status = 'redirect'">
                            <xsl:apply-templates select="url" mode="meta-redirect" />
                        </xsl:when>
                        <xsl:when test="@status = 'complete'">
                            <xsl:apply-templates select="message" mode="meta-redirect" />
                        </xsl:when>
                        <xsl:when test="@status = 'session'">
                            <xsl:apply-templates select="url" mode="meta-redirect" />
                        </xsl:when>
                    </xsl:choose>
                </xsl:if>
                <xsl:if test="not(@redirect-type = 'meta')">
                    <script type="text/javascript">
                    <![CDATA[
                    var fs = new RegExp("%2F", "g");
                    var colon = new RegExp("%3A;", "g");
                    var amp = new RegExp("&amp;", "g");
                    function doRedirect(escapedURI){
                        window.location.replace(escapedURI.replace(fs, "/").replace(colon, ":").replace(amp, "&"));
                    };
                    ]]>
                    </script>
                </xsl:if>
            </head>
            <xsl:if test="not(@redirect-type = 'meta')">
                <xsl:choose>
                    <xsl:when test="@status = 'redirect'">
                        <xsl:apply-templates select="url" mode="replace" />
                    </xsl:when>
                    <xsl:when test="@status = 'complete'">
                        <xsl:apply-templates select="message" mode="replace" />
                    </xsl:when>
                    <xsl:when test="@status = 'session'">
                        <xsl:apply-templates select="url" mode="replace" />
                    </xsl:when>
                </xsl:choose>
            </xsl:if>
        </html>
    </xsl:template>

    <xsl:template match="url" mode="replace">
        <body onload="doRedirect('{.}');">
            <h3>Redirecting...</h3>
        </body>
    </xsl:template>

    <xsl:template match="url" mode="meta-redirect">
        <meta http-equiv="Refresh" content="0;url={.}" />
    </xsl:template>

    <xsl:template match="message" mode="replace">
        <body
            onload="doRedirect('{concat('http://sonicradar.com/service/session?return_uri=http://sonicradar.com/?', substring-before(substring-after(., 'http%3A%2F%2F'), '%2F'))}');">
            <h3>Redirecting...</h3>
        </body>
    </xsl:template>

    <xsl:template match="message" mode="meta-redirect">
        <meta http-equiv="Refresh"
            content="0;url={concat($return-uri, substring-before(substring-after(., 'http%3A%2F%2F'), '%2F'))}"
         />
    </xsl:template>

</xsl:stylesheet>
