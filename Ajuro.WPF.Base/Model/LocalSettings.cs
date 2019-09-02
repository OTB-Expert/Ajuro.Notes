using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ajuro.WPF.Base.Model
{
	public class LocalSettings
	{
		public LocalSettings()
		{
			LastDocumentNames = new List<string>();
		}

		private string profileName { get; set; }
		public string ProfileName
		{
			get
			{
				return profileName;
			}
			set
			{
				profileName = value;
			}
		}
		private string lastDocumentName { get; set; }
		public string LastDocumentName
		{
			get
			{
				return lastDocumentName;
			}
			set
			{
				lastDocumentName = value;
			}
		}
		private List<string> lastDocumentNames { get; set; }
		public List<string> LastDocumentNames
		{
			get
			{
				return lastDocumentNames;
			}
			set
			{
				lastDocumentNames = value;
			}
		}
		private string lastFilterValue { get; set; }
		public string LastFilterValue
		{
			get
			{
				return lastFilterValue;
			}
			set
			{
				lastFilterValue = value;
			}
		}
	}
}
