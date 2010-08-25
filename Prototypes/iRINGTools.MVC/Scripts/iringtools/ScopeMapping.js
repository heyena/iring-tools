new Ext.tree.TreePanel({
    
});

/*!
* Ext JS Library 3.2.1
* Copyright(c) 2006-2010 Ext JS, Inc.
* licensing@extjs.com
* http://www.extjs.com/license
*/
Ext.ns('iIRNGTools', 'iIRNGTools.ScopeEditor');
/**
* @class iIRNGTools.ScopeMapping
* @extends TreePanel
* @author by Gert Jansen van Rensburg
*/
iIRNGTools.ScopeEditor.ScopeMapping = Ext.extend(Ext.tree.TreePanel, {
    id: 'tree-panel',
    title: 'Registered Scopes',
    region: 'north',
    split: true,
    height: 300,
    minSize: 150,
    autoScroll: true,

    // tree-specific configs:
    rootVisible: false,
    lines: false,
    singleExpand: true,
    useArrows: true,    
        
    /**
    * initComponent
    * @protected
    */
    initComponent: function () {
        // build panel-buttons
        this.buttons = this.buildUI();

        // add a create event for convenience in our application-code.
        //this.addEvents({
            /**
            * @event click
            * Fires when user clicks on a treenode
            * @param {TreePanel} this
            * @param {Object} values, the Form's values object
            */
        //    click: true
        //});        

        // super
        iIRNGTools.ScopeEditor.ScopeMapping.superclass.initComponent.call(this);
    },

    /**
    * buildUI
    * @private
    */
    buildUI: function () {
        return [{
            text: 'Save',
            iconCls: 'icon-save',
            //handler: this.onUpdate,
            scope: this
        }, {
            text: 'Create',
            iconCls: 'silk-user-add',
            //handler: this.onCreate,
            scope: this
        }, {
            text: 'Reset',
            //handler: null,
            scope: this
        }];
    },

    /**
    * onClick
    */
    onClick: function (node, ev) {
        this.fireEvent('click', node, this);        
    }
    
});