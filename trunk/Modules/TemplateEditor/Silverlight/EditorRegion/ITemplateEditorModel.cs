using System;
using System.ComponentModel;
using System.Linq;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

using org.ids_adi.qmxf;

namespace org.iringtools.modules.templateeditor.editorregion
{
    public interface ITemplateEditorModel
    {
        QMXF QMXF { get; }

        string Heading { get; }

        string Name { get; set; }

        string Description { get; set; }

        string StatusAuthority { get; }

        string StatusClass { get; }

        string StatusFrom { get; }

        string StatusTo { get; }

        ObservableCollection<KeyValuePair<string, object>> Roles { get; set; }

        string Identifier { get; set; }

        Boolean IsReadOnly { get; }
        Boolean IsEnabled { get; }
        Boolean IsBaseTemplate { get; }
        Boolean IsSpecializedTemplate { get; }

        object SelectedRole { get; set; }

        string SelectedRoleIdentifier { get; set; }

        string SelectedRoleDesignation { get; set; }

        string SelectedRoleName { get; set; }

        string SelectedRoleDescription { get; set; }

        KeyValuePair<string, string> SelectedRoleRange { get; set; }

        string SelectedRoleValueReference { get; set; }

        string SelectedRoleValueLiteral { get; set; }

        KeyValuePair<string, string> SelectedRoleValueLiteralDatatype { get; set; }

        ObservableCollection<KeyValuePair<string, string>> Ranges { get; }

        ObservableCollection<KeyValuePair<string, string>> LiteralDataTypes { get; }

        ObservableCollection<KeyValuePair<string, object>> SelectedRoleRestrictions { get; set; }

        void AddRole(string name, string description, string uri);

        void ApplyRole(object objRole, string name, string description, string uri);
        
    }
}
