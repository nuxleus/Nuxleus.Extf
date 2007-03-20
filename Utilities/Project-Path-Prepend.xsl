<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0">
    <xsl:variable name="prepend-path" select="'..\WebApp'"/>
    <xsl:variable name="remove-path-string" select="'\WebApp'"/>
    <xsl:output method="xml" indent="yes"/>
    <xsl:template match="/include">
        <ItemGroup>
            <xsl:apply-templates select="file"/>
        </ItemGroup>
    </xsl:template>
    <xsl:template match="file">
        <Content Include="{concat($prepend-path, substring-after(.,$remove-path-string))}">
            <Link>
                <xsl:value-of select="substring-after(.,'\WebApp\bin\')"/>
            </Link>
            <CopyToOutputDirectory>Always</CopyToOutputDirectory>
        </Content>
    </xsl:template>
</xsl:stylesheet>
