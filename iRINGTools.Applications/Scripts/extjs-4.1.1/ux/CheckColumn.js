/**
 * @class Ext.ux.CheckColumn
 * @extends Ext.grid.column.Column
 * A Header subclass which renders a checkbox in each column cell which toggles the truthiness of the associated data field on click.
 *
 * Example usage:
 * 
 *    // create the grid
 *    var grid = Ext.create('Ext.grid.Panel', {
 *        ...
 *        columns: [{
 *           text: 'Foo',
 *           ...
 *        },{
 *           xtype: 'checkcolumn',
 *           text: 'Indoor?',
 *           dataIndex: 'indoor',
 *           width: 55
 *        }]
 *        ...
 *    });
 *
 * In addition to toggling a Boolean value within the record data, this
 * class adds or removes a css class <tt>'x-grid-checked'</tt> on the td
 * based on whether or not it is checked to alter the background image used
 * for a column.
 */
/*Ext.define('Ext.ux.CheckColumn', {
    extend: 'Ext.grid.column.Column',
    alias: 'widget.checkcolumn',

    *//**
     * @cfg {Boolean} [stopSelection=true]
     * Prevent grid selection upon mousedown.
     *//*
    stopSelection: true,

    tdCls: Ext.baseCSSPrefix + 'grid-cell-checkcolumn',

    constructor: function() {
        this.addEvents(
            *//**
             * @event beforecheckchange
             * Fires when before checked state of a row changes.
             * The change may be vetoed by returning `false` from a listener.
             * @param {Ext.ux.CheckColumn} this CheckColumn
             * @param {Number} rowIndex The row index
             * @param {Boolean} checked True if the box is to be checked
             *//*
            'beforecheckchange',
            *//**
             * @event checkchange
             * Fires when the checked state of a row changes
             * @param {Ext.ux.CheckColumn} this CheckColumn
             * @param {Number} rowIndex The row index
             * @param {Boolean} checked True if the box is now checked
             *//*
            'checkchange'
        );
        this.callParent(arguments);
    },

    *//**
     * @private
     * Process and refire events routed from the GridView's processEvent method.
     *//*
    processEvent: function(type, view, cell, recordIndex, cellIndex, e, record, row) {
        var me = this,
            key = type === 'keydown' && e.getKey(),
            mousedown = type == 'mousedown';

        if (mousedown || (key == e.ENTER || key == e.SPACE)) {
            var dataIndex = me.dataIndex,
                checked = !record.get(dataIndex);

            // Allow apps to hook beforecheckchange
            if (me.fireEvent('beforecheckchange', me, recordIndex, checked) !== false) {
                record.set(dataIndex, checked);
                me.fireEvent('checkchange', me, recordIndex, checked);

                // Mousedown on the now nonexistent cell causes the view to blur, so stop it continuing.
                if (mousedown) {
                    e.stopEvent();
                }

                // Selection will not proceed after this because of the DOM update caused by the record modification
                // Invoke the SelectionModel unless configured not to do so
                if (!me.stopSelection) {
                    view.selModel.selectByPosition({
                        row: recordIndex,
                        column: cellIndex
                    });
                }

                // Prevent the view from propagating the event to the selection model - we have done that job.
                return false;
            } else {
                // Prevent the view from propagating the event to the selection model if configured to do so.
                return !me.stopSelection;
            }
        } else {
            return me.callParent(arguments);
        }
    },

    // Note: class names are not placed on the prototype bc renderer scope
    // is not in the header.
    renderer : function(value){
        var cssPrefix = Ext.baseCSSPrefix,
            cls = [cssPrefix + 'grid-checkheader'];

        if (value) {
            cls.push(cssPrefix + 'grid-checkheader-checked');
        }
        return '<div class="' + cls.join(' ') + '">&#160;</div>';
    }
});*/

