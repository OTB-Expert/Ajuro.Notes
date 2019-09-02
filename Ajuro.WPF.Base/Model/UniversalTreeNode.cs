using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Web.Script.Serialization;
using System.Windows.Controls;

namespace Ajuro.WPF.Base.Model
{
	public class UniversalTreeNode : INotifyPropertyChanged
	{
		private bool isExpanded { get; set; }
		public bool IsExpanded
		{
			get
			{
				return isExpanded;
			}
			set
			{
				isExpanded = value;
				NotifyPropertyChanged();
			}
		}
		private bool isSelected { get; set; }
		public bool IsSelected
		{
			get
			{
				return isSelected;
			}
			set
			{
				isSelected = value;
				NotifyPropertyChanged();
			}
		}
		private string name { get; set; }
		public string Name
		{
			get
			{
				return name;
			}
			set
			{
				name = value;
				NotifyPropertyChanged();
			}
		}
		private string nodeValue { get; set; }
		public string Value
		{
			get
			{
				return nodeValue;
			}
			set
			{
				nodeValue = value;
				NotifyPropertyChanged();
			}
		}
		public List<UniversalTreeNode> Children { get; set; } = new List<UniversalTreeNode>();

		public static UniversalTreeNode CreateTree(object obj)
		{
			JavaScriptSerializer jss = new JavaScriptSerializer();
			var serialized = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
			Dictionary<string, object> dic = jss.Deserialize<Dictionary<string, object>>(serialized);
			var root = new UniversalTreeNode();
			root.Name = "Root";
			BuildTree(dic, root);
			return root;
		}

		internal static void BuildTree(object item, UniversalTreeNode node)
		{
			if (item is KeyValuePair<string, object>)
			{
				KeyValuePair<string, object> kv = (KeyValuePair<string, object>)item;
				UniversalTreeNode keyValueNode = new UniversalTreeNode();
				keyValueNode.Name = kv.Key;
				keyValueNode.Value = GetValueAsString(kv.Value);
				node.Children.Add(keyValueNode);
				BuildTree(kv.Value, keyValueNode);
			}
			else if (item is ArrayList)
			{
				ArrayList list = (ArrayList)item;
				int index = 0;
				foreach (object value in list)
				{
					UniversalTreeNode arrayItem = new UniversalTreeNode();
					arrayItem.Name = $"[{index}]";
					arrayItem.Value = "";
					node.Children.Add(arrayItem);
					BuildTree(value, arrayItem);
					index++;
				}
			}
			else if (item is Dictionary<string, object>)
			{
				Dictionary<string, object> dictionary = (Dictionary<string, object>)item;
				foreach (KeyValuePair<string, object> d in dictionary)
				{
					BuildTree(d, node);
				}
			}
		}

		private static string GetValueAsString(object value)
		{
			if (value == null)
				return "null";
			var type = value.GetType();
			if (type.IsArray)
			{
				return "[]";
			}

			if (value is ArrayList)
			{
				var arr = value as ArrayList;
				return $"[{arr.Count}]";
			}

			if (type.IsGenericType)
			{
				return "{}";
			}

			return value.ToString();
		}

		public event PropertyChangedEventHandler PropertyChanged;
		private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
