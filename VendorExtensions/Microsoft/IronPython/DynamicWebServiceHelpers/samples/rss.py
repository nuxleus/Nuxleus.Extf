#####################################################################################
#
#  Copyright (c) Microsoft Corporation. All rights reserved.
#
#  This source code is subject to terms and conditions of the Shared Source License
#  for IronPython. A copy of the license can be found in the License.html file
#  at the root of this distribution. If you can not locate the Shared Source License
#  for IronPython, please send an email to ironpy@microsoft.com.
#  By using this source code in any fashion, you are agreeing to be bound by
#  the terms of the Shared Source License for IronPython.
#
#  You must not remove this notice, or any other, from this software.
#
######################################################################################

# reading RSS feed with the help of the attribute injector for XML

import clr
clr.AddReference("DynamicWebServiceHelpers.dll")
from DynamicWebServiceHelpers import *

print 'loading RSS channel'
rss = WebService.Load('http://rss.msnbc.msn.com/id/3032091/device/rss/rss.xml')

print 'processing the RSS XML using attribute injectors'
print '%s (%s)' % (rss.channel.title, rss.channel.lastBuildDate)
for i in rss.channel.items: print '    %s (%s)' % (i.title, i.link)
