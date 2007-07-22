<?xml version="1.0"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">

  <xsl:param name="pos"/>
  <xsl:variable name="data-feeds" select="document('http://66.93.224.14/~mdavid/xameleon/webtop/data-feeds.xml')"/>
  <xsl:template match="/">
    <xsl:variable name="posStart">
      <xsl:choose>
        <xsl:when test="not($pos)">1</xsl:when>
        <xsl:when test="$pos &lt; 0">
          <xsl:value-of select="(count($data-feeds/fpFormats/data) + 1) + $pos"/>
        </xsl:when>
        <xsl:when test="$pos &gt; count($data-feeds/fpFormats/data)">
          <xsl:value-of select="-1 *(count($data-feeds/fpFormats/data) - $pos)"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$pos"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <table style="width:100%">
      <tr>
        <xsl:apply-templates select="$data-feeds/fpFormats/data[position() &gt;= $posStart and position() &lt; ($posStart + 5)]"/>
      </tr>
    </table>
  </xsl:template>
  <xsl:template match="data">
    <td>
      <a href="#" onclick="changeDivVis('formats');SetParam('format','{@match}');Defaults();Transform();">
        <div style="width:175px; height:225px; background:#ffe; padding:5px; border:1px solid #ccc;">
        <div style="color:#555; font-size:small">Feed <xsl:value-of select="@match"/></div>
        </div>
      </a>
    </td>
  </xsl:template>
</xsl:stylesheet>
