var excanvas=function(canvas){if(arguments.length==1)return canvas;};if(!window.CanvasRenderingContext2D){excanvas=function(cS){var m=Math;var bf=m.round;var gL=m.sin;var bm=m.cos;var Z=10;var dK=Z/2;var G_vmlCanvasManager_={init:function(opt_doc,cS){var cC=opt_doc||document;if(/MSIE/.test(navigator.userAgent)&& !window.opera){var self=this;if(typeof cS!="undefined"){return self.init_(cC,cS);}else{cC.attachEvent("onreadystatechange",function(){self.init_(cC);});}}},init_:function(cC,cS){if(typeof cS!="undefined"){if(!cS.getContext){return this.initElement(cS);}return;}if(cC.readyState=="complete"){if(!cC.namespaces["g_vml_"]){cC.namespaces.add("g_vml_","urn:schemas-microsoft-com:vml");}var ss=cC.createStyleSheet();ss.cssText="canvas{display:inline-block;/*overflow:hidden;*/"+"text-align:left;width:30000px;height:30000px}"+"g_vml_\\:*{behavior:url(#default#VML)}";var gn=cC.getElementsByTagName("canvas");for(var i=0;i<gn.length;i++){if(!gn[i].getContext){this.initElement(gn[i]);}}}},fixElement_:function(M){var outerHTML=M.outerHTML;var newEl=M.ownerDocument.createElement(outerHTML);if(outerHTML.slice(-2)!="/>"){var tagName="/"+M.tagName;var ns;while((ns=M.nextSibling)&&ns.tagName!=tagName){ns.removeNode();}if(ns){ns.removeNode();}}if(M.parentNode==null){return M;}M.parentNode.replaceChild(newEl,M);return newEl;},initElement:function(M){M=this.fixElement_(M);M.getContext=function(){if(this.hW){return this.hW;}return this.hW=new CanvasRenderingContext2D_(this);};M.attachEvent('onpropertychange',onPropertyChange);M.attachEvent('onresize',onResize);var dS=M.attributes;if(dS.width&&dS.width.specified){M.style.width=dS.width.nodeValue+"px";}else{M.width=M.clientWidth;}if(dS.height&&dS.height.specified){M.style.height=dS.height.nodeValue+"px";}else{M.height=M.clientHeight;}return M;}};function onPropertyChange(e){var M=e.srcElement;switch(e.propertyName){case 'width':M.style.width=M.attributes.width.nodeValue+"px";M.getContext().clearRect();break;case 'height':M.style.height=M.attributes.height.nodeValue+"px";M.getContext().clearRect();break;}};function onResize(e){var M=e.srcElement;if(M.firstChild){M.firstChild.style.width=M.clientWidth+'px';M.firstChild.style.height=M.clientHeight+'px';}};var newCanvas=G_vmlCanvasManager_.init(null,cS);var hI=[];for(var i=0;i<16;i++){for(var j=0;j<16;j++){hI[i*16+j]=i.toString(16)+j.toString(16);}}function createMatrixIdentity(){return[[1,0,0],[0,1,0],[0,0,1]];};function matrixMultiply(m1,m2){var result=createMatrixIdentity();for(var x=0;x<3;x++){for(var y=0;y<3;y++){var hY=0;for(var z=0;z<3;z++){hY+=m1[x][z]*m2[z][y];}result[x][y]=hY;}}return result;};function copyState(o1,o2){o2.fillStyle=o1.fillStyle;o2.lineCap=o1.lineCap;o2.lineJoin=o1.lineJoin;o2.lineWidth=o1.lineWidth;o2.miterLimit=o1.miterLimit;o2.shadowBlur=o1.shadowBlur;o2.shadowColor=o1.shadowColor;o2.shadowOffsetX=o1.shadowOffsetX;o2.shadowOffsetY=o1.shadowOffsetY;o2.strokeStyle=o1.strokeStyle;o2.eP=o1.eP;o2.eZ=o1.eZ;};function processStyle(cK){var K,alpha=1;cK=String(cK);if(cK.substring(0,3)=="rgb"){var start=cK.indexOf("(",3);var end=cK.indexOf(")",start+1);var gi=cK.substring(start+1,end).split(",");K="#";for(var i=0;i<3;i++){K+=hI[Number(gi[i])];}if((gi.length==4)&&(cK.substr(3,1)=="a")){alpha=gi[3];}}else{K=cK;}return[K,alpha];};function processLineCap(lineCap){switch(lineCap){case "butt":return "flat";case "round":return "round";case "square":default:return "square";}};function CanvasRenderingContext2D_(ei){this.m_=createMatrixIdentity();this.mStack_=[];this.aStack_=[];this.cp=[];this.strokeStyle="#000";this.fillStyle="#000";this.lineWidth=1;this.lineJoin="miter";this.lineCap="butt";this.miterLimit=Z*1;this.globalAlpha=1;this.canvas=ei;var M=ei.ownerDocument.createElement('div');M.style.width=ei.clientWidth+'px';M.style.height=ei.clientHeight+'px';M.style.overflow='hidden';M.style.position='absolute';ei.appendChild(M);this.gk=M;this.eP=1;this.eZ=1;};var contextPrototype=CanvasRenderingContext2D_.prototype;contextPrototype.clearRect=function(){this.gk.innerHTML="";this.cp=[];};contextPrototype.beginPath=function(){this.cp=[];};contextPrototype.moveTo=function(aI,aJ){this.cp.push({type:"moveTo",x:aI,y:aJ});this.eB=aI;this.eC=aJ;};contextPrototype.lineTo=function(aI,aJ){this.cp.push({type:"lineTo",x:aI,y:aJ});this.eB=aI;this.eC=aJ;};contextPrototype.bezierCurveTo=function(kM,lm,lE,lN,aI,aJ){this.cp.push({type:"bezierCurveTo",fO:kM,fI:lm,gT:lE,gO:lN,x:aI,y:aJ});this.eB=aI;this.eC=aJ;};contextPrototype.quadraticCurveTo=function(lJ,lo,aI,aJ){var fO=this.eB+2.0/3.0*(lJ-this.eB);var fI=this.eC+2.0/3.0*(lo-this.eC);var gT=fO+(aI-this.eB)/3.0;var gO=fI+(aJ-this.eC)/3.0;this.bezierCurveTo(fO,fI,gT,gO,aI,aJ);};contextPrototype.arc=function(aI,aJ,ea,jb,hZ,iU){ea*=Z;var jK=iU?"at":"wa";var en=aI+(bm(jb)*ea)-dK;var hl=aJ+(gL(jb)*ea)-dK;var fl=aI+(bm(hZ)*ea)-dK;var gQ=aJ+(gL(hZ)*ea)-dK;if(en==fl&& !iU){en+=0.125;}this.cp.push({type:jK,x:aI,y:aJ,fg:ea,en:en,hl:hl,fl:fl,gQ:gQ});};contextPrototype.rect=function(aI,aJ,cR,cV){this.moveTo(aI,aJ);this.lineTo(aI+cR,aJ);this.lineTo(aI+cR,aJ+cV);this.lineTo(aI,aJ+cV);this.closePath();};contextPrototype.strokeRect=function(aI,aJ,cR,cV){this.beginPath();this.moveTo(aI,aJ);this.lineTo(aI+cR,aJ);this.lineTo(aI+cR,aJ+cV);this.lineTo(aI,aJ+cV);this.closePath();this.stroke();};contextPrototype.fillRect=function(aI,aJ,cR,cV){this.beginPath();this.moveTo(aI,aJ);this.lineTo(aI+cR,aJ);this.lineTo(aI+cR,aJ+cV);this.lineTo(aI,aJ+cV);this.closePath();this.fill();};contextPrototype.createLinearGradient=function(iq,iD,ko,ku){var gradient=new CanvasGradient_("gradient");return gradient;};contextPrototype.createRadialGradient=function(iq,iD,kf,ko,ku,jO){var gradient=new CanvasGradient_("gradientradial");gradient.iQ=kf;gradient.iR=jO;gradient.fJ.x=iq;gradient.fJ.y=iD;return gradient;};contextPrototype.drawImage=function(image,lZ){var bD,cr,dY,dh,sx,sy,sw,sh;var oldRuntimeWidth=image.runtimeStyle.width;var oldRuntimeHeight=image.runtimeStyle.height;image.runtimeStyle.width='auto';image.runtimeStyle.height='auto';var w=image.width;var h=image.height;image.runtimeStyle.width=oldRuntimeWidth;image.runtimeStyle.height=oldRuntimeHeight;if(arguments.length==3){bD=arguments[1];cr=arguments[2];sx=sy=0;sw=dY=w;sh=dh=h;}else if(arguments.length==5){bD=arguments[1];cr=arguments[2];dY=arguments[3];dh=arguments[4];sx=sy=0;sw=w;sh=h;}else if(arguments.length==9){sx=arguments[1];sy=arguments[2];sw=arguments[3];sh=arguments[4];bD=arguments[5];cr=arguments[6];dY=arguments[7];dh=arguments[8];}else{throw "Invalid number of arguments";}var d=this.getCoords_(bD,cr);var w2=sw/2;var h2=sh/2;var vmlStr=[];var W=10;var H=10;vmlStr.push(' <g_vml_:group',' coordsize="',Z*W,',',Z*H,'"',' coordorigin="0,0"',' style="width:',W,';height:',H,';position:absolute;');if(this.m_[0][0]!=1||this.m_[0][1]){var filter=[];filter.push("M11='",this.m_[0][0],"',","M12='",this.m_[1][0],"',","M21='",this.m_[0][1],"',","M22='",this.m_[1][1],"',","Dx='",bf(d.x/Z),"',","Dy='",bf(d.y/Z),"'");var max=d;var ev=this.getCoords_(bD+dY,cr);var iL=this.getCoords_(bD,cr+dh);var ja=this.getCoords_(bD+dY,cr+dh);max.x=Math.max(max.x,ev.x,iL.x,ja.x);max.y=Math.max(max.y,ev.y,iL.y,ja.y);vmlStr.push("padding:0 ",bf(max.x/Z),"px ",bf(max.y/Z),"px 0;filter:progid:DXImageTransform.Microsoft.Matrix(",filter.join(""),", sizingmethod='clip');")}else{vmlStr.push("top:",bf(d.y/Z),"px;left:",bf(d.x/Z),"px;")}vmlStr.push(' ">','<g_vml_:image src="',image.src,'"',' style="width:',Z*dY,';',' height:',Z*dh,';"',' cropleft="',sx/w,'"',' croptop="',sy/h,'"',' cropright="',(w-sx-sw)/w,'"',' cropbottom="',(h-sy-sh)/h,'"',' />','</g_vml_:group>');this.gk.insertAdjacentHTML("BeforeEnd",vmlStr.join(""));};contextPrototype.stroke=function(fd){var lineStr=[];var lineOpen=false;var a=processStyle(fd?this.fillStyle:this.strokeStyle);var color=a[0];var opacity=a[1]*this.globalAlpha;var W=10;var H=10;lineStr.push('<g_vml_:shape',' fillcolor="',color,'"',' filled="',Boolean(fd),'"',' style="position:absolute;width:',W,';height:',H,';"',' coordorigin="0 0" coordsize="',Z*W,' ',Z*H,'"',' stroked="',!fd,'"',' strokeweight="',this.lineWidth,'"',' strokecolor="',color,'"',' path="');var newSeq=false;var min={x:null,y:null};var max={x:null,y:null};for(var i=0;i<this.cp.length;i++){var p=this.cp[i];if(p.type=="moveTo"){lineStr.push(" m ");var c=this.getCoords_(p.x,p.y);lineStr.push(bf(c.x),",",bf(c.y));}else if(p.type=="lineTo"){lineStr.push(" l ");var c=this.getCoords_(p.x,p.y);lineStr.push(bf(c.x),",",bf(c.y));}else if(p.type=="close"){lineStr.push(" x ");}else if(p.type=="bezierCurveTo"){lineStr.push(" c ");var c=this.getCoords_(p.x,p.y);var iu=this.getCoords_(p.fO,p.fI);var ev=this.getCoords_(p.gT,p.gO);lineStr.push(bf(iu.x),",",bf(iu.y),",",bf(ev.x),",",bf(ev.y),",",bf(c.x),",",bf(c.y));}else if(p.type=="at"||p.type=="wa"){lineStr.push(" ",p.type," ");var c=this.getCoords_(p.x,p.y);var cStart=this.getCoords_(p.en,p.hl);var jf=this.getCoords_(p.fl,p.gQ);lineStr.push(bf(c.x-this.eP*p.fg),",",bf(c.y-this.eZ*p.fg)," ",bf(c.x+this.eP*p.fg),",",bf(c.y+this.eZ*p.fg)," ",bf(cStart.x),",",bf(cStart.y)," ",bf(jf.x),",",bf(jf.y));}if(c){if(min.x==null||c.x<min.x){min.x=c.x;}if(max.x==null||c.x>max.x){max.x=c.x;}if(min.y==null||c.y<min.y){min.y=c.y;}if(max.y==null||c.y>max.y){max.y=c.y;}}}lineStr.push(' ">');if(typeof this.fillStyle=="object"){var focus={x:"50%",y:"50%"};var width=(max.x-min.x);var height=(max.y-min.y);var hX=(width>height)?width:height;focus.x=bf((this.fillStyle.fJ.x/width)*100+50)+"%";focus.y=bf((this.fillStyle.fJ.y/height)*100+50)+"%";var colors=[];if(this.fillStyle.type_=="gradientradial"){var inside=(this.fillStyle.iQ/hX*100);var iO=(this.fillStyle.iR/hX*100)-inside;}else{var inside=0;var iO=100;}var ef={aN:null,color:null};var eI={aN:null,color:null};this.fillStyle.fE.sort(function(kK,kI){return kK.aN-kI.aN;});for(var i=0;i<this.fillStyle.fE.length;i++){var fs=this.fillStyle.fE[i];colors.push((fs.aN*iO)+inside,"% ",fs.color,",");if(fs.aN>ef.aN||ef.aN==null){ef.aN=fs.aN;ef.color=fs.color;}if(fs.aN<eI.aN||eI.aN==null){eI.aN=fs.aN;eI.color=fs.color;}}colors.pop();lineStr.push('<g_vml_:fill',' color="',eI.color,'"',' color2="',ef.color,'"',' type="',this.fillStyle.type_,'"',' focusposition="',focus.x,', ',focus.y,'"',' colors="',colors.join(""),'"',' opacity="',opacity,'" />');}else if(fd){lineStr.push('<g_vml_:fill color="',color,'" opacity="',opacity,'" />');}else{lineStr.push('<g_vml_:stroke',' opacity="',opacity,'"',' joinstyle="',this.lineJoin,'"',' miterlimit="',this.miterLimit,'"',' endcap="',processLineCap(this.lineCap),'"',' weight="',this.lineWidth,'px"',' color="',color,'" />');}lineStr.push("</g_vml_:shape>");this.gk.insertAdjacentHTML("beforeEnd",lineStr.join(""));this.cp=[];};contextPrototype.fill=function(){this.stroke(true);};contextPrototype.closePath=function(){this.cp.push({type:"close"});};contextPrototype.getCoords_=function(aI,aJ){return{x:Z*(aI*this.m_[0][0]+aJ*this.m_[1][0]+this.m_[2][0])-dK,y:Z*(aI*this.m_[0][1]+aJ*this.m_[1][1]+this.m_[2][1])-dK}};contextPrototype.save=function(){var o={};copyState(this,o);this.aStack_.push(o);this.mStack_.push(this.m_);this.m_=matrixMultiply(createMatrixIdentity(),this.m_);};contextPrototype.restore=function(){copyState(this.aStack_.pop(),this);this.m_=this.mStack_.pop();};contextPrototype.translate=function(aI,aJ){var m1=[[1,0,0],[0,1,0],[aI,aJ,1]];this.m_=matrixMultiply(m1,this.m_);};contextPrototype.rotate=function(je){var c=bm(je);var s=gL(je);var m1=[[c,s,0],[-s,c,0],[0,0,1]];this.m_=matrixMultiply(m1,this.m_);};contextPrototype.scale=function(aI,aJ){this.eP*=aI;this.eZ*=aJ;var m1=[[aI,0,0],[0,aJ,0],[0,0,1]];this.m_=matrixMultiply(m1,this.m_);};contextPrototype.clip=function(){};contextPrototype.arcTo=function(){};contextPrototype.createPattern=function(){return new CanvasPattern_;};function CanvasGradient_(jT){this.type_=jT;this.iQ=0;this.iR=0;this.fE=[];this.fJ={x:0,y:0};};CanvasGradient_.prototype.addColorStop=function(ke,gS){gS=processStyle(gS);this.fE.push({aN:1-ke,color:gS});};function CanvasPattern_(){};G_vmlCanvasManager=G_vmlCanvasManager_;CanvasRenderingContext2D=CanvasRenderingContext2D_;CanvasGradient=CanvasGradient_;CanvasPattern=CanvasPattern_;return newCanvas;}}excanvas();