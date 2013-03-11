﻿Ext.ns('AdapterManager');
/**
* @class AdapterManager.AjaxRowExpander
* @extends Panel
* @author by Gert Jansen van Rensburg
*/

AdapterManager.AjaxRowExpander = function (config, previewURL) {
    AdapterManager.AjaxRowExpander.superclass.constructor.call(this, config, previewURL);
    this.previewURL = previewURL;
    this.enableCaching = false;
}

Ext.extend(AdapterManager.AjaxRowExpander, Ext.ux.grid.RowExpander, {
    getBodyContent: function (record, index) {
        var body = '<div id="tmp">Loading…</div>';
        Ext.Ajax.request({
            url: this.previewURL + record.id,
            disableCaching: true,
            success: function (response, options) {
                Ext.getDom('tmp' + options.objId).innerHTML = response.responseText;
            },
            failure: function (error) {
                //alert(DWRUtil.toDescriptiveString(error, 3));
            },
            objId: record.id
        });

        return body;
    },
    beforeExpand: function (record, body, rowIndex) {
        if (this.fireEvent('beforeexpand', this, record, body, rowIndex) !== false) {
            body.innerHTML = this.getBodyContent(record, rowIndex);
            return true;
        } else {
            return false;
        }
    }
});