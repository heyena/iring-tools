using System;
using System.ComponentModel;
using System.Linq;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Xml.Schema;
using System.Reflection;
using org.ids_adi.qmxf;

namespace org.iringtools.modules.templateeditor.editorregion
{
    public class TemplateDefinitionModel : TemplateModel, ITemplateEditorModel
    {
        private TemplateDefinition _templateDefinition;
                        
        public TemplateDefinitionModel(QMXF qmxf, EditorMode editorMode)
        {
            _heading = " Role Definition ";
            _readonly = false;
            _isBaseTemplate = true;
            _isEditMode = (editorMode == EditorMode.Edit);

            if (qmxf != null)
            {
                _qmxf = qmxf;

                if (!string.IsNullOrEmpty(qmxf.sourceRepository))
                  _templateDefinition = qmxf.templateDefinitions.FirstOrDefault<TemplateDefinition>(c=>c.repositoryName == qmxf.sourceRepository);
                else
                  _templateDefinition = qmxf.templateDefinitions.FirstOrDefault<TemplateDefinition>();

                foreach (RoleDefinition s in _templateDefinition.roleDefinition)
                {
                    Roles.Add(new KeyValuePair<string, object>(s.name.FirstOrDefault().value, s));
                }
            }
            else
            {
                _qmxf = new QMXF();
                _templateDefinition = new TemplateDefinition();
                _qmxf.templateDefinitions.Add(_templateDefinition);
            }

        }
                
        public override QMXF QMXF
        {
            get
            {
                //int i = 1;                
                _templateDefinition.roleDefinition.Clear();

                foreach (KeyValuePair<string, object> lstItm in _roles)
                {
                    RoleDefinition role = (RoleDefinition)lstItm.Value;
                   // if (string.IsNullOrEmpty(role.identifier))
                   // role.identifier = (i++).ToString();
                    _templateDefinition.roleDefinition.Add(role);
                }

                _qmxf.templateDefinitions.Clear();
                _qmxf.templateDefinitions.Add(_templateDefinition);

                return _qmxf;
            }
        }

        public override string Name
        {
            get
            {
                try
                {
                    return _templateDefinition.name.FirstOrDefault().value;
                }
                catch
                {
                    return "";
                }
            }
            set
            {
                if (_templateDefinition.name.Count == 0)
                {
                  _templateDefinition.name.Add(new QMXFName() { lang = null, value = value });
                }
                else
                {
                    _templateDefinition.name.FirstOrDefault().value = value;
                }
                               
                RaisePropertyChanged(this, "Name");                
            }
        }

        public override string Qualifies
        {
            get
            {
                return "";                
            }
            set
            {
                return;
            }
        }

        public override string Description
        {
            get
            {
                try
                {
                    return _templateDefinition.description.FirstOrDefault().value;
                }
                catch
                {
                    return "";
                }
            }
            set
            {

                if (_templateDefinition.description.Count == 0)
                {
                    _templateDefinition.description.Add(new Description() { contentType = null, parseType = null, lang = null, value = value });
                }
                else
                {
                    _templateDefinition.description.FirstOrDefault().value = value;
                }

                RaisePropertyChanged(this, "Description");
            }
        }

        public override string StatusAuthority
        {
            get
            {
                try
                {
                    return _templateDefinition.status.FirstOrDefault().authority;
                }
                catch
                {
                    return "";
                }
            }
            /*
            set
            {
                _classDefinition.status.FirstOrDefault().authority = value;             
                if ( PropertyChanged != null ) {
                    PropertyChanged(this, new PropertyChangedEventArgs("StatusAuthority"));             
                }
            }
            */
        }

        public override string StatusClass
        {
            get
            {
                try
                {
                    return _templateDefinition.status.FirstOrDefault().Class;
                }
                catch
                {
                    return "";
                }
            }
            /*
            set
            {
                _classDefinition.status.FirstOrDefault().Class = value;             
                if ( PropertyChanged != null ) {
                    PropertyChanged(this, new PropertyChangedEventArgs("StatusClass"));             
                }
            }
            */
        }

        public override string StatusFrom
        {
            get
            {
                try
                {
                    return _templateDefinition.status.FirstOrDefault().from;
                }
                catch
                {
                    return "";
                }
            }
            /*
            set
            {
                _classDefinition.status.FirstOrDefault().from = value;             
                if ( PropertyChanged != null ) {
                    PropertyChanged(this, new PropertyChangedEventArgs("StatusFrom"));             
                }
            }
            */
        }

        public override string StatusTo
        {
            get
            {
                try
                {
                    return _templateDefinition.status.FirstOrDefault().to;
                }
                catch
                {
                    return "";
                }
            }
            /*
            set
            {
                _classDefinition.status.FirstOrDefault().to = value;             
                if ( PropertyChanged != null ) {
                    PropertyChanged(this, new PropertyChangedEventArgs("StatusTo"));             
                }
            }
            */

        }

