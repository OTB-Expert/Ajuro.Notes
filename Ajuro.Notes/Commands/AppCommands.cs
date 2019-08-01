using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Ajuro.Net.Util.Models;
using Ajuro.Net.Util.Resource;

namespace Ajuro.Notes.Commands
	{
		public class AppCommands
		{
			ResourceManager resourceManager = new ResourceManager();

			public AppCommands()
			{
				newTemplateCommand = new SaveContent(ExecuteSave, CanExecute);
			}

			private bool CanExecute(object parameter)
			{
				return true;
			}

			private void ExecuteSave(object parameter)
			{
				if (!(parameter is ResourceDescriptor))
				{
					return;
				}
				var resourceDescriptor = new ResourceDescriptor()
				{
					Solution = Ajuro.Net.Util.Resource.Helper.DefaultSolution,
					Project = Ajuro.Net.Util.Resource.Helper.DefaultProject
				};
				var providers = new List<ResourceProvider>
			{
				new ResourceProvider() {ProviderType = ProviderType.FileSystem},
				new ResourceProvider() {ProviderType = ProviderType.Api}
			};
				List<ResourceProvider> descriptors = Helper.ListTrackedResourceItem<AjuroTemplate>(resourceDescriptor, null, null, providers);

				ObservableCollection<AjuroTemplate> list = new ObservableCollection<AjuroTemplate>();
				foreach (var descriptor in descriptors)
				{
					var result = (List<AjuroTemplate>)descriptor.Items;
					ObservableCollection<AjuroTemplate> rootNode = list;
					foreach (var item in result)
					{
						rootNode = list;
						// Find node
						foreach (var fragment in item.Name.Split(new char[] { '.' }))
						{
							var existentNode = rootNode.Where(p => p.Name.ToLower().Equals(fragment.ToLower())).FirstOrDefault();
							if (existentNode == null)
							{
								existentNode = new AjuroTemplate(0, fragment);
								rootNode.Add(existentNode);
							}
							if (existentNode.Children == null)
							{
								existentNode.Children = new ObservableCollection<AjuroTemplate>();
							}
							rootNode = existentNode.Children;
						}
					}
				}

				//  MainModel = new AppModel() { List = list };
				SelectedItem = (ResourceDescriptor)parameter;
				resourceManager.Save(SelectedItem);
			}

			public ICommand openTemplateCommand { get; set; }
			public ICommand exploreTemplateCommand { get; set; }
			public ICommand newTemplateCommand { get; set; }
			public ICommand saveTemplateCommand { get; set; }
			public ICommand versionTemplateCommand { get; set; }
			public ICommand cloneTemplateCommand { get; set; }
			public ICommand publishTemplateCommand { get; set; }
			public ICommand syncTemplateCommand { get; set; }
			public ICommand unpublishTemplateCommand { get; set; }
			public ICommand deleteTemplateCommand { get; set; }
			public ICommand projectInputSettingsCommand { get; set; }
			public ICommand projectOutputSettingsCommand { get; set; }
			public ICommand databaseConnectionSettingsCommand { get; set; }

			public ICommand projectCommitCommand { get; set; }
			public ICommand projectDeployCommand { get; set; }
			public ICommand projectBuildCommand { get; set; }
			public ICommand projectCompareCommand { get; set; }
			public ICommand projectLunchCommand { get; set; }

			public ICommand templateInputSettingsCommand { get; set; }
			public ICommand templateOutputSettingsCommand { get; set; }
			public ICommand templateDatabaseConnectionSettingsCommand { get; set; }
			public ICommand templateEditorRepeatCommand { get; set; }
			public ICommand templateEditorConditionalCommand { get; set; }
			public ICommand templateEditorReplaceCommand { get; set; }
			public ICommand templateCommitCommand { get; set; }

			public ICommand processorGeneratedCommand { get; set; }
			public ICommand processorChecklistCommand { get; set; }
			public ICommand processorCommitCOmmand { get; set; }
			public ICommand processorApplyCommand { get; set; }
			public ICommand processorHistoryCommand { get; set; }
			public ICommand applicationHelp { get; set; }

			public ICommand viewLayoutGalleryCommand { get; set; }
			public ICommand viewLayoutWorkspaceCommand { get; set; }
			public ICommand viewLayoutEditorCommand { get; set; }
			public ICommand viewProjectsAllCommand { get; set; }
			public ICommand viewProjectsLastCommand { get; set; }
			public ICommand viewTemplatesAllCommand { get; set; }
			public ICommand viewTemplatesLastCommand { get; set; }
			public ICommand viewWorkspacesAllCommand { get; set; }
			public ICommand viewWorkspacesLastCommand { get; set; }
			public ICommand viewSettingsCommand { get; set; }
			public ICommand viewEditorsSettingsCommand { get; set; }

			public ICommand optionsGitSettingsCommand { get; set; }
			public ResourceDescriptor SelectedItem { get; private set; }
		}
	}
