using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Ajuro.WPF.Wizard.ViewModel
{
	public class PagingViewModel : ViewModelBase
	{
		private ObservableCollection<WizardStep> _data;

		private int start = 0;
		private int itemCount = 10;
		private int totalItems = 0;
		private readonly List<int> count;

		private ICommand _firstCommand;
		private ICommand _previousCommand;
		private ICommand _nextCommand;
		private ICommand _lastCommand;
		private ICommand _countchangedCommand;

		public ObservableCollection<WizardStep> Data
		{
			get { return _data; }
			set
			{
				if (_data != value)
				{
					_data = value;
					OnPropertyChanged("Data");
				}
			}
		}

		public PagingViewModel()
		{
			count = new List<int> { 10, 20, 30 };
			RefreshData();
		}

		public int Start { get { return start + 1; } }

		public int End { get { return start + itemCount < totalItems ? start + itemCount : totalItems; } }

		public int TotalItems { get { return totalItems; } }

		public List<int> Count { get { return count; } }

		public int ItemCount { get { return itemCount; } set { itemCount = value; OnPropertyChanged("ItemCount"); } }

		public ICommand FirstCommand
		{
			get
			{
				if (_firstCommand == null)
				{
					_firstCommand = new RelayCommand
					(
						param =>
						{
							start = 0;
							RefreshData();
						},
						param =>
						{
							return start - itemCount >= 0 ? true : false;
						}
					);
				}

				return _firstCommand;
			}
		}

		public ICommand PreviousCommand
		{
			get
			{
				if (_previousCommand == null)
				{
					_previousCommand = new RelayCommand
					(
						param =>
						{
							start -= itemCount;
							RefreshData();
						},
						param =>
						{
							return start - itemCount >= 0 ? true : false;
						}
					);
				}

				return _previousCommand;
			}
		}

		public ICommand NextCommand
		{
			get
			{
				if (_nextCommand == null)
				{
					_nextCommand = new RelayCommand
					(
						param =>
						{
							start += itemCount;
							RefreshData();
						},
						param =>
						{
							return start + itemCount < totalItems ? true : false;
						}
					);
				}

				return _nextCommand;
			}
		}

		public ICommand LastCommand
		{
			get
			{
				if (_lastCommand == null)
				{
					_lastCommand = new RelayCommand
					(
						param =>
						{
							start = (totalItems / itemCount - 1) * itemCount;
							start += totalItems % itemCount == 0 ? 0 : itemCount;
							RefreshData();
						},
						param =>
						{
							return start + itemCount < totalItems ? true : false;
						}
					);
				}

				return _lastCommand;
			}
		}

		public ICommand CountChangedCommand
		{
			get
			{
				if (_countchangedCommand == null)
				{
					_countchangedCommand = new RelayCommand
					(
						param =>
						{
							start = 0;
							RefreshData();
						},
						param =>
						{
							return ((totalItems - itemCount) > -10) ? true : false;
						}
					);
				}

				return _countchangedCommand;
			}
		}

		public void RefreshData()
		{
			_data = GetData(start, itemCount, out totalItems);
			DataViewModel vm = new DataViewModel(this);

			OnPropertyChanged("Start");
			OnPropertyChanged("End");
			OnPropertyChanged("TotalItems");
		}
	}