﻿using System;
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
    public class TemplateQualificationModel : TemplateModel, ITemplateEditorModel, INotifyPropertyChanged
    {
        private TemplateQualification _templateQualification = null;        
                        
        public TemplateQualificationModel(QMXF qmxf, EditorMode editorMode)
        {
            _heading = " Role Qualification ";
            _readonly = true;
            _isBaseTemplate = false;
            _isEditMode = (editorMode == EditorMode.Edit);
            
            if (qmxf != null)
            {
                _qmxf = qmxf;

                _templateQualification = qmxf.templateQualifications.FirstOrDefault<TemplateQualification>();

                foreach (RoleQualification s in _templateQualification.roleQualification)
                {
                    _roles.Add(new KeyValuePair<string, object>(s.name.FirstOrDefault().value, s));
                }
            }
            else
            {
                _qmxf = new QMXF();
                _templateQualification = new TemplateQualification();
                _qmxf.templateQualifications.Add(_templateQualification);
            }

        }

        public override QMXF QMXF
        {
            get
            {

                _templateQualification.roleQualification.Clear();
                foreach (KeyValuePair<string, object> lstItm in _roles)
                {
                    _templateQualification.roleQualification.Add((RoleQualification)lstItm.Value);
                }

                _qmxf.templateQualifications.Clear();
                _qmxf.templateQualifications.Add(_templateQualification);

                return _qmxf;
            }
        }

        public override string Name
        {
            get
            {
                try
                {
                   return _templateQualification.name.FirstOrDefault().value;
                }
                catch
                {
                    return "";
                }
            }
            set
            {
                if (_templateQualification.name.Count == 0)
                {
                  _templateQualification.name.Add(new QMXFName() { lang = null, value = value });
                }
                else
                {
                   _templateQualification.name.FirstOrDefault().value = value;
                }

                RaisePropertyChanged(this, "Name");
            }
        }

        public override string Qualifies
        {
            get
            {
                try
                {
                    return _templateQualification.qualifies;
                }
                catch
                {
                    return "";
                }
            }
            set
            {
                _templateQualification.qualifies = value;

                RaisePropertyChanged(this, "Qualifies");
            }
        }

        public override string Description
        {
            get
            {
                try
                {
                    return _templateQualification.description.FirstOrDefault().value;
                }
                catch
                {
                    return "";
                }
            }
            set
            {

                if (_templateQualification.description.Count == 0)
                {
                    _templateQualification.description.Add(new Description() { contentType = null, parseType = null, lang = null, value = value });
                }
                else
                {
                    _templateQualification.description.FirstOrDefault().value = value;
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
                    return _templateQualification.status.FirstOrDefault().authority;
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
                    return _templateQualification.status.FirstOrDefault().Class;
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
                    return _templateQualification.status.FirstOrDefault().from;
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
                    return _templateQualification.status.FirstOrDefault().to;
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
                return _templateQualification.identifier;
            }
            set
            {
                _templateQualification.identifier = value;

                RaisePropertyChanged(this, "Identifier");   
            }
        }

        private RoleQualification selectedRole
        {
            get
            {
                return (RoleQualification)_selectedRole;
            }
        }
                
        public override string SelectedRoleIdentifier
        {
            get
            {
                if (_selectedRole != null)
                {
                    return selectedRole.qualifies;
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
                    selectedRole.qualifies = value;

                    RaisePropertyChanged(this, "SelectedRoleIdentifier");
                }
            }
        }

        public override string SelectedRoleDesignation
        {
            get
            {
                //if (_selectedRole != null)
                //{
                //    return selectedRole.designation.value;
                //}
                //else
                //{
                //    return "";
                //}
                return null;
            }
            set
            {
                //if (_selectedRole != null)
                //{
                //    selectedRole.designation.value = value;

                //    RaisePropertyChanged(this, "SelectedRoleDesignation");
                //}
            }
        }

        public override string SelectedRoleName
        {
            get
            {
                try
                {
                    return selectedRole.name.FirstOrDefault().value;
                }
                catch
                {
                    return "";
                }
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
                try
                {
                    return selectedRole.description.FirstOrDefault().value;
                }
                catch
                {
                    return "";
                }
            }
            set
            {
                if (_selectedRole != null)
                {
                    if (selectedRole.description.Count == 0)
                    {
                        selectedRole.description.Add(new Description() { contentType = null, parseType = null, lang = null, value = value });
                    }
                    else
                    {
                        selectedRole.description.FirstOrDefault().value = value;
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

        public override string SelectedRoleValue
        {
            get
            {
                try
                {
                    if (_selectedRole != null)
                    {
                        if (selectedRole.value.reference != string.Empty)
                        {
                            return selectedRole.value.reference;
                        }
                        else
                        {
                            return selectedRole.value.text;
                        }
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
                catch
                {
                    return string.Empty;
                }
            }
            set
            {
                if (_selectedRole != null)
                {
                    selectedRole.value.text = value;

                    RaisePropertyChanged(this, "SelectedRoleValue");
                }
            }
        }

        public override void AddRole(string name, string description, string uri)
        {
            RoleQualification role = new RoleQualification();
            role.name.Add(new QMXFName() { lang = null, value = name });
            role.description.Add(new Description() { contentType = null, lang = null, parseType = null, value = description });
            role.qualifies = uri;

            _roles.Add(new KeyValuePair<string, object>(name, role));
        }

        public override void ApplyRole(object objRole, string name, string description, string uri)
        {
            RoleQualification role = (RoleQualification)objRole;

            role.name.Clear();
            role.name.Add(new QMXFName() { lang = null, value = name });

            role.description.Clear();
            role.description.Add(new Description() { contentType = null, lang = null, parseType = null, value = description });

            role.qualifies = uri;
        }

    }
}
