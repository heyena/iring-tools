
/*
 * @File Name : federation-tree.js
 * @Path : resources/js
 * @Using Lib : Ext JS Library 3.2.1(lib/ext-3.2.1)
 *
 * This file intended to make Federation Tree using JSON data string
 * It contains different event handlers functions to perform particular action at each time
 * It also used to display Detail panel that is binded with Federation panel
 * Anf it generate Forms to display and edit the properties of Tree node in a Tab panel
 *
 */

var tree

Ext.onReady(function(){

    Ext.QuickTips.init();

    // turn on validation errors beside the field globally
    Ext.form.Field.prototype.msgTarget = 'side';


    tree = new Ext.tree.TreePanel({
            region:'north',
            split:true,
            id:'federation-tree',
            height:300,
            bodyBorder:false,
            border:false,
            hlColor:'C3DAF',
            layout:'fit',
            useArrows:false, // true for vista like
            autoScroll:true,
            animate:true,
            margins: '0 0 0 0',
            lines :true,
            containerScroll: true,
            rootVisible: true,
            root: {
              nodeType: 'async',
              Name:'Federation',
              Description:'Descripton of Federation',
              icon: 'resources/images/16x16/internet-web-browser.png',
              text: 'Federation'
            },
            dataUrl: 'federation-tree.json',
            tbar: new Ext.Toolbar({
                    xtype: "toolbar",
                    items:[{
                    xtype:"tbbutton",
                    icon:'resources/images/16x16/view-refresh.png',
                    tooltip:'Reload',
                    disabled: false,
                    handler:function(){
                            Ext.state.Manager.clear("treestate");
                            tree.root.reload();
                              }
                        },{
                                // For Open and Edit
                                xtype:"tbbutton",
                                text:'Open',
                                icon:'resources/images/16x16/document-open.png',
                                id: 'headExchange',
                                tooltip:'Open',
                                disabled: false,
                                handler: function(){showCentralEditForms(tree.getSelectionModel().getSelectedNode());
                              }
                           },{
                                xtype:"tbbutton",
                                text:'Add New',
                                icon:'resources/images/16x16/document-new.png',
                                tooltip:'Add New',
                                disabled: false,
                                handler:function(){
                                   // new form with blank fields be there
                                }
                            }
                        ]
                 }),
            listeners: {
                click: {
                 fn: function(node){
                     //get all the attributes of node
                     obj = node.attributes

                     var details_data = []
                       for(var key in obj){
                         // restrict some of the properties to be displayed
                         if(key!='nodeType' && key!='cls' && key!='id' && key!='text' && key!='icon' && key!='children' && key!='loader' && key!='leaf' && key!='applicationId'){
                            details_data[key]=obj[key]
                         }
                       }

                       // get the property grid component
                        var propGrids = Ext.getCmp('propGrid');
                        // make sure the property grid exists
                        if (propGrids) {
                          // populate the property grid with details_data
                          propGrids.setSource(details_data);
                        }
                  }
                },
                expandnode:{
                    fn : function (node){
                        Ext.state.Manager.set("treestate", node.getPath())
                    }
                },
                dblclick :{
		fn : function (node){
                   showCentralEditForms(node);
                }
              }
           }
        });

        var contextMenu = new Ext.menu.Menu({
        items: [
            {
              text: 'Sort',
              handler: sortHandler
            }
        ]
      });
      function sortHandler() {
        tree.getSelectionModel().getSelectedNode().sort(
          function (leftNode, rightNode) {
            return 1;
          }
          );
      }

      /* to maintain the state of the tree */
      Ext.state.Manager.setProvider(new Ext.state.CookieProvider());
      tree.on('contextmenu', function (node){
        node.select();
        contextMenu.show(node.ui.getAnchor());
      });

    /* to maintain the state of the tree */
      var treeState = Ext.state.Manager.get("treestate");
      if (treeState){
              if(tree.expandPath(treeState)){ //check the
                      tree.expandPath(treeState);
              }else{
                      Ext.state.Manager.clear("treestate");
                      tree.root.reload();
              }
      }


function showCentralEditForms(node){
  var obj = node.attributes
  var nId  = obj['id']

  if('children' in obj !=true){ // restrict to generate form those have children

      /*
       * Generate the fields items dynamically
       */
       var list_items=''
           for(var key in obj){
             // restrict some of the properties to be displayed
             if(key!='nodeType' && key!='cls' && key!='id' && key!='text' && key!='icon' && key!='children' && key!='loader' && key!='leaf' && key!='applicationId'){

                if(list_items!=''){
                     list_items = list_items+ ',{' +'fieldLabel:"' +key+'",name:"'+ key+'",allowBlank:false}'
                }else{
                     list_items = '{' +'fieldLabel:"' +key+'",name:"'+ key+'",allowBlank:false}'
                }
              }
        }
        list_items=eval('['+list_items+']')

        // generate form for editing purpose
         var edit_form = new Ext.FormPanel({
            labelWidth: 75, // label settings here cascade unless overridden
           // url:'save-form.php',
           //url: BASE_URL + 'user/ext_login', method: 'POST', id: 'frmLogin',
           border:false, //  removing the border of the form
            //renderTo:'centerPanel',
            id:'frm_'+nId,
            //renderTo:document.body,
            renderTo:Ext.getBody(),
            frame:true,
            bodyStyle:'padding:5px 5px 0',
            width: 350,
            closable:true,
            defaults: {width: 230},
            defaultType: 'textfield',
            items:  list_items , // binding with the fields list
            buttonAlign: 'left', //buttons aligned to the left
            buttons: [{
                text: 'Save'
            },{
                text: 'Cancel'
            }/*{ text: 'Reset', handler: function() {
                    formLogin.getForm().reset();
                }*/]
        });

    // fill the data in all fields of form
    edit_form.getForm().setValues(node.attributes);

        // display the form in the center panel as a tab
        Ext.getCmp('centerPanel').enable();
        Ext.getCmp('centerPanel').add(
        Ext.apply(edit_form,{
        id:'tab-'+obj['id'],
        title: node.parentNode.text+' : '+obj['text'],
        closable:true
        })).show();

  }// end of if
}
}); // end on onReady function

