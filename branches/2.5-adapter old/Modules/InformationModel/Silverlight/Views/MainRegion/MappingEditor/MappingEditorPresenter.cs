using OntologyService.Interface;
using PrismContrib.Base;
using System.Windows.Controls;
using OntologyService.Interface.PresentationModels;

namespace MappingEditor.Views.Main
{
  /// <summary>
  /// Information Model View Presenter
  /// </summary>
    public class MappingEditorPresenter :  PresenterBase<IMappingEditorView>
    {/// <summary>
      /// Initializes a new instance of the <see cref="InformationModelPresenter"/> class.
      /// </summary>
      /// <param name="view">The view.</param>
      /// <param name="model">The model.</param>
      public MappingEditorPresenter(MappingEditorView view, IIMPresentationModel model)
            : base(view, model)
        {}
    }
}
