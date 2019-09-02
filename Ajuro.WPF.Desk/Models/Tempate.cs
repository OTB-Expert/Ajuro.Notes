using Ajuro.WPF.Base.Model;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Ajuro.WPF.Desk.Models 
{
    [Serializable]
    public class MainModel : INotifyPropertyChanged
    {
        public string json { get; set; }
        public string output { get; set; }
        public AjuroTemplate selectedTemplate { get; set; }
        public string message { get; set; }
        public string Json
        {
            get
            {
                return json;
            }
            set
            {
                json = value;
                this.OnPropertyChanged("Json");
            }
        }

        public AjuroTemplate SelectedTemplate
        {
            get
            {
                return selectedTemplate;
            }
            set
            {
                selectedTemplate = value;
                this.OnPropertyChanged("SelectedTemplate");
            }
        }

        public string Output
        {
            get
            {
                return output;
            }
            set
            {
                output = value;
                this.OnPropertyChanged("Output");
            }
        }

        public string Message
        {
            get
            {
                return message;
            }
            set
            {
                message = value;
                this.OnPropertyChanged("Message");
            }
        }

        public ObservableCollection<AjuroTemplate> List { get; set; } = new ObservableCollection<AjuroTemplate>();

        public event PropertyChangedEventHandler PropertyChanged;  

        /// <summary>
        /// Notifies objects registered to receive this event that a property value has changed.
        /// </summary>
        /// <param name="propertyName">The name of the property that was changed.</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}