using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using System;
using System.Linq;
using System.Reflection;
using org.ids_adi.qmxf;

namespace org.iringtools.modules.templateeditor.rolespopup
{
    public class RolesPopupModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        internal ObservableCollection<KeyValuePair<string, string>> _ranges;
        internal ObservableCollection<KeyValuePair<string, string>> _literalDatatypes;
        internal ObservableCollection<KeyValuePair<string, string>> _restrictionTypes;
        internal ObservableCollection<KeyValuePair<string, object>> _roleRestrictions = new ObservableCollection<KeyValuePair<string,object>>();

        private KeyValuePair<string, string> _RoleRange;
        private string _RoleValueReference;
        private string _RoleValueLiteral;
        private KeyValuePair<string, string> _RoleValueLiteralDatatype;
        
        private bool _HasRange;
        private bool _HasValue;
        private bool _ValueHasReference;
        private bool _ValueHasLiteral;
        private bool _IsBaseTemplate;
        private bool _IsSpecializedTemplate;

        private string _ModelSelectedIMLabel;
        private string _ModelSelectedIMURI;
        
        //private string _SelectedRoleRestrictionType;
        //private string _SelectedRoleRestrictionValue;
        private object _SelectedRoleRestriction;


        public void RaisePropertyChanged(object sender, string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(sender, new PropertyChangedEventArgs(propertyName));
            }
        }

        public string ModelSelectedIMLabel
        {
            get
            {
                return _ModelSelectedIMLabel;
            }
            set
            {
                _ModelSelectedIMLabel = value;
                RaisePropertyChanged(this, "ModelSelectedIMLabel");
            }
        }

        public string ModelSelectedIMURI
        {
            get
            {
                return _ModelSelectedIMURI;
            }
            set
            {
                _ModelSelectedIMURI = value;
                RaisePropertyChanged(this, "ModelSelectedIMURI");
            }
        }
        
        public KeyValuePair<string, string> RoleRange
        {
            get
            {
                return _RoleRange;

            }
            set
            {
                _RoleRange = value;
            }
        }

        public string RoleValueReference
        {
            get
            {
                return _RoleValueReference;
            }
            set
            {
                _RoleValueReference = value;
                RaisePropertyChanged(this, "RoleValueReference");
            }
        }

        public string RoleValueLiteral
        {
            get
            {
                return _RoleValueLiteral;
            }
            set
            {
                _RoleValueLiteral = value;
                RaisePropertyChanged(this, "RoleValueLiteral");
            }
        }

        public KeyValuePair<string, string> RoleValueLiteralDatatype
        {
            get
            {
                return _RoleValueLiteralDatatype;
            }
            set
            {
                _RoleValueLiteralDatatype = value;
                RaisePropertyChanged(this, "RoleValueLiteralDatatype");
            }
        }

        public ObservableCollection<KeyValuePair<string, string>> Ranges
        {
            get
            {
                return _ranges;

            }
            set
            {
                _ranges = value;
            }
        }

        public ObservableCollection<KeyValuePair<string, string>> LiteralDataTypes
        {
            get
            {
                return _literalDatatypes;
            }
            set
            {
                _literalDatatypes = value;
            }
        }

        public bool HasRange
        {
            get
            {
                return _HasRange;
            }
            set
            {
                _HasRange = value;
                RaisePropertyChanged(this, "HasRange");
            }
        }

        public bool HasValue
        {
            get
            {
                return _HasValue;
            }
            set
            {
                _HasValue = value;
                RaisePropertyChanged(this, "HasValue");
            }
        }

        public bool ValueHasReference
        {
            get
            {
                return _ValueHasReference;
            }
            set
            {
                _ValueHasReference = value;
                RaisePropertyChanged(this, "ValueHasReference");
            }
        }

        public bool ValueHasLiteral
        {
            get
            {
                return _ValueHasLiteral;
            }
            set
            {
                _ValueHasLiteral = value;
                RaisePropertyChanged(this, "ValueHasLiteral");
            }
        }

        public bool IsBaseTemplate
        {
            get
            {
                return _IsBaseTemplate;
            }
            set
            {
                _IsBaseTemplate = value;
                RaisePropertyChanged(this, "IsBaseTemplate");
            }
        }

        public bool IsSpecializedTemplate
        {
            get
            {
                return _IsSpecializedTemplate;
            }
            set
            {
                _IsSpecializedTemplate = value;
                RaisePropertyChanged(this, "IsSpecializedTemplate");
            }
        }

        public bool HasRestrictions
        {
            get
            {
                if (RoleRestrictions == null)
                    return false;
                else if (RoleRestrictions.Count > 0)
                    return false;
                else
                    return true;
            }
        }

        public bool IsRestrictionSelected
        {
            get
            {
                return SelectedRoleRestriction != null;
            }            
        }
        
        public virtual ObservableCollection<KeyValuePair<string, object>> RoleRestrictions
        {
            get
            {
                return _roleRestrictions;
            }
            set
            {
                _roleRestrictions = value;

                RaisePropertyChanged(this, "RoleRestrictions");
            }
        }

        public object SelectedRoleRestriction
        {
            get
            {
                return _SelectedRoleRestriction;
            }
            set
            {
                _SelectedRoleRestriction = value;

                RaisePropertyChanged(this, "SelectedRoleRestriction");
                RaisePropertyChanged(this, "IsRestrictionSelected");
                RaisePropertyChanged(this, "SelectedRoleRestrictionType");
                RaisePropertyChanged(this, "SelectedRoleRestrictionValue"); 
            }
        }

        private PropertyRestriction selectedRoleRestriction
        {
            get
            {
                return (PropertyRestriction)_SelectedRoleRestriction;
            }
        }

        public KeyValuePair<string, string> SelectedRoleRestrictionType
        {
            get
            {
            try
                {
                    var items = from query in this.RestrictionTypes
                                where query.Key.ToLower() == selectedRoleRestriction.type
                                select query;

                    if (items.Count() == 0)
                    {
                        KeyValuePair<string, string> restrictiontype = new KeyValuePair<string, string>(selectedRoleRestriction.type, selectedRoleRestriction.type);

                        this.LiteralDataTypes.Add(restrictiontype);
                        return restrictiontype;
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
                if (selectedRoleRestriction != null)
                {
                    selectedRoleRestriction.type = value.Key;

                    RaisePropertyChanged(this, "SelectedRoleRestrictionType");
                }
            }
        }

        public string SelectedRoleRestrictionValue
        {
            get
            {
                try
                {
                    return selectedRoleRestriction.value;
                }
                catch
                {
                    return "";
                }
            }
            set
            {
                if (selectedRoleRestriction != null)
                {
                    selectedRoleRestriction.value = value;

                    RaisePropertyChanged(this, "SelectedRoleRestrictionValue");
                }
            }
        }

        public void AddRestriction(string type, string value)
        {
            PropertyRestriction restriction = new PropertyRestriction()
            {
                type = type,
                value = value
            };
            _roleRestrictions.Add(new KeyValuePair<string, object>(type, restriction));            
        }

        public void RemoveRestriction()
        {
            _roleRestrictions.Remove(new KeyValuePair<string, object>(selectedRoleRestriction.type, SelectedRoleRestriction));            
        }

        public ObservableCollection<KeyValuePair<string, string>> RestrictionTypes
        {
            get
            {
                if (_restrictionTypes == null)
                {
                    _restrictionTypes = new ObservableCollection<KeyValuePair<string, string>>();

                    foreach (string str in GetValues<RestrictionType>())
                    {
                        _restrictionTypes.Add(new KeyValuePair<string, string>(str, str));                        
                    }
                }

                return _restrictionTypes;
            }
            set
            {
                _restrictionTypes = value;

                RaisePropertyChanged(this, "RestrictionTypes");
            }
        }

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

        public enum RestrictionType
        { 
            allValuesFrom,
            someValuesFrom,
            hasValue,
            maxCardinality,
            minCardinality,
            cardinality
        }
        
    }
}