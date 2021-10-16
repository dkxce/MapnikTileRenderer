/*******************************************
********************************************
		milokz [doggy] gmail.com
********************************************
*******************************************/

// DOC EXISTS

	/* 
		XSLT Quick Guide
			http://www.stylusstudio.com/docs/v62/d_xslt12.html
			http://gazette.linux.ru.net/lg89/danguer.html
			http://www.zvon.org/xxl/XSLTutorial/Output_rus/38.htm
		http://www.w3schools.com/xsl/xsl_client.asp
		http://dev.ektron.com/kb_article.aspx?id=482
		http://ajaxian.com/archives/jslt-a-javascript-alternative-to-xslt
	*/

	// PUBLIC
	function XSLTParser() { this.author = 'Milok Zbrozek (milokz [doggy] gmail.com)'; this.div = document; }
	XSLTParser.prototype.xml = null;
	XSLTParser.prototype.xsl = null;
	XSLTParser.prototype.setDiv 	= function(div) { this.div = div; }
	XSLTParser.prototype.SetXMLFile = function(file_name) { this.xml = loadXMLDoc(file_name); }
	XSLTParser.prototype.SetXMLText = function(text) { this.xml = getXMLDoc(text); }
	XSLTParser.prototype.SetXSLFile = function(file_name) { this.xsl = loadXMLDoc(file_name); }
	XSLTParser.prototype.SetXSLText = function(text) { this.xsl = getXMLDoc(text); }
	XSLTParser.prototype.Execute 	= function(HTMLDiv) {
		var out_div = HTMLDiv ? HTMLDiv : this.div;
		if (window.ActiveXObject) { /* code for IE */ out_div.innerHTML = this.xml.transformNode(this.xsl); }		
		else if (document.implementation && document.implementation.createDocument)
		{  // code for Mozilla, Firefox, Opera, etc.
			xsltProcessor = new XSLTProcessor();
			xsltProcessor.importStylesheet(this.xsl);
			resultDocument = xsltProcessor.transformToFragment(this.xml,document);
			out_div.appendChild(resultDocument);
		};
	}
	XSLTParser.prototype.GetExecutedText  = function() {
		var out_div = document.createElement('div');
		this.Execute(out_div);
		return out_div.innerHTML;
	}
	
	/*
	Example:
		var psr = new XSLTParser();
		psr.setDiv(document.getElementById("test_div"));
		// psr.SetXMLFile('cdcatalog.xml');
		psr.SetXMLText('<?xml version="1.0" encoding="ISO-8859-1"?><catalog><cd><title>Empire Burlesque</title><artist>Bob Dylan</artist><country>USA</country><company>Columbia</company><price>10.90</price><year>1985</year></cd></catalog>');
		// psr.SetXSLFile('cdcatalog.xsl');
		psr.SetXSLText('<?xml version="1.0" encoding="ISO-8859-1"?><xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"><xsl:template match="/"><h2>My CD Collection</h2> <table border="1"><tr bgcolor="#9acd32"><th align="left">Title</th> <th align="left">Artist</th> </tr><xsl:for-each select="catalog/cd"><tr><td><xsl:value-of select="title" /></td><td><xsl:value-of select="artist" /></td></tr></xsl:for-each></table></xsl:template></xsl:stylesheet>');
		psr.Execute();
		alert(psr.GetExecutedText());
		psr = null;
	*/	
	
	// PUBLIC
	function JSLTParser() { this.author = 'Milok Zbrozek (milokz [doggy] gmail.com)'; this.div = document; }
	JSLTParser.prototype.xml = null;
	JSLTParser.prototype.jstext = null;
	JSLTParser.prototype.setDiv 	= function(div) { this.div = div; }
	JSLTParser.prototype.SetXMLFile = function(file_name) { this.xml = loadXMLDoc(file_name); }
	JSLTParser.prototype.SetXMLText = function(text) { this.xml = getXMLDoc(text); }
	JSLTParser.prototype.SetJSFile = function(file_name) { this.jstext = openFile(file_name); }
	JSLTParser.prototype.SetJSText = function(text) { this.jstext = getXMLDoc(text); }
	JSLTParser.prototype.Execute 	= function(HTMLDiv) { (HTMLDiv ? HTMLDiv : this.div).innerHTML = JSLT.compile(this.jstext,true)[0](this.xml); }
	JSLTParser.prototype.GetExecutedText  = function() {
		var out_div = document.createElement('div');
		this.Execute(out_div);
		return out_div.innerHTML;
	}
	
	/*
		Примеры аналогичны с XSLTParser'ом
	*/
	
	/*	
		class XMLConverter
				// Создание экземпляра класса, obj - HTMLDiv для вывода toXml() или null
			public XMLConverter(obj)
				// Преобразование объекта в XML, tagName - имя заглавного тега, options[] - массив опций
			public string toXml(tagName, options)
				// Преобразование объекта в XML, obj - объект, tagName - имя заглавного тега, options[] - массив опций
			public string Obj2Xml(obj,tagName,options)
				// Получение объекта из XML текста
			public object fromXMLText(xml_text)
			public object Xml2Obj(xml_text)
				// Получение объекта из XML файла
			public object fromXMLFile(file_name)
			public object XmlFile2Obj(file_name)
				// Опции преобразования
				// allowOneVALUE - Преобразование: a.value = b --> a = b
				// allowArrayItem - Преобразование: a.item= [b,c,d] --> a = [b,c,d];
			public void SetOptions(allowOneVALUE, allowArrayItem);
			this.allowOneVALUE = false || true;
			this.allowArrayItem = false || true;
			this.ArrayItemText = 'items'; // текст элемента массива 
			
	*/
	
	// PUBLIC
	function XMLConverter(obj) { this.author = 'Milok Zbrozek (milokz [doggy] gmail.com)'; this.obj = obj; }
	
	/**
		* Don't overwrite this method. Overwrite .toXmlTokens() to customize xml for your class.
		* - options[]:
		*   - add_xml_decl: Default true. Set to false to omit <?xml version="1.0" ... ?> declaration.
		*   - version: Default 1.0
		*   - encoding: Default to utf-8
		*   - rails: Replace underscores with '-' for attribute names, just like Ruby on Rails likes. Default false.
	*/
	
	XMLConverter.prototype.type = 'XMLConverter';
	
	//PUBLIC
	XMLConverter.prototype.Obj2Xml = function(obj,tagName,options)
	{
		this.obj = obj;
		return this.toXml(tagName,options);
	}
	
	// PUBLIC
	XMLConverter.prototype.toXml = function(tagName, options) 
	{
		if(!tagName) tagName = '';
		if(!options) options = [];
		options['add_xml_decl'] = options['add_xml_decl'] == undefined ? true    : options['add_xml_decl']
		options['version']      = options['version']      == undefined ? '1.0'   : options['version']
		options['encoding']     = options['encoding']     == undefined ? 'utf-8' : options['encoding']
		options['rails']        = options['rails']        == undefined ? false   : options['rails']
		var tokens = this.toXmlTokens(tagName)
		var chunks = []
		var state = 'STARTDOC'
		for (var i = 0; i < tokens.length; i++) {
			token = tokens[i]
			switch (token[0]) {
			case 'START':
				if (state == 'STARTDOC') {
				if (options['add_xml_decl'] != false) {
					chunks.push('<?xml version="' + options['version'] + '" encoding="' + options['encoding'] + '"?>')
				}
				}
				if (state == 'IN_START') chunks.push('>')
				var name = token[1].toString();
				if (options['rails'])
				name = name.replace(/_/g, '-')
				chunks.push('<' + name)
				state = 'IN_START'
				break
			case 'ATTR':
				var value = token[2].toString().replace(/"/g, '&quot;')
				chunks.push(' ' + token[1] + '="' + value + '"')
				state = 'IN_START'
				break
			case 'TEXT':
				if (state == 'IN_START')
				chunks.push('>')
				var text = token[1].toString();
				text = text.replace(/&/g, '&amp;')
				text = text.replace(/</g, '&lt;')
				text = text.replace(/>/g, '&gt;')
				chunks.push(text)
				state = 'IN_TEXT'
				break
			case 'END':
				if (state == 'IN_START') {
				chunks.push(' />')
				} else {
				var name = token[1].toString();
				if (options['rails'])
					name = name.replace(/_/g, '-')
				chunks.push('</' + name + '>')
				}
				state = 'IN_END'
				break
			}
		}
		var res = chunks.join('');
		if(typeof(this.obj) != 'object' && tagName != '') res = res.replace(typeof(this.obj),tagName).replace(typeof(this.obj),tagName);
		return res;
	};
	
	// PRIVATE
	XMLConverter.prototype.toXmlTokens = function(tagName) 
	{
		var attr_tokens = []
		var other_tokens = []
		
		if (!tagName) tagName = this.obj.tagName || 'object';
		if(parseInt(tagName) == tagName) tagName = 'item'; // ARRAY
		
		if(typeof(this.obj) == 'object') 
		for (e in this.obj) 
		{
			if (this.obj.hasOwnProperty(e)) {
			if (e == 'tagName') {
				tagName = this.obj[e]
			} else if (e == 'innerHTML') {
				other_tokens.push(['TEXT', this.obj[e]])
			} else if (e.charAt(0) == '@') {
				attr_tokens.push(['ATTR', e.substring(1, e.length), this.obj[e]])
			} else 
			{		
				if(typeof(this.obj[e]) == 'object') 
				{
					var _obj = (new XMLConverter(this.obj[e])).toXmlTokens(e);
					other_tokens = other_tokens.concat( _obj );
				}
				else other_tokens = other_tokens.concat( this.toXmlTokens2(e,this.obj[e]) )
			}
		}
	};
	
	var ab = [['START', tagName]];
	ab = ab.concat(attr_tokens);
	ab = ab.concat(other_tokens);
	ab = ab.concat([['END', tagName]]);
	if(typeof(this.obj) != 'object') ab = this.toXmlTokens2(typeof(this.obj),this.obj);	
	return ab; // Array.concat([['START', tagName]], attr_tokens, other_tokens, [['END', tagName]]);
	}

	/*
		*
		* From .toXmlTokens() just return an array with the following tokens:
		* ['START', tagName]
		* ['END', tagName]
		* ['TEXT', text]
		* ['ATTR', attrName, value]
		*
		* Remember that ATTR tokens need to be immediatelly after START token.
	*/
	
	// PRIVATE
	XMLConverter.prototype.toXmlTokens2 = function(tagName,_pbj) 
	{
	if(parseInt(tagName) == tagName) tagName = 'item'; // ARRAY
	var tokens = []
	tokens.push(['START', tagName]);
	tokens.push(['TEXT', _pbj.toString()]);
	tokens.push(['END', tagName]);
	return tokens;
	};
	
	// PUBLIC
	XMLConverter.prototype.Xml2Obj = function(xml_text)
	{
		return this.fromXMLText(xml_text);
	}
	
	// PUBLIC
	XMLConverter.prototype.fromXMLText = function(xml_text)
	{
		var _xmldom = getXMLDoc(xml_text);
		var nn = _xmldom.documentElement.nodeName;
		var obj = [];
		obj[nn] = this.XMLNode2Obj(_xmldom.documentElement,nn);
		return obj;
	}
	
	//PUBLIC
	XMLConverter.prototype.XmlFile2Obj = function(file_name)
	{
		return this.fromXMLFile(file_name);
	}
	
	// PUBLIC
	XMLConverter.prototype.fromXMLFile = function(file_name)
	{
		var _xmldom = loadXMLDoc(file_name);
		var nn = _xmldom.documentElement.nodeName;
		var obj = [];
		obj[nn] = this.XMLNode2Obj(_xmldom.documentElement,nn);
		return obj;
	}
	
	// PUBLIC
	XMLConverter.prototype.fromXMLDOM = function(XMLDOM)
	{
		var _xmldom = XMLDOM;
		var nn = _xmldom.documentElement.nodeName;
		var obj = [];
		obj[nn] = this.XMLNode2Obj(_xmldom.documentElement,nn);
		return obj;
	}
	
	// PUBLIC
	XMLConverter.prototype.SetOptions = function(allowOneVALUE, allowArrayItem)
	{
		this.allowOneVALUE = allowOneVALUE;
		this.allowArrayItem = allowArrayItem;
	}
	XMLConverter.prototype.allowOneVALUE = false;
	XMLConverter.prototype.allowArrayItem = false;
	XMLConverter.prototype.ArrayItemText = 'item';
	
	// PRIVATE
	XMLConverter.prototype.Normalize = function(val)
	{
		if(val == 'false') return false;
		if(val == 'true') return true;
		if(parseInt(val) == val) return parseInt(val);
		if(parseFloat(val) == val) return parseFloat(val);
		return val;
	}
	
	// PRIVATE
	XMLConverter.prototype.XMLNode2Obj = function(node,txt)
	{
		var obj = [];
		if(node.attributes)
		for(i=0;i<node.attributes.length;i++)
		{
			var an = node.attributes[i].nodeName;
			var av = node.attributes[i].nodeValue;
			obj['@'+an] = this.Normalize(av);
		};
		var i = 0;
		var txt = '';
		while(i<node.childNodes.length)
		{
			var nn = node.childNodes[i].nodeName;
			var nv = node.childNodes[i].nodeValue;
			if(nn == '#text' || nn == '#cdata-section') 
			{
				if(obj.value == undefined) obj.value = '';
				obj.value += this.Normalize(nv);
			}
			else
			{
				if(obj[nn] == undefined) obj[nn] = [];
				var val = this.XMLNode2Obj(node.childNodes[i],txt+'.'+nn+'['+(obj[nn].length)+']');
				obj[nn].push(val);
			};
			i++;
		};
		var i = 0;
		while(i<node.childNodes.length)
		{
			var nn = node.childNodes[i].nodeName;
			if((obj[nn] != undefined) && (obj[nn].length != undefined) && (obj[nn].length == 1)) obj[nn] = obj[nn][0];
			i++;
		};
		var pno = 0;
		var eno = 0;
		var ino = 0;
		for(e in obj)
		{
			pno++;
			if(e == 'value') eno++;
			if(e == this.ArrayItemText) ino++;
		};
		if(!this.allowOneVALUE) if(pno == 1 && eno == 1) obj = obj.value;
		if(!this.allowArrayItem) if(pno == ino) obj = obj[this.ArrayItemText];
		return obj;
	}
	
	/* * Example 1 * toXml()
		
			var my_point = {'@id':'1', name:'Точка №1', WGS:{lat:55.45,lon:37.39}, XY:{'@z':'14'},POI:['poi_1','poi_2']};
			my_point.XY.@x = 55450;
			my_point.XY.@y = 37390;
			var options = []; 
			// options['add_xml_decl'] = false; // no add <?xml ...
			// options['encoding'] = 'utf-8'; // no add <?xml ...
			alert((new XMLConverter(my_point)).toXml('mypoint'));
			// alert((new XMLConverter(my_point)).toXml('mypoint',options)); // флаги
		
		** RESULT:
	
			<?xml version="1.0" encoding="utf-8"?>
			<mypoint id="1">
				<name>Точка №1</name>
				<WGS>
					<lat>55.45</lat>
					<lon>37.39</lon>
				</WGS>
				<POI>
					<0>poi_1</0>
					<1>poi_2</1>
				</POI>
				<XY x="55450" y="37390" z="14" />
			</mypoint>
	*/
	
	/* * Example 2 * fromXMLText()
		
			var xml_text = '<?xml version="1.0" encoding="utf-8"?><mypoint id="1"><name>Точка №1</name><WGS><lat>55.45</lat><lon>37.39</lon></WGS><POI><0>poi_1</0><1>poi_2</1></POI><XY x="55450" y="37390" z="14"'; /></mypoint>
			var obj = (new XMLConverter()).fromXMLText(xml_text);
	*/
	
	
	
	
	
	
	/** Global Functions Global Functions Global Functions Global Functions Global Functions Global Functions Global Functions Global Functions Global Functions Global Functions Global Functions  */
	// http://www.devguru.com/Technologies/ecmaScript/quickref/call.html
	
	// load xml document from url 
	function loadXMLDoc(fname)
	{
		var xmlDoc;		
		if (window.ActiveXObject) { xmlDoc=new ActiveXObject("Microsoft.XMLDOM"); /* code for IE */ }		
		else if (document.implementation && document.implementation.createDocument) { xmlDoc=document.implementation.createDocument("","",null); /* code for Mozilla, Firefox, Opera, etc. */ } else throw 'Your browser cannot handle this script';
		xmlDoc.async=false;
		if((navigator.userAgent.indexOf('Chrome') < 0) && (navigator.userAgent.indexOf('Safari') < 0))
		{
			xmlDoc.load(fname);
			return(xmlDoc);
		}
		else 
		{
			//throw 'loadXMLDoc null';
			var xmlhttp = new window.XMLHttpRequest();
			xmlhttp.open("GET",fname,false);
			xmlhttp.send(null);
			xmlDoc = xmlhttp.responseXML;
			return xmlDoc;
		};
	}

	// create xml document from text
	function getXMLDoc(xmldata)
	{
		var xmlDoc = false;
		if (window.ActiveXObject)
		{  // Internet Explorer	
			xmlDoc = new ActiveXObject("Microsoft.XMLDOM"); //new ActiveXObject("Msxml­2.DOMDocument.3.0");+
			xmlDoc.loadXML(xmldata);
			if (xmlDoc.parseError.errorCode) { throw 'IE XML ERROR: '+xml.parseError.reason+' ('+xml.parseError.errorCode+')'; return false; };
			return xmlDoc;
		}
		if (document.implementation && document.implementation.createDocument)
		{  // Mozilla, Opera // xmlDoc = document.implementation.createDocument("","",null);
			var parser = new DOMParser();
			xmlDoc = parser.parseFromString(xmldata, "text/xml");
			return xmlDoc;
		};
		throw 'Your browser cannot handle this script!';
		return false;
	};
	
	// load from server JS script & run it immediately
	// onload = func(url)
	function LoadJS(url, onload, _notHttpReq)
	{
		var notHttpReq = true;
		// if(navigator.userAgent.indexOf("MSIE") > 0) notHttpReq = true;
		// http://www.ejeliot.com/blog/109
		// http://www.javascriptkit.com/javatutors/loadjavascriptcss.shtml
		var _or = onload ? onload : false;		
		var did = document.getElementsByTagName("head")[0];
		var lScript = document.createElement('script');
	    lScript.type = 'text/javascript';
		if(notHttpReq)
		{
			lScript.src = url;
			lScript.onload = onload ? function() { onload(url); } : function() { };
			lScript.onreadystatechange = function() { if (this.readyState == 'loaded' || this.readyState == 'complete') { if(_or) _or(url); };};
		}
		else
		{		
			lScript.innerHTML = openFile(url);
		};
		did.appendChild(lScript);
		if(!notHttpReq) 
		{
			if(onload) onload(url);
		};
	}
	
	// load from server CSS
	// onload = func(url)
	function LoadCSS(url, onload)
	{	
		// http://www.ejeliot.com/blog/109
		// http://www.javascriptkit.com/javatutors/loadjavascriptcss.shtml
		var _or = onload ? onload : false;		
		var did = document.getElementsByTagName("head")[0];
		var lCSS = document.createElement('link');
		lCSS.setAttribute("rel", "stylesheet")
		lCSS.setAttribute("type", "text/css")
		lCSS.setAttribute("href", url)
		lCSS.onload = onload ? function() { onload(url); } : function() { };
		lCSS.onreadystatechange = function() { if (this.readyState == 'loaded' || this.readyState == 'complete') { if(_or) _or(url); };};
		did.appendChild(lCSS);
	}
	
	// load from server JSON object {} and returns it
	function LoadJSONFile(url,obj)
	{
		return (new Function('obj','return '+openFile(url)))(obj);
	}
	
	// load from server JSON object Async {} and call func whit it
	// func (func_obj, status, responseObject, httpReq)
	function LoadJSONFile_Async(url, func, func_obj)
	{
		var fa = function(obj, status, responeText, httpReq) 
		{
			func(obj,status,(new Function('obj','return '+responeText))(obj),httpReq);
		};
		openFile_Async(url,fa,func_obj);
	}
	
	
	// get any url from server & returns response text
	function openFile(url)
	{
		try  {
			netscape.security.PrivilegeManager.enablePrivilege("UniversalBrowserRead");
			//netscape.security.PrivilegeManager.enablePrivilege("UniversalBrowserAccess");
		} catch (e) {};

   
		var httpReq = false;
		if (typeof XMLHttpRequest!='undefined') {
			httpReq = new XMLHttpRequest();
		} else {
			try {
				httpReq = new ActiveXObject("Msxml2.XMLHTTP.4.0");
			} catch (e) {
				try {
					httpReq = new ActiveXObject("Msxml2.XMLHTTP");
				} catch (ee) {
					try {
						httpReq = new ActiveXObject("Microsoft.XMLHTTP");
					} catch (eee) {
						httpReq = false;
					}
				}
			}
		}
		httpReq.open("GET", url, false);
		httpReq.setRequestHeader('Content-Type', 'text/xml');
		if((navigator.userAgent.indexOf('Chrome') < 0) && (navigator.userAgent.indexOf('Safari') < 0))
		  httpReq.setRequestHeader("Content-length", 0);
		//httpReq.setRequestHeader("Cookie","a=b");
		//httpReq.abort() отменяет текущий запрос 
		//httpReq.getAllResponseHeaders() возвращает полный список HTTP-заголовков в виде строки 
		//httpReq.getResponseHeader(headerName) возвращает значение указанного заголовка 
		//httpReq.open(method, URL, async, userName, password) определяет метод, URL и другие опциональные параметры запроса;
		//httpReq.параметр async определяет, происходит ли работа в асинхронном режиме 
		//httpReq.send(content) отправляет запрос на сервер 
		//httpReq.setRequestHeader(label, value) добавляет HTTP-заголовок к запросу 
		//httpReq.overrideMimeType(mimeType) 
		//
		// onreadystatechange Sets or retrieves the event handler for asynchronous requests. 
		// readyState Retrieves the current state of the request operation. 
		// responseBody Retrieves the response body as an array of unsigned bytes. 
		// responseText Retrieves the response body as a string. 
		// responseXML Retrieves the response body as an XML Document Object Model (DOM) object.  
		// status Retrieves the HTTP status code of the request. 
		// statusText Retrieves the friendly HTTP status of the request. 
	
		httpReq.send(null);
		if (httpReq.status == 200) {return httpReq.responseText;} else
		{
			if (httpReq.status == 500) alert("Server Exception: "+httpReq.status);
			return httpReq.status;
		};
	}
	
	// GET URL
	// call func when done
	// func (func_obj, status, responeText, httpReq)
	function openFile_Async(url, func, func_obj)
	{
		try  { netscape.security.PrivilegeManager.enablePrivilege("UniversalBrowserRead"); }  catch (e)  { };
		var httpReq = false;
		if(typeof XDomainRequest!='undefined') httpReq = new XDomainRequest(); // IE8 Beta
		{ if (typeof XMLHttpRequest!='undefined') httpReq = new XMLHttpRequest(); else { try {
		httpReq = new ActiveXObject("Msxml2.XMLHTTP.4.0"); } catch (e) { try { httpReq = new ActiveXObject("Msxml2.XMLHTTP"); } catch (ee) {
		try { httpReq = new ActiveXObject("Microsoft.XMLHTTP"); } catch (eee) { httpReq = false; } } } } };

		httpReq.onreadystatechange = function()
		{ if(httpReq.readyState == 4) func(func_obj,httpReq.status, httpReq.responseText, httpReq); };

		httpReq.open("GET", url, true);
		httpReq.send('');
	}
	
	
	/* JSON EXAMPLE
	
		myopbj = {al:101,kill:false,die:true,'@set':[0,1,2],_arr:['a','b',{c:'d'}],'@str':'hello',vovka:[{},{author:'Milokz'}]};
		var jc = new JSONConverter(myopbj);
		var myopbjT = jc.ToText()
		alert("{al:101,kill:false,die:true,'@set':[0,1,2],_arr:['a','b',{c:'d'}],'@str':'hello',vovka:[{},{author:'Milokz'}]}"+'\n'+myopbjT);
		myopbjO = jc.FromText(myopbjT);
	
	*/ 
	
	// PUBLIC
	function JSONConverter(obj) { this.obj = obj; }
	
	// public
	JSONConverter.prototype.ToText = function(obj)
	{
		this.text = '';		
		if(obj) this.obj = obj;
		if(typeof(this.obj) == 'function') throw "JSON Object cannot be a function";
		if(typeof(this.obj) == 'string') throw '\''+this.obj.toString()+'\'';
		if(typeof(this.obj) != 'object') return this.obj.toString();
		// var myopbj = {al:1,'@set':[0,1,2],_arr:['a','b',{c:'d'}],'@str':'hello'};
		var pc = 0;
		var is_array = false;
		for(var param in this.obj)
		{
			if((parseInt(param) == param) && (this.obj.length != undefined)) is_array = true;
			var isattr = param.indexOf('@') == 0;
			
			var res = {ok:false};
			var txt = this.ParseObj(this.obj[param],res);
			if(res.ok)
			{				
				if(pc > 0) this.text += ',';
				if(!is_array) this.text += (isattr ? '\'' : '') + param +(isattr ? '\'' : '') + ':' 
				this.text += txt;
				pc++;
			};			
		};
		this.text = (is_array ? '[' : '{') + this.text + (is_array ? ']' : '}');
		return this.text;
	}
	
	//private
	JSONConverter.prototype.ParseObj = function(obj,res)
	{
		var str = '';
		if(typeof(obj) == 'function') return;
		res.ok = true;
		if(typeof(obj) == 'number') return obj.toString();
		if(typeof(obj) == 'string') return '\''+obj.toString()+'\'';
		if(typeof(obj) == 'boolean') return obj.toString();
		
		var pc = 0;
		var is_array = false;
		for(var param in obj)
		{
			if((parseInt(param) == param) && (obj.length != undefined)) is_array = true;
			var isattr = param.indexOf('@') == 0;
			
			var res = {ok:false};
			var txt = this.ParseObj(obj[param],res);
			if(res.ok)
			{				
				if(pc > 0) str += ',';
				if(!is_array) str += (isattr ? '\'' : '') + param +(isattr ? '\'' : '') + ':' 
				str += txt;
				pc++;
			};			
		};
		str = (is_array ? '[' : '{') + str + (is_array ? ']' : '}');
		return str;
	}
	
	// public
	JSONConverter.prototype.FromText = function(text)
	{
		this.text = text;
		this.obj = (new Function('','return '+this.text+';'))();
		return this.obj;
	}