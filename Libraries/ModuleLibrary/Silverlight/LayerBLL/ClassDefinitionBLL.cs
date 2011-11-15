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

namespace org.iringtools.modulelibrary.layerbll
{
    public class ClassDefinitionBLL : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private QMXF _qmxf;
        private ClassDefinition _classDefinition;
        private ObservableCollection<ListBoxItem> _classification = new ObservableCollection<ListBoxItem>();
        private ObservableCollection<ListBoxItem> _specialization = new ObservableCollection<ListBoxItem>();

        public ClassDefinitionBLL(ClassDefinition classDef)
        {
          if (classDef != null)
            {
              _classDefinition = classDef;

                //_classDefinition = qmxf.classDefinitions.FirstOrDefault(c => c.identifier == identifier);
                
                foreach (Specialization s in _classDefinition.specialization) 
                {

                    _specialization.Add(new ListBoxItem() { Content = s.label.Split('@')[0], Tag = s });
                }

                foreach (Classification s in _classDefinition.classification)
                {
                  _classification.Add(new ListBoxItem() { Content = s.label.Split('@')[0], Tag = s });
                }

            }
            else
            {
                _qmxf = new QMXF();
                _classDefinition = new ClassDefinition();
                _qmxf.classDefinitions.Add(_classDefinition);
            }            

        }

        public QMXF QMXF {
            get {

                _classDefinition.specialization.Clear();
                foreach (ListBoxItem lstItm in _specialization)
                {
                    _classDefinition.specialization.Add((Specialization)lstItm.Tag);                    
                }

                _classDefinition.classification.Clear();
                foreach (ListBoxItem lstItm in _classification)
                {                    
                    _classDefinition.classification.Add((Classification)lstItm.Tag);
                }
                if (_qmxf == null)
                  _qmxf = new ids_adi.qmxf.QMXF();
                _qmxf.classDefinitions.Clear();
                _qmxf.classDefinitions.Add(_classDefinition);

                return _qmxf;
            }
        }

        public string Name
        {
            get 
            {
                try
                {
                    return _classDefinition.name.FirstOrDefault().value;
                } catch {
                    return "";
                }
            }
            set
            {
                if (_classDefinition.name.Count == 0)
                {
                  _classDefinition.name.Add(new QMXFName() { lang = null, value = value });
                }
                else
                {
                    _classDefinition.name.FirstOrDefault().value = value;
                }

                if ( PropertyChanged != null ) {
                    PropertyChanged(this, new PropertyChangedEventArgs("Name"));             
                }
            }
        }

        public string EntityType
        {
           
            get {
                try
                {
                    return _classDefinition.entityType.reference;
                }
                catch
                {
                    return "";
                }
            }
          set
          {
            _classDefinition.entityType = new EntityType { reference = value };  
            
            if (PropertyChanged != null)
            {
              PropertyChanged(this, new PropertyChangedEventArgs("EntityType"));
            }
          } 
        }

        public string Description
        {
            get 
            {
                try
                {
                    return _classDefinition.description.FirstOrDefault().value;
                }
                catch
                {
                    return "";
                }
            }
            set
            {

                if (_classDefinition.description.Count == 0)
                {
                    _classDefinition.description.Add(new Description() { contentType = null, parseType = null, lang = null, value = value });
                }
                else
                {
                    _classDefinition.description.FirstOrDefault().value = value;
                }

                if ( PropertyChanged != null ) {
                    PropertyChanged(this, new PropertyChangedEventArgs("Description"));             
                }
            }
        }

        
        
        public string Designation
        {
            get 
            {
                try
                {
                    return _classDefinition.designation.value;
                }
                catch
                {
                    return "";
                }
            }
            set
            {
                if (_classDefinition.designation == null)
                {
                    _classDefinition.designation = new Designation() { value = value };
                }
                else
                {
                    _classDefinition.designation.value = value;
                }
         
                if ( PropertyChanged != null ) {
                    PropertyChanged(this, new PropertyChangedEventArgs("Designation"));             
                }
            }
        }

        public string StatusAuthority
        {
            get
            {
                try
                {
                    return _classDefinition.status.FirstOrDefault().authority;
                }
                catch
                {
                    return "";
                }
            }
        }

        public string StatusClass
        {
            get
            {
                try
                {
                    return _classDefinition.status.FirstOrDefault().Class;
                }
                catch
                {
                    return "";
                }
            }
        }

        public string StatusFrom
        {
            get
            {
                try
                {
                    return _classDefinition.status.FirstOrDefault().from;
                }
                catch
                {
                    return "";
                }
            }
        }

        public string StatusTo
        {
            get
            {
                try
                {
                    return _classDefinition.status.FirstOrDefault().to;
                }
                catch
                {
                    return "";
                }
            }            

        }


        public ObservableCollection<ListBoxItem> Classification
        {
            get
            {
                return _classification;
            }
            set
            {
                _classification = value;

                PropertyChanged(this, new PropertyChangedEventArgs("Classification"));
            }
        }

        public ObservableCollection<ListBoxItem> Specialization
        {
            get 
            { 
                return _specialization; 
            }
            set
            {
                _specialization = value;

                PropertyChanged(this, new PropertyChangedEventArgs("Specialization"));
            }
        }
                
        public string Identifier
        {
            get 
            { 
                return _classDefinition.identifier; 
            }
            set            
            {   
                _classDefinition.identifier = value; 
            
                if ( PropertyChanged != null ) {
                    PropertyChanged(this, new PropertyChangedEventArgs("Identifier"));             
                }
            }
        }
    }
}
