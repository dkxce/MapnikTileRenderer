/*******************************************
********************************************
		milokz [doggy] gmail.com
********************************************
*******************************************/

// doc exists

////// PARSE URL
	// after #
	
	/*INTERFACE
	
		public class ParseURL
		{
			// Create Method
			public ParseURL(); 
			
			// Get # Parameter
			public string[] getHashParams();
			public string[] getHashParamValues();
			public string getHashParam(name_of);
			// Set # parameter
			public void setHashParam(name_of, val)
			// Del # parameter
			public void delHashParam(name_of)
			
			// Get ? Parameter
			public string[] getSearchParams();
			public string[] getSearchParamValues();
			public string getSearchParam(name_of);
			// Set ? parameter
			public void setSearchParam(name_of, val)
			// Del ? parameter
			public void delSearchParam(name_of)
			
			public string getHashURL(); // #a=1&b=2
			public string getSearchURL(); // ?a=1&b=2
			public string getURL(); // ?a=1&b=2#c=1&d=2
			
			public void SetLoc(); // set document location
			public void Update(); // update from address line
			
			// replaceNormal %20 +
			public string replaceNormal(str_val);
		}
	*/
	
	function ParseURL()
	{
		this.author = 'Milok Zbrozek (milokz [doggy] gmail.com)';
		
	    // #
		
		this.fullhash = document.location.hash;
		
		var hash = this.fullhash.substr(1,this.fullhash.length-1);

		var hash_array = new Array();
		var hash_index = 0;
		while(hash.length > 0)
		{			
			hash_array[hash_index] = new Object();
			if(hash.indexOf('&') > 0) 
			{
				hash_array[hash_index].original = hash.substring(0,hash.indexOf('&'))
				hash = hash.substring(hash.indexOf('&')+1,hash.length);
			}
			else 
			{
				hash_array[hash_index].original = hash;
				hash = '';
			};
			var or = this.replaceNormal(hash_array[hash_index].original);
			hash_array[hash_index].nam = or.substring(0,or.indexOf('='));
			hash_array[hash_index].val = unescape(or.substring(or.indexOf('=')+1,1024));
			hash_index++;
		};
		
		this.hash_array = hash_array;
		
		// ?
		this.fullsearch = document.location.search;
		var search = this.fullsearch.substr(1,this.fullsearch.length-1);
		var search_array = new Array();
		
		var search_index = 0;
		while(search.length > 0)
		{			
			search_array[search_index] = new Object();
			if(search.indexOf('&') > 0) 
			{
				search_array[search_index].original = search.substring(0,search.indexOf('&'))
				search = search.substring(search.indexOf('&')+1,search.length);
			}
			else 
			{
				search_array[search_index].original = search;
				search = '';
			};
			var or = this.replaceNormal(search_array[search_index].original);
			search_array[search_index].nam = or.substring(0,or.indexOf('='));
			search_array[search_index].val = unescape(or.substring(or.indexOf('=')+1,1024));
			search_index++;
		};
		
		this.search_array = search_array;
		
		return this;
	}
	
	ParseURL.prototype.SetLoc = function(reload)
	{
		document.location.replace(this.getURL()); // no refresh #
		if(reload) document.location.reload(this.getURL()); // refresh #
	}
		
	ParseURL.prototype.Update = function()
	{
		var upd = new ParseURL();
		this.fullhash = upd.fullhash;
		this.hash_array = upd.hash_array;
		this.fullsearch = upd.fullsearch;
		this.search_array = upd.search_array;
	}
	
	ParseURL.prototype.replaceNormal = function(str_val)
	{
		var tmphash = str_val;		
		
		var replArrF = new Array("+","%20");
		var replArrT = new Array(" "," ");
		
		for(i=0;i<replArrF.length;i++)
		while(tmphash.indexOf(replArrF[i]) >= 0)
		{
			tmphash = tmphash.substring(0,tmphash.indexOf(replArrF[i])) + replArrT[i] + tmphash.substring(tmphash.indexOf(replArrF[i])+replArrF[i].length,tmphash.length);
		};
		return tmphash;
	}

	ParseURL.prototype.getHashParams = function()
	{
		var outarr = new Array();
		for(i=0;i<this.hash_array.length;i++) outarr[i] = this.hash_array[i].nam;
		return outarr;
	}
	
	ParseURL.prototype.getHashParamValues = function()
	{
		var outarr = new Array();
		for(i=0;i<this.hash_array.length;i++) outarr[i] = this.hash_array[i].val;
		return outarr;
	}
	
	ParseURL.prototype.getHashParam = function(name_of)
	{
		for(i=0;i<this.hash_array.length;i++)
		{
			if(this.hash_array[i].nam.toLowerCase() == name_of.toLowerCase()) return this.hash_array[i].val;
		};
		return null;
	}
	
	ParseURL.prototype.delHashParam = function(name_of)
	{
		var tmpa = [];
		var z = 0;
		for(i=0;i<this.hash_array.length;i++)
		{
			if(this.hash_array[i].nam.toLowerCase() != name_of.toLowerCase()) tmpa[z++] = this.hash_array[i];
		};
		this.hash_array = tmpa;
	}
	
	ParseURL.prototype.setHashParam = function(name_of, val)
	{
		var ex = false;
		for(i=0;i<this.hash_array.length;i++)
		{
			if(this.hash_array[i].nam.toLowerCase() == name_of.toLowerCase()) 
			{
				this.hash_array[i].val = val;
				ex = true;
			};
		};
		if(!ex) 
		{
			var ind = this.hash_array.length;
			this.hash_array[ind] = {};
			this.hash_array[ind].nam = name_of;
			this.hash_array[ind].val = val;
		};
	}
	
	ParseURL.prototype.getSearchParams = function()
	{
		var outarr = new Array();
		for(i=0;i<this.search_array.length;i++) outarr[i] = this.search_array[i].nam;
		return outarr;
	}
	
	ParseURL.prototype.getSearchParamValues = function()
	{
		var outarr = new Array();
		for(i=0;i<this.search_array.length;i++) outarr[i] = this.search_array[i].val;
		return outarr;
	}
	
	ParseURL.prototype.getSearchParam = function(name_of)
	{
		for(i=0;i<this.search_array.length;i++)
		{
			if(this.search_array[i].nam.toLowerCase() == name_of.toLowerCase()) return this.search_array[i].val;
		};
		return null;
	}
	
	ParseURL.prototype.delSearchParam = function(name_of)
	{
		var tmpa = [];
		var z = 0;
		for(i=0;i<this.search_array.length;i++)
		{
			if(this.search_array[i].nam.toLowerCase() != name_of.toLowerCase()) tmpa[z++] = this.search_array[i];
		};
		this.search_array = tmpa;
	}
	
	ParseURL.prototype.setSearchParam = function(name_of, val)
	{
		var ex = false;
		for(i=0;i<this.search_array.length;i++)
		{
			if(this.search_array[i].nam.toLowerCase() == name_of.toLowerCase()) 
			{
				this.search_array[i].val = val;
				ex = true;
			};
		};
		if(!ex) 
		{
			var ind = this.search_array.length;
			this.search_array[ind] = {};
			this.search_array[ind].nam = name_of;
			this.search_array[ind].val = val;
		};
	}
	ParseURL.prototype.getHashURL = function()
	{
		var str = "#";
		for(i=0;i<this.hash_array.length;i++)
		{
			if(str.length > 1) str += "&";
			str += this.hash_array[i].nam.toLowerCase() + "=" + escape(this.hash_array[i].val); 
		};
		if(str.length == 1) str = "";
		return str;
	}
	
	ParseURL.prototype.getSearchURL = function()
	{
		var str = "?";
		for(i=0;i<this.search_array.length;i++)
		{
			if(str.length > 1) str += "&";
			str += this.search_array[i].nam.toLowerCase() + "=" + escape(this.search_array[i].val); 
		};
		if(str.length == 1) str = "";
		return str;
	}
	
	ParseURL.prototype.getURL = function()
	{
		return this.getSearchURL() + this.getHashURL();
	}
	////// END PARSE URL