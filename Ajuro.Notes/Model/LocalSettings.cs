using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoDrops.Model
{
	public class LocalSettings
	{
		private string profileName { get; set; }
		public string ProfileName
		{
			get {
				return profileName;
			}
			set
			{
				profileName = value;
			}
		}
	}
}
