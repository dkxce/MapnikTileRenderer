/*******************************************************
********************************************************
  * NaviMap JSLib
  * Copyrights (с) 2009-2015 milokz[at]gmail[dot]com
********************************************************
********************************************************/
		
		include("navimap/js/scroll.js");
		include("navimap/js/parseurl.js");
		include("navimap/js/xml_xslt.js");
		include("navimap/js/jquery.js");
		include("navimap/js/raphael.js"); // Vector Graphic Library
		// include("navimap/js/newExcanvas.js");

			var GVARS = {SAL:[],CAL:[]};

			var TNaviMap = function(map_div_id,zooms_div_id,navi_div_id)
			{							
				var thisObj = this; // переменная внутри объекта класса
				(new Function('sender','document.NaviMap_'+map_div_id+' = sender;'))(this); // добавляем ссылку на объект класса к документу
				
				// Registration
				var dc = new Date();
				var ex = Date.parse("Jan 1, 2030");
				if(ex <= dc)
				{
					alert('Регистрационный период истек!');
					return;
				};
				
				//const
				thisObj.image_overflow = 40;
				thisObj.lockable_array = [false /*LoadMap*/,false /*LineMeter a href*/];
				thisObj.nextZindex = 0;
				
				//private
				thisObj.Raphael = null;
				thisObj.RapahelObjects = [];
				
				//public property bool MapLocked readonly 
				thisObj.MapLocked = false;
				
				//private property
				thisObj.MapZooming = false;
				
				//private
				thisObj.LockMap = function(id)
				{
					thisObj.lockable_array[id] = true;
					thisObj.MapLocked = true;
				};
				
				//private
				thisObj.UnlockMap = function(id)
				{
					thisObj.lockable_array[id] = false;
					thisObj.MapLocked = false;
					for(var i=0;i<thisObj.lockable_array.length;i++) if(thisObj.lockable_array[i]) thisObj.MapLocked = true;
				};
				
				//public property debug
				thisObj.DEBUG = false;
				
				// public property tool readonly
				thisObj.tool = 1;
				
				//private
				thisObj.tool_prev = 1;
				
				// public property cursor readonly
				thisObj.cursor = 1;
				
				// public method void SetTool
				thisObj.SetTool = function(tool_no)
				{
					switch (parseInt(tool_no))
					{
						case 0:
						{
							thisObj.tool = 0; // NoAction
							thisObj.cursor = 0;
							thisObj.map_div.prev_cursor = "default";
							if (thisObj.map_div.style.cursor != 'wait') thisObj.map_div.style.cursor = thisObj.map_div.prev_cursor;
							break;
						};
						case 1:
						{
							thisObj.tool = 1; // MapMove
							thisObj.cursor = 1;
							thisObj.map_div.prev_cursor = "url('navimap/cursors/shift_in.cur'), pointer";
							if (thisObj.map_div.style.cursor != 'wait') thisObj.map_div.style.cursor = thisObj.map_div.prev_cursor;
							break;
						};
						case 2:
						{
							thisObj.tool = 2; // ZoomIns
							thisObj.cursor = 2;
							thisObj.map_div.prev_cursor = "url('navimap/cursors/zoom_ins.cur'), crosshair";
							if (thisObj.map_div.style.cursor != 'wait') thisObj.map_div.style.cursor = thisObj.map_div.prev_cursor;
							break;
						};
						case 3:
						{
							thisObj.tool = 3; // ZoomOut
							thisObj.cursor = 3;
							thisObj.map_div.prev_cursor = "url('navimap/cursors/zoom_out.cur'), crosshair";
							if (thisObj.map_div.style.cursor != 'wait') thisObj.map_div.style.cursor = thisObj.map_div.prev_cursor;
							break;
						};
						case 4:
						{
							thisObj.tool = 4; // ZoomIn
							thisObj.cursor = 4;
							thisObj.map_div.prev_cursor = "url('navimap/cursors/zoom_in.cur'), crosshair";
							if (thisObj.map_div.style.cursor != 'wait') thisObj.map_div.style.cursor = thisObj.map_div.prev_cursor;
							break;
						};
						case 5:
						{
							thisObj.tool = 5; // Add vector mode
							thisObj.cursor = 5;
							thisObj.map_div.prev_cursor = "url('navimap/cursors/cross_empty.cur'), crosshair";
							if (thisObj.map_div.style.cursor != 'wait') thisObj.map_div.style.cursor = thisObj.map_div.prev_cursor;
							break;
						};
						case 6:
						{
							thisObj.tool = 6; // Line Meter
							thisObj.cursor = 6;
							thisObj.map_div.prev_cursor = "url('navimap/cursors/cross_empty.cur'), crosshair";
							if (thisObj.map_div.style.cursor != 'wait') thisObj.map_div.style.cursor = thisObj.map_div.prev_cursor;
							//thisObj.ClearTool6();
							break;
						};						
					};		
					
					thisObj.UpdateToolCurrent();
					
					if(thisObj.tool != thisObj.tool_prev) if (thisObj.Events.Map.onChangeTool) thisObj.Events.Map.onChangeTool(thisObj);
					thisObj.tool_prev = thisObj.tool;
					thisObj.ParseUrl.SetLoc();
				};
				
				// pubic User Agent properties information
				thisObj.ua = navigator.userAgent;
				thisObj.ns = ((thisObj.ua.indexOf("etscape") > 0) || (thisObj.ua.indexOf("irefox") > 0));
				thisObj.km = (thisObj.ua.indexOf("K-Meleon") > 0);
				thisObj.sm = (thisObj.ua.indexOf("SeaMonkey") > 0);
				thisObj.ie = (thisObj.ua.indexOf("MSIE") > 0);
				thisObj.op = (thisObj.ua.indexOf("pera") > 0);
				
				thisObj.Assembly = { Title:'NaviMap',Description:'NaviMap JavaScript Library',Configuration:'',Company:'',Product:'NaviMap JSLib',Copyright:'Copyright © milokz[at]gmail[dot]com 2015',Trademark:'',Author:'MZ',Version:'0.2.7.15 MSMC Edition',VersionDate:'21.10.2015' };
				
				// public property string className
				thisObj.className = "TNaviMap";
				// property string interface
				thisObj.interface = "INaviMap";
				
				// public
				thisObj.ToString = function(noCopy)
				{
					var txt = thisObj.Assembly.Title+' ('+(thisObj.Assembly.Description)+') ver. '+thisObj.Assembly.Version;
					if(noCopy != true) txt += ' \n'+thisObj.Assembly.Copyright;
					return txt;
				};
				
				// public property TEventListener EventListener 
				thisObj.EventListener = new TEventListener('document.NaviMap_'+map_div_id+'.EventListener');
				// example to register // sender id thisObj
				// thisObj.EventListener.AddListen('Events.Global.onMouseClick',function(sender,evnt){alert('click');})
				
				// public mouse properties
				// Mouse.Global.x Mouse.Global.y
				// Mouse.Map.x Mouse.Map.y
				thisObj.Mouse = 
				{
					Global:
					{
						x: 0, y: 0,
						lbtnPressed: false,
						click_x: 0, click_y: 0
					},
					Map:
					{
						x: 0, y: 0, 
						lbtnPressed: false,
						click_x: 0, click_y: 0
					},
					MapCenter:
					{
						x: 0, y: 0,
						click_x: 0, click_y: 0
					},
					MapZoom:
					{
						x: 0, y: 0,
						cx: 0, cy: 0
					},
					IsOverMap: false,
					MapMove: false,
					MapSizing: false,
					MapZooming: false
				}
				
				// private override
				thisObj.global_events = 
				{ 
					onmousemove: document.onmousemove,
					onmouseup: document.onmouseup,
					onmousedown: document.onmousedown,
					onclick: document.onclick,
					oncontextmenu: document.oncontextmenu,
					onscroll: document.global_tmp_func_scrollevent
				};
				
				// private property events
				//
				// example to register // sender id thisObj
				// thisObj.EventListener.AddListen('Events.Global.onMouseClick', function(sender,evnt){alert('click');} )
				//
				thisObj.Events =
				{
					Global:
					{
						onMouseMove: thisObj.EventListener.AddEvent('Events.Global.onMouseMove'), // sender, event
						onMouseUp: thisObj.EventListener.AddEvent('Events.Global.onMouseUp'), // sender, event
						onMouseDown: thisObj.EventListener.AddEvent('Events.Global.onMouseDown'), // sender, event
						onMouseClick: thisObj.EventListener.AddEvent('Events.Global.onMouseClick'), // sender, event
						onContextMenu: thisObj.EventListener.AddEvent('Events.Global.onContextMenu'), // sender, event
						onScroll: thisObj.EventListener.AddEvent('Events.Global.onScroll') // sender, delta
					},
					Map:
					{
						onloadimage: thisObj.EventListener.AddEvent('Events.Map.onloadimage'), // sender
						onRedrawVector: thisObj.EventListener.AddEvent('Events.Map.onRedrawVector'), // sender
						onChangeTool: thisObj.EventListener.AddEvent('Events.Map.onChangeTool'), // sender
						onMouseClick: thisObj.EventListener.AddEvent('Events.Map.onMouseClick') // sender
					}
				};
				
				// private 
				thisObj.global_mousemove = function(evnt)
				{
					if(thisObj.ie) evnt = window.event;
					thisObj.Mouse.Global.x = evnt.x;
					thisObj.Mouse.Global.y = evnt.y;
					if(thisObj.ns)
					{
						thisObj.Mouse.Global.x = thisObj.getEventPos(evnt).x;
						thisObj.Mouse.Global.y = thisObj.getEventPos(evnt).y;
					};
					thisObj.Mouse.Map.x = thisObj.Mouse.Global.x - thisObj.left;
					thisObj.Mouse.Map.y = thisObj.Mouse.Global.y - thisObj.top;
					
					thisObj.Mouse.MapCenter.x = thisObj.Mouse.Global.x - thisObj.left - thisObj.width/2;
					thisObj.Mouse.MapCenter.y = thisObj.Mouse.Global.y - thisObj.top - thisObj.height/2;
					
					if ((thisObj.Mouse.Map.x<0) || (thisObj.Mouse.Map.y<0) || (thisObj.Mouse.Map.x>thisObj.width) || (thisObj.Mouse.Map.y>thisObj.height))
						thisObj.Mouse.IsOverMap = false;
					
					if(thisObj.DEBUG)
						if(thisObj.debug_div != null) thisObj.debug_div.innerHTML = thisObj.tool+' '+thisObj.Mouse.MapMove+' locked: '+thisObj.MapLocked+'<br/>'+thisObj.Mouse.Global.x+' '+thisObj.Mouse.Global.y+' '+thisObj.Mouse.Global.lbtnPressed+'<br/>'+thisObj.Mouse.Map.x+' '+thisObj.Mouse.Map.y+' '+thisObj.Mouse.Map.lbtnPressed+'<br/>'+thisObj.Mouse.MapCenter.x+' '+thisObj.Mouse.MapCenter.y+' '+thisObj.Mouse.IsOverMap;
					//									
					
					if(((thisObj.tool == 1) || (thisObj.tool == 6)) && (thisObj.Mouse.Map.lbtnPressed))
					{
						thisObj.Mouse.MapMove = true;
						if((thisObj.tool == 1))
						{
							if(thisObj.cursor != 11) thisObj.map_div.style.cursor = "url('navimap/cursors/hand_move.cur'), crosshair";
							thisObj.cursor = 11;
						};
						var dx = thisObj.Mouse.Global.x-thisObj.Mouse.Global.click_x;
						var dy = thisObj.Mouse.Global.y-thisObj.Mouse.Global.click_y;
						thisObj.map_image_div.style.left = dx+'px';						
						thisObj.map_image_div.style.top = dy+'px';
						thisObj.map_vlayerLineMeter.style.left = dx+'px';
						thisObj.map_vlayerLineMeter.style.top = dy+'px';
					};
					if((thisObj.tool == 2) && (thisObj.Mouse.Map.lbtnPressed))
					{										
						thisObj.Mouse.MapSizing = true;
						thisObj.zoom_overlay.style.display = 'block';
						
						var dxy = thisObj.width / thisObj.height;
						var dx = thisObj.Mouse.Map.x-thisObj.Mouse.Map.click_x;
						var dy = thisObj.Mouse.Map.y-thisObj.Mouse.Map.click_y;
						
						if((dx >= 0) && (dy >= 0))
						{
							var ny = dx/dxy; if(ny>dy) dy = ny;
							var nx = dy*dxy; if(nx>dx) dx = nx;
							
							if((dy+thisObj.Mouse.Map.click_y > thisObj.height))
							{
								dy = thisObj.height - thisObj.Mouse.Map.click_y;
								dx = dy*dxy;
							};
							if((dx+thisObj.Mouse.Map.click_x > thisObj.width))
							{
								dx = thisObj.width - thisObj.Mouse.Map.click_x;
								dy = dx/dxy;
							};
							
							thisObj.zoom_overlay.style.left = thisObj.Mouse.Map.click_x+'px';
							thisObj.zoom_overlay.style.top = thisObj.Mouse.Map.click_y+'px';
							thisObj.zoom_overlay.style.width = Math.abs(dx) + 'px';
							thisObj.zoom_overlay.style.height = Math.abs(dy) + 'px';
						};
						if((dx >= 0) && (dy < 0))
						{
							var ddy = -1*dy							
							var ny = Math.abs(dx/dxy); if(ny>Math.abs(ddy)) ddy = dx/dxy;
							var nx = Math.abs(ddy*dxy); if(nx>Math.abs(dx)) dx = ddy*dxy;
							
							if((thisObj.Mouse.Map.click_y-ddy)<0) 
							{
								ddy = thisObj.Mouse.Map.click_y;
								dx = ddy*dxy;
							};
							if((dx+thisObj.Mouse.Map.click_x > thisObj.width))
							{
								dx = thisObj.width - thisObj.Mouse.Map.click_x;
								ddy = dx/dxy;
							};
							thisObj.zoom_overlay.style.left = thisObj.Mouse.Map.click_x+'px';
							thisObj.zoom_overlay.style.top = thisObj.Mouse.Map.click_y-ddy+'px';
							thisObj.zoom_overlay.style.width = Math.abs(dx) + 'px';
							thisObj.zoom_overlay.style.height = Math.abs(ddy) + 'px';
						};
						if((dx < 0) && (dy >= 0))
						{
							var ddx = -1*dx;
							var ny = Math.abs(ddx/dxy); if(ny>Math.abs(dy)) dy = ddx/dxy;
							var nx = Math.abs(dy*dxy); if(nx>Math.abs(dx)) ddx = dy*dxy;
							
							if((dy+thisObj.Mouse.Map.click_y > thisObj.height))
							{
								dy = thisObj.height - thisObj.Mouse.Map.click_y;
								ddx = dy*dxy;
							};
							if((thisObj.Mouse.Map.click_x-ddx)<0) 
							{
								ddx = thisObj.Mouse.Map.click_x;
								dy = ddx/dxy;
							};
							
							thisObj.zoom_overlay.style.left = thisObj.Mouse.Map.click_x-ddx+'px';
							thisObj.zoom_overlay.style.top = thisObj.Mouse.Map.click_y+'px';
							thisObj.zoom_overlay.style.width = Math.abs(ddx) + 'px';
							thisObj.zoom_overlay.style.height = Math.abs(dy) + 'px';
						};
						if((dx < 0) && (dy < 0))
						{
							var ddx = -1*dx;
							var ddy = -1*dy;
							var ny = Math.abs(ddx/dxy); if(ny>Math.abs(ddy)) ddy = ddx/dxy;
							var nx = Math.abs(ddy*dxy); if(nx>Math.abs(ddx)) ddx = ddy*dxy;
							
							if((thisObj.Mouse.Map.click_y-ddy)<0) 
							{
								ddy = thisObj.Mouse.Map.click_y;
								ddx = ddy*dxy;
							};
							if((thisObj.Mouse.Map.click_x-ddx)<0) 
							{
								ddx = thisObj.Mouse.Map.click_x;
								ddy = ddx/dxy;
							};
														
							thisObj.zoom_overlay.style.left = thisObj.Mouse.Map.click_x-ddx+'px';
							thisObj.zoom_overlay.style.top = thisObj.Mouse.Map.click_y-ddy+'px';
							thisObj.zoom_overlay.style.width = Math.abs(ddx) + 'px';
							thisObj.zoom_overlay.style.height = Math.abs(ddy) + 'px';
						};
					};
					// 
					if (thisObj.Events.Global.onMouseMove) thisObj.Events.Global.onMouseMove(thisObj,evnt);
					if (thisObj.global_events.onmousemove) thisObj.global_events.onmousemove(evnt);
					return false;
				};
				
				// private
				thisObj.getEventPos = function(evnt)
				{
					var ex, ey;
					var tmp_offset = thisObj.getScrollXY();
					if (thisObj.ns || thisObj.km || thisObj.sm) 
					{
						ex = evnt.clientX;
						ey = evnt.clientY;
					}
					else 
					{
						ex = event.clientX;
						ey = event.clientY;
					};
					return {x:ex+tmp_offset.x,y:ey+tmp_offset.y}
				};
				
				// private
				thisObj.getScrollXY = function() 
				{
					var scrOfX = 0, scrOfY = 0;
					if( typeof( window.pageYOffset ) == 'number' ) {
						//Netscape compliant
						scrOfY = window.pageYOffset;
						scrOfX = window.pageXOffset;
					} else if( document.body && ( document.body.scrollLeft || document.body.scrollTop ) ) {
						//DOM compliant
						scrOfY = document.body.scrollTop;
						scrOfX = document.body.scrollLeft;
					} else if( document.documentElement && ( document.documentElement.scrollLeft || document.documentElement.scrollTop ) ) {
						//IE6 standards compliant mode
						scrOfY = document.documentElement.scrollTop;
						scrOfX = document.documentElement.scrollLeft;
					}
					return {x:scrOfX, y:scrOfY};
				}
				
				// private 
				thisObj.global_mouseup = function(evnt)
				{				
					if(thisObj.ie) evnt = window.event;
					var skip = false;
					if(evnt.button == 2) skip = true;
					if(thisObj.ie) if(evnt.button == 0) skip = true;
					if(evnt.which == 3) skip = true;
					var call_click = true;
					if (!skip)
					{
						if(thisObj.tool == 1)
						{
							thisObj.map_div.style.cursor = "url('navimap/cursors/shift_in.cur'), pointer";
							thisObj.cursor = 1;
						};
						if(thisObj.Mouse.MapMove)
						{							
							call_click = false;
							thisObj.Mouse.MapMove = false;
							var dx = thisObj.Mouse.Global.x-thisObj.Mouse.Global.click_x;
							var dy = thisObj.Mouse.Global.y-thisObj.Mouse.Global.click_y;							
							thisObj.map_image_div.style.left = dx+'px';
							thisObj.map_image_div.style.top = dy+'px';
							thisObj.map_vlayerLineMeter.style.left = dx+'px';
							thisObj.map_vlayerLineMeter.style.top = dy+'px';
							//if((Math.abs(dx)>2) && (Math.abs(dy) > 2))
							{		
								var cur_z = thisObj.XYZ.z/(thisObj.width+thisObj.image_overflow);
								var new_x = parseInt(thisObj.XYZ.x-dx*cur_z);
								var new_y = parseInt(thisObj.XYZ.y+dy*cur_z);
								thisObj.LoadMap(new_x,new_y,thisObj.XYZ.z);
								//if(thisObj.ns) thisObj.fixmove.focus();
							};
						};
						if(thisObj.Mouse.MapSizing)
						{
							call_click = false;
							thisObj.Mouse.MapSizing = false;
							thisObj.zoom_overlay.style.display = 'none';
							var tmp_w = parseInt(thisObj.zoom_overlay.style.width); //
							var tmp_h = parseInt(thisObj.zoom_overlay.style.height); //
							var cx = parseInt(thisObj.zoom_overlay.style.left)+tmp_w/2;
							var cy = parseInt(thisObj.zoom_overlay.style.top)+tmp_h/2;
							var cur_z = thisObj.XYZ.z/(thisObj.width+thisObj.image_overflow);
							var new_z = cur_z*tmp_w/(thisObj.width+thisObj.image_overflow);
							new_z = thisObj.Zooms[thisObj.GetZoomIDFromZooms(new_z)];
							var new_x = parseInt(thisObj.XYZ.x+(cx-thisObj.width/2)*cur_z);
							var new_y = parseInt(thisObj.XYZ.y-(cy-thisObj.height/2)*cur_z);
							if(tmp_w > 5)
							{		
								var zpz = new_z/cur_z;
								thisObj.PreZoom(parseInt(cx),parseInt(cy),zpz);
								thisObj.LoadMap2(new_x,new_y,new_z);
								//if(thisObj.ns) thisObj.fixmove.focus();
							};
						};
						thisObj.Mouse.Global.lbtnPressed = false;
						thisObj.Mouse.Map.lbtnPressed = false;
					};
					if (thisObj.Events.Global.onMouseUp) thisObj.Events.Global.onMouseUp(thisObj,evnt);
					if (thisObj.global_events.onmouseup) thisObj.global_events.onmouseup(evnt);
					if (call_click && thisObj.Mouse.IsOverMap && ((thisObj.tool == 1) || (thisObj.tool > 6))) 
						if (thisObj.Events.Map.onMouseClick) 
							thisObj.Events.Map.onMouseClick(thisObj,evnt);
					return false;
				};
				
				// private 
				thisObj.global_mousedown = function(evnt)
				{		
					if(thisObj.ie) evnt = window.event;
					var skip = false;
					if(evnt.button == 2) skip = true;
					if(thisObj.ie) if(evnt.button == 0) skip = true;
					if(evnt.which == 3) skip = true;
					if ((!skip) && (!thisObj.MapLocked) && (!thisObj.MapZooming))
					{
						thisObj.Mouse.Global.click_x = thisObj.Mouse.Global.x;
						thisObj.Mouse.Global.click_y = thisObj.Mouse.Global.y;
						thisObj.Mouse.Map.click_x = thisObj.Mouse.Map.x;
						thisObj.Mouse.Map.click_y = thisObj.Mouse.Map.y;
						thisObj.Mouse.MapCenter.click_x = thisObj.Mouse.MapCenter.x;
						thisObj.Mouse.MapCenter.click_y = thisObj.Mouse.MapCenter.y;
						thisObj.Mouse.Global.lbtnPressed = true;
						if(thisObj.Mouse.IsOverMap) thisObj.Mouse.Map.lbtnPressed = true;
					};
					if (thisObj.Events.Global.onMouseDown) thisObj.Events.Global.onMouseDown(thisObj,evnt);
					if (thisObj.global_events.onmousedown) thisObj.global_events.onmousedown(evnt);
					return true;
				};
				
				// private 
				thisObj.global_mouseclick = function(evnt)
				{						
				    if(document.layers || (document.getElementById && !document.all))
					if (evnt.which==2 || evnt.which==3)
					{
						thisObj.global_oncontextmenu(evnt);
						return true;
					};
				
					if(thisObj.ie) evnt = window.event;
					var skip = false;					
					if(evnt.button == 2) skip = true;
					if(evnt.which == 3) skip = true;
					if ((!skip) && (!thisObj.MapLocked) && (!thisObj.MapZooming))
					{
						if((thisObj.Mouse.IsOverMap) && ((thisObj.tool == 3) || (thisObj.tool == 4))) 
						{
							var tmp = thisObj.tool == 3 ? 2 : 0.5;
							var cur_z = thisObj.XYZ.z/(thisObj.width+thisObj.image_overflow);
							var new_z = (thisObj.XYZ.z*tmp)/(thisObj.width+thisObj.image_overflow);
							if(new_z < thisObj.Zooms[thisObj.GetZoomIDFromZooms(thisObj.Maps[thisObj.XYZ.index].MaxZoom)]) new_z = thisObj.Zooms[thisObj.GetZoomIDFromZooms(thisObj.Maps[thisObj.XYZ.index].MaxZoom)];
							if(new_z > thisObj.Zooms[thisObj.GetZoomIDMaxFromZooms(thisObj.Maps[thisObj.XYZ.index].MinZoom)]) new_z = thisObj.Zooms[thisObj.GetZoomIDMaxFromZooms(thisObj.Maps[thisObj.XYZ.index].MinZoom)];
							var new_x = parseInt(thisObj.XYZ.x+thisObj.Mouse.MapCenter.x*cur_z-thisObj.Mouse.MapCenter.x*new_z);
							var new_y = parseInt(thisObj.XYZ.y-thisObj.Mouse.MapCenter.y*cur_z+thisObj.Mouse.MapCenter.y*new_z);
							thisObj.PreZoom(thisObj.Mouse.Map.x,thisObj.Mouse.Map.y,new_z/cur_z);
							thisObj.LoadMap(new_x,new_y,parseInt(new_z*(thisObj.width+thisObj.image_overflow)));
							//if(thisObj.ns) thisObj.fixmove.focus();
						};
						if((thisObj.Mouse.IsOverMap) && (thisObj.tool == 5)) 
						{
							thisObj.CallTool5();
							//if(thisObj.ns) thisObj.fixmove.focus();
						};
						if((thisObj.Mouse.IsOverMap) && (thisObj.tool == 6)) 
						{
							thisObj.CallTool6();
							//if(thisObj.ns) thisObj.fixmove.focus();
						};
					};
					if (thisObj.Events.Global.onMouseClick) thisObj.Events.Global.onMouseClick(thisObj,evnt);
					if (thisObj.global_events.onclick) thisObj.global_events.onclick(evnt);
				};
				
				// private 
				thisObj.global_oncontextmenu = function(evnt)
				{		
					
					if(thisObj.ie) evnt = window.event;
					var targ = {}; // targ.tagName;
					if (evnt.target) { targ = evnt.target; }
					else if (evnt.srcElement) { targ = evnt.srcElement; };
					
					if (thisObj.Events.Global.onContextMenu) thisObj.Events.Global.onContextMenu(thisObj,evnt);
					if (thisObj.global_events.oncontextmenu) thisObj.global_events.oncontextmenu(evnt);						
				};
				
				// private
				thisObj.global_scroll = function(delta)
				{
					var res = false;
					if(thisObj.HideGroup) return res;
					if((thisObj.Mouse.IsOverMap) && (!thisObj.Mouse.MapMove) && (!thisObj.Mouse.MapSizing)) // && (!thisObj.MapLocked)
					{					
						res = true;
						if(!thisObj.Mouse.MapZooming)
						{
							thisObj.Mouse.MapZoom.x = thisObj.Mouse.Map.x;
							thisObj.Mouse.MapZoom.y = thisObj.Mouse.Map.y;
							thisObj.Mouse.MapZoom.cx = thisObj.Mouse.MapCenter.x;
							thisObj.Mouse.MapZoom.cy = thisObj.Mouse.MapCenter.y;
						}
						else
						{
							thisObj.Mouse.Map.x = thisObj.Mouse.MapZoom.x;
							thisObj.Mouse.Map.y = thisObj.Mouse.MapZoom.y;
							thisObj.Mouse.MapCenter.x = thisObj.Mouse.MapZoom.cx;
							thisObj.Mouse.MapCenter.y = thisObj.Mouse.MapZoom.cy;
						};
						var tmp = delta > 0 ? 0.5 : 2;
							var cur_z = thisObj.XYZ.z/(thisObj.width+thisObj.image_overflow);
							var new_z = (thisObj.XYZ.z*tmp)/(thisObj.width+thisObj.image_overflow);
							new_z = thisObj.Zooms[thisObj.GetZoomIDFromZooms(new_z)];
							if(new_z < thisObj.Zooms[thisObj.GetZoomIDFromZooms(thisObj.Maps[thisObj.XYZ.index].MaxZoom)]) new_z = thisObj.Zooms[thisObj.GetZoomIDFromZooms(thisObj.Maps[thisObj.XYZ.index].MaxZoom)];
							if(new_z > thisObj.Zooms[thisObj.GetZoomIDMaxFromZooms(thisObj.Maps[thisObj.XYZ.index].MinZoom)]) new_z = thisObj.Zooms[thisObj.GetZoomIDMaxFromZooms(thisObj.Maps[thisObj.XYZ.index].MinZoom)];							var new_x = parseInt(thisObj.XYZ.x+thisObj.Mouse.MapCenter.x*cur_z-thisObj.Mouse.MapCenter.x*new_z);
							var new_y = parseInt(thisObj.XYZ.y-thisObj.Mouse.MapCenter.y*cur_z+thisObj.Mouse.MapCenter.y*new_z);												
						thisObj.PreZoom(thisObj.Mouse.Map.x,thisObj.Mouse.Map.y,tmp);	
						thisObj.MapZooming = true;
						thisObj.LoadMap(new_x,new_y,parseInt(new_z*(thisObj.width+thisObj.image_overflow)));						
					};
					//
					if (thisObj.Events.Global.onScroll) if(thisObj.Events.Global.onScroll(thisObj,delta)) res = true;
					if (thisObj.global_events.onscroll) if(thisObj.global_events.onscroll(delta)) res = true;
					return res;
				};							
				
				// перехватываем IE
				document.onmousemove = thisObj.global_mousemove;
				document.onmouseup = thisObj.global_mouseup;
				document.onmousedown = thisObj.global_mousedown;
				document.onclick = thisObj.global_mouseclick;
				document.global_tmp_func_scrollevent = thisObj.global_scroll;				
				if(thisObj.ie)
					document.oncontextmenu = thisObj.global_oncontextmenu;
				
				// перехватываем FF
				if (thisObj.ns) 
				{
					try { document.captureEvents(Event.MOUSEDOWN|Event.MOUSEMOVE|Event.MOUSEUP); } catch (e) {};
					try { document.addEventListener("onmousemove",thisObj.global_mousemove,false);
						  document.addEventListener("onmouseup",thisObj.global_mouseup,false);
						  document.addEventListener("onmousedown",thisObj.global_mousedown,false); } catch (e) {};
				};
				
				//private
				thisObj.PreZoom = function(x,y,scal)
				{							
					var scale = thisObj.PreZoomTTlScale*scal;					
					var nxo = x-(x+thisObj.image_overflow/2)/scale;
					var nyo = y-(y+thisObj.image_overflow/2)/scale;
					
					thisObj.Mouse.MapZooming = true;
					
					if (thisObj.XYZ.zoom*scale < thisObj.Zooms[0]) return;
					if (thisObj.XYZ.zoom*scale < thisObj.Zooms[thisObj.GetZoomIDFromZooms(thisObj.Maps[thisObj.XYZ.index].MaxZoom)]) return;
					if (thisObj.XYZ.zoom*scale > thisObj.Zooms[thisObj.Zooms.length]) return;
					if (thisObj.XYZ.zoom*scale > thisObj.Zooms[thisObj.GetZoomIDMaxFromZooms(thisObj.Maps[thisObj.XYZ.index].MinZoom)]) return;
										
					thisObj.CurrentImage.style.left = nxo+'px';
					thisObj.CurrentImage.style.top = nyo+'px';
					thisObj.CurrentImage.style.width = parseInt((thisObj.width+thisObj.image_overflow)/scale)+'px';
					thisObj.CurrentImage.style.height = parseInt((thisObj.height+thisObj.image_overflow)/scale)+'px';
					thisObj.PreZoomTTlScale = scale;
				};
				thisObj.PreZoomTTlScale = 1;
				
				// protected property object debug_div
				thisObj.debug_div = document.getElementById('debug_div');
				
				// protected property object map_div readonly
				thisObj.map_div = document.getElementById(map_div_id);
				thisObj.map_div.style.cursor = "url('navimap/cursors/shift_in.cur'), pointer";
				thisObj.map_div.style.background = "url(navimap/pngs/NaviMap.png)";
				thisObj.map_div.onmouseover = function() { thisObj.Mouse.IsOverMap = true; };
				thisObj.map_div.onmouseout = function() { thisObj.Mouse.IsOverMap = false; };
				
				// protected property int width readonly
				thisObj.width = parseInt(thisObj.map_div.style.width);
				// protected property int height readonly
				thisObj.height = parseInt(thisObj.map_div.style.height);
				// protected property int left readonly
				thisObj.left = parseInt(thisObj.map_div.style.left);
				// protected property int top readonly
				thisObj.top = parseInt(thisObj.map_div.style.top);
				
				// protected property object map_container readonly
				thisObj.map_container = document.createElement('div');
				thisObj.map_div.appendChild(thisObj.map_container);
				thisObj.map_container.id = map_div_id+"_container";
				thisObj.map_container.style.position = 'absolute';
				thisObj.map_container.style.left = '0px';
				thisObj.map_container.style.top = '0px';
				thisObj.map_container.style.width = thisObj.width+'px';
				thisObj.map_container.style.height = thisObj.height+'px';
				thisObj.map_container.style.zIndex = thisObj.nextZindex++;
				
				// protected property object map_image_div readonly
				thisObj.map_image_div = document.createElement('div');
				thisObj.map_container.appendChild(thisObj.map_image_div);
				thisObj.map_image_div.id = map_div_id+"_image_container";
				thisObj.map_image_div.style.position = 'absolute';
				thisObj.map_image_div.style.left = '0px';
				thisObj.map_image_div.style.top = '0px';
				thisObj.map_image_div.style.width = thisObj.width+'px';
				thisObj.map_image_div.style.height = thisObj.height+'px';
				thisObj.map_image_div.style.zIndex = 0;				
				
				// protected
				thisObj.TileUrlPage = 'http://127.0.0.1:7759/?';
				thisObj.WMSProjection = "EPSG:3395"; // EPSG:3395 or EPSG:4326 or EPSG:3857 
				
				// protected
				thisObj.FormatURL = function(cx,cy,zm)
				{				  
					var mob = thisObj.MapOverflowBounds(); // EPSG:3395					
					var res = thisObj.TileUrlPage;
					if(thisObj.WMSProjection == "EPSG:3395")
					{
						res += '&REQUEST=GetMap&SERVICE=WMS&SRS=EPSG:3395&BBOX='+mob.left+','+mob.bottom+','+mob.right+','+mob.top+'';
					};
					if(thisObj.WMSProjection == "EPSG:4326")
					{
						var leftbottom = thisObj.ConvertPointToLatLon(mob.left,mob.bottom);
						var righttop = thisObj.ConvertPointToLatLon(mob.right,mob.top);
						res += '&REQUEST=GetMap&SERVICE=WMS&SRS=EPSG:4326&BBOX='+leftbottom.lon+','+leftbottom.lat+','+righttop.lon+','+righttop.lat+'';
					};
					if(thisObj.WMSProjection == "EPSG:3857")
					{
						var leftbottom = thisObj.ConvertPointToLatLon(mob.left,mob.bottom);
						var righttop = thisObj.ConvertPointToLatLon(mob.right,mob.top);						
						leftbottom = thisObj.ConvertLatLonToMercatorSphere(leftbottom.lat,leftbottom.lon);
						righttop = thisObj.ConvertLatLonToMercatorSphere(righttop.lat,righttop.lon);
						res += '&REQUEST=GetMap&SERVICE=WMS&SRS=EPSG:3857&BBOX='+leftbottom.x+','+leftbottom.y+','+righttop.x+','+righttop.y+'';
					};
					res += '&HEIGHT='+(thisObj.height+thisObj.image_overflow)+'&WIDTH='+(thisObj.width+thisObj.image_overflow)+'';
					res += '&rnd='+Math.floor(Math.random()*1000000);
					return res;
				}
				
				
				// protected property object map_image readonly
				thisObj.map_image = document.createElement('img');				
				thisObj.map_image_div.appendChild(thisObj.map_image);
				thisObj.map_image.id = map_div_id+"_image_0";
				thisObj.map_image.border = 0;
				thisObj.map_image.style.position = 'absolute';
				thisObj.map_image.style.left = -1*thisObj.image_overflow/2+'px';
				thisObj.map_image.style.top = -1*thisObj.image_overflow/2+'px';
				thisObj.map_image.style.display = 'none';				
				
				// protected property object map_image readonly
				thisObj.map_image2 = document.createElement('img');				
				thisObj.map_image_div.appendChild(thisObj.map_image2);
				thisObj.map_image2.id = map_div_id+"_image_1";
				thisObj.map_image2.border = 0;
				thisObj.map_image2.style.position = 'absolute';
				thisObj.map_image2.style.left = -1*thisObj.image_overflow/2+'px';
				thisObj.map_image2.style.top = -1*thisObj.image_overflow/2+'px';
				thisObj.map_image2.style.display = 'none';
				
				// private
				thisObj.onloadimage = function()
				{	
					if(thisObj.map_image_tmpdiv == null)
					{
						thisObj.map_image_tmpdiv = document.createElement('div');
						thisObj.map_image_tmpdiv.style.position = 'absolute';
						thisObj.map_image_tmpdiv.style.border = 'none';
						thisObj.map_image_tmpdiv.style.zIndex = -1;
						
						thisObj.map_image_tmpimg = document.createElement('img');								
						thisObj.map_image_tmpimg.border = 0;
						thisObj.map_image_tmpimg.style.position = 'absolute';
						thisObj.map_image_tmpimg.style.left = -1*thisObj.image_overflow/2+'px';
						thisObj.map_image_tmpimg.style.top = -1*thisObj.image_overflow/2+'px';						
						thisObj.map_image_tmpdiv.appendChild(thisObj.map_image_tmpimg);
					};
					thisObj.map_image_tmpdiv.style.left = '0px';
					thisObj.map_image_tmpdiv.style.top = '0px';
					thisObj.map_image_tmpdiv.style.width = thisObj.width+'px';
					thisObj.map_image_tmpdiv.style.height = thisObj.height+'px';
					thisObj.map_image_tmpimg.src = thisObj.AnotherImage.src;					
					
					thisObj.map_container.appendChild(thisObj.map_image_tmpdiv);
					setTimeout('document.NaviMap_'+map_div_id+'.onloadimagedone1();',35)
				};
				
				// private
				thisObj.onloadimagedone1 = function()
				{
					thisObj.map_image_div.style.display = 'none';
						
					thisObj.Image = thisObj.CurrentImage;					
					thisObj.CurrentImage = thisObj.AnotherImage;
					thisObj.CurrentImage.style.width = thisObj.width + thisObj.image_overflow;
					thisObj.CurrentImage.style.height = thisObj.height + thisObj.image_overflow;
					
					thisObj.map_image_div.style.left = '0px';
					thisObj.map_image_div.style.top = '0px';					
					thisObj.map_vlayerLineMeter.style.left = '0px';
					thisObj.map_vlayerLineMeter.style.top = '0px';
					
					thisObj.AnotherImage = thisObj.Image;					
					thisObj.AnotherImage.style.display = 'none';															
					thisObj.AnotherImage.style.left = -1*thisObj.image_overflow/2+'px';
					thisObj.AnotherImage.style.top = -1*thisObj.image_overflow/2+'px';
					thisObj.AnotherImage.style.width = thisObj.width + thisObj.image_overflow;
					thisObj.AnotherImage.style.height = thisObj.height + thisObj.image_overflow;
					
					thisObj.CurrentImage.style.display = 'block';
					thisObj.map_image_div.style.display = 'block';
					setTimeout('document.NaviMap_'+map_div_id+'.onloadimagedone2();',35);
				}
				
				thisObj.onloadimagedone2 = function()
				{					
					thisObj.map_container.removeChild(thisObj.map_image_tmpdiv);

					thisObj.UnlockMap(0);
					thisObj.MapZooming = false;
					thisObj.loadingbar.style.display = 'none';
					thisObj.map_div.style.cursor = thisObj.map_div.prev_cursor;
					thisObj.PreZoomTTlScale = 1;
					thisObj.Mouse.MapZooming = false;
					thisObj.map_vector_div.style.display = 'block';
					thisObj.map_vlayerLineMeter.style.display = 'block';					
					
					if (thisObj.Events.Map.onloadimage) thisObj.Events.Map.onloadimage(thisObj);
					
					if (!thisObj.HideGroup)
					{
						thisObj.DoRaphael(); // create vector canvas					
						thisObj.RecalculateZoom(); // draw scale
						if((thisObj.LineMeterTool_MI !== undefined) && (thisObj.LineMeterTool_MI != thisObj.XYZ.index)) 
							thisObj.ClearTool6(); // remove lineMeter
						else
							thisObj.ReloadTool6(); // redraw lineMeter
					};																				
				};
				thisObj.map_image.onload = thisObj.onloadimage;
				thisObj.map_image2.onload = thisObj.onloadimage;
				
				// private
				thisObj.onerrorimage = function(){};
				thisObj.map_image.onerror = thisObj.onerrorimage;
				thisObj.map_image2.onerror = thisObj.onerrorimage;
				
				// private
				thisObj.onabortimage = function(){};
				thisObj.map_image.onabort = thisObj.onabortimage;
				thisObj.map_image2.onabort = thisObj.onabortimage;
				
				thisObj.CurrentImage = thisObj.map_image2;
				thisObj.AnotherImage = thisObj.map_image;
				
				// public информация о центре карты read only
				thisObj.XYZ = 
				{
					index: 0, // Map Index in thisObj.Maps
					x: 0, y: 0, z: 0,
					zoom: 0, // meters per pixel
					zoomID: 0 // zoom index from zooms
				};
															
				//private
				thisObj.Zooms = [];
				thisObj.ZoomsX100 = [];
				for (var tmp_i=2;tmp_i<=19;tmp_i++) thisObj.ZoomsX100[19-tmp_i] = parseInt((thisObj.Zooms[19-tmp_i] = 156543.0339 / Math.pow(2,tmp_i))*100);
				
				//public информация о параметрах карт
				thisObj.Maps =
				[
					{   
						WMSProjection: "EPSG:4326",
						DefaultTileUrlPage: "http://maps.navicom.ru/nms/getMapWMS.ashx?key=TEST",
						MinZoom: thisObj.Zooms[19-4], // Google Zoom 4
						MaxZoom: thisObj.Zooms[19-18], // Google Zoom 18
						DefaultLat: 52.60282642391331, // Latitude
						DefaultLon: 39.57739986595604, // Longitude
						DefaultZoom: thisObj.Zooms[19-12] // Google Zoom 12
					},
					{   
						WMSProjection: "EPSG:3395",
						DefaultTileUrlPage: "http://127.0.0.1:7759/?",
						MinZoom: thisObj.Zooms[19-4], // Google Zoom 4
						MaxZoom: thisObj.Zooms[19-19], // Google Zoom 18
						DefaultLat: 52.60914307763663, // Latitude
						DefaultLon: 39.55299263968651, // Longitude
						DefaultZoom: thisObj.Zooms[19-10] // Google Zoom 10
					}					
				];
				
				// public GetLower ZoomID from Zoom
				thisObj.GetZoomIDFromZooms = function(zoom)
				{					
					var ret = 0; 
					var zoomX100 = parseInt(zoom*100);
					for(var i=0;i<thisObj.Zooms.length;i++) if (thisObj.ZoomsX100[i] <= zoomX100) ret = i;
					return ret;
				};
				
				// public GetHigher ZoomID from Zoom
				thisObj.GetZoomIDMaxFromZooms = function(zoom)
				{
					var ret = thisObj.Zooms.length-1;
					var zoomX100 = parseInt(zoom*100);
					for(var i=thisObj.Zooms.length-1;i>=0;i--) if (thisObj.ZoomsX100[i] >= zoomX100) ret = i;
					return ret;
				};
				
				//public void LoadMapDefaults
				thisObj.LoadMapDefaults = function(mapIndex)
				{
					if(!isNaN(parseInt(mapIndex))) thisObj.XYZ.index = mapIndex; 
					thisObj.WMSProjection = thisObj.Maps[thisObj.XYZ.index].WMSProjection;
					thisObj.TileUrlPage = thisObj.Maps[thisObj.XYZ.index].DefaultTileUrlPage;
					var xy = thisObj.ConvertLatLonToPoint(thisObj.Maps[thisObj.XYZ.index].DefaultLat,thisObj.Maps[thisObj.XYZ.index].DefaultLon);
					thisObj.LoadMap2(xy.x,xy.y,thisObj.Maps[thisObj.XYZ.index].DefaultZoom);
				}
				
				thisObj.ChangeMapIndex = function(mapIndex)
				{										
					if(!isNaN(parseInt(mapIndex))) thisObj.XYZ.index = mapIndex; 										
					thisObj.WMSProjection = thisObj.Maps[thisObj.XYZ.index].WMSProjection;
					thisObj.TileUrlPage = thisObj.Maps[thisObj.XYZ.index].DefaultTileUrlPage;
					thisObj.RefreshMap();
				}
				
				// public
				thisObj.ZoomIn = function()
				{
					thisObj.PreZoom(thisObj.width/2,thisObj.height/2,0.5);
					thisObj.LoadMap(thisObj.XYZ.x,thisObj.XYZ.y,parseInt(thisObj.XYZ.z/2));
				}
				
				// public
				thisObj.SetZoomFromLevels = function(level)
				{
					var d = Math.pow(2,level-thisObj.XYZ.zoomID);
					thisObj.PreZoom(thisObj.width/2,thisObj.height/2,d);
					thisObj.LoadMap2(thisObj.XYZ.x,thisObj.XYZ.y,thisObj.Zooms[level]);
				}
				
				// public
				thisObj.ZoomOut = function()
				{
					thisObj.PreZoom(thisObj.width/2,thisObj.height/2,2);
					thisObj.LoadMap(thisObj.XYZ.x,thisObj.XYZ.y,thisObj.XYZ.z*2);
				}
				
				// public void LoadMap
				thisObj.LoadMap = function(cx,cy,zm)
				{		
					thisObj.XYZ.x = parseInt(cx);
					thisObj.XYZ.y = parseInt(cy);
					thisObj.XYZ.z = parseInt(zm);
					
					thisObj.XYZ.zoom = thisObj.XYZ.z/(thisObj.width+thisObj.image_overflow); // meters per pixel
					if(thisObj.XYZ.zoom < thisObj.Zooms[thisObj.GetZoomIDFromZooms(thisObj.Maps[thisObj.XYZ.index].MaxZoom)])
					{
						thisObj.XYZ.zoom = thisObj.Zooms[thisObj.GetZoomIDFromZooms(thisObj.Maps[thisObj.XYZ.index].MaxZoom)];
						thisObj.XYZ.z = parseInt(thisObj.XYZ.zoom * (thisObj.width+thisObj.image_overflow));
					};
					if(thisObj.XYZ.zoom > thisObj.Zooms[thisObj.GetZoomIDMaxFromZooms(thisObj.Maps[thisObj.XYZ.index].MinZoom)])
					{
						thisObj.XYZ.zoom = thisObj.Zooms[thisObj.GetZoomIDMaxFromZooms(thisObj.Maps[thisObj.XYZ.index].MinZoom)];
						thisObj.XYZ.z = parseInt(thisObj.XYZ.zoom * (thisObj.width+thisObj.image_overflow));
					};
					thisObj.XYZ.zoomID = thisObj.GetZoomIDFromZooms(thisObj.XYZ.zoom);
					thisObj.XYZ.zoom = thisObj.Zooms[thisObj.XYZ.zoomID];
					thisObj.XYZ.z = parseInt(thisObj.XYZ.zoom * (thisObj.width+thisObj.image_overflow));
					thisObj.LockMap(0);
					thisObj.loadingbar.style.display = 'block';
					thisObj.map_vector_div.style.display = 'none';
					thisObj.map_vlayerLineMeter.style.display = 'none';
					if (thisObj.map_div.style.cursor != 'wait') thisObj.map_div.prev_cursor = thisObj.map_div.style.cursor;
					thisObj.map_div.style.cursor = 'wait';
					thisObj.UpdateZoomCurrent();
					thisObj.AnotherImage.src = thisObj.FormatURL(thisObj.XYZ.x,thisObj.XYZ.y,thisObj.XYZ.z);
					thisObj.UpdateURL();
				};
				
				// pubic 
				thisObj.UpdateURL = function()
				{
					if(thisObj.ParseUrlEnabled)
					{
						var cll = thisObj.ConvertPointToLatLon(thisObj.XYZ.x,thisObj.XYZ.y);
						if(thisObj.XYZ.index == 0)
							thisObj.ParseUrl.delHashParam('map');
						else
							thisObj.ParseUrl.setHashParam('map',thisObj.XYZ.index);
						thisObj.ParseUrl.setHashParam('lat',cll.lat);
						thisObj.ParseUrl.setHashParam('lon',cll.lon);
						thisObj.ParseUrl.setHashParam('zoom',19-thisObj.XYZ.zoomID);
						thisObj.ParseUrl.SetLoc();
					};
				};
				
				// public void LoadMap2 zoom - meters per pixel
				thisObj.LoadMap2 = function(cx,cy,zoom)
				{
					thisObj.LoadMap(cx,cy,zoom*(thisObj.width+thisObj.image_overflow));
				};
				
				// public void LoadMap3
				thisObj.LoadMap3 = function(cx,cy,zoomID)
				{
					thisObj.LoadMap(cx,cy,thisObj.Zooms[zoomID]*(thisObj.width+thisObj.image_overflow));
				};								
				
				// public void LoadMap4
				thisObj.LoadMap4 = function(lat,lon,googleZoomId)
				{
					var xy = thisObj.ConvertLatLonToPoint(lat,lon);
					thisObj.LoadMap(xy.x,xy.y,thisObj.Zooms[19-googleZoomId]*(thisObj.width+thisObj.image_overflow));
				};	
				
				
				// public void RefreshMap
				thisObj.RefreshMap = function()
				{		
					thisObj.LockMap(0);
					thisObj.loadingbar.style.display = 'block';
					thisObj.map_vector_div.style.display = 'none';
					thisObj.map_vlayerLineMeter.style.display = 'none';
					thisObj.AnotherImage.src = thisObj.FormatURL(thisObj.XYZ.x,thisObj.XYZ.y,thisObj.XYZ.z);
					return false;
				};
								
				// private
				thisObj.Copyrght_Prefix = thisObj.Assembly.Title+' '+thisObj.Assembly.Version + ' &copy; ' + thisObj.Assembly.Author + ' 2015';
				thisObj.Copyright = thisObj.Copyrght_Prefix;
				
				// public void SetCopyright
				thisObj.SetCopyright = function(text)
				{
					if(thisObj.HideGroup) return;
					thisObj.Copyright = thisObj.Copyrght_Prefix + ', Maps ' + text;
					thisObj.overmap_layer.innerHTML = thisObj.Copyright+' &nbsp;';
				};
				
				//protected property object vector map layers container
				thisObj.map_vector_div = document.createElement('div');	
				thisObj.map_vector_div.id = map_div_id+'_vector_container';
				thisObj.map_vector_div.style.width = '100%';
				thisObj.map_vector_div.style.height = '100%';
				thisObj.map_vector_div.style.position = 'absolute';
				thisObj.map_vector_div.style.left = '0px';
				thisObj.map_vector_div.style.top = '0px';
				thisObj.map_vector_div.style.display = 'none';
				thisObj.map_image_div.appendChild(thisObj.map_vector_div);
				
				//protected property object vector_1 map layer
				thisObj.map_vlayer1 = document.createElement('div');
				thisObj.map_vlayer1.id = map_div_id+'_vector_layer_01';
				thisObj.map_vlayer1.style.width = '100%';
				thisObj.map_vlayer1.style.height = '100%';
				thisObj.map_vlayer1.style.position = 'absolute';
				thisObj.map_vlayer1.style.left = '0px';
				thisObj.map_vlayer1.style.top = '0px';				
				thisObj.map_vlayer1.style.overflow = 'hidden';
				thisObj.map_vector_div.appendChild(thisObj.map_vlayer1);
				
				//protected property object scale div
				thisObj.scale_div = document.createElement('div');	
				thisObj.scale_div.id = map_div_id+'_scale_container';	
				thisObj.scale_div.style.width = '100%';
				thisObj.scale_div.style.height = '20px';
				thisObj.scale_div.style.position = 'absolute';
				thisObj.scale_div.style.left = '0px';
				thisObj.scale_div.style.top = '100%';
				thisObj.scale_div.style.marginTop = '-20px';
				thisObj.scale_div.style.marginLeft = '5px';
				thisObj.scale_div.style.fontSize = '12px';
				//thisObj.scale_div.style.fontWeight = 'bold';
				if(thisObj.ie) thisObj.scale_div.style.filter = 'alpha(opacity=90)'
				else thisObj.scale_div.style.opacity = '0.9';
				thisObj.scale_div.innerHTML = '&nbsp;';
				thisObj.map_div.appendChild(thisObj.scale_div);	
							
				// private
				thisObj.RecalculateZoom = function()
				{
					// private 
					thisObj.Z_Array = [20,50,100,200,400,500,1000,2000,2500,4000,5000,10000,20000,25000,40000,50000,100000,200000,250000,400000,500000,1000000,2000000,2500000,4000000,5000000,10000000];
					thisObj.Z_Elements = 4;
					
					var z_indexes = [], p_length = 0, z_length = 0;
					var d_length = thisObj.GetDistanceInMeters( (thisObj.XYZ.x),(thisObj.XYZ.y),((thisObj.XYZ.x+1000*thisObj.XYZ.zoom)),(thisObj.XYZ.y) ) / 1000;
					//d_length = d_length * 1.8;
					
					for(var i=0;i<thisObj.Z_Array.length;i++)
					{
						var pxls = thisObj.Z_Array[i] / d_length;
						if(pxls >= 48 && pxls <= 140) z_indexes.push(i);
					};
					
					z_length = thisObj.Z_Array[z_indexes[z_indexes.length-1]];
					p_length = Math.floor(z_length / d_length);
					d_length = z_length;
					
					var changed = true;
					if(thisObj.d_d_length_prev !== undefined) changed = thisObj.d_d_length_prev != d_length;
					thisObj.d_d_length_prev = d_length;
					if(changed)
					{		
						var val_ = '<div style="position:absolute;top:0px;left:0px;font-size:10px;width:'+p_length+'px;font-family:Times New Roman;color:#000033;" align="center"><span style="background:white;filter:alpha(opacity=85);-moz-opacity:0.85;opacity: 0.85;-khtml-opacity: 0.85;filter:progid:DXImageTransform.Microsoft.Alpha(opacity=85);">&nbsp;'+(d_length < 1000 ? (d_length + ' м') : (d_length / 1000) + ' км')+'&nbsp;</span></div><div style="width:'+p_length+'px;height:'+(thisObj.ie ? 6 : 4)+'px;background:#000000;border:solid 1px #666666;position:absolute;top:11px;left:0px;filter:alpha(opacity=75);-moz-opacity:0.75;opacity: 0.75;-khtml-opacity: 0.75;filter:progid:DXImageTransform.Microsoft.Alpha(opacity=75);font-size:4px;overflow:hidden;" onclick="MessageDlg(\'Масштаб карты\',\'<br/>Текущий масштаб карты:<br/><b>В '+p_length+' пикселях '+(d_length < 1000 ? (d_length + ' метров') : (d_length / 1000) + ' километров')+'</b>.\',2);">';
						for(var ii=0;ii<thisObj.Z_Elements;ii++)
						{
							val_ += '<div style="position:absolute;left:'+parseInt(p_length/thisObj.Z_Elements*ii)+'px;top:0px;background:'+(ii % 2 ? 'blue' : 'yellow')+';height:4px;width:'+parseInt(p_length/thisObj.Z_Elements+1)+'px;">&nbsp;</div>';
						}
						val_ += '</div><div style="border:solid 1px #666666;border-bottom:none;border-top:none;position:absolute;top:8px;left:0px;width:'+p_length+'px;height:8px;font-size:3px;">&nbsp;</div>';
						
						thisObj.scale_div.innerHTML = val_;
					};														
				}
				
				// public
				// Получения расстояния между 2-мя точками
				thisObj.GetDistanceInMeters = function(x0,y0,x1,y1)
				{
					var spoint = thisObj.ConvertPointToLatLon(x0,y0);					
					var test = thisObj.ConvertLatLonToPoint(spoint.lat,spoint.lon);
					var epoint = thisObj.ConvertPointToLatLon(x1,y1);
					return thisObj.GetDistanceInLatLon(spoint.lat,spoint.lon,epoint.lat,epoint.lon);
				}
				
				// public
				// Получения расстояния между 2-мя точками
				thisObj.GetDistanceInLatLon	 = function(sLat,sLon,eLat,eLon)
				{
					// var pi = 3.14159265358979323;
					var EarthRadius = 6378137.0;
	
					var lon1, lon2, lat1, lat2;
			
					/*
					var dlon, dlat, a, c,  dist;
				
					lon1 = parseFloat (sLon);
					lat1 = parseFloat (sLat);
					lon2 = parseFloat (eLon);
					lat2 = parseFloat (eLat);

					// This algorithm is called Sinnott's Formula
			
					dlon = DegToRad (lon2) - DegToRad (lon1);
					dlat = DegToRad (lat2) - DegToRad (lat1);
					a = Math.pow (Math.sin (dlat/2), 2.0) + Math.cos (lat1) * Math.cos (lat2) * Math.pow (Math.sin (dlon/2), 2.0);
					c = 2 * Math.asin (Math.sqrt (a));
					dist = EarthRadius * c;
					}*/
			
					lon1 = DegToRad (parseFloat (sLon));
					lon2 = DegToRad (parseFloat (eLon));
					lat1 = DegToRad (parseFloat (sLat));
					lat2 = DegToRad (parseFloat (eLat));
					return  EarthRadius * (Math.acos (Math.sin (lat1) * Math.sin (lat2) + Math.cos (lat1) * Math.cos (lat2) * Math.cos (lon1-lon2)));
					return true;
				}
				
				thisObj.lat2y_m = function (lat) { return 6378137 * Math.log(Math.tan(3.14159265358979323 / 4 + DegToRad(lat) / 2)); }
				thisObj.lon2x_m = function (lon) { return DegToRad(lon) * 6378137; }
				thisObj.ConvertLatLonToMercatorSphere = function(lat,lon) { return {x:thisObj.lon2x_m(lon),y:thisObj.lat2y_m(lat)}; };
				
				// public
				// Преобразование метровых координат в Lat Lon result: {Lat,Lon}
				thisObj.ConvertPointToLatLon = function(x_meters,y_meters)
				{
					var Easting = x_meters;
					var Northing = y_meters;
					//return ConvertXYToGEO(x_meters,y_meters);
					var PI = 3.14159265358979323;
					var PI_OVER_2 = (PI / 2.0);
					// MAX_LAT = ((PI * 89.5) / 180.0); /* 89.5 degrees in radians         */

					/* Ellipsoid Parameters, default to WGS 84 */
					var Merc_a = 6378137.0;    /* Semi-major axis of ellipsoid in meters */

						/* Mercator projection Parameters */
					var Merc_Origin_Long = 0.0;     /* Longitude of origin in radians    */
					var Merc_False_Northing = 0.0;  /* False northing in meters          */
					var Merc_False_Easting = 0.0;   /* False easting in meters           */
					var Merc_Scale_Factor = 1.0;    /* Scale factor                      */

						/* Isometric to geodetic latitude parameters, default to WGS 84 */
					var Merc_ab = 0.00335655146887969400;
					var Merc_bb = 0.00000657187271079536;
					var Merc_cb = 0.00000001764564338702;
					var Merc_db = 0.00000000005328478445;
	
						/* Maximum variance for easting and northing values for WGS 84.
						*/
					var Merc_Delta_Easting = 20237883.0;
					var Merc_Delta_Northing = 23421740.0;
	
					var dx;     /* Delta easting - Difference in easting (easting-FE)      */
					var dy;     /* Delta northing - Difference in northing (northing-FN)   */
					var xphi;   /* Isometric latitude                                      */
					var Longitude;
					var Latitude;
					var latlong = {lat:0,lon:0};
					var Error_Code = false;

					if ((Easting < (Merc_False_Easting - Merc_Delta_Easting)) || (Easting > (Merc_False_Easting + Merc_Delta_Easting)))
					{ /* Easting out of range */
						Error_Code = false;
					}
					if ((Northing < (Merc_False_Northing - Merc_Delta_Northing)) || (Northing > (Merc_False_Northing + Merc_Delta_Northing)))
					{ /* Northing out of range */
						Error_Code = false;
					}
					if (!Error_Code)
					{ /* no errors */
						dy = Northing - Merc_False_Northing;
						dx = Easting - Merc_False_Easting;
						Longitude = Merc_Origin_Long + dx / (Merc_Scale_Factor * Merc_a);
						xphi = PI / 2 - 2 * Math.atan(1 / Math.exp(dy / (Merc_Scale_Factor * Merc_a)));
						Latitude = xphi + Merc_ab * Math.sin(2 * xphi) + Merc_bb * Math.sin(4 * xphi) + Merc_cb * Math.sin(6 * xphi) + Merc_db * Math.sin(8 * xphi);
						if (Longitude > PI)
							Longitude -= (2 * PI);
						if (Longitude < -PI)
							Longitude += (2 * PI);

						// convert radians to degrees before outputting.
						latlong.lat = (Latitude * (180 / PI));
						latlong.lon = (Longitude * (180 / PI));
					}
					else
					{
						latlong.lat = -999;
						latlong.lon = -999;
					}
					return (latlong);
				}
		
				// public
				// Преобразование Lat Lon в метровые координаты result: {x,y}
				thisObj.ConvertLatLonToPoint = function(lat,lon)
				{
					//return ConvertGEOToXY(lat,lon);
					var r_major = 6378137.000;				
					var r_minor = 6356752.3142;
					var temp = r_minor / r_major;
					var es = 1.0 - (temp * temp);
					var eccent = Math.sqrt(es);
					var phi = DegToRad(lat);
					var sinphi = Math.sin(phi);
					var con = eccent * sinphi;
					var com = .5 * eccent;
					con = Math.pow(((1.0-con)/(1.0+con)), com);
					var ts = Math.tan(.5 * ((Math.PI*0.5) - phi))/con;
					var res = {};
					res.y = 0 - r_major * Math.log(ts);
					res.x = r_major * DegToRad(lon);
					return res;
				}
				
				// public
				// перевод числа в base-ричную систему
				function IntToBase(d,base,digits) 
				{
					var base35 = '0123456789ABCDEFGHIJKLMNOPQRSTUVWXY';
					var d2 = d, res = '';
					while(d2 >= base)
					{
						res = base35.charAt(d2 % base) + res;
						d2  = parseInt(d2 / base);
					};
					if(d2 > 0) res = base35.charAt(d2)+res;
					if(res.length == 0) res = '0';
					if(digits) while (res.length < digits) res = '0'+res;
					return res;
				}
				
				// public
				// перевод числа в 16-ричную систему
				function IntToHex(intval,digits) { return IntToBase(intval,16,digits); }
				// public
				// перевод числа в 35-ричную систему
				function IntTo35(intval,digits) { return IntToBase(intval,35,digits); }
				// public
				// перевод градусов в радианы
				function DegToRad (deg) { var pi = 3.14159265358979323; return (deg / 180.0 * pi); }
				
				//overmap objects
				//protected property object copyright object
				thisObj.overmap_layer = document.createElement('div');			
				thisObj.overmap_layer.id = map_div_id+'_copyright_container';
				thisObj.overmap_layer.style.position = 'absolute';
				thisObj.overmap_layer.style.width = '100%';
				thisObj.overmap_layer.style.height = '12px';
				thisObj.overmap_layer.style.textAlign = 'right';
				thisObj.overmap_layer.style.left = '0px';
				thisObj.overmap_layer.style.top = '100%';
				thisObj.overmap_layer.style.marginTop = '-16px';
				thisObj.overmap_layer.style.fontSize = '10px';
				//thisObj.overmap_layer.style.fontWeight = 'bold';
				thisObj.overmap_layer.style.fontFamily = 'Verdana, Arial, MS Sans Serif';
				if(thisObj.ie) thisObj.overmap_layer.style.filter = 'alpha(opacity=60)'
				else thisObj.overmap_layer.style.opacity = '0.6';
				thisObj.overmap_layer.innerHTML = thisObj.Copyright+' &nbsp;';
				thisObj.map_div.appendChild(thisObj.overmap_layer);	
				
				// protected property object zoom_overlay for ZoomIns readonly
				thisObj.zoom_overlay = document.createElement('div');
				thisObj.map_div.appendChild(thisObj.zoom_overlay);
				thisObj.zoom_overlay.id = map_div_id+"_zoom_container";
				thisObj.zoom_overlay.style.position = 'absolute';
				thisObj.zoom_overlay.style.left = '10px';
				thisObj.zoom_overlay.style.top = '10px';
				thisObj.zoom_overlay.style.width = '200px';
				thisObj.zoom_overlay.style.height = '100px';
				thisObj.zoom_overlay.style.zIndex = thisObj.nextZindex++;				
				thisObj.zoom_overlay.style.border = 'solid 1px red';	
				thisObj.zoom_overlay.style.fontSize = '0px';
				thisObj.zoom_overlay_fill = document.createElement('div');
				thisObj.zoom_overlay_fill.id = map_div_id+'_zoom_container_fill';
				thisObj.zoom_overlay.appendChild(thisObj.zoom_overlay_fill);
				thisObj.zoom_overlay_fill.style.background = 'red';
				thisObj.zoom_overlay_fill.style.width = '100%';
				thisObj.zoom_overlay_fill.style.height = '100%';
				if(thisObj.ie) {
					thisObj.zoom_overlay.style.filter = 'alpha(opacity=80)';
					thisObj.zoom_overlay_fill.style.filter = 'alpha(opacity=20)';
				} else {
					thisObj.zoom_overlay.style.opacity = '0.8';
					thisObj.zoom_overlay_fill.style.opacity = '0.2';
				};
				thisObj.zoom_overlay.style.display = 'none';
				
				// protected property object map_overlay protection readonly
				thisObj.map_overlay = document.createElement('div');
				thisObj.map_overlay.id = map_div_id+'_map_protect';
				thisObj.map_div.appendChild(thisObj.map_overlay);
				thisObj.map_overlay.id = map_div_id+"_fixmove";
				thisObj.map_overlay.style.position = 'absolute';
				thisObj.map_overlay.style.left = '0px';
				thisObj.map_overlay.style.top = '0px';
				thisObj.map_overlay.style.width = thisObj.width+'px';
				thisObj.map_overlay.style.height = thisObj.height+'px';
				thisObj.map_overlay.style.zIndex = thisObj.nextZindex++;
				thisObj.map_overlay.style.background = "url(navimap/gifs/devider.gif) repeat";
				thisObj.fixmove = document.createElement('input');
				thisObj.fixmove.id = map_div_id+'_fix_move';
				thisObj.fixmove.type="text";
				thisObj.fixmove.style.display = 'none';
				thisObj.map_overlay.appendChild(thisObj.fixmove);
				
				//protected property object vector_2 map layer
				thisObj.map_vlayerLineMeter = document.createElement('div');
				thisObj.map_vlayerLineMeter.id = map_div_id+'_vector_layer_LineMeter';
				thisObj.map_vlayerLineMeter.style.width = '100%';
				thisObj.map_vlayerLineMeter.style.height = '100%';
				thisObj.map_vlayerLineMeter.style.position = 'absolute';
				thisObj.map_vlayerLineMeter.style.left = '0px';
				thisObj.map_vlayerLineMeter.style.top = '0px';				
				thisObj.map_vlayerLineMeter.style.overflow = 'hidden';
				thisObj.map_vlayerLineMeter.style.fontFamily = 'Arial, Verdana, MS Sans Serif';
				thisObj.map_overlay.appendChild(thisObj.map_vlayerLineMeter);	
				
				// protected property object loadingbar
				thisObj.loadingbar = document.createElement('div');
				thisObj.loadingbar.id = map_div_id+'_loading_bar';
				//thisObj.loadingbar.style.background = 'white';
				//thisObj.loadingbar.style.border = 'dashed 1px silver';
				thisObj.loadingbar.style.padding = '5px';
				thisObj.loadingbar.style.position = 'absolute';
				thisObj.loadingbar.style.left = '50%';
				thisObj.loadingbar.style.top = '50%';
				thisObj.loadingbar.style.marginLeft = '-150px';
				thisObj.loadingbar.style.marginTop = '-31px';
				thisObj.loadingbar.style.width = '300px';
				thisObj.loadingbar.style.height = '62px';
				thisObj.loadingbar.style.textAlign='center';
				thisObj.loadingbar.style.zIndex = thisObj.nextZindex++;
				thisObj.loadingbar.innerHTML = '<img src="navimap/gifs/loading.gif"/><br/>Подождите, выполняется загрузка карты...<br/>';
				thisObj.loadingrefresh = document.createElement('a');
				thisObj.loadingrefresh.style.fontSize = '11px';
				thisObj.loadingrefresh.style.textDecoration = 'none';
				thisObj.loadingrefresh.href = '#';
				thisObj.loadingrefresh.innerHTML = 'если долго не грузится - нажмите здесь';
				thisObj.loadingrefresh.onclick = function() { thisObj.RefreshMap(); return false; };
				thisObj.loadingbar.appendChild(thisObj.loadingrefresh);
				thisObj.loadingbar.style.display = 'none';
				//if(thisObj.ie) thisObj.loadingbar.style.filter = 'alpha(opacity=70)'
				//else thisObj.loadingbar.style.opacity = '0.7';
				thisObj.map_div.appendChild(thisObj.loadingbar);		
				
				// public method void SetMapSize(int width, int height);
				thisObj.SetMapSize = function(wi,he)
				{
					var wasw = thisObj.width;
					var wash = thisObj.height;
					var indw = wi/thisObj.width;
					var indh = he/thisObj.he;
					thisObj.width = wi;
					thisObj.height = he;
					
					thisObj.map_div.style.width = thisObj.width+'px';
					thisObj.map_div.style.height = thisObj.height+'px';

					thisObj.map_container.style.width = thisObj.width+'px';
					thisObj.map_container.style.height = thisObj.height+'px';
					
					thisObj.map_image_div.style.width = thisObj.width+'px';
					thisObj.map_image_div.style.height = thisObj.height+'px';
					
					thisObj.map_overlay.style.width = thisObj.width+'px';
					thisObj.map_overlay.style.height = thisObj.height+'px';
					
					thisObj.Mouse.MapCenter.x = thisObj.Mouse.Global.x - thisObj.left - thisObj.width/2;
					thisObj.Mouse.MapCenter.y = thisObj.Mouse.Global.y - thisObj.top - thisObj.height/2;
					
					thisObj.XYZ.x = thisObj.XYZ.x+(thisObj.width-wasw)/2*(thisObj.XYZ.zoom);
					thisObj.XYZ.y = thisObj.XYZ.y-(thisObj.height-wash)/2*(thisObj.XYZ.zoom);
					thisObj.XYZ.zoomID = thisObj.GetZoomIDFromZooms(thisObj.XYZ.z*indw/(thisObj.width+thisObj.image_overflow));
					thisObj.XYZ.zoom = thisObj.Zooms[thisObj.XYZ.zoomID];
					thisObj.XYZ.z = parseInt(thisObj.XYZ.zoom * (thisObj.width+thisObj.image_overflow));
										
					thisObj.LockMap(0);
					thisObj.loadingbar.style.display = 'block';
					thisObj.map_vector_div.style.display = 'none';
					thisObj.map_vlayerLineMeter.style.display = 'none';
					thisObj.AnotherImage.src = thisObj.FormatURL(thisObj.XYZ.x,thisObj.XYZ.y,thisObj.XYZ.z);
					thisObj.UpdateURL();
				};
				
				// public method void MoveMap(int left, int top);
				thisObj.MoveMap = function(left,top)
				{
					thisObj.left = left;
					thisObj.top = top;
					thisObj.map_div.style.left = thisObj.left+'px';
					thisObj.map_div.style.top = thisObj.top+'px';
					
					thisObj.Mouse.Map.x = thisObj.Mouse.Global.x - thisObj.left;
					thisObj.Mouse.Map.y = thisObj.Mouse.Global.y - thisObj.top;
					
					thisObj.Mouse.MapCenter.x = thisObj.Mouse.Global.x - thisObj.left - thisObj.width/2;
					thisObj.Mouse.MapCenter.y = thisObj.Mouse.Global.y - thisObj.top - thisObj.height/2;
				};
				
				// private
				thisObj.ParseUrlEnabled = true;
				thisObj.ParseUrl = thisObj.ParseUrlEnabled ? new ParseURL() : null;				
				
				// public
				thisObj.LoadMapFromUrl = function(mapIndex)
				{
					if (!thisObj.ParseUrlEnabled) return false;
					if (thisObj.ParseUrl.getHashParam('lat') && thisObj.ParseUrl.getHashParam('lon') && thisObj.ParseUrl.getHashParam('zoom'))
					{
						thisObj.XYZ.index = 0;
						if(!isNaN(parseInt(mapIndex))) 
							thisObj.XYZ.index = mapIndex; 
						else if(thisObj.ParseUrl.getHashParam('map'))
							thisObj.XYZ.index = thisObj.ParseUrl.getHashParam('map'); 
						thisObj.WMSProjection = thisObj.Maps[thisObj.XYZ.index].WMSProjection;
						thisObj.TileUrlPage = thisObj.Maps[thisObj.XYZ.index].DefaultTileUrlPage;
						thisObj.LoadMap4(thisObj.ParseUrl.getHashParam('lat'),thisObj.ParseUrl.getHashParam('lon'),thisObj.ParseUrl.getHashParam('zoom'));
						return true;
					};
					return false;
				};
				
				// private
				thisObj.DrawArrow = function( in_path, in_x1, in_y1, in_x2, in_y2)
				{
					// Arrow 
					var k1 = (in_y2-in_y1)/(in_x2-in_x1);
					var k2 = -1/k1;					
					if ((k2 == Infinity) || (k2 == -Infinity))
					{
						in_path.moveTo(in_x2+(k2==Infinity?20:-20),in_y2+5);
						in_path.lineTo(in_x2,in_y2);
						in_path.moveTo(in_x2+(k2==Infinity?20:-20),in_y2-5);
						in_path.lineTo(in_x2,in_y2);
					}
					else
					{
						var alpha = Math.atan(k1);
						var len = (in_y2-in_y1)/Math.sin(alpha);										
						if ((in_x2-in_x1) >= 0) len-=20; else len+=20;
						var cx = in_x1+len*Math.cos(alpha);
						var cy = in_y1+len*Math.sin(alpha);
						var b = cy - k2*cx;
					
						
						var dx = cx+5*Math.sin(alpha);
						var dy = k2*dx+b;					
						in_path.moveTo(parseInt(dx),parseInt(dy));
						in_path.lineTo(in_x2,in_y2);
						dx = cx-5*Math.sin(alpha);
						dy = k2*dx+b
						in_path.moveTo(parseInt(dx),parseInt(dy));
						in_path.lineTo(in_x2,in_y2);
					};
				};
				
				// private
				thisObj.DoRaphael = function()
				{	
					return;															
					
					try {  if(!thisObj.Raphael) thisObj.Raphael = Raphael(thisObj.map_vlayer1,thisObj.width,thisObj.height);  }
					catch (e) {  setTimeout('document.NaviMap_'+map_div_id+'.DoRaphael();',200); return;  };
					
					if (thisObj.Events.Map.onRedrawVector) thisObj.Events.Map.onRedrawVector(thisObj); // Call Vector Methods
					
					while (thisObj.RapahelObjects.length > 0)
					{
						thisObj.RapahelObjects[thisObj.RapahelObjects.length-1].remove();
						thisObj.RapahelObjects.pop();
					};										
					
					var p5 = thisObj.Raphael.path({stroke:"#000000",'width':'2px'});
					p5.moveTo(70,80);
					var x1 = 170;
					var y1 = 280;
					var x2 = 250;
					var y2 = 170;
					p5.lineTo(x1,y1);
					p5.lineTo(x2,y2);
					thisObj.DrawArrow(p5,x1,y1,x2,y2);
					thisObj.RapahelObjects.push(p5);
				}
				
				//private
				thisObj.CallTool5 = function()
				{
							if (thisObj.ra === undefined) 
							{	
								thisObj.ra = thisObj.Raphael.path({stroke:"#FF0000",'stroke-width':'2px'});
								thisObj.dots = [];
								thisObj.ra_a = [];																
							};								
							thisObj.ra_a.push({x:thisObj.Mouse.Map.x,y:thisObj.Mouse.Map.y});
							thisObj.ra.remove();
							thisObj.ra = thisObj.Raphael.path({stroke:"#FF0000",'stroke-width':'2px'});
							if(thisObj.ra_a.length > 0) thisObj.ra.moveTo(thisObj.ra_a[0].x,thisObj.ra_a[0].y);
							for(var i=1;i<thisObj.ra_a.length;i++) thisObj.ra.lineTo(thisObj.ra_a[i].x,thisObj.ra_a[i].y);
							if(thisObj.ra_a.length > 1) thisObj.DrawArrow(thisObj.ra,thisObj.ra_a[thisObj.ra_a.length-2].x,thisObj.ra_a[thisObj.ra_a.length-2].y,thisObj.ra_a[thisObj.ra_a.length-1].x,thisObj.ra_a[thisObj.ra_a.length-1].y);
							if(thisObj.ra_a.length == 1)
							{
								var dot = thisObj.Raphael.circle(thisObj.Mouse.Map.x,thisObj.Mouse.Map.y,2).attr({fill: "#fff", stroke: "#f00"});
								thisObj.dots.push(dot);
							};
				}
				
				//private
				thisObj.CallTool6 = function()
				{
					if ((thisObj.LineMeterTool === undefined) || (thisObj.LineMeterTool == false)) 
					{	
						thisObj.LineMeterTool = thisObj.Raphael.path({stroke:"#0000FF",'stroke-width':'4px'});
						thisObj.LineMeterTool_Dots = [];
						thisObj.LineMeterTool_Dots_Length = 0;
						thisObj.LineMeterTool_Points = [];							
					};						
					thisObj.LineMeterTool_MI = thisObj.XYZ.index;
					var m_dot = thisObj.ScreenToMeters(thisObj.Mouse.Map.x,thisObj.Mouse.Map.y);
					thisObj.LineMeterTool_Points.push(m_dot);
					thisObj.ReloadTool6();
				}
				
				// private
				thisObj.ReloadTool6 = function()
				{
					if (thisObj.LineMeterTool === undefined) return;
					if (thisObj.LineMeterTool_Points.length == 0) return;
					
					thisObj.LineMeterTool.remove();
					thisObj.LineMeterTool = thisObj.Raphael.path({stroke:"#0000FF",'stroke-width':'2px'});
					if(thisObj.LineMeterTool_Points.length > 0) 
					{
						var s_dot = thisObj.MetersToScreen(thisObj.LineMeterTool_Points[0].x,thisObj.LineMeterTool_Points[0].y);
						thisObj.LineMeterTool.moveTo(parseInt(s_dot.x),parseInt(s_dot.y));
					};
					for(var i=1;i<thisObj.LineMeterTool_Points.length;i++) 
					{
						var s_dot = thisObj.MetersToScreen(thisObj.LineMeterTool_Points[i].x,thisObj.LineMeterTool_Points[i].y);
						thisObj.LineMeterTool.lineTo(parseInt(s_dot.x),parseInt(s_dot.y));
					};
					for(var i=0;i<thisObj.LineMeterTool_Dots.length;i++) thisObj.LineMeterTool_Dots[i].remove();
					thisObj.LineMeterTool_Dots = [];
					thisObj.LineMeterTool_Dots_Length = 0;
					thisObj.map_vlayerLineMeter.innerHTML = '';
					for(var i=0;0<thisObj.LineMeterTool_Points.length;i++) 
					{
						var curr_length = 0;
						if(i>0)
						{
							curr_length = thisObj.GetDistanceInMeters(thisObj.LineMeterTool_Points[i-1].x,thisObj.LineMeterTool_Points[i-1].y,thisObj.LineMeterTool_Points[i].x,thisObj.LineMeterTool_Points[i].y) / 1000;// * 1.8;
						};
						thisObj.LineMeterTool_Dots_Length += curr_length;
					
						var fc = "#fff";
						if (i==0) fc = "#ff0";
						 else if (i==thisObj.LineMeterTool_Points.length-1) fc = "#0f0";
						var s_dot = thisObj.MetersToScreen(thisObj.LineMeterTool_Points[i].x,thisObj.LineMeterTool_Points[i].y);
						var dot = thisObj.Raphael.circle(parseInt(s_dot.x),parseInt(s_dot.y),3).attr({fill: fc, stroke: "#00f"});
						thisObj.LineMeterTool_Dots.push(dot);
												
						var dot_txt = '&nbsp;'+thisObj.LineMeterTool_Dots_Length.toFixed(2)+' км&nbsp;';
						if(i==0) dot_txt = '&nbsp;старт&nbsp;';
						else
							if(i==thisObj.LineMeterTool_Points.length-1) dot_txt += ' <a href="#" onclick="document.NaviMap_'+map_div_id+'.ClearTool6(); setTimeout(\'document.NaviMap_'+map_div_id+'.UnlockMap(1);\',250); return false;" title="закрыть" onmouseover="document.NaviMap_'+map_div_id+'.LockMap(1);" onmouseout="document.NaviMap_'+map_div_id+'.UnlockMap(1);">x</a>';
												
						var dot_div = document.createElement('div');
						dot_div.style.position = 'absolute';
						dot_div.style.left = parseInt(s_dot.x)-10+'px';
						dot_div.style.top = parseInt(s_dot.y)+5+'px';
						dot_div.style.width = 'auto';
						dot_div.style.textAlign = 'center';
						dot_div.style.fontSize = '11px';
						dot_div.style.border = 'solid 1px silver';
						dot_div.style.background = '#FFFFFF';
						if(thisObj.ie) dot_div.style.filter = 'alpha(opacity=85)'
						else dot_div.style.opacity = '0.85';
						dot_div.innerHTML = dot_txt;
						thisObj.map_vlayerLineMeter.appendChild(dot_div);
					};
				}
				
				// private
				thisObj.ClearTool6 = function()
				{
					if ((thisObj.LineMeterTool !== undefined) && (thisObj.LineMeterTool != false)) 
					{
						thisObj.LineMeterTool.remove();
						thisObj.LineMeterTool = false;
						for(var i=0;i<thisObj.LineMeterTool_Dots.length;i++) thisObj.LineMeterTool_Dots[i].remove();
						thisObj.LineMeterTool_Dots = [];
						thisObj.LineMeterTool_Dots_Length = 0;
						thisObj.map_vlayerLineMeter.innerHTML = '';
					};			
				}
				
				// public {left,top,right,bottom,width,height,center_x,center_y}
				thisObj.MapOverflowBounds = function()
				{
					var ret = {};
					ret.center_x = thisObj.XYZ.x;
					ret.center_y = thisObj.XYZ.y;
					var zdxdy = (thisObj.height+thisObj.image_overflow)/(thisObj.width+thisObj.image_overflow);
					ret.left = ret.center_x - thisObj.XYZ.zoom*(thisObj.width+thisObj.image_overflow)/2;
					ret.right = ret.center_x + thisObj.XYZ.zoom*(thisObj.width+thisObj.image_overflow)/2;
					ret.top = ret.center_y + thisObj.XYZ.zoom*(thisObj.height+thisObj.image_overflow)/2;
					ret.bottom = ret.center_y - thisObj.XYZ.zoom*(thisObj.height+thisObj.image_overflow)/2;
					ret.width = thisObj.XYZ.z;
					ret.height = thisObj.XYZ.zoom*thisObj.height;
					return ret;
				}
				
				// public {left,top,right,bottom,width,height,center_x,center_y}
				thisObj.MapBounds = function()
				{
					var ret = {};
					ret.center_x = thisObj.XYZ.x;
					ret.center_y = thisObj.XYZ.y;
					ret.left = ret.center_x - thisObj.XYZ.z/2;
					ret.right = ret.center_x + thisObj.XYZ.z/2;
					ret.top = ret.center_y + thisObj.XYZ.zoom*thisObj.height/2;
					ret.bottom = ret.center_y - thisObj.XYZ.zoom*thisObj.height/2;
					ret.width = thisObj.XYZ.z;
					ret.height = thisObj.XYZ.zoom*thisObj.height;
					return ret;
				}
				
				//public screen x,y to meters x,y
				thisObj.ScreenToMeters = function(x,y)
				{
					return {"x":thisObj.XYZ.x+(x-thisObj.width/2)*thisObj.XYZ.zoom,"y":thisObj.XYZ.y+(-y+thisObj.height/2)*thisObj.XYZ.zoom};
				}
				
				//public meters to screen
				thisObj.MetersToScreen = function(x,y)
				{
					return {"x":(x-thisObj.XYZ.x)/thisObj.XYZ.zoom+thisObj.width/2,"y":(-y+thisObj.XYZ.y)/thisObj.XYZ.zoom+thisObj.height/2};
				}
				
				//private property				
				thisObj.zooms_div = document.getElementById(zooms_div_id);
				if(thisObj.HideGroup) thisObj.zooms_div.style.display = 'none';	
				//
				thisObj.zoom_ramka2 = document.createElement('div');	
				thisObj.zoom_ramka2.id = zooms_div_id+'_ramka';
				thisObj.zoom_ramka2.style.position = 'absolute';
				thisObj.zoom_ramka2.style.width = (thisObj.ie ? 27 : 25)+'px';
				thisObj.zoom_ramka2.style.height = '222px';
				thisObj.zoom_ramka2.style.fontSize = '0px';
				thisObj.zoom_ramka2.style.left = '0px';
				thisObj.zoom_ramka2.style.top = '0px';
				thisObj.zoom_ramka2.style.border = 'solid 1px white';
				thisObj.zoom_ramka2.style.background = 'white';
				if(thisObj.ie) thisObj.zoom_ramka2.style.filter = 'alpha(opacity=80)'
				else thisObj.zoom_ramka2.style.opacity = '0.8';				
				thisObj.zooms_div.appendChild(thisObj.zoom_ramka2);	
				//
				thisObj.zoom_ramka = document.createElement('div');	
				thisObj.zoom_ramka.id = zooms_div_id+'_ramka';
				thisObj.zoom_ramka.style.position = 'absolute';
				thisObj.zoom_ramka.style.width = (thisObj.ie ? 23 : 21)+'px';
				thisObj.zoom_ramka.style.height = (thisObj.ie ? 184 : 182)+'px';
				thisObj.zoom_ramka.style.fontSize = '0px';
				thisObj.zoom_ramka.style.left = '2px';
				thisObj.zoom_ramka.style.top = '20px';
				thisObj.zoom_ramka.style.border = 'solid 1px blue';
				if(thisObj.ie) thisObj.zoom_ramka.style.filter = 'alpha(opacity=30)'
				else thisObj.zoom_ramka.style.opacity = '0.3';	
				thisObj.zooms_div.appendChild(thisObj.zoom_ramka);						
				//
				thisObj.zooms_divs = [];
				thisObj.zooms_divs_plus = document.createElement('div');	
				thisObj.zooms_divs_plus.id = zooms_div_id+'_plus';
				thisObj.zooms_divs_plus.style.position = 'absolute';
				thisObj.zooms_divs_plus.style.width = '21px';
				thisObj.zooms_divs_plus.style.height = '20px';
				thisObj.zooms_divs_plus.style.fontSize = '0px';
				thisObj.zooms_divs_plus.style.left = '3px';
				thisObj.zooms_divs_plus.style.top = '1px';
				thisObj.zooms_divs_plus.style.background = 'url(navimap/pngs/zooms.png) 0px -190px no-repeat';
				thisObj.zooms_divs_plus.style.cursor = 'pointer';
				thisObj.zooms_divs_plus.title = 'Приблизить карту';
				thisObj.zooms_div.appendChild(thisObj.zooms_divs_plus);	
				thisObj.zooms_divs_plus.onclick = function(){thisObj.ZoomIn();};
				for(var i=0;i<thisObj.Zooms.length;i++)
				{
					thisObj.zooms_divs[i] = document.createElement('div');			
					thisObj.zooms_divs[i].id = zooms_div_id+'_'+i;
					thisObj.zooms_divs[i].style.position = 'absolute';
					thisObj.zooms_divs[i].style.width = '21px';
					thisObj.zooms_divs[i].style.height = '10px';
					thisObj.zooms_divs[i].style.fontSize = '0px';
					thisObj.zooms_divs[i].style.left = '3px';
					thisObj.zooms_divs[i].style.top = 22+(i*10)+'px';
					thisObj.zooms_divs[i].style.background = 'url(navimap/pngs/zooms.png) 0px -'+(i*10)+'px no-repeat';
					thisObj.zooms_divs[i].style.cursor = 'pointer';
					thisObj.zooms_divs[i].title = 'Зум '+(19-i);
					thisObj.zooms_div.appendChild(thisObj.zooms_divs[i]);
					thisObj.zooms_divs[i].level = i;
					thisObj.zooms_divs[i].onclick = function(){thisObj.SetZoomFromLevels(this.level);};
				};
				thisObj.zooms_divs_minus = document.createElement('div');	
				thisObj.zooms_divs_minus.id = zooms_div_id+'_plus';
				thisObj.zooms_divs_minus.style.position = 'absolute';
				thisObj.zooms_divs_minus.style.width = '21px';
				thisObj.zooms_divs_minus.style.height = '20px';
				thisObj.zooms_divs_minus.style.fontSize = '0px';
				thisObj.zooms_divs_minus.style.left = '3px';
				thisObj.zooms_divs_minus.style.top = '210px';
				thisObj.zooms_divs_minus.style.background = 'url(navimap/pngs/zooms.png) 0px -210px no-repeat';
				thisObj.zooms_divs_minus.style.cursor = 'pointer';
				thisObj.zooms_divs_minus.title = 'Отдалить карту';
				thisObj.zooms_div.appendChild(thisObj.zooms_divs_minus);	
				thisObj.zooms_divs_minus.onclick = function(){thisObj.ZoomOut();};
				//
				thisObj.zooms_divs_current = document.createElement('div');	
				thisObj.zooms_divs_current.id = zooms_div_id+'_current';
				thisObj.zooms_divs_current.style.position = 'absolute';
				thisObj.zooms_divs_current.style.width = '21px';
				thisObj.zooms_divs_current.style.height = '10px';
				thisObj.zooms_divs_current.style.fontSize = '0px';
				thisObj.zooms_divs_current.style.left = '3px';
				thisObj.zooms_divs_current.style.top = '22px';
				thisObj.zooms_divs_current.style.background = 'url(navimap/pngs/zooms.png) 0px -180px no-repeat';
				thisObj.zooms_divs_current.style.cursor = 'pointer';
				thisObj.zooms_divs_current.title = 'Текущий зум 0';
				thisObj.zooms_div.appendChild(thisObj.zooms_divs_current);					
				
				//private
				thisObj.UpdateZoomCurrent = function()
				{
					thisObj.zooms_divs_current.style.top = (22+10*thisObj.XYZ.zoomID)+'px';
					thisObj.zooms_divs_current.title = 'Текущий зум '+(19-thisObj.XYZ.zoomID);
				};
				
				// private Navigator
				thisObj.navi_div_id = document.getElementById(navi_div_id);
				thisObj.navi_divs = [];
				thisObj.navi_divs_sx = 1;
				var sxt = 0;
				thisObj.navi_divs[sxt] = document.createElement('div');	
				thisObj.navi_divs[sxt].id = navi_div_id+'_'+i;
				thisObj.navi_divs[sxt].style.position = 'absolute';
				thisObj.navi_divs[sxt].style.left = '0px';
				thisObj.navi_divs[sxt].style.height = '23px';
				thisObj.navi_divs[sxt].style.width = '27px';
				thisObj.navi_divs[sxt].style.fontSize = '0px';
				thisObj.navi_divs[sxt].style.left = thisObj.navi_divs_sx+'px';
				thisObj.navi_divs[sxt].io = 0;
				thisObj.navi_divs_sx += 28;
				thisObj.navi_divs[sxt].style.top = '1px';
				thisObj.navi_divs[sxt].style.background = 'url(navimap/pngs/tools.png) 0px -23px no-repeat';
				thisObj.navi_divs[sxt].style.cursor = 'pointer';
				thisObj.navi_divs[sxt].title = 'Двигать карту';
				thisObj.navi_divs[sxt].sel = true;
				thisObj.navi_divs[sxt].onmouseover = function() {this.style.background = 'url(navimap/pngs/tools.png) 0px -'+(this.tool == thisObj.tool ? 23 : 46)+'px no-repeat';};
				thisObj.navi_divs[sxt].tool = 1;
				thisObj.navi_divs[sxt].onclick = function() {thisObj.SetTool(this.tool);};
				thisObj.navi_divs[sxt].onmouseout = function() {this.style.background = 'url(navimap/pngs/tools.png) 0px -'+(this.tool == thisObj.tool ? 23 : 0)+'px no-repeat';};
				thisObj.navi_div_id.appendChild(thisObj.navi_divs[sxt]);
				sxt++;
				//
				thisObj.navi_divs[sxt] = document.createElement('div');	
				thisObj.navi_divs[sxt].id = navi_div_id+'_'+i;
				thisObj.navi_divs[sxt].style.position = 'absolute';
				thisObj.navi_divs[sxt].style.left = '0px';
				thisObj.navi_divs[sxt].style.height = '23px';
				thisObj.navi_divs[sxt].style.width = '27px';
				thisObj.navi_divs[sxt].style.fontSize = '0px';
				thisObj.navi_divs[sxt].style.left = thisObj.navi_divs_sx+'px';
				thisObj.navi_divs[sxt].io = 27;
				thisObj.navi_divs_sx += 28;
				thisObj.navi_divs[sxt].style.top = '1px';
				thisObj.navi_divs[sxt].style.background = 'url(navimap/pngs/tools.png) -27px -0px no-repeat';
				thisObj.navi_divs[sxt].style.cursor = 'pointer';
				thisObj.navi_divs[sxt].title = 'Приблизить';
				thisObj.navi_divs[sxt].sel = true;
				thisObj.navi_divs[sxt].onmouseover = function() {this.style.background = 'url(navimap/pngs/tools.png) -27px -'+(this.tool == thisObj.tool ? 23 : 46)+'px no-repeat';};
				thisObj.navi_divs[sxt].tool = 4;
				thisObj.navi_divs[sxt].onclick = function() {thisObj.SetTool(this.tool);};
				thisObj.navi_divs[sxt].onmouseout = function() {this.style.background = 'url(navimap/pngs/tools.png) -27px -'+(this.tool == thisObj.tool ? 23 : 0)+'px no-repeat';};
				thisObj.navi_div_id.appendChild(thisObj.navi_divs[sxt]);
				sxt++;
				//
				
				if(!thisObj.HideGroup)
				{
				thisObj.navi_divs[sxt] = document.createElement('div');	
				thisObj.navi_divs[sxt].id = navi_div_id+'_'+i;
				thisObj.navi_divs[sxt].style.position = 'absolute';
				thisObj.navi_divs[sxt].style.left = '0px';
				thisObj.navi_divs[sxt].style.height = '23px';
				thisObj.navi_divs[sxt].style.width = '27px';
				thisObj.navi_divs[sxt].style.fontSize = '0px';
				thisObj.navi_divs[sxt].style.left = thisObj.navi_divs_sx+'px';
				thisObj.navi_divs[sxt].io = 81;
				thisObj.navi_divs_sx += 28;
				thisObj.navi_divs[sxt].style.top = '1px';
				thisObj.navi_divs[sxt].style.background = 'url(navimap/pngs/tools.png) -81px -0px no-repeat';
				thisObj.navi_divs[sxt].style.cursor = 'pointer';
				thisObj.navi_divs[sxt].title = 'Приблизить выделенную область';
				thisObj.navi_divs[sxt].sel = true;
				thisObj.navi_divs[sxt].onmouseover = function() {this.style.background = 'url(navimap/pngs/tools.png) -81px -'+(this.tool == thisObj.tool ? 23 : 46)+'px no-repeat';};
				thisObj.navi_divs[sxt].tool = 2;
				thisObj.navi_divs[sxt].onclick = function() {thisObj.SetTool(this.tool);};
				thisObj.navi_divs[sxt].onmouseout = function() {this.style.background = 'url(navimap/pngs/tools.png) -81px -'+(this.tool == thisObj.tool ? 23 : 0)+'px no-repeat';};
				thisObj.navi_div_id.appendChild(thisObj.navi_divs[sxt]);
				sxt++;
				};
				
				//
				thisObj.navi_divs[sxt] = document.createElement('div');	
				thisObj.navi_divs[sxt].id = navi_div_id+'_'+i;
				thisObj.navi_divs[sxt].style.position = 'absolute';
				thisObj.navi_divs[sxt].style.left = '0px';
				thisObj.navi_divs[sxt].style.height = '23px';
				thisObj.navi_divs[sxt].style.width = '27px';
				thisObj.navi_divs[sxt].style.fontSize = '0px';
				thisObj.navi_divs[sxt].style.left = thisObj.navi_divs_sx+'px';
				thisObj.navi_divs[sxt].io = 54;
				thisObj.navi_divs_sx += 28;
				thisObj.navi_divs[sxt].style.top = '1px';
				thisObj.navi_divs[sxt].style.background = 'url(navimap/pngs/tools.png) -54px -0px no-repeat';
				thisObj.navi_divs[sxt].style.cursor = 'pointer';
				thisObj.navi_divs[sxt].title = 'Отдалить';
				thisObj.navi_divs[sxt].sel = true;
				thisObj.navi_divs[sxt].onmouseover = function() {this.style.background = 'url(navimap/pngs/tools.png) -54px -'+(this.tool == thisObj.tool ? 23 : 46)+'px no-repeat';};
				thisObj.navi_divs[sxt].tool = 3;
				thisObj.navi_divs[sxt].onclick = function() {thisObj.SetTool(this.tool);};
				thisObj.navi_divs[sxt].onmouseout = function() {this.style.background = 'url(navimap/pngs/tools.png) -54px -'+(this.tool == thisObj.tool ? 23 : 0)+'px no-repeat';};
				thisObj.navi_div_id.appendChild(thisObj.navi_divs[sxt]);
				sxt++;
				//
				
				thisObj.UpdateToolCurrent = function()
				{
					for(var i=0;i<thisObj.navi_divs.length;i++)
						thisObj.navi_divs[i].style.background = 'url(navimap/pngs/tools.png) -'+thisObj.navi_divs[i].io+'px -'+(thisObj.navi_divs[i].tool == thisObj.tool ? 23 : 0)+'px no-repeat';
				};
				
			};
			
		//////////////////////
		/*
			class TEventListener
			{
					// конструктор; variable_name - имя переменной хранения класса;
				public TEventListener(string  variable_name)
					
					// Добавление получателя события
				public Function AddEvent(string event_name);
					// удаление события
				public void RemoveEvent(string event_name);
				
					// Добавление получателя события, возвращает имя получателя
				public string AddListen = function(string event_name, Function call_function)
					// Удаление получателя события, call_function_name - имя получателя события
				public bool RemoveListen = function(string event_name, string call_function_name)
			}
		*/
			
		function TEventListener(myname)
		{
			this.author = 'Milok Zbrozek (milokz [doggy] gmail.com)';
			this.vname = myname;
			this.events = [];
			return this;
		}
		
		// private
		TEventListener.prototype.Call = function(event_name,event_arguments)
		{
			if(this.events[event_name] && this.events[event_name].c.length > 0) 
			for(var i=0;i<this.events[event_name].c.length;i++)
			{
				switch (event_arguments.length)
				{
					case 10: this.events[event_name].c[i].func(event_arguments[0],event_arguments[1],event_arguments[2],event_arguments[3],event_arguments[4],event_arguments[5],event_arguments[6],event_arguments[7],event_arguments[8],event_arguments[9]); break;
					case 9: this.events[event_name].c[i].func(event_arguments[0],event_arguments[1],event_arguments[2],event_arguments[3],event_arguments[4],event_arguments[5],event_arguments[6],event_arguments[7],event_arguments[8]); break;
					case 8: this.events[event_name].c[i].func(event_arguments[0],event_arguments[1],event_arguments[2],event_arguments[3],event_arguments[4],event_arguments[5],event_arguments[6],event_arguments[7]); break;
					case 7: this.events[event_name].c[i].func(event_arguments[0],event_arguments[1],event_arguments[2],event_arguments[3],event_arguments[4],event_arguments[5],event_arguments[6]); break;
					case 6: this.events[event_name].c[i].func(event_arguments[0],event_arguments[1],event_arguments[2],event_arguments[3],event_arguments[4],event_arguments[5]); break;
					case 5: this.events[event_name].c[i].func(event_arguments[0],event_arguments[1],event_arguments[2],event_arguments[3],event_arguments[4]); break;
					case 4: this.events[event_name].c[i].func(event_arguments[0],event_arguments[1],event_arguments[2],event_arguments[3]); break;
					case 3: this.events[event_name].c[i].func(event_arguments[0],event_arguments[1],event_arguments[2]); break;
					case 2: this.events[event_name].c[i].func(event_arguments[0],event_arguments[1]); break;
					case 1: this.events[event_name].c[i].func(event_arguments[0]); break;
					case 0: this.events[event_name].c[i].func(); break;
				};
			};
		}
		
		// Add Event To Listen
		TEventListener.prototype.AddEvent = function(event_name)
		{			
			this.events[event_name] = {f:new Function('',''+this.vname+".Call('"+event_name+"',arguments);"),c:[]};
			return this.events[event_name].f;
		}		
		
		// Remove Event from listen
		TEventListener.prototype.RemoveEvent = function(event_name)
		{
			this.events[event_name] = {f:new Function('',''),c:[]};
		}
		
		// Addfunction to listen Event
		TEventListener.prototype.AddListen = function(event_name,call_function)
		{
			if(this.events[event_name])
			this.events[event_name].c[this.events[event_name].c.length] = {func:call_function,name:"cfn"+Math.random()};
			else return false;
			return this.events[event_name].c[this.events[event_name].c.length-1].name;
		}
		
		// Remove function from listen event
		TEventListener.prototype.RemoveListen = function(event_name,call_function_name)
		{
			if(this.events[event_name])
			{
				var tmpe = [];
				var tmpi = 0;
				for(var i=0;i<this.events[event_name].c.length;i++)
				if(this.events[event_name].c[i].name != call_function_name) tmpe[tmpi++] = this.events[event_name].c[i];
				this.events[event_name].c = tmpe;
			} 
			else return false;
			return true;
		}
		
		// загрузка файлов JS
		// -1 - on document ready; false/0 - now; 1-99999 - timeOut
		function include(path, _to)
		{
			if(!(_to))
				document.write('<script type="text/javascript"  src="'+path+'"></script>');
			else if(_to == -1)
				GVARS.SAL.push(path);
			else 
				setTimeout("LoadJS('"+path+"');",_to);
		}
		
		// загрузка файлов СSS
		// -1 - on document ready; false/0 - now; 1-99999 - timeOut
		function includeCSS(path, _to)
		{
			if(!(_to))
				document.write('<link type="text/css" rel="stylesheet" href="'+path+'"/>');
			else if(_to == -1)
				GVARS.CAL.push(path);
			else 
				setTimeout("LoadCSS('"+path+"');",_to);
		}
		
		// загрузка файлов JS и CSS после прогрузки страницы
		function includeDone()
		{
			for(var i=0;i<GVARS.SAL.length;i++) LoadJS(GVARS.SAL[i]);				
			GVARS.SAL = [];
			for(var i=0;i<GVARS.CAL.length;i++) LoadCSS(GVARS.CAL[i]);
			GVARS.CAL = [];
			// if(document.plot) document.plot();
		}
		
		function makeRPCCall(url,xmldata)
		{
			try 
			{
				netscape.security.PrivilegeManager.enablePrivilege("UniversalBrowserRead");
				//netscape.security.PrivilegeManager.enablePrivilege("UniversalBrowserAccess");
			} 
			catch (e) 
			{
				//alert("Permission UniversalBrowserRead denied.");
			};
	
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
			//var myReq = httpReq;
			//window.status = 'Загрузка данных...';
			//httpReq.onreadystatechange = new Function('','return true;');
			
			httpReq.open("POST", url, false);
			httpReq.setRequestHeader('Content-Type', 'text/xml');
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
			httpReq.send(xmldata);	
			if (httpReq.status == 200) {return httpReq.responseText;} else
			{
				if (httpReq.status == 500) alert("Server Exception: "+httpReq.status);
				return httpReq.status;
			};
		}

		// загрузка файлов JS и CSS после прогрузки страницы
		// document.onready = function() { setTimeout('includeDone();',2000); };  //  works only w/jQuery	
		document.dkxce_onload = function() { setTimeout('includeDone();',2500); };
		if (document.addEventListener) { document.addEventListener( "DOMContentLoaded", function() //Mozilla, Opera
		{ document.removeEventListener( "DOMContentLoaded", arguments.callee, false ); document.dkxce_onload(); }, false ); }
		else if (document.attachEvent) { document.attachEvent("onreadystatechange", function() { if ( document.readyState === "complete" )  // IE
		{ document.detachEvent( "onreadystatechange", arguments.callee ); document.dkxce_onload(); }; }) }
		else window.onload = function() { includeDone(); }; // always works	