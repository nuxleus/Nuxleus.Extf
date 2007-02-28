<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0">
    <xsl:variable name="prepend-path" select="'..\..\vendor\Python-Lib'"/>
    <xsl:variable name="remove-path-string" select="'WebApp\Lib'"/>
    <xsl:output method="xml" indent="yes"/>
    <xsl:template match="/ItemGroup">
        <ItemGroup>
            <xsl:apply-templates select="Content"/>
        </ItemGroup>
    </xsl:template>
    <xsl:template match="Content">
        <Content Include="{concat($prepend-path, substring-after(@Include,$remove-path-string))}">
            <Link>
                <xsl:value-of select="@Include"/>
            </Link>
        </Content>
    </xsl:template>
</xsl:stylesheet>
