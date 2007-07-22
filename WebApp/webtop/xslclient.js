  
  var platformMoz = (document.implementation && document.implementation.createDocument);
	var platformIE6 = (!platformMoz && document.getElementById && window.ActiveXObject);
	var noXSLT      = (!platformMoz && !platformIE6);

	var msxmlVersion = '3.0';

	var urlXML;
	var urlXSL;
	var docXML;
	var docXSL;
	var target;
	var cache;
	var processor;
  var pos = 0;

	var paramNames = new Array('format','name', 'pos');
	var paramValues = new Array('','','');
	var nParams = paramNames.length;
  
  function flipDiv(div){
    var divVis = document.getElementById(div).style.display;
    document.getElementById(div).style.display = (divVis == 'none' ? 'inline' : 'none');
  }
  
  function showDiv(div){
    var divVis = document.getElementById(div).style.display;
    document.getElementById(div).style.display = (divVis == 'none' ? 'inline' : 'inline');
  }
  
  var foc;
  function changeDivVis(div){
    var nav = foc.concat('nav');
    var divnav = div.concat('nav');
    document.getElementById(divnav).className = 'on';
    document.getElementById(nav).className = 'off';
    document.getElementById(foc).style.display = 'none';
    document.getElementById(div).style.display = 'inline';
    foc = div;
  }
  function setDiv(div){
    foc = div;
  }
	

	if (platformIE6)
	{
		// TODO... find out version of MSXML installed
		cache = new ActiveXObject('Msxml2.XSLTemplate.' + msxmlVersion);
	}

	function initialisexsl()
	{
		/*
    if (noXSLT)
		{
			FatalError();
			return;
		}
		Defaults();
		go();
    */
	}
	function go() {
		//SetParam('format','A');
		//Transform();
	}
  
  function setTransformVars (lxml, lxsl, ltarget) {
    SetTarget(ltarget);
    SetInput(lxml);
    SetStylesheet(lxsl);
  }
  
  function setgetPos(position){
    pos = pos + position;
    return pos;
  }

	function Defaults()
	{
    		SetTarget('target');
    		//SetInput('demo.xml');
    		SetStylesheet('https://www.mygdc.com/static/xsl/lib/xsl/flight-plans-html-css-output.xsl');
	}

	function SetTarget(id)
	{
		target = document.getElementById(id);
	}

	function SetInput(url)
	{
		urlXML = url;
	}

	function SetStylesheet(url)
	{
		urlXSL = url;
	}

	function SetParam(name, value)
	{
		var found = false;

		for (i = 0; i< nParams; i++)
		{
			if (paramNames[i] == name)
			{
				paramValues[i] = value;
				found = true;
			}
		}
		if (!found)
		{
			NoSuchParam(name);
		}
	}

	function FatalError()
	{
		alert("Sorry, this doesn't work in your browser");
	}

	function NoSuchParam(name)
	{
		alert("There is no " + name + " parameter");
	}

	function CreateDocument()
	{
		var doc = null;

		if (platformMoz)
		{
			doc = document.implementation.createDocument('', '', null);
		}
		else if (platformIE6)
		{
			doc = new ActiveXObject('Msxml2.FreeThreadedDOMDocument.' + msxmlVersion);
		}
		return doc;
	}

	function Transform() 
	{
		var strXML = document.getElementById('fpData').innerHTML;
		
		var myXMLDocument_string = strXML.toString();

		if (noXSLT)
		{
			FatalError();
			return;
		}
		
		
		if (platformMoz)
		{
			var parser_XML = new DOMParser();

			docXML = parser_XML.parseFromString(myXMLDocument_string, "text/xml");
			docXSL = CreateDocument();

			docXSL.addEventListener('load', DoTransform, false);
			docXSL.load(urlXSL);
		}
		else if (platformIE6)
		{
			
			docXML = new ActiveXObject("Microsoft.XMLDOM")
      docXML.async="false";
      docXML.loadXML(myXMLDocument_string); 
          
        try {
          process = new ActiveXObject('Msxml2.XSLTemplate.5.0');
          docXSL = new ActiveXObject('Msxml2.FreeThreadedDOMDocument.5.0');
        }
        catch(e){
          try {
            process = new ActiveXObject('Msxml2.XSLTemplate.4.0');
            docXSL = new ActiveXObject('Msxml2.FreeThreadedDOMDocument.4.0');
          }
          catch(e) {
            try {
                process = new ActiveXObject('Msxml2.XSLTemplate.3.0');
                docXSL = new ActiveXObject('Msxml2.FreeThreadedDOMDocument.3.0');
            }
            catch(e) {
                alert('You do not have a supported version of the Microsoft XML Processor Installed.');
            }
        }
      }


			docXSL.async = false;
			docXSL.load(urlXSL);

			DoTransform();
		}
	}

	function DoTransform() 
	{
		if (platformMoz)
		{
			processor = new XSLTProcessor();
			processor.importStylesheet(docXSL);

			AddParams(processor);

			var fragment = processor.transformToFragment(docXML, document);

			while (target.hasChildNodes()) target.removeChild(target.childNodes[0]);

			target.appendChild(fragment);
		}
		else if (platformIE6)
		{
			process.stylesheet = docXSL;

			processor = process.createProcessor();
			processor.input = docXML;

			AddParams(processor);

			processor.transform();

			target.innerHTML = processor.output;
		}
	}

	function AddParams(processor) 
	{
		for (i = 0; i< nParams; i++)
			if (paramValues[i] != '')
				AddParam(processor, paramNames[i], paramValues[i]);
	}

	function AddParam(processor, name, value) 
	{
		if (platformMoz)
		  processor.setParameter(null, name, value);
		else if (platformIE6)
			processor.addParameter(name, value);
	}
  function cancel(){
    return true;
  }
