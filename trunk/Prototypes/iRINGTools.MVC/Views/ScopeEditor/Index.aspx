<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html>
<head runat="server">
    <title>Scope Editor</title>
    <link rel="stylesheet" type="text/css" href="~/Scripts/ext/resources/css/ext-all.css"/>    
    <script type="text/javascript" src="~/Scripts/ext/adapter/ext/ext-base.js"></script>
    <script type="text/javascript" src="~/Scripts/ext/ext-all-debug.js"></script> 
    <script type="text/javascript">

        Ext.onReady(function () {

            var projectStore = new Ext.data.JsonStore({
                url: 'http://localhost:1357/AdapterService/Scopes',
                root: 'Projects',
                idProperty: 'Name',
                totalProperty: 'Count',
                fields: ['Name', 'Description', 'Applications'],
                remoteSort: true,
                autoDestroy: true,
                autoLoad: true
            });

            var scopesGrid = new Ext.grid.GridPanel({
                title: 'Projects',
                store: projectStore,
                columns: [
                    { id: 'project-col', header: "Project", width: 180, dataIndex: 'Name', sortable: true },
                    { header: "Description", width: 65, dataIndex: 'Description', sortable: true, align: 'right' }                    
                ],
                autoExpandColumn: 'project-col',
                renderTo: Ext.getBody(),
                width: 400,
                height: 250,
                loadMask: true,
                columnLines: true
            });

        });
        
    </script>
</head>
<body>
    <div>
    
    </div>
</body>
</html>