        public override string Identifier
        {
            get
            {
                return _templateDefinition.identifier;
            }
            set
            {
                _templateDefinition.identifier = value;
                
                RaisePropertyChanged(this, "Identifier");

            }
        }

        private RoleDefinition selectedRole
        {
            get
            {
                return (RoleDefinition)_selectedRole;
            }
        }

        public override string SelectedRoleIdentifier
        {
            get
            {
                if (_selectedRole != null)
                {
                    return selectedRole.identifier;
                }
                else
                {
                    return "";
                }
            }
            set
            {
                if (_selectedRole != null)
                {
                    selectedRole.identifier = value;

                    RaisePropertyChanged(this, "SelectedRoleIdentifier");
                }
            }
        }

        public override string SelectedRoleDesignation
        {
            get
            {
                if (_selectedRole != null)
                {
                    return selectedRole.designation.value;
                }
                else
                {
                    return "";
                }
            }
            set
            {
                if (_selectedRole != null)
                {
                    selectedRole.designation.value = value;

                    RaisePropertyChanged(this, "SelectedRoleDesignation");
                }
            }
        }

        public override string SelectedRoleName
        {
            get
            {
                if (selectedRole != null && selectedRole.name != null)
                  return selectedRole.name.FirstOrDefault().value;

                return String.Empty;
            }
            set
            {
                if (_selectedRole != null)
                {
                    if (selectedRole.name.Count == 0)
                    {
                      selectedRole.name.Add(new QMXFName() { lang = null, value = value });
                    }
                    else
                    {
                      selectedRole.name.FirstOrDefault().value = value;
                    }

                    RaisePropertyChanged(this, "SelectedRoleName");
                }
            }
        }

        public override string SelectedRoleDescription
        {
            get
            {
              if (selectedRole != null && selectedRole.description != null)
                return selectedRole.description.value;
              
              return String.Empty;
            }
            set
            {
                if (_selectedRole != null)
                {
                    if (selectedRole.description == null)
                    {
                        selectedRole.description = new Description() { contentType = null, parseType = null, lang = null, value = value };
                    }
                    else
                    {
                        selectedRole.description.value = value;
                    }

                    RaisePropertyChanged(this, "SelectedRoleDescription");
                }
            }
        }

        public override KeyValuePair<string, string> SelectedRoleRange
        {
            get
            {
                try
                {
                    var items = from query in this.Ranges
                                where query.Key == selectedRole.range
                                select query;

                    if (items.Count() == 0)
                    {
                        KeyValuePair<string, string> range = new KeyValuePair<string, string>(selectedRole.range, selectedRole.range);

                        this.Ranges.Add(range);
                        return range;
                    }
                    else
                    {
                        return items.FirstOrDefault();
                    }
                }
                catch
                {
                    return new KeyValuePair<string, string>("", "");
                }
            }
            set
            {
                if (_selectedRole != null)
                {
                    selectedRole.range = value.Key;

                    RaisePropertyChanged(this, "SelectedRoleRange");
                }
            }
        }

        public override string SelectedRoleValueReference
        {
            get
            {
                return "";
            }
            set
            {
                return;
            }
        }

        public override string SelectedRoleValueLiteral
        {
            get
            {
                return "";
            }
            set
            {
                return;
            }
        }

        public override KeyValuePair<string, string> SelectedRoleValueLiteralDatatype
        {
            get
            {
                return new KeyValuePair<string, string>("", "");
            }
            set
            {
                return;
            }
        }

        public override ObservableCollection<KeyValuePair<string, object>> SelectedRoleRestrictions
        {
            get
            {
                ObservableCollection<KeyValuePair<string, object>> restrictions = new ObservableCollection<KeyValuePair<string, object>>();
                if (_selectedRole != null)
                {
                    foreach(PropertyRestriction restriction in selectedRole.restrictions)
                    {
                        restrictions.Add(new KeyValuePair<string,object>(restriction.type, restriction));
                    }
                }
                return restrictions;
            }
            set
            {
                //_selectedRoleRestrictions = value;
                foreach (KeyValuePair<string, object> kvpair in value)
                {
                    selectedRole.restrictions.Add(kvpair.Value as PropertyRestriction);
                }

                RaisePropertyChanged(this, "SelectedRoleRestrictions");
            }
        }

        public override void AddRole(string name, string description, string uri)
        {
            RoleDefinition role = new RoleDefinition();
            role.name.Add(new QMXFName() { lang = null, value = name });
            role.description = new Description() { contentType = null, lang = null, parseType = null, value = description };
            role.range = uri;

            _roles.Add(new KeyValuePair<string, object>(name, role));
        }

        public override void ApplyRole(object objRole, string name, string description, string uri)
        {

                KeyValuePair<string, object> _object = (KeyValuePair<string, object>)objRole;
                RoleDefinition role = (RoleDefinition)_object.Value;

                role.name.Clear();
                role.name.Add(new QMXFName() { lang = null, value = name });
                role.description = new Description() { contentType = null, lang = null, parseType = null, value = description };
                role.range = uri;
        }

    }
        
}
