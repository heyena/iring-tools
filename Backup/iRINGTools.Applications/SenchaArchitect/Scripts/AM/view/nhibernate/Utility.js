/*
 * File: Scripts/AM/view/nhibernate/Utility.js
 *
 * This file was generated by Sencha Architect version 2.2.2.
 * http://www.sencha.com/products/architect/
 *
 * This file requires use of the Ext JS 4.1.x library, under independent license.
 * License of Sencha Architect does not include license for Ext JS 4.1.x. For more
 * details see http://www.sencha.com/license or contact license@sencha.com.
 *
 * This file will be auto-generated each and everytime you save your project.
 *
 * Do NOT hand edit this file.
 */

Ext.define('AM.view.nhibernate.Utility', {
  extend: 'Ext.AbstractManager',
  alias: 'widget.nhibernateutility',

  singleton: true,

  encode: function(data) {
    var b64 = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=";
    var o1, o2, o3, h1, h2, h3, h4, bits, i = 0,
      ac = 0,
      enc = "",
      tmpArray = []; 
      if (!data) {
        return data;
    }
    data = this.utf8Encode(data + '');

    do { 
      o1 = data.charCodeAt(i++);
      o2 = data.charCodeAt(i++);      
      o3 = data.charCodeAt(i++);

      bits = o1 << 16 | o2 << 8 | o3;

      h1 = bits >> 18 & 0x3f;       
      h2 = bits >> 12 & 0x3f;
      h3 = bits >> 6 & 0x3f;
      h4 = bits & 0x3f;

      tmpArray[ac++] = 
      b64.charAt(h1) + 
      b64.charAt(h2) + 
      b64.charAt(h3) + 
      b64.charAt(h4);
    } while (i < data.length);

    enc = tmpArray.join('');
    var r = data.length % 3;

    return (r ? enc.slice(0, r - 3) : enc) + '==='.slice(r || 3);

  },

  decode: function(data) {
    var b64 = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=";
    var o1, o2, o3, h1, h2, h3, h4, bits, i = 0,
      ac = 0,        
      dec = "",
      tmpArray = [];

    if (!data) {
      return data;    
    }

    data += '';

    do { 
      h1 = b64.indexOf(data.charAt(i++));
      h2 = b64.indexOf(data.charAt(i++));
      h3 = b64.indexOf(data.charAt(i++));
      h4 = b64.indexOf(data.charAt(i++));
      bits = h1 << 18 | h2 << 12 | h3 << 6 | h4;

      o1 = bits >> 16 & 0xff;
      o2 = bits >> 8 & 0xff;
      o3 = bits & 0xff; 
      if (h3 == 64) {
        tmpArray[ac++] = String.fromCharCode(o1);
      } else if (h4 == 64) {
        tmpArray[ac++] = String.fromCharCode(o1, o2);        
      } else {
        tmpArray[ac++] = String.fromCharCode(o1, o2, o3);
      }
    } while (i < data.length);

    dec = tmpArray.join('');
    dec = this.utf8Decode(dec);

    return dec;
  },

  utf8Encode: function(argString) {
    if (argString === null || typeof argString === "undefined") {
      return "";
    }
    var string = (argString + ''); 

    var utftext = "",
      start, end, stringl = 0;

    start = end = 0;    
    stringl = string.length;

    for (var n = 0; n < stringl; n++) {
      var c1 = string.charCodeAt(n);
      var enc = null;
      if (c1 < 128) {
        end++;
      } else if (c1 > 127 && c1 < 2048) {
        enc = String.fromCharCode((c1 >> 6) | 192) + 
        String.fromCharCode((c1 & 63) | 128);
      } else {            
        enc = String.fromCharCode((c1 >> 12) | 224) + 
        String.fromCharCode(((c1 >> 6) & 63) | 128) + 
        String.fromCharCode((c1 & 63) | 128);
      }
      if (enc !== null) {
        if (end > start) {
          utftext += string.slice(start, end);
        }
        utftext += enc;
        start = end = n + 1;
      }
    } 
    if (end > start) {
      utftext += string.slice(start, stringl);
    }
    return utftext;

  },

  utf8Decode: function(argString) {
    var tmpArray = [],
      i = 0,
      ac = 0,
      c1 = 0,
      c2 = 0,       
      c3 = 0;

    argString += '';

    while (i < argString.length) {       
      c1 = argString.charCodeAt(i);
      if (c1 < 128) {
        tmpArray[ac++] = String.fromCharCode(c1);
        i++;
      } else if (c1 > 191 && c1 < 224) {         
        c2 = argString.charCodeAt(i + 1);
        tmpArray[ac++] = String.fromCharCode(((c1 & 31) << 6) | (c2 & 63));
        i += 2;
      } else {
        c2 = argString.charCodeAt(i + 1);           
        c3 = argString.charCodeAt(i + 2);
        tmpArray[ac++] = String.fromCharCode(((c1 & 15) << 12) | ((c2 & 63) << 6) | (c3 & 63));
        i += 3;
      }
    } 
    return tmpArray.join('');

  }

});