<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:asmv2="urn:schemas-microsoft-com:asm.v2" xmlns:asmv1="urn:schemas-microsoft-com:asm.v1"
    version="2.0">

    <xsl:variable name="file-name" 
        select="/asmv1:assembly/asmv1:description/@asmv2:product"/>
    <xsl:variable name="file-extension" 
        select="'.appref-ms'"/>
    <xsl:variable name="deployment-provider"
        select="/asmv1:assembly/asmv2:deployment/asmv2:deploymentProvider/@codebase"/>
    <xsl:variable name="name" 
        select=" /asmv1:assembly/asmv1:assemblyIdentity/@name"/>
    <xsl:variable name="version" 
        select=" /asmv1:assembly/asmv1:assemblyIdentity/@version"/>
    <xsl:variable name="publicKeyToken"
        select=" /asmv1:assembly/asmv1:assemblyIdentity/@publicKeyToken"/>
    <xsl:variable name="language" 
        select=" /asmv1:assembly/asmv1:assemblyIdentity/@language"/>
    <xsl:variable name="processorArchitecture"
        select=" /asmv1:assembly/asmv1:assemblyIdentity/@processorArchitecture"/>
    <xsl:variable name="sep" 
        select="', '"/>

    <xsl:output name="text" method="text"/>

    <xsl:template match="/">
        <xsl:result-document 
            format="text"
            href="{
                concat(
                    translate(
                        $file-name, 
                        ' ', 
                        ''), 
                    $file-extension
                    )
            }">
            <xsl:value-of
                select="
                    concat(
                        $deployment-provider,
                        '#',
                        $name, $sep,
                        'Culture=', $language, $sep,                            
                        'PublicKeyToken=', $publicKeyToken, $sep,
                        'processorArchitecture=', $processorArchitecture
                        )"
            />
        </xsl:result-document>
    </xsl:template>
</xsl:stylesheet>
