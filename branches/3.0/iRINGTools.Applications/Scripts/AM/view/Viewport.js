Ext.define('AM.view.Viewport', {
    extend: 'Ext.container.Viewport',
	alias: 'viewport',
    requires: [
        'AM.view.directory.DirectoryPanel',
        'AM.view.common.CenterPanel'
    ],

    layout: 'border',

    initComponent: function () {
        var me = this;

        Ext.applyIf(me, {
            items: [
            {
                xtype: 'panel',
                region: 'north',
                height: 52,
				html: '<div id="header" class="banner">' +
                      '  <span style="float:left">' +
                      '    <img style="margin:0 5px 0 5px" src="./Content/img/iringtools-logo.png"/>' +
                      '       <font style="font-size:24px;font-family:Arial">Adapter Manager</font>' +
                      '  	 </span>' +
					  '  	 <span style="float:right;margin-top:16px">' +
					  '		 <input type="checkbox"  id="gridCheckbox" style="display:none;" />' +
                      '      <a id="schedule-link" href="#" class="header-link"> Schedule Cache Upadte  </a> ' +
//                      '      <a id="schedule-link" ><br><input type="checkbox"  name="njob" value="NewJob">View Schedule Jobs </a>' +
					  '      <a id="setting-link" href="#" class="header-link">Settings</a>' +
					  '      <a href="http://iringug.org/wiki/index.php?title=IRINGTools" target="_blank" class="header-link">Help</a>' +
					  '      <a id="about-link" href="#" class="header-link">About</a>' +
                      '  </span>' +
                      '</div>'
				
            },
            {
                xtype: 'directorypanel',
                id: 'directoryTreeID',
                padding: 2,
                width: 260,
                region: 'west',
                listeners: {
                    afterrender: {
                        fn: me.onPanelAfterRender,
                        scope: me
                    }
                }
            },
            {
                xtype: 'centerpanel',
                region: 'center'
            }],
            renderTo: Ext.getBody()
        });

        me.callParent(arguments);

        Ext.onReady(function () {
            Ext.get('about-link').on('click', function () {
                var win = new Ext.Window({
                    title: 'About Adapter Manager',
                    autoLoad: 'about.aspx',
                    bodyStyle: 'background:#fff;padding:5px',
                    width: 725,
                    height: 508,
                    closable: true,
                    autoScroll: true,
                    modal: true
                });
                win.show();
            });
			 Ext.get('setting-link').on('click', function () {
				var scroll = false;
				var pagination = false;
				if(Ext.getElementById('gridCheckbox').checked)
					 scroll = true;
				else
					 pagination = true;
					 
                var win = new Ext.Window({
                    title: 'Grid Settings',
                    //autoLoad: 'about.aspx',
                    bodyStyle: 'background:#fff;padding:5px',
                    width: 260,
                    height: 150,
                    closable: true,
                    //autoScroll: true,
                    modal: true,
					items:[
						{
							xtype:'radiofield',
							boxLabel  : 'Enable Grid Pagination',
							name      : 'gridSettings',
							inputValue: 'pagination',
							id        : 'pagenationRadio',
							checked	  :  pagination,
							//boxLabelAlign : 'before',
							margin	  : '10 0 0 10'
						},
						{
							xtype:'radiofield',
							boxLabel  : 'Enable Grid Scrolling',
							name      : 'gridSettings',
							inputValue: 'scrolling',
							id        : 'scrollingRadio',
							checked	  :  scroll,
							//boxLabelAlign : 'before',
							margin	  : '13 0 0 10'
						}
					],
				    dockedItems: [{
							xtype: 'toolbar',
							dock: 'bottom',
							layout: {pack: 'end' } ,
							items: [
									{ 
										xtype: 'button',
										text: 'Apply',
										handler:function(){
										    var checkboxfield = Ext.getElementById('gridCheckbox');
											if(Ext.getCmp('pagenationRadio').checked){
												Ext.getElementById('gridCheckbox').checked = false;
											}
											else if(Ext.getCmp('scrollingRadio').checked){
													Ext.getElementById('gridCheckbox').checked = true;
												}
											this.up().up().close();
										}
									},
									{ 
										xtype: 'button',
										text: 'Cancel',
										handler:function(){
											this.up().up().close();
										}
									}
							]
					    }]
                });
                win.show();
            });


            //start viewschedule

            Ext.get('schedule-link').on('click', function () {
                var win = new Ext.Window({
                    bodyStyle: 'background:#fff;padding:5px',
                    width:1050,
                    height: 350,
                    tbar: [  // <--- ToolBar
                        {
                             text: 'Refresh',
                             icon: 'Content/img/16x16/view-refresh.png'
                            }
                ],

                   bbar: [
                {
                    text: 'Close',
                    handler: function () { this.up('.window').close(); }
                }
            ],
            autoShow: true,
                                //width: 607,
                    //height: 450,
                    closable: true,
                    autoScroll: true,
                    modal: true,
                    layout: 'vbox',

                    items: [{
                        xtype: 'viewjobsform'
                    }]


                });
                win.show();
                win.center();
               

            });
            //end viewschedule
        });
    },

    onPanelAfterRender: function (component, eOpts) {
        Ext.getBody().on("contextmenu", Ext.emptyFn, null, {
            preventDefault: true
        });
    }
});