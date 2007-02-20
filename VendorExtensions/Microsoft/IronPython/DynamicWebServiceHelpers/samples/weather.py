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

# web service returning weather data as complex objects

import clr
clr.AddReference("DynamicWebServiceHelpers.dll")
from DynamicWebServiceHelpers import *

print 'loading web service'
ws = WebService.Load('http://www.webservicex.net/WeatherForecast.asmx')

print 'calling web service to get forecast'
f = ws.GetWeatherByZipCode('98052')

print 'Forecast for %s, %s ...' % (f.PlaceName, f.StateCode)
for d in f.Details:
    print '    %s: %s - %s' % (d.Day, d.MinTemperatureF, d.MaxTemperatureF)
