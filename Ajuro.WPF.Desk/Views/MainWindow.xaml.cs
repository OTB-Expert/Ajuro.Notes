using Ajuro.WPF.Base;
using Ajuro.WPF.Base.Model;
using Ajuro.WPF.Desk.Models;
using Ajuro.WPF.Desk.Views;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace Ajuro.WPF.Desk.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
	{
		Ajuro.WPF.Base.Views.MainWindowLogic logic = new WPF.Base.Views.MainWindowLogic();

		// MainView view = new MainView();
        // MainModel mv = null;
        public MainWindow()
        {
            InitializeComponent();
			logic.CustomInitialize();

			ResourceContentTextBox.PreviewKeyUp += ResourceContentTextBox_PreviewKeyUp;
			ResourceContentTextBox.PreviewKeyDown += ResourceContentTextBox_PreviewKeyDown;

			DataContext = Ajuro.WPF.Base.Model.MainModel.Instance;
			Ajuro.WPF.Base.Model.MainModel.Instance.CommandRequestedEvent += Instance_CommandRequestedEvent;

			// Provide me as model
			DataContext = Ajuro.WPF.Base.Model.MainModel.Instance;
		}

		private string Instance_CommandRequestedEvent(KnownCommands str)
		{
			switch(str)
			{
				case KnownCommands.EditTemplate:
					EditTemplate();
					break;
				case KnownCommands.ExecuteTemplate:
					logic.ExecuteQuery(Key.F5, false);
					break;
			}
			return string.Empty;
		}

		private void EditTemplate()
		{
			JObject SampleJson = null;

			   // InitializeComponent();
			var MainViewModel = WizardModel.Instance;
			WizardModel.DataJsonPath = "C:\\OTB\\templates\\profile.json";
			if (!File.Exists(WizardModel.DataJsonPath))
			{
				WizardModel.DataJsonPath = "Resources\\data.json";
			}


			if (File.Exists(WizardModel.DataJsonPath))
			{
				MainViewModel.SampleJson = File.ReadAllText(WizardModel.DataJsonPath);
				SampleJson = JObject.Parse(MainViewModel.SampleJson);
				MainViewModel.SampleTree = UniversalTreeNode.CreateTree(SampleJson);
				MainViewModel.SampleJson = JsonConvert.SerializeObject(SampleJson, Formatting.Indented);
			}

			if (!File.Exists("Resources\\meta2.json"))
			{
				var stream = File.Create("Resources\\meta2.json");
				stream.Close();
				File.WriteAllText("Resources\\meta2.json", "{}");
			}

			if (File.Exists("Resources\\meta2.json"))
			{
				MainViewModel.MetaJson = File.ReadAllText("Resources\\meta2.json");
				var metaJson = JObject.Parse(MainViewModel.MetaJson);
				MainViewModel.MetaJson = JsonConvert.SerializeObject(metaJson, Formatting.Indented);
			}
			var steps = Newtonsoft.Json.JsonConvert.DeserializeObject<WizardStep>(MainViewModel.MetaJson).Children;
			if (steps == null)
			{
				steps = new ObservableCollection<WizardStep>();
			}
			foreach (var step in steps)
			{
				WizardModel.Instance.WizardSteps.Add(step);
			}
			this.AjuroJsonEditor.DataContext = MainViewModel;
		}

		// DataContext = view;
		// mv = view.MainModel;

		/// <summary>
		/// User is editing the note name. From this point the user can either save, creating a new file and deleting the old one or he can duplicate, creating a new file.
		/// </summary>
		/// <param name="sender">Control</param>
		/// <param name="e">Event</param>
		private void Resource_Name_PreviewKeyUp(object sender, KeyEventArgs e)
		{
			logic.Resource_Name_PreviewKeyUp(e.Key);
		}

		private void ResourceContentTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			logic.ResourceContentTextBox_PreviewKeyDown();
		}

		/// <summary>
		/// When user edits the content of the note.
		/// </summary>
		/// <param name="sender">Control</param>
		/// <param name="e">Event</param>
		private void ResourceContentTextBox_PreviewKeyUp(object sender, KeyEventArgs e)
		{
			logic.ResourceContentTextBox_PreviewKeyUp(e.Key);
		}

		/// <summary>
		/// When another note is selected in the list.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void FilesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			logic.FilesList_SelectionChanged();
		}

		private void Image_MouseUp(object sender, MouseButtonEventArgs e)
		{
			MultiFileDocument item = (MultiFileDocument)((Image)sender).Tag;
			logic.Image_MouseUp(item);
		}

		private void FilesList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			logic.FilesList_MouseDoubleClick();
		}

		private void FilterTemplateItems_KeyUp(object sender, KeyEventArgs e)
		{
			logic.FilterTemplateItems_KeyUp(e.Key);
		}

		private void FilterVersionItems_KeyUp(object sender, KeyEventArgs e)
		{
			logic.FilterVersionItems_KeyUp(e.Key);
		}

		private void FilterProjectItems_KeyUp(object sender, KeyEventArgs e)
		{
			logic.FilterProjectItems_KeyUp(e.Key);
		}

		private void LinksReader_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			logic.LinksReader_MouseDoubleClick();
		}

		private void Label_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			logic.Label_MouseDoubleClick();
		}

		private void Label_MouseUp(object sender, MouseButtonEventArgs e)
		{
			logic.Label_MouseUp();
		}

		private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {/*
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            view.SelectedTemplate = (AjuroTemplate)(((ListView)sender).SelectedItem);
            var uri = (MainView.Host +  "template/get/" + view.SelectedTemplate.Id);
            HttpHelper mr = new HttpHelper(uri, "GET");
            string template = mr.GetResponse();
            mv.SelectedTemplate = (JsonConvert.DeserializeObject<List<AjuroTemplate>>(template))[0];
            stopwatch.Stop();
            mv.Message = mv.SelectedTemplate.Name + ". Got in: " + stopwatch.ElapsedMilliseconds + " ms";*/
        }

        private void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {/*
            // AjuroTemplate selectedTemplate = (AjuroTemplate)(((ListView)sender).SelectedItem);
            try
            {
                System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
                stopwatch.Start();
                var uri = (MainView.Host +  "template/process/" + view.SelectedTemplate.Id);
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    string json = mv.Json;

                    streamWriter.Write(json);
                    streamWriter.Flush();
                    streamWriter.Close();
                }

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    mv.Output = result;
                    stopwatch.Stop();
                    mv.Message = "Executed in: " + stopwatch.ElapsedMilliseconds + " ms";
                }
            }
            catch (Exception ex)
            {

            }*/
        }

        private void TemplateResult_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void TemplateText_TextChanged(object sender, TextChangedEventArgs e)
        {/*
            string text = new TextRange(((RichTextBox)sender).Document.ContentStart, ((RichTextBox)sender).Document.ContentEnd).Text;
            if (mv == null)
            {
                return;
            }
            // text = mv.Template;
            view.SelectedTemplate.Template = text;
            
            try
            {
                mv.Message = "Updating...";
                System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
                stopwatch.Start();
                var uri = (MainView.Host +  "template/update/" + view.SelectedTemplate.Id);
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    string json = JsonConvert.SerializeObject(view.SelectedTemplate);

                    streamWriter.Write(json);
                    streamWriter.Flush();
                    streamWriter.Close();
                }

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    mv.Output = result;
                    stopwatch.Stop();
                    mv.Message = "Updated in: " + stopwatch.ElapsedMilliseconds + " ms";
                }
            }
            catch (Exception ex)
            {

            }*/
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {/*
            try
            {
                System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
                stopwatch.Start();
                var uri = (MainView.Host +  "template/save/" + view.SelectedTemplate.Id);
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    string json = JsonConvert.SerializeObject(view.SelectedTemplate);
                    
                    streamWriter.Write(json);
                    streamWriter.Flush();
                    streamWriter.Close();
                }

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    mv.Output = result;
                    stopwatch.Stop();
                    mv.Message = "Executed in: " + stopwatch.ElapsedMilliseconds + " ms";
                }
            }
            catch (Exception ex)
            {

            }*/
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }

        private void Chromium_Drop(object sender, DragEventArgs e)
        {
        }

		private void RemoteContentTextBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{

		}

		private void AffectedFilesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			logic.AffectedFilesList_SelectionChanged();
		}

		private void Browser_OnLoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
		{
			logic.Browser_OnLoadCompleted();
		}
	}
}
