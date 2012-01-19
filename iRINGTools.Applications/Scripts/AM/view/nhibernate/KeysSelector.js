Ext.define('AM.view.nhibernate.KeysSelector', {
    extend: 'Ext.ux.ItemSelector',
    alias: 'widget.keysSelector',
    bodyStyle: 'background:#eee',
    name: 'keySelector',
    frame: true,
    imagePath: 'scripts/ext-3.3.1/examples/ux/images/',
    hideLabel: true,

    initComponent: function () {
        this.multiselects = [{
				width: 240,
				height: 370,
				border: 0,
				store: availItems,
				displayField: 'keyName',
				valueField: 'keyValue'
			}, {
				width: 240,
				height: 370,
				border: 0,
				store: selectedItems,
				displayField: 'keyName',
				valueField: 'keyValue'
			}];
		this.treeNode = node
		});
    }
});

