Ext.ns('FederationManager');
/**
* @class FederationManager.SearchPanel
* @author by Aswini Nayak
*/

//image path
var IMG_CLASS = 'Content/img/class.png';
var IMG_TEMPLATE = 'Content/img/template.png';
//renderer function
function renderIcon(value, p, record) {
    var label = null;

    if (record.data.Uri.indexOf("tpl") != -1) {
        label = '<img src="' + IMG_TEMPLATE + '" align="top"> ' + value;
    } else {
        label = '<img src="' + IMG_CLASS + '" align="top"> ' + value;
    }
    return label;
}

FederationManager.SearchPanel = Ext.extend(Ext.Panel, {
    title: 'Reference Data Search',
    layout: 'border',
    border: true,
    split: true,
    searchUrl: null,
    limit: 100,
    refClassTabPanel:null,
    propertyPanel:null,

    /**
    * initComponent
    * @protected
    */
    initComponent: function () {
        this.tbar = this.buildToolbar();
        this.propertyPanel = new Ext.grid.PropertyGrid( {
            id : 'class-property-panel',
            title : 'Details',
            region : 'east',
            // layout: 'fit',
            stripeRows : true,
            collapsible : true,
            autoScroll : true,
            width:350,
        	split: true,
        	bodyBorder:true,
        	collapsed : false,
            border : true,
            frame : true,
            //bodyStyle: 'padding-bottom:15px;background:#eee;',
            source : {},
            listeners : {
              // to disable editable option of the property grid
              beforeedit : function(e) {
                e.cancel = true;
              }
            }
          });

        this.refClassTabPanel = new Ext.TabPanel({
            id: 'content-pane',
            region: 'center',
            deferredRender: false,
            enableTabScroll: true,
            border: true,
            activeItem: 0
          });
        
        
        this.items =[this.refClassTabPanel,this.propertyPanel];

        // super
        FederationManager.SearchPanel.superclass.initComponent.call(this);
    },

    buildToolbar: function () {
        return [ ' Search:  ',' ',
                 {
        			xtype: 'textfield',
        			allowBlank:false,
        			blankText :'This field can not be blank',
        			name: 'referencesearch',
        			id:'referencesearch',
        			listeners: {
        	              specialkey: function(f,e){
        	                if (e.getKey() == e.ENTER) {
        	                    alert("about to search: "+Ext.get('referencesearch').getValue());
        	                }
        	              }
        	            }
            	},
                 {
                	xtype: 'checkbox',
               	  	boxLabel:'Reset',
               	  	text: 'Reset',
               	  	name: 'reset'
               	  },
               	  {
                     xtype : "tbbutton",
                     text : 'Promote',
                     //icon : 'resources/images/16x16/view-refresh.png',
                     tooltip : 'Promote',
                     disabled : false,
                     handler: this.onPromote,
                     scope : this
                   },
                   {
                     xtype : "tbbutton",
                     text : 'Add Class',
                     tooltip : 'Add Class',
                     disabled : false,
                     handler: this.onClassAdd,
                     scope : this
                   },
                   {
                     xtype : "tbbutton",
                     text : 'Edit Class',
                     tooltip : 'Edit Class',
                     disabled : true,
                     handler: this.onClassEdit,
                     scope : this
                   },
                   {
                     xtype : "tbbutton",
                     text : 'Add Template',
                     tooltip : 'Add Template',
                     disabled : false,
                     handler: this.onTemplateAdd,
                     scope : this
                   },
                   {
                     xtype : "tbbutton",
                     text : 'Edit Template',
                     tooltip : 'Edit Template',
                     disabled : true,
                     handler: this.onTemplateEdit,
                     scope : this
                   }];
      },
    load: function () {
        //this.searchStore.load({ params: { start: 0, limit: this.limit} });
        return;
    }

});