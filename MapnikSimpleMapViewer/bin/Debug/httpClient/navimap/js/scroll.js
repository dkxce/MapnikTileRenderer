/*******************************************
********************************************
		milokz [doggy] gmail.com
********************************************
*******************************************/
<!--
	/** Event handler for mouse wheel event.  */
	//var onthemap = false;
	function wheel(event)
	{	
        var delta = 0;
		/* For IE. */
        if (!event) event = window.event;
		/* IE/Opera. */
        if (event.wheelDelta) 
		{ 
                delta = event.wheelDelta/120;
                /** In Opera 9, delta differs in sign as compared to IE. */
                if (window.opera) delta = -delta;
        } 
		else if (event.detail) 
		{ 
			/** Mozilla case. */
            /** In Mozilla, sign of delta is different than in IE.
                                  * Also, delta is multiple of 3. */
            delta = -event.detail/3;
        }
        /** If delta is nonzero, handle it.
		* Basically, delta is now positive if wheel was scrolled up,
		* and negative, if wheel was scrolled down. */
		var res = true;
		try
		{
			if (delta) if(document.global_tmp_func_scrollevent) res = document.global_tmp_func_scrollevent(delta);			
		}
		catch (e){};        
		if(navigator.userAgent.indexOf('MSIE') < 0)
		{
			/** Prevent default actions caused by mouse wheel.
			* That might be ugly, but we handle scrolls somehow
			* anyway, so don't bother here..*/
			if (res && event.preventDefault) event.preventDefault();
			event.returnValue = false;
		} 
		else 
		{
			res = !res;
		};
		return res;
	}

	/** Initialization code.  */
	/** DOMMouseScroll is for mozilla. */
	if (window.addEventListener)
	{
		window.addEventListener('DOMMouseScroll', wheel, false);
	};
	/** IE/Opera. */
	window.onmousewheel = document.onmousewheel = wheel;

-->