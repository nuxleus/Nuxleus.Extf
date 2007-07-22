<?xml version="1.0"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">

  <xsl:variable name="files" select="document('xfiles.xml')/*"/>
  <xsl:variable name="user-settings" select="document('user-settings.xml')"/>
  <xsl:variable name="x-webtop-menu" select="document('xwebtop.xml')"/>
  <xsl:variable name="data-feeds" select="document('data-feeds.xml')"/>
  <xsl:output method="html" indent="yes"/>

  <xsl:template match="/">
    <html>
      <head>
        <title>XaMeLeon Webtop Project - Early Alpha</title>
        <style type="text/css">
          <xsl:value-of select="unparsed-text('common.css', 'utf-8')"/>
        </style>
        <script type="text/javascript">
          <xsl:value-of select="unparsed-text('sarissa.js', 'utf-8')"/>
        </script>
        <script type="text/javascript">
          <xsl:value-of select="unparsed-text('xslclient.js', 'utf-8')"/>
        </script>
      </head>
      <body onload="setDiv('{translate($files//file[1]/@src, '-.', '')}')">
        <div id="header" style="float:left; clear:both; width:100%">
          <div style="float:left; background-image: url(x5-webtop.gif)">
            <img src="x5-webtop.gif"/>
          </div>
          <div style="float:right;">
            <div style="float:left; border:1px solid #555; color:#777;padding:2px;margin-left:5px;font-size:small">
            Browse Mode
            </div>
            <div style="float:left; border:1px solid #555; color:#777;padding:2px;margin-left:5px;font-size:small">
            Play Mode
            </div>
            <div style="float:left; border:1px solid #555; color:#777;padding:2px;margin-left:5px;font-size:small">
            Communication Mode
            </div>
            <div style="float:left; border:1px solid #555; color:#777;padding:2px;margin-left:5px;font-size:small">
            Work Mode
            </div>
          </div>
          <div style="float:right:clear:both;color:#933; font-size:large;border-bottom:1px solid #ccc;">
            As of <span style="color:#555"><xsl:value-of select="current-time()"/>
          </span> on <span style="color:#555"><xsl:value-of select="current-date()"/>
          </span>
        </div>
        </div>
        <div id="topbar">
          <div style="float:left;">
            <a href="#" onclick="flipDiv('xwebtop')">Access X:WebTop Menu</a>
          </div>
          <div style="float:left; margin:0px 5px 0px 5px">
            <xsl:text>|</xsl:text>
          </div>
          <div style="float:left;">
            <div style="float:left; color:#933">Data Feeds >></div>
            <div style="float:left">
              <a href="#" onclick="flipDiv('formats')">QuickView</a>
            </div>
            <div style="float:left">:</div>
            <div style="float:left">
              <a href="#" onclick="flipDiv('listall')">View All</a>
            </div>
          </div>
          <div style="float:left; margin:0px 5px 0px 5px">
            <xsl:text>|</xsl:text>
          </div>

          <div style="float:left;">
            <div style="float:left; color:#933">Perspectives >></div>
            <div style="float:left">
              <a href="#" onclick="flipDiv('formats')">QuickView</a>
            </div>
            <div style="float:left">:</div>
            <div style="float:left">
              <a href="#" onclick="flipDiv('perspective')">Change</a>
            </div>
          </div>
          <div style="float:left; margin:0px 5px 0px 5px">
            <xsl:text>|</xsl:text>
          </div>
          <!-- <div style="float:right">
            <div style="float:right;">
              <a href="#" onclick="flipDiv('help')">[Help]</a>
            </div>
            <div style="float:right;">
              <a href="#" onclick="flipDiv('psettings')">[Settings]</a>
            </div>
            <div style="float:right;">
              <a href="#" onclick="flipDiv('search')">[Search]</a>
            </div>
          </div> -->
        </div>
        <div id="xmenu" style="position:relative; clear:both; float:left;z-index:110">
        <!-- X-WebTop DropDown -->
          <div id="xwebtop" style="display:none">
            <ul>
              <xsl:apply-templates select="$x-webtop-menu/xwebtop/menu[@id = 'xwebtop']/item"/>
              <li style="border-bottom:0px;">
                <a href="#" onclick="flipDiv('xwebtop');cancel();">Cancel</a>
              </li>
            </ul>
          </div>
        <!-- End X-WebTop DropDown -->
        <!-- X-WebTop DropDown -->
          <div id="perspective" style="display:none">
            <ul>
              <xsl:apply-templates select="$x-webtop-menu/xwebtop/menu[@id = 'perspectives']/item"/>
              <li style="border-bottom:0px;">
                <a href="#" onclick="flipDiv('perspective');cancel();">Cancel</a>
              </li>
            </ul>
          </div>
        <!-- End X-WebTop DropDown -->
        </div>
        <div id="formats" style="position:relative;float:left;clear:both;background:#fff; width:100%; height:100px;border-bottom:3px solid #933;font-size:14px; display:none">
          <xsl:variable name="left-right-5-nav">
            <div style="width:100%; border-top:1px solid #ccc; border-bottom:1px solid #ccc; margin-top:5px; margin-bottom:5px;">
              <div style="float:left;">
                <a href="#" onclick="setTransformVars('http://66.93.224.14/~mdavid/xameleon/webtop/data-feeds.xml', 'http://66.93.224.14/~mdavid/xameleon/webtop/quick-view.xsl', 'quickview'); SetParam('pos',setgetPos(-5)); Transform();">&lt; Previous 5</a>
              </div>
              <div style="float:right; text-align:right;">
                <a href="#" onclick="setTransformVars('https://www.mygdc.com/static/xsl/lib/xml/fpFormats.xml', 'https://www.mygdc.com/static/xsl/lib/xsl/quick-view.xsl', 'quickview'); SetParam('pos',setgetPos(5)); Transform();">Next 5 &gt;</a>
              </div>
            </div>
          </xsl:variable>
          <xsl:copy-of select="$left-right-5-nav"/>
          <div id="quickview" style="width:100%; float:left;">
            <table style="width:100%">
              <tr>
                <xsl:apply-templates select="$data-feeds/data-feeds/data[position() &lt; 6]" mode="quickview"/>
              </tr>
            </table>
          </div>
          <!-- <xsl:copy-of select="$left-right-5-nav"/> -->
        </div>
        <div id="listall" style="position:relative;float:left;clear:both;width:100%; display:none; padding:5px; margin:5px;margin-bottom:0px;border-top:1px solid #ccc;border-bottom:3px solid #933; ">
          <xsl:apply-templates select="$data-feeds/data-feeds/data" mode="listall"/>
        </div>
        <div style="float:left:clear:both;color:#933;font-size:medium;border-bottom:1px solid #ccc;margin:0px;">
          Current Mode: Browse Mode | Current Perspective: XML Feed Reader
        </div>
        
        <div style="clear:both; display:inline; z-index:20; width:100% padding:5px;">
          <div style="float:left;width:165px;margin-top:5px;">
            <div style="float:left;width:100%;margin-top:5px;margin-right:5px;">
              <div class="mred">Directory</div>
            </div>
          </div>

          <div style="float:right;width:190px;margin-top:5px;">
            <div class="menu">Tasks</div>
            <div class="menu">Messages</div>
            <div class="menu">Contacts</div>
            <div class="menu">XML Channels</div>
            <div class="menu">XML Site Feeds</div>
          </div>

          <div style="float:right;width:60%;margin:5px 5px 5px 5px;">
            <div style="float:left;width:100%">
              <div id="tabnav">
                <xsl:apply-templates select="$files//file" mode="nav"/>
              </div>
              <div id="target">
                <xsl:apply-templates select="$files//file" mode="body"/>
              </div>
            </div>
          </div>

        </div>
      </body>
    </html>
  </xsl:template>
  <xsl:template match="file" mode="nav">
    <xsl:variable name="class">
      <xsl:choose>
        <xsl:when test="position() = 1">
          <xsl:text>on</xsl:text>
        </xsl:when>
        <xsl:otherwise>
          <xsl:text>off</xsl:text>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="id" select="translate(@src, '.-', '')"/>
    <div id="nav">
      <a id="{$id}nav" href="#" class="{$class}" onclick="changeDivVis('{$id}');">
        <xsl:value-of select="@src"/>
      </a>
    </div>
  </xsl:template>
  <xsl:template match="file" mode="body">
    <xsl:variable name="pos" select="position()"/>
    <xsl:variable name="display">
      <xsl:choose>
        <xsl:when test="$pos = 1">
          <xsl:text>inline</xsl:text>
        </xsl:when>
        <xsl:otherwise>
          <xsl:text>none</xsl:text>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="id" select="translate(@src, '.-', '')"/>
    <xsl:variable name="file" select="document(@src)"/>
    <div id="{$id}" style="display:{$display};">
      <h3>
        <xsl:value-of select="@src"/>
      </h3>
      <div id="menu-toolbar" style="margin-bottom:5px;">
        <div id="save{$pos}" style="display:none;border:2px solid #fff;padding:2px;margin:2px;color:#fff; background:#933">
          <a href="#" style="color:#fff;text-decoration:none;font-weight:bold;" onclick="flipDiv('save{$pos}')">Document has Changed. Click to Save.</a>
        </div>
      </div>
      <textarea id="textarea{$pos}" onkeypress="showDiv('save{$pos}');">
        <xsl:copy-of select="$file/*"/>
      </textarea>
    </div>
  </xsl:template>
  <xsl:template match="item">
    <li>
      <a href="#" onclick="flipDiv('{parent::*/@id}');SetParam('format','{@format}');Defaults();Transform();">
        <xsl:value-of select="@name"/>
      </a>
    </li>
  </xsl:template>
  <xsl:template match="data" mode="quickview">
    <td>
      <a href="#" onclick="changeDivVis('formats');SetParam('format','{@match}');Defaults();Transform();">
        <div style="width:175px; height:225px; background:#ffe; padding:5px; border:1px solid #ccc;">
          <div style="color:#555; font-size:small">Feed <xsl:value-of select="@match"/>
          </div>
        </div>
      </a>
    </td>
  </xsl:template>
  <xsl:template match="data" mode="listall">
    <div style="float:left; width:75px; background:#ffe; padding:1px; border:1px solid #ccc; text-align:center">
      <a href="#" onmouseover="document.getElementById('{@match}').style.display = 'inline';" onmouseout="document.getElementById('{@match}').style.display = 'none';" onclick="document.getElementById('{@match}').style.display = 'none';changeDivVis('listall');SetParam('format','{@match}');Defaults();Transform();" style="font-size:x-small; color:#555;">Feed <span style="color:red"><xsl:value-of select="@match"/>
        </span>
      </a>
      <div id="{@match}" style="width:175px; height:225px; background:#ffe; padding:5px; border:1px solid #ccc; display:none; position:absolute; z-index:10">
        Feed <xsl:value-of select="@match"/>
      </div>
    </div>
  </xsl:template>
</xsl:stylesheet>