Ext.define('Ext.ux.CheckColumn', {
    extend: 'Ext.grid.column.Column',
    alias: 'widget.checkcolumn',

    disableColumn: false,
    disableFunction: null,
    disabledColumnDataIndex: null,
    columnHeaderCheckbox: false,

    constructor: function(config) {

        var me = this;
        if(config.columnHeaderCheckbox)
        {
            var store = config.store;
            store.on("datachanged", function(){
                me.updateColumnHeaderCheckbox(me);
            });
            store.on("update", function(){
                me.updateColumnHeaderCheckbox(me);
            });
            config.text = me.getHeaderCheckboxImage(store, config.dataIndex);
        }

        me.addEvents(
            /**
             * @event checkchange
             * Fires when the checked state of a row changes
             * @param {Ext.ux.CheckColumn} this
             * @param {Number} rowIndex The row index
             * @param {Boolean} checked True if the box is checked
             */
            'beforecheckchange',
            /**
             * @event checkchange
             * Fires when the checked state of a row changes
             * @param {Ext.ux.CheckColumn} this
             * @param {Number} rowIndex The row index
             * @param {Boolean} checked True if the box is checked
             */
            'checkchange'
        );

        me.callParent(arguments);
    },

    updateColumnHeaderCheckbox: function(column){
        var image = column.getHeaderCheckboxImage(column.store, column.dataIndex);
        column.setText(image);
    },

    toggleSortState: function(){
        var me = this;
        if(me.columnHeaderCheckbox)
        {
            var store = me.up('tablepanel').store;
            var isAllChecked = me.getStoreIsAllChecked(store, me.dataIndex);
            store.each(function(record){
                record.set(me.dataIndex, !isAllChecked);
                record.commit();
            });
        }
        else
            me.callParent(arguments);
    },

    getStoreIsAllChecked: function(store, dataIndex){
        var allTrue = true;
        store.each(function(record){
            if(!record.get(dataIndex))
                allTrue = false;
        });
        return allTrue;
    },

    getHeaderCheckboxImage: function(store, dataIndex){

        var allTrue = this.getStoreIsAllChecked(store, dataIndex);

        var cssPrefix = Ext.baseCSSPrefix,
            cls = [cssPrefix + 'grid-checkheader'];

        if (allTrue) {
            cls.push(cssPrefix + 'grid-checkheader-checked');
        }
        return '<div class="' + cls.join(' ') + '">&#160;</div>'
    },

    /**
     * @private
     * Process and refire events routed from the GridView's processEvent method.
     */
    processEvent: function(type, view, cell, recordIndex, cellIndex, e) {
        if (type == 'mousedown' || (type == 'keydown' && (e.getKey() == e.ENTER || e.getKey() == e.SPACE))) {
            var record = view.panel.store.getAt(recordIndex),
                dataIndex = this.dataIndex,
                checked = !record.get(dataIndex),
                column = view.panel.columns[cellIndex];
            if(!(column.disableColumn || record.get(column.disabledColumnDataIndex) || (column.disableFunction && column.disableFunction(checked, record))))
            {
                if(this.fireEvent('beforecheckchange', this, recordIndex, checked, record))
                {
                    record.set(dataIndex, checked);
                    this.fireEvent('checkchange', this, recordIndex, checked, record);
                }
            }
            // cancel selection.
            return false;
        } else {
            return this.callParent(arguments);
        }
    },

    // Note: class names are not placed on the prototype bc renderer scope
    // is not in the header.
    renderer : function(value, metaData, record, rowIndex, colIndex, store, view){
        var disabled = "",
            column = view.panel.columns[colIndex];
        if(column.disableColumn || column.disabledColumnDataIndex || (column.disableFunction && column.disableFunction(value, record)))
            disabled = "-disabled";
        var cssPrefix = Ext.baseCSSPrefix,
            cls = [cssPrefix + 'grid-checkheader' + disabled];

        if (value) {
            cls.push(cssPrefix + 'grid-checkheader-checked' + disabled);
        }
        return '<div class="' + cls.join(' ') + '">&#160;</div>';
    }
});
