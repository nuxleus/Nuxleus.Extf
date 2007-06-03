<?xml version="1.0" encoding="UTF-8"?>
<xsl:transform xmlns="http://www.w3.org/1999/xhtml" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:app="http://purl.org/atom/app#" xmlns:atom="http://www.w3.org/2005/Atom"
    exclude-result-prefixes="app atom" version="2.0">

    <xsl:param name="xml.base"/>
    <xsl:param name="css-base-class"/>
    <xsl:param name="rights"/>

    <xsl:template match="app:service">
        <xsl:apply-templates/>
    </xsl:template>

    <xsl:template match="app:workspace">
        <xsl:apply-templates/>
    </xsl:template>

    <xsl:template match="atom:title">
        <li>
            <a href="{@href}">
                <xsl:value-of select="."/>
            </a>
        </li>

    </xsl:template>

    <xsl:template match="app:collection">
        <li>
            <xsl:if test="position() = 1">
                <xsl:attribute name="class">selected</xsl:attribute>
            </xsl:if>
            <a href="{@href}">
                <xsl:value-of select="atom:title"/>
            </a>
        </li>
    </xsl:template>

    <xsl:template match="atom:feed" mode="collection-summary">
        <h4><xsl:value-of select="atom:title"/> Feed Info </h4>
        <ul>
            <xsl:apply-templates select="*[not(atom:title)]" mode="collection"/>
            <xsl:if test="atom:entry">
                <li>
                    <h4><xsl:value-of select="atom:title"/> Entries </h4>
                    <xsl:apply-templates select="atom:entry" mode="collection"/>
                </li>
            </xsl:if>
        </ul>
    </xsl:template>
</xsl:transform>
