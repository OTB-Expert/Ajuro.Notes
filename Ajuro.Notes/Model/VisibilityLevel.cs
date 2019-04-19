using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MemoDrops.Model
{
	public enum VisibilityType { OnlyMe = 0, ByInvitation, WithLink, Public }
	public class VisibilityLevel : INotifyPropertyChanged
	{
		public int Key { get; set; }
		public string Name { get; set; }
		public string Image { get; set; }

		public event PropertyChangedEventHandler PropertyChanged;
		private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
