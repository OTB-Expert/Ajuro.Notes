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
	public partial class InputBox : Window
	{
		public event Action<InputBox, string> Answered;

		public InputBox(string messageBoxText, string title = "InputBox", string caption = "Please provide a value:", string yes = "Yes", string no = "No")
		{
			InitializeComponent();
			Title = caption;
			DilogueDescription.Content = messageBoxText;
			YesButton.Content = yes;
			NoButton.Content = no;
		}

		private void CustomInitialization()
		{
		}

		private void Window_Closing(object sender, CancelEventArgs e)
		{
		}

		private void QuestionAnswer_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
		{
			if(e.Key == System.Windows.Input.Key.Enter)
			{
				ReturnAnswer();
			}
		}

		private void ReturnAnswer()
		{
			if (Answered != null)
			{
				Answered(this, QuestionAnswer.Text);
			}
		}

		private void NoButton_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void YesButton_Click(object sender, RoutedEventArgs e)
		{
			ReturnAnswer();
		}

		private void RetryButton_Click(object sender, RoutedEventArgs e)
		{

		}

		private void CancelButton_Click(object sender, RoutedEventArgs e)
		{

		}
	}
}
