using OntologyService.Interface.PresentationModels;
using PrismContrib.Base;
using System.Windows.Controls;
using System.Windows;
using Microsoft.Practices.Composite.Logging;
using System.Windows.Browser;
using Microsoft.Practices.Composite.Events;
using ModuleLibrary.Events;
using ModuleLibrary.Types;
using InformationModel.UserControls;

namespace InformationModel.Views.MenuRegionRight
{
  public class LoginViewPresenter : PresenterBase<ILoginView>
  {
    #region rdLogin 
    private RowDefinition rdLogin
    {
      get { return RowDefinitionCtrl("rdLogin"); }
    }
    
    #endregion
    #region rdLogout 
    private RowDefinition rdLogout
    {
      get { return RowDefinitionCtrl("rdLogout"); }
    }
    
    #endregion

    #region btnLogin 
    private Button btnLogin
    {
      get { return ButtonCtrl("btnLogin"); }
    }
    #endregion

    private ItemsControl itcSpinner { get { return GetControl<ItemsControl>("itcSpinner"); } }

    private IEventAggregator aggregator;

    public LoginViewPresenter(ILoginView view, IIMPresentationModel model,
      IWorkingSpinner spinner,
      IEventAggregator aggregator)
      : base(view, model)
    {
      this.aggregator = aggregator;

      // Hide the logout grid row
      // rdLogout.Height = new GridLength(0);
      itcSpinner.Items.Add(spinner);


      //ButtonCtrl("btnLogout").Click += buttonClickHandler;
      //TextCtrl("txtLogin").TextChanged += new TextChangedEventHandler(TextChangedHandler);
      
      //btnLogin.Click += buttonClickHandler;
      //btnLogin.IsEnabled = false;
    }

    /// <summary>
    /// Only enable the login button if a login name is provided
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.Windows.Controls.TextChangedEventArgs"/> instance containing the event data.</param>
    void TextChangedHandler(object sender, TextChangedEventArgs e)
    {
      btnLogin.IsEnabled = ((TextBox)sender).Text.Length > 0;
    }

    /// <summary>
    /// Button click handler.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
    void buttonClickHandler(object sender, RoutedEventArgs e)
    {
      Button button = sender as Button;


      switch (button.Name)
      {
        case "btnLogin":
          // Get the password and set the lblLogin value
          PasswordBox passResults = PasswordBoxCtrl("txtPassword");
          TextBlockCtrl("lblLoginName").Text = TextCtrl("txtLogin").Text;
          bool isValid = passResults.Password.Equals("iring");

          // Set row definitions based on status
          rdLogin.Height = new GridLength(isValid ? 1 : 20);
          rdLogout.Height = new GridLength(isValid ? 22 : 0);


          if (!isValid)
            aggregator.GetEvent<StatusEvent>().Publish(new StatusEventArgs
            {
              Message = "Password is iring ",
              StatusPanel = StatusType.Middle
            });
          else
            aggregator.GetEvent<StatusEvent>().Publish(new StatusEventArgs
            {
              Message = "logged in as " + TextCtrl("txtLogin").Text,
              StatusPanel = StatusType.Right
            });
          break;

        case "btnLogout":
          rdLogin.Height = new GridLength(20);
          rdLogout.Height = new GridLength(0);
          break;
      }
    }

  }
}
