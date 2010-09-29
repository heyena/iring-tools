using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

using PrismContrib.Base;

using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Composite.Logging;
using Microsoft.Practices.Composite.Regions;
using Microsoft.Practices.Composite.Presentation.Regions.Behaviors;

using org.iringtools.library.presentation.events;

using org.iringtools.modulelibrary.events;
using org.iringtools.modulelibrary.types;
using org.iringtools.modulelibrary.extensions;

using org.iringtools.modules.popup;
using org.iringtools.modulelibrary.layerdal;
using org.iringtools.modulelibrary.layerbll;

using org.iringtools.ontologyservice.presentation;
using org.iringtools.ontologyservice.presentation.presentationmodels;

using org.ids_adi.qmxf;
using System.Collections.Generic;
using org.iringtools.library;

namespace org.iringtools.modelling.classdefinition.classdefinitioneditor
{
    public enum EditorMode
    {
        Add,
        Edit,
    }

    public class ClassDefinitionEditorPresenter : PresenterBase<IClassDefinitionEditorView>
    {
        private IEventAggregator aggregator;
        IReferenceData referenceDataService = null;
        private IRegionManager regionManager;
        private IMPresentationModel model;
        private ClassDefinitionBLL _classBLL = null;
        private EditorMode _editorMode = EditorMode.Add;

        private Button btnOK
        {
            get { return ButtonCtrl("btnOK1"); }
        }

        private Button btnCancel
        {
            get { return ButtonCtrl("btnCancel1"); }
        }

        private Button btnApply
        {
            get { return ButtonCtrl("btnApply1"); }
        }

        private Button addSpecialization
        {
            get { return ButtonCtrl("addSpecialization1"); }
        }

        private Button addClassification
        {
            get { return ButtonCtrl("addClassification1"); }
        }

        private Button removeSpecialization
        {
            get { return ButtonCtrl("removeSpecialization1"); }
        }

        private Button removeClassification
        {
            get { return ButtonCtrl("removeClassification1"); }
        }

        private ComboBox cmbRepositories
        {
            get { return ComboBoxCtrl("cmbRepositories"); }
        }

        public ClassDefinitionEditorPresenter(IClassDefinitionEditorView view, IIMPresentationModel model,
            IRegionManager regionManager,
            IReferenceData referenceDataService,
            IEventAggregator aggregator)
            : base(view, model)
        {
            try
            {
                this.model = (IMPresentationModel)model;
                this.regionManager = regionManager;
                this.aggregator = aggregator;
                this.referenceDataService = referenceDataService;
                referenceDataService.GetRepositories();

                addSpecialization.Click += (object sender, RoutedEventArgs e)
                => { addSpecializationClickHandler(sender, e); };

                addClassification.Click += (object sender, RoutedEventArgs e)
                => { addClassificationClickHandler(sender, e); };

                removeSpecialization.Click += (object sender, RoutedEventArgs e)
                => { removeSpecializationClickHandler(sender, e); };

                removeClassification.Click += (object sender, RoutedEventArgs e)
                => { removeClassificationClickHandler(sender, e); };

                btnOK.Click += (object sender, RoutedEventArgs e) =>
                {
                    buttonClickHandler(new ButtonEventArgs(this, btnOK));
                };

                btnCancel.Click += (object sender, RoutedEventArgs e) =>
                {
                    buttonClickHandler(new ButtonEventArgs(this, btnCancel));
                };

                btnApply.Click += (object sender, RoutedEventArgs e) =>
                {
                    buttonClickHandler(new ButtonEventArgs(this, btnApply));
                };

                cmbRepositories.SelectionChanged += (object sender, SelectionChangedEventArgs e) =>
                {
                    repositorySelectionChanged(sender, e);
                };

                referenceDataService.OnDataArrived += OnDataArrivedHandler;

                aggregator.GetEvent<ButtonEvent>().Subscribe(showClassEditorHandler);

            }
            catch (Exception ex)
            {
                Error.SetError(ex);
            }
        }


        void OnDataArrivedHandler(object sender, System.EventArgs e)
        {
            try
            {
                CompletedEventArgs args = (CompletedEventArgs)e;
                if (args.CheckForType(CompletedEventType.GetRepositories))
                {
                   if (args.Error != null)
                   {
                       MessageBox.Show(args.FriendlyErrorMessage, "Get Repositories Error", MessageBoxButton.OK);
                       return;
                   }
                   object obj = args.Data;
                   foreach (Repository repository in (List<Repository>)obj)
                   {
                      string label = repository.name;
                      if (repository.isReadOnly)
                      {
                        label += " (Read Only)";
                      }
                        
                       ComboBoxItem item = new ComboBoxItem
                       {
                         Content = label,
                           Tag = repository,
                           Height = 20,
                           FontSize =10
                       };

                       cmbRepositories.Items.Add(item);
                   }

                }

                if (args.CheckForType(CompletedEventType.PostClass))
                {
                    if (args.Error != null)
                    {
                        MessageBox.Show(args.FriendlyErrorMessage, "Post Class Error", MessageBoxButton.OK);
                        return;
                    }
                    MessageBox.Show("Class posted successfully", "Post Class", MessageBoxButton.OK);
                    return;
                }
            }
            catch (Exception ex)
            {
                Error.SetError(ex, "Error occurred... \r\n" + ex.Message + ex.StackTrace, 
                    Category.Exception, Priority.High);
            }
        }


        private void repositorySelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ComboBox cb = sender as ComboBox;
                ComboBoxItem cbi = cb.SelectedItem as ComboBoxItem;

