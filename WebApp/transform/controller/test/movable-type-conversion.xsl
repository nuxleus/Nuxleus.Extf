<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0">

    <xsl:param name="old-base-uri" select="'http://www.lessig.org'"/>
    <xsl:param name="new-base-uri" select="'http://beta.lessig.org'"/>
    
    <xsl:variable name="output">
        <output>
            <file href=".htaccess" type="apache.htaccess" />
            <file href="compare.xml" type="http-redirect-title-test" />
        </output>
    </xsl:variable>

    <xsl:variable name="old" select="document('old.xml')/entries" />
    <xsl:variable name="new" select="document('new.xml')/entries" />
    
    <xsl:variable name="lb">
        <xsl:text>
</xsl:text>
    </xsl:variable>

    <xsl:output name="text" method="text" encoding="UTF-8" />
    <xsl:output name="xml" method="xml" indent="yes" encoding="UTF-8"/>

    <xsl:template match="/">
        <xsl:apply-templates select="$output/output/file" />
    </xsl:template>

    <xsl:template match="file[@type = 'apache.htaccess']">
        <xsl:result-document href="{resolve-uri(@href)}" method="text">
            <xsl:apply-templates select="$new/entry" mode="text" />
        </xsl:result-document>
    </xsl:template>

    <xsl:template match="file[@type = 'http-redirect-title-test']">
        <xsl:result-document href="{resolve-uri(@href)}" method="xml">
            <entries>
                <xsl:apply-templates select="$new/entry" mode="xml" />
            </entries>
        </xsl:result-document>
    </xsl:template>

    <xsl:template match="entry" mode="xml">
        <xsl:variable name="old-uri"
            select="substring-after($old/entry[text() = current()][1]/@href, $old-base-uri)" />
        <entry uri="{concat($new-base-uri, $old-uri)}">
            <xsl:value-of select="$old/entry[text() = current()]/text()" />
        </entry>
    </xsl:template>

    <xsl:template match="entry" mode="text">
        <xsl:variable name="old-uri"
            select="substring-after($old/entry[text() = current()][1]/@href, $old-base-uri)" />
        <xsl:variable name="new-uri" select="@href" />
        <xsl:value-of select="concat('RedirectPermanent ', $old-uri, ' ', $new-uri, $lb)" />
    </xsl:template>

</xsl:stylesheet>
