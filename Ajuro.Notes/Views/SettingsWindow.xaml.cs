using MemoDrops.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MemoDrops.View
{
	/// <summary>
	/// Interaction logic for AccountWindow.xaml
	/// </summary>
	public partial class SettingsWindow : Window
	{
		// Holds the controls of each property
		List<PropertyControl> PropertyControls = new List<PropertyControl>();

		public SettingsWindow()
		{
			InitializeComponent();
			CustomInitialization();
			DataContext = MainModel.Instance;
		}

		private void CustomInitialization()
		{
			if (Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/OTB/MemoDrops/Account"))
			{
				if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/OTB/MemoDrops/Account/settings.json"))
				{
					string mySettings = File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/OTB/MemoDrops/Account/settings.json");
					var profiles = JsonConvert.DeserializeObject<List<SettingsProfile>>(mySettings);
					MainModel.Instance.SettingProfiles = new ObservableCollection<SettingsProfile>();
					foreach (var profile in profiles)
					{
						MainModel.Instance.SettingProfiles.Add(profile);
					}
				}
			}

			if(MainModel.Instance.SettingProfiles == null)
			{
				MainModel.Instance.SettingProfiles = new ObservableCollection<SettingsProfile>();

				MainModel.Instance.SettingProfiles.Add(new SettingsProfile()
				{
					Name = "Pre-Alfa",
					Properties = new List<KeyValue>()
				});

				MainModel.Instance.SettingProfiles.Add(new SettingsProfile()
				{
					Name = "Alfa",
					Properties = new List<KeyValue>()
				});

				MainModel.Instance.SettingProfiles.Add(new SettingsProfile()
				{
					Name = "Beta",
					Properties = new List<KeyValue>()
				});

				MainModel.Instance.SettingProfiles.Add(new SettingsProfile()
				{
					Name = "RC",
					Properties = new List<KeyValue>()
				});

				MainModel.Instance.SettingProfiles.Add(new SettingsProfile()
				{
					Name = "RTM",
					Properties = new List<KeyValue>()
				});

				MainModel.Instance.SettingProfiles.Add(new SettingsProfile()
				{
					Name = "GA",
					Properties = new List<KeyValue>()
				});

				MainModel.Instance.SettingProfiles.Add(new SettingsProfile()
				{
					Name = "Live",
					Properties = new List<KeyValue>()
				});
			}
		}

		private void Window_Closing(object sender, CancelEventArgs e)
		{
			File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/OTB/MemoDrops/Account/settings.json", JsonConvert.SerializeObject(MainModel.Instance.SettingProfiles));
		}

		private void AddProperty_Click(object sender, RoutedEventArgs e)
		{
			CreateControlableProperty(new KeyValue() { Key = "Key", Value = "Value" }, true);
		}

		private void NewItemValueTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			TextBox textBox = (TextBox)sender;
			KeyValue keyValue = (KeyValue)textBox.Tag;
			keyValue.Value = textBox.Text;
		}

		private void NewItemNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			TextBox textBox = (TextBox)sender;
			KeyValue keyValue = (KeyValue)textBox.Tag;
			keyValue.Key = textBox.Text;
		}

		private void NewItemButton_Click(object sender, RoutedEventArgs e)
		{
			PropertyControl propertyControl = (PropertyControl)((Button)sender).Tag;
			foreach(Control control in propertyControl.Controls)
			{
				PropertiesStackPanel.Children.Remove(control);
				MainModel.Instance.SelectedProfile.Properties.Remove(propertyControl.Property);
			}
		}

		private void SaveButton_Click(object sender, RoutedEventArgs e)
		{
			File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/OTB/MemoDrops/Account/settings.json", JsonConvert.SerializeObject(MainModel.Instance.SettingProfiles));
		}

		private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			UpdateControls();
		}

		private void UpdateControls()
		{
			PropertiesStackPanel.Children.Clear();
			foreach (var property in MainModel.Instance.SelectedProfile.Properties)
			{
				CreateControlableProperty(property, false);
			}
		}

		private void CreateControlableProperty(KeyValue property, bool add)
		{
			PropertyControl propertyControl = new PropertyControl();

			if (property != null)
			{
				propertyControl.Property = property;
			}

			StackPanel newItemStackPanel = new StackPanel();
			TextBox newItemNameTextBox = new TextBox();
			newItemNameTextBox.Background = Brushes.OldLace;
			newItemNameTextBox.Text = property.Key;
			newItemNameTextBox.TextChanged += NewItemNameTextBox_TextChanged;
			TextBox newItemValueTextBox = new TextBox();
			newItemValueTextBox.Text = property.Value;
			newItemValueTextBox.TextChanged += NewItemValueTextBox_TextChanged; ;
			Separator newItemSeparator = new Separator();
			Button newItemButton = new Button();
			newItemButton.Content = "Delete Above";
			newItemButton.Tag = propertyControl;
			newItemButton.Click += NewItemButton_Click;

			newItemValueTextBox.Tag = propertyControl.Property;
			newItemNameTextBox.Tag = propertyControl.Property;

			PropertiesStackPanel.Children.Add(newItemSeparator);
			PropertiesStackPanel.Children.Add(newItemNameTextBox);
			PropertiesStackPanel.Children.Add(newItemValueTextBox);
			PropertiesStackPanel.Children.Add(newItemButton);

			propertyControl.Controls.Add(newItemSeparator);
			propertyControl.Controls.Add(newItemNameTextBox);
			propertyControl.Controls.Add(newItemValueTextBox);
			propertyControl.Controls.Add(newItemButton);

			if (add)
			{
				MainModel.Instance.SelectedProfile.Properties.Add(propertyControl.Property);
			}
			PropertyControls.Add(propertyControl);
		}
	}

	public class PropertyControl
	{
		public List<Control> Controls = new List<Control>();
		public KeyValue Property = new KeyValue();
	}
}
