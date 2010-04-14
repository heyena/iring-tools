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

using Library.Interface.Events;

using ModuleLibrary.Events;
using ModuleLibrary.Types;

using Modules.Popup;
using ModuleLibrary.LayerDAL;
using ModuleLibrary.LayerBLL;

using OntologyService.Interface;
using OntologyService.Interface.PresentationModels;

using org.ids_adi.qmxf;

namespace Modelling.ClassDefinition.ClassDefinitionEditor
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
        //private org.ids_adi.qmxf.ClassDefinition _classDefinition = null;
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

                aggregator.GetEvent<ButtonEvent>().Subscribe(showClassEditorHandler);

            }
            catch (Exception ex)
            {
                Error.SetError(ex);
            }
        }

        public void InitializeEditorForAdd()
        {
            EditorMode editorMode = EditorMode.Add;
            InitializeEditor(editorMode, null);
        }

        public void InitializeEditor(EditorMode editorMode, QMXF qmxf)
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

        public void showClassEditorHandler(ButtonEventArgs e)
        {
            if (e.ButtonClicked.Tag.Equals("AddClass1"))
            {

                InitializeEditorForAdd();
                
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
            else if (e.ButtonClicked.Tag.Equals("EditClass1"))
            {

            }
        }

        public void buttonClickHandler(ButtonEventArgs e)
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

                //IRegion region = regionManager.Regions["ClassEditorRegion"];
                //foreach (UserControl userControl in region.Views)
                //{
                //    userControl.Visibility = Visibility.Collapsed;
                //}
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
            ListBox lstCtrl = ListBoxCtrl("specialization");
            _classBLL.Specialization.Remove((ListBoxItem)lstCtrl.SelectedItem);
        }

        void removeClassificationClickHandler(object sender, System.Windows.RoutedEventArgs e)
        {
            ListBox lstCtrl = ListBoxCtrl("classification");
            _classBLL.Classification.Remove((ListBoxItem)lstCtrl.SelectedItem);
        }
    }
}
