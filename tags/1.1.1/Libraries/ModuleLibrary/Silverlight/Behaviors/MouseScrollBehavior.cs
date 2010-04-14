using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace ModuleLibrary.Behaviors
{
	/// <summary>
	/// Adds mouse wheel scroll to scrollable controls
	/// </summary>
	/// <remarks>
  /// The expression blend 3 sdk libraries are required for this
  /// and can be downloaded from 
  /// http://www.microsoft.com/downloads/details.aspx?FamilyID=F1AE9A30-4928-411D-970B-E682AB179E17&displaylang=en
	/// <code>
	/// &lt;DataGrid&gt;
	///		&lt;i:Interaction.Behaviors&gt;
	///			<behaviors:MouseScrollBehavior /&gt;
	///		&lt;/i:Interaction.Behaviors&gt;
	/// &lt;/ScrollViewer&gt;
	/// </code>
	/// </remarks>
	public class MouseScrollBehavior : Behavior<Control>
	{
		IScrollProvider scrollProvider;
		
		/// <summary>
		/// Called after the behavior is attached to an AssociatedObject.
		/// </summary>
		/// <remarks>Override this to hook up functionality to the AssociatedObject.</remarks>
		protected override void OnAttached()
		{
			AssociatedObject.MouseWheel += AssociatedObject_MouseWheel;
			base.OnAttached();
		}

		/// <summary>
		/// Called when the behavior is being detached from its AssociatedObject, 
		/// but before it has actually occurred.
		/// </summary>
		/// <remarks>Override this to unhook functionality from the AssociatedObject.</remarks>
		protected override void OnDetaching()
		{
			AssociatedObject.MouseWheel -= AssociatedObject_MouseWheel;
			scrollProvider = null;
			base.OnDetaching();
		}

		private void AssociatedObject_MouseWheel(object sender, MouseWheelEventArgs e)
		{
			if (e.Delta < 0)
			{
 //       if (Scroll(ScrollAmount.LargeIncrement))
				if (Scroll(ScrollAmount.SmallIncrement))
					e.Handled = true;
			}
			else
			{
//        if (Scroll(ScrollAmount.LargeDecrement))
				if (Scroll(ScrollAmount.SmallDecrement))
					e.Handled = true;
			}
		}

		private bool Scroll(ScrollAmount amount)
		{
			if (ScrollProvider != null)
			{
				bool shift = (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift;
				if (shift) //horizontal scroll 
				{
					if (!ScrollProvider.HorizontallyScrollable)
						return false;
					ScrollProvider.Scroll(amount, ScrollAmount.NoAmount);
					return true;
				}
				else
				{
					if (!ScrollProvider.VerticallyScrollable)
						return false;
					ScrollProvider.Scroll(ScrollAmount.NoAmount, amount);
					return true;
				}
			}
			return false;
		}
		
		private IScrollProvider ScrollProvider
		{
			get
			{
				if (scrollProvider == null)
				{
					AutomationPeer automationPeer = 
            FrameworkElementAutomationPeer.FromElement(this.AssociatedObject);
					if (automationPeer == null)
					{
						automationPeer = 
              FrameworkElementAutomationPeer.CreatePeerForElement(this.AssociatedObject);
					}
					scrollProvider = 
            automationPeer.GetPattern(PatternInterface.Scroll) as IScrollProvider;
				}
				return scrollProvider;
			}
		}
	}
}
