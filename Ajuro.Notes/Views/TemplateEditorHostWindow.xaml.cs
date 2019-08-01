susing System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

namespace MemoDrops.View
{
	public partial class TemplateEditorHostWindow : Window
	{
		public TemplateEditorHostWindow()
		{
			InitializeComponent();
			CustomInitialization();
		}

		private void CustomInitialization()
		{
			
		}
		
		private void Loaded_Loaded(object sender, RoutedEventArgs e)
		{
			// Create the interop host control.
			System.Windows.Forms.Integration.WindowsFormsHost host =
				new System.Windows.Forms.Integration.WindowsFormsHost();

			// Create the MaskedTextBox control.
			MaskedTextBox mtbDate = new MaskedTextBox("00/00/0000");

			// Assign the MaskedTextBox control as the host control's child.
			host.Child = mtbDate;

			// Add the interop host control to the Grid
			// control's collection of child controls.
			this.grid1.Children.Add(host);
		}
	}

	public class PropertyControl
	{
		public List<Control> Controls = new List<Control>();
		public KeyValuePair<string, string> Property = new KeyValuePair<string, string>();
	}
}