                if (this.model.SelectedQMXF == null)
                    this.model.SelectedQMXF = new QMXF();
    
            Repository rep = cbi.Tag as Repository;
                if (rep.isReadOnly == true)
                {
                    btnOK.IsEnabled = false;
                    btnApply.IsEnabled = false;
                }
                else
                {
                    btnOK.IsEnabled = true;
                    btnApply.IsEnabled = true;
                }
                if(_classBLL != null)
                _classBLL.QMXF.targetRepository = rep.name;
            }
            catch (Exception ex)
            {
                Error.SetError(ex);
            }          
            
        }

        public void InitializeEditorForAdd()
        {
            try
            {
                EditorMode editorMode = EditorMode.Add;
                InitializeEditor(editorMode, null);
            }
            catch (Exception ex)
            {
               throw ex;
            }
        }

        public void InitializeEditor(EditorMode editorMode, QMXF qmxf)
        {
            try
            {
                _editorMode = editorMode;
                _classBLL = new ClassDefinitionBLL(qmxf);

                TextCtrl("className").DataContext = _classBLL;
                TextCtrl("entityType").DataContext = _classBLL;
                TextCtrl("description").DataContext = _classBLL;

                TextCtrl("authority").DataContext = _classBLL;
                TextCtrl("recorded").DataContext = _classBLL;
                TextCtrl("dateFrom").DataContext = _classBLL;
                TextCtrl("dateTo").DataContext = _classBLL;

                ListBoxCtrl("specialization").DataContext = _classBLL;
                ListBoxCtrl("classification").DataContext = _classBLL;
            }
            catch (Exception ex)
            {
                Error.SetError(ex);
            }
        }

        public void showClassEditorHandler(ButtonEventArgs e)
        {
            try
            {
                if (e.ButtonClicked.Tag.Equals("AddClass1"))
                {

                    InitializeEditorForAdd();
                    if (cmbRepositories.Items.Count > 0)
                        cmbRepositories.SelectedIndex = 0;

                    IRegion region = regionManager.Regions["ClassEditorRegion"];

                    foreach (UserControl userControl in region.Views)
                    {
                        userControl.Visibility = Visibility.Visible;
                    }

                }
                else if (e.ButtonClicked.Tag.Equals("EditClass1"))
                {
                    InitializeEditor(EditorMode.Edit, model.SelectedQMXF);

                    IRegion region = regionManager.Regions["ClassEditorRegion"];

                    foreach (UserControl userControl in region.Views)
                    {
                        userControl.Visibility = Visibility.Visible;
                    }
                }
            }
            catch (Exception ex)
            {
                
                throw ex;
            }
        }

        public void buttonClickHandler(ButtonEventArgs e)
        {
            try
            {
                if (e.Name.ToString() == "btnOK1")
                {
                    QMXF @qmxf = _classBLL.QMXF;
                    referenceDataService.PostClass(@qmxf);

                    IRegion region = regionManager.Regions["ClassEditorRegion"];
                    foreach (UserControl userControl in region.Views)
                    {
                        userControl.Visibility = Visibility.Collapsed;
                    }
                }
                else if (e.Name.ToString() == "btnCancel1")
                {
                    _classBLL = null;

                    IRegion region = regionManager.Regions["ClassEditorRegion"];
                    foreach (UserControl userControl in region.Views)
                    {
                        userControl.Visibility = Visibility.Collapsed;
                    }
                }
                else if (e.Name.ToString() == "btnApply1")
                {
                    QMXF @qmxf = _classBLL.QMXF;
                    referenceDataService.PostClass(@qmxf);

                    InitializeEditor(EditorMode.Edit, @qmxf);

                }
            }
            catch (Exception ex)
            {
                Error.SetError(ex, "Error occurred while trying to post the class. \r\n" + ex.Message + ex.StackTrace, 
                    Category.Exception, Priority.High);
            }
        }

        void addSpecializationClickHandler(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                foreach (ListBoxItem lb in _classBLL.Specialization)
                {
                    if ((String)lb.Content == model.SelectedIMLabel)
                        throw new Exception("Specialization already exist..");
                }
                _classBLL.Specialization.Add(new ListBoxItem() { Content = model.SelectedIMLabel, Tag = new Specialization() { label = model.SelectedIMLabel, reference = model.SelectedIMUri } });
            }
            catch (Exception ex)
            {
                Error.SetError(ex);
            }
        }

        void addClassificationClickHandler(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                foreach (ListBoxItem lb in _classBLL.Classification)
                {
                    if ((String)lb.Content == model.SelectedIMLabel)
                        throw new Exception("Classification already exist..");
                }
               _classBLL.Classification.Add(new ListBoxItem() { Content = model.SelectedIMLabel, Tag = new Classification() { label = model.SelectedIMLabel, reference = model.SelectedIMUri } } );
            }
            catch (Exception ex)
            {
                Error.SetError(ex);
            }
        }

        void removeSpecializationClickHandler(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                ListBox lstCtrl = ListBoxCtrl("specialization");
                _classBLL.Specialization.Remove((ListBoxItem)lstCtrl.SelectedItem);
            }
            catch (Exception ex)
            {
                Error.SetError(ex);
            }
        }

        void removeClassificationClickHandler(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                ListBox lstCtrl = ListBoxCtrl("classification");
                _classBLL.Classification.Remove((ListBoxItem)lstCtrl.SelectedItem);
            }
            catch (Exception ex)
            {
                Error.SetError(ex);
            }
        }
    }
}
