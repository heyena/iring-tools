/*!
* Ext JS Library 3.2.1
* Copyright(c) 2006-2010 Ext JS, Inc.
* licensing@extjs.com
* http://www.extjs.com/license
*/
Ext.ns('iIRNGTools', 'iIRNGTools.ScopeEditor');
/**
* @class iIRNGTools.ScopeDetails
* @extends FormPanel
* @author by Gert Jansen van Rensburg
*/
iIRNGTools.ScopeEditor.ScopeDetails = Ext.extend(Ext.grid.PropertyGrid, {
    iconCls: 'silk-user',
    frame: true,
    width: 500,
    viewConfig: {
        forceFit: true
    },

    record: null,

    /**
    * initComponent
    * @protected
    */
    initComponent: function () {
        // build form-top toolbar buttons        
        this.tbar = this.buildUI();

        // add a create event for convenience in our application-code.
        this.addEvents({
            /**
            * @event create
            * Fires when user clicks [create] button
            * @param {FormPanel} this
            * @param {Object} values, the Form's values object
            */
            create: true
        });

        // super
        iIRNGTools.ScopeEditor.ScopeDetails.superclass.initComponent.call(this);
    },

    /**
    * buildUI
    * @private
    */
    buildUI: function () {
        return [{
            text: 'Save',
            //iconCls: 'icon-save',
            handler: this.onUpdate,
            scope: this
        }, {
            text: 'Create',
            //iconCls: 'silk-user-add',
            handler: this.onCreate,
            scope: this
        }, {
            text: 'Reset',
            handler: function (btn, ev) {
                loadRecord(record);
            },
            scope: this
        }];
    },

    /**
    * loadRecord
    * @param {Record} rec
    */
    loadRecord: function (rec) {
        this.record = rec;
        this.setSource(rec);
    },

    /**
    * onUpdate
    */
    onUpdate: function (btn, ev) {
        if (this.record == null) {
            return;
        }
        if (!this.getForm().isValid()) {
            App.setAlert(false, "Form is invalid.");
            return false;
        }
        this.getForm().updateRecord(this.record);
    },

    /**
    * onCreate
    */
    onCreate: function (btn, ev) {
        if (!this.getForm().isValid()) {
            App.setAlert(false, "Form is invalid");
            return false;
        }
        this.fireEvent('create', this, this.getForm().getValues());
        this.getForm().reset();
    },

    /**
    * onReset
    */
    onReset: function (btn, ev) {
        this.fireEvent('update', this, this.getForm().getValues());
        this.getForm().reset();
    }
});