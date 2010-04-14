using System;
using System.ComponentModel;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Xml.Schema;
using System.Reflection;

using org.ids_adi.qmxf;

namespace Modules.TemplateEditor.EditorRegion
{
    public abstract class TemplateModel : ITemplateEditorModel, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        internal ObservableCollection<KeyValuePair<string, string>> _ranges;
        internal ObservableCollection<KeyValuePair<string, object>> _roles = new ObservableCollection<KeyValuePair<string, object>>();
        internal string _heading = "";
        internal Boolean _readonly = false;
        internal QMXF _qmxf;
        
        internal object _selectedRole = null;
        
        public virtual void RaisePropertyChanged(object sender, string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(sender, new PropertyChangedEventArgs(propertyName));
            }
        }

        public abstract QMXF QMXF { get; }
                
        public abstract string Name { get; set; }

        public abstract string Description { get; set; }

        public abstract string StatusAuthority { get; }

        public abstract string StatusClass { get; }

        public abstract string StatusFrom { get; }

        public abstract string StatusTo { get; }

        public virtual ObservableCollection<KeyValuePair<string, object>> Roles
        {
            get
            {
                return _roles;
            }
            set
            {
                _roles = value;

                PropertyChanged(this, new PropertyChangedEventArgs("Roles"));
            }
        }

        public abstract string Identifier { get; set; }

        public virtual string Heading
        {
            get
            {
                return _heading;
            }
        }

        public virtual Boolean IsReadOnly
        {
            get
            {
                return _readonly;
            }
        }

        public virtual Boolean IsEnabled
        {
            get
            {
                return !_readonly;
            }
        }

        public virtual object SelectedRole
        {
            get
            {
                return _selectedRole;
            }
            set
            {
                _selectedRole = value;
                
                RaisePropertyChanged(this, "SelectedRole");
                RaisePropertyChanged(this, "SelectedRoleIdentifier");
                RaisePropertyChanged(this, "SelectedRoleName");
                RaisePropertyChanged(this, "SelectedRoleDescription");
                RaisePropertyChanged(this, "SelectedRoleRange");
                RaisePropertyChanged(this, "SelectedRoleDesignation");
            }
        }

        public abstract string SelectedRoleIdentifier { get; set; }

        public abstract string SelectedRoleDesignation { get; set; }

        public abstract string SelectedRoleName { get; set; }

        public abstract string SelectedRoleDescription { get; set; }

        public abstract KeyValuePair<string, string> SelectedRoleRange { get; set; }

        public abstract void AddRole(string name, string description, string range);

        public abstract void ApplyRole(object objRole, string name, string description, string range);
        
        private static List<string> GetValues<T>()
        {
            Type enumType = typeof(T);

            if (!enumType.IsEnum)
            {
                throw new ArgumentException("Type '" + enumType.Name + "' is not an enum");
            }

            List<string> values = new List<string>();

            var fields = from field in enumType.GetFields()
                         where field.IsLiteral
                         select field;

            foreach (FieldInfo field in fields)
            {
                object value = field.GetValue(enumType);
                values.Add(value.ToString());
            }

            values.Sort();

            return values;
        }

        public ObservableCollection<KeyValuePair<string, string>> Ranges
        {
            get
            {
                if (_ranges == null)
                {
                    _ranges = new ObservableCollection<KeyValuePair<string, string>>();

                    _ranges.Add(new KeyValuePair<string, string>("SelectClass", "<Use Selected Item>"));

                    foreach (string str in GetValues<XmlTypeCode>())
                    {
                        _ranges.Add(new KeyValuePair<string, string>("http://www.w3.org/2001/XMLSchema#" + str.ToLower(), str));
                        //_ranges.Add(new KeyValuePair<string, string>(str, str));
                    }
                }
                                                                
                return _ranges;
            }
            set
            {
                _ranges = value;

                RaisePropertyChanged(this, "Ranges"); 
            }
        }

    }
}
