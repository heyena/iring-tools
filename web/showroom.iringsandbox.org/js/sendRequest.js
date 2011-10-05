function getXmlHttpConnection() {
  var xmlHttp = null;

  try {
    xmlHttp = new XMLHttpRequest();  // Firefox, Opera 8.0+, Safari
  }
  catch(e) {
    try {
      xmlHttp = new ActiveXObject('Msxml2.XMLHTTP');  // IE
    }
    catch(e) {
      try {
        xmlHttp = new ActiveXObject('Microsoft.XMLHTTP');
      }
      catch(e) {
        return false;
      }
    }
  }

  return xmlHttp;
}

function sendRequest(url, callback) {
  var i = url.indexOf('?');
  var action = url.substring(0, i);
  var params = url.substring(i + 1);    
  var xmlHttp = getXmlHttpConnection();
    
  if (xmlHttp != null) {
    xmlHttp.open('GET', url, true);
    xmlHttp.send();
    
    var interval = window.setInterval(
      function() {
        if (xmlHttp.readyState == 4) {
          window.clearInterval(interval);
		  callback(xmlHttp.responseText);
          xmlHttp = null;
        }
      }, 100
    );
  }
}