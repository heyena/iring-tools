var iRINGTools = {
  transId: 0,
  transList: [],  
  isIE: navigator.userAgent.toLowerCase().indexOf('msie') != -1,
  isChrome: navigator.userAgent.toLowerCase().indexOf('chrome') != -1,
  isFirefox: navigator.userAgent.toLowerCase().indexOf('firefox') != -1,
  
  getHttpConnection: function() {
    var conn = null;

    try {
      conn = new XMLHttpRequest();  // Google Chrome, Firefox, Opera 8.0+, Safari
    }
    catch(ex) {
      try {
        conn = new ActiveXObject('Msxml2.XMLHTTP');  // IE
      }
      catch(ex) {
        try {
          conn = new ActiveXObject('Microsoft.XMLHTTP');
        }
        catch(ex) {
          return false;
        }
      }
    }

    return conn;
  },
  
  sendHttpRequest: function(method, url, isAsync, callback, context) {
    var conn = this.getHttpConnection();
     
    if (conn != null) {
      if (method.toUpperCase() == 'POST') {   
        var i = url.indexOf('?');
        var action = url.substring(0, i);    
        var params = url.substring(i + 1);              
        conn.open('POST', action, isAsync);
        conn.setRequestHeader('Content-type', 'application/x-www-form-urlencoded');
        conn.setRequestHeader('Content-length', params.length);
        conn.setRequestHeader('Connection', 'close');
        conn.send(params);
      }
      else {
        conn.open('GET', url, isAsync);
        conn.send(null);
      }
	    
	    if (isAsync) {
	    	var transId = this.transId++;
	    	
	    	this.transList[transId] = window.setInterval(
		      function() {
		      	if (conn.readyState == 4) {
		        	window.clearInterval(iRINGTools.transList[transId]);	
		        	delete iRINGTools.transList[transId];
              
              iRINGTools.processHttpResponse(conn.responseText, callback, context);
		        	conn = null;
		        }
		      }, 100);
		  }
		  else {		   
		    processHttpResponse(conn.responseText, callback, context);        
		    conn = null;
		  }
		}
    else {
      alert('Failed getting connection!');
    }
  },  
  
  processHttpResponse: function(responseText, callback, context) {
    if (responseText == null) {
      alert('Request error');    
    }
    else {
      if (context != null) {
        callback(responseText, context);
      }
      else {
        callback(responseText);
      }
    }
  },
  
  getXmlDOM: function(xml) {
    var xmlDOM;

    if (window.ActiveXObject) {  // IE
      xmlDOM = new ActiveXObject('Microsoft.xmlDOM');
      xmlDOM.async = 'false';
      xmlDOM.loadXML(xml);
    }
    else {  // Mozilla, Firefox, Opera, etc.
      var oParser = new DOMParser();
      xmlDOM = oParser.parseFromString(xml, 'text/xml');
    }

    return xmlDOM;
  }
}