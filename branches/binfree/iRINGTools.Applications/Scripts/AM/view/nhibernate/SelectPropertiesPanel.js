Ext.define('AM.view.nhibernate.SelectPropertiesPanel', {
    extend: 'Ext.form.Panel',
    alias: 'widget.selectProperties',
    bodyStyle: 'background:#eee;padding:10px 10px 0px 10px',
    border: false,
    frame: false,
    autoScroll: true,
    layout: 'fit',
    defaults: { anchor: '100%' },
    labelWidth: 160,

    initComponent: function () {
        this.items = [{
            xtype: 'label',
            fieldLabel: 'Select Properties',
            itemCls: 'form-title',
            labelSeparator: ''
        }, propertiesItemSelector];

        this.tbar = new Ext.Toolbar({
            items: [{
                xtype: 'tbspacer',
                width: 4
            }, {
                xtype: 'tbbutton',
                icon: 'Content/img/16x16/apply.png',
                text: 'Apply',
                tooltip: 'Apply the current changes to the data objects tree',
                handler: function (f) {
                    var selectedValues = propertiesItemSelector.toMultiselect.store.data.items;
                    var treeNode = propertiesItemSelector.treeNode;

                    for (var i = 0; i < treeNode.childNodes.length; i++) {
                        var found = false;

                        for (var j = 0; j < selectedValues.length; j++) {
                            if (selectedValues[j].data.text.toLowerCase() == treeNode.childNodes[i].text.toLowerCase()) {
                                found = true;
                                break;
                            }
                        }

                        if (!found)
                            treeNode.childNodes[i].getUI().hide();
                        else
                            treeNode.childNodes[i].getUI().show();

                        if (treeNode.expanded == false)
                            treeNode.expand();
                    }
                }
            }, {
                xtype: 'tbspacer',
                width: 4
            }, {
                xtype: 'tbbutton',
                icon: 'Content/img/16x16/edit-clear.png',
                text: 'Reset',
                tooltip: 'Reset to the latest applied changes',
                handler: function (f) {
                    var availProps = new Array();
                    var selectedProps = new Array();
                    var availPropsSingle = new Array();
                    var toPropsSingle = new Array();
                    var firstAvailProps = new Array();
                    var firstToProps = new Array();
                    for (var i = 0; i < node.childNodes.length; i++) {
                        var itemName = node.childNodes[i].text;
                        if (node.childNodes[i].hidden == false) {
                            selectedProps.push([itemName, itemName]);
                            toPropsSingle.push(itemName);
                        }
                        else {
                            availProps.push([itemName, itemName]);
                            availPropsSingle.push(itemName);
                        }
                    }

                    if (availProps[0]) {
                        firstAvailProps.push(availProps[0]);

                        if (propertiesItemSelector.fromMultiselect.store.data)
                            removeSelectorStore(propertiesItemSelector.fromMultiselect);

                        loadSelectorStore(propertiesItemSelector.fromMultiselect, firstAvailProps);
                        var firstAvailPropsItems = propertiesItemSelector.fromMultiselect.store.data.items;
                        var loadSingle = false;
                        var availPropName = firstAvailPropsItems[0].data.text;

                        if (availPropName[1])
                            if (availPropName[1].length > 1)
                                var loadSingle = true;

                        if (!loadSingle)
                            setSelectorStore(propertiesItemSelector.fromMultiselect, availProps);
                        else
                            setSelectorStore(propertiesItemSelector.fromMultiselect, availPropsSingle);
                    }
                    else
                        cleanSelectorStore(propertiesItemSelector.fromMultiselect);

                    if (selectedProps[0]) {
                        firstToProps.push(selectedProps[0]);

                        if (propertiesItemSelector.toMultiselect.store.data)
                            removeSelectorStore(propertiesItemSelector.toMultiselect);

                        loadSelectorStore(propertiesItemSelector.toMultiselect, firstToProps);
                        var firstToPropsItems = propertiesItemSelector.toMultiselect.store.data.items;
                        var loadSingle = false;
                        var toPropName = firstToPropsItems[0].data.text;

                        if (toPropName[1])
                            if (toPropName[1].length > 1)
                                var loadSingle = true;

                        if (!loadSingle)
                            setSelectorStore(propertiesItemSelector.toMultiselect, selectedProps);
                        else
                            setSelectorStore(propertiesItemSelector.toMultiselect, toPropsSingle);
                    }
                    else
                        cleanSelectorStore(propertiesItemSelector.toMultiselect);
                }
            }]
        });

        editPane.add(propertiesSelectorPanel);
        var panelIndex = editPane.items.indexOf(propertiesSelectorPanel);
        editPane.getLayout().setActiveItem(panelIndex);
    }
});

function removeSelectorStore(selector) {
	selector.reset();
	selector.store.removeAll();
}

function loadSelectorStore(selector, storeData) {
	selector.store.loadData(storeData);
	selector.store.commitChanges();
}

function setSelectorStore(selector, storeData) {
	removeSelectorStore(selector);
	loadSelectorStore(selector, storeData);
}

function cleanSelectorStore(selector) {
	removeSelectorStore(selector);
	selector.store.commitChanges();
}