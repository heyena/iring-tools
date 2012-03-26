Ext.define('AM.view.Viewport', {
  extend: 'Ext.container.Viewport',
  layout: {
    type: 'border'
  },
  items: [
        {
          xtype: 'box',
          height: 55,
          region: 'north',
          html: '<div id="header" class="exchangeBanner">' +
                  '<span style="float: left">' +
                  '<img alt="q" src="Content/img/iRINGTools_logo.png" style="margin: 0 0 0 11px; vertical-align: -20%" />' +
                  '<span style="margin: 0 0 0 6px;"><font size="5px" style="font-family: Arial, Helvetica, Sans-Serif">' +
                  'Adapter Manager</font></span> </span><span style="float: right; margin: 18px 36px 1px 0">' +
                  '<a href="http://iringug.org/wiki/index.php?title=IRINGTools" target="_blank" class="headerLnkBlack">' +
                  'Help</a>&nbsp;&nbsp;&nbsp;&nbsp;<a id="about-link" href="#" class="headerLnkBlack">About</a></span></div>'
        },
        { xtype: 'centerpanel', region: 'center' },
        {
          xtype: 'directorypanel',
          region: 'west',
          items: [
                { xtype: 'directorytree', region: 'center' },
                { xtype: 'propertypanel', region: 'south', height: 250 }
            ]
        }
    ],
  listeners: {
    render: function () {
      Ext.getBody().on("contextmenu", Ext.emptyFn, null, {
        preventDefault: true
      });
    }
  }
});
