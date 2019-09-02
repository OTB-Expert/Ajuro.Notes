using Ajuro.WPF.Base.Model;
using Ajuro.WPF.Desk.Models;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.IO;

namespace Ajuro.WPF.Desk.Views
{
    public class MainView
    {
        // public static string Host = "http://localhost:5000/"; // "http://alfa.drops.jobit.io/
        public static string Host = "http://alfa.drops.jobit.io/";
       
        public Models.MainModel MainModel { get; set; }
        public AjuroTemplate SelectedTemplate = null;

        public MainView()
        {
            ObservableCollection<AjuroTemplate> list = new ObservableCollection<AjuroTemplate>();
            MainModel = new Models.MainModel() { List = list };

            var uri = (Host + "template/list");
            HttpHelper mr = new HttpHelper(uri, "GET");
            string json = "[{\"Name\":\"Tables (SQL Server)\",\"Id\":0},{\"Name\":\"Tables (SQLite)\",\"Id\":1}]";
            try
            {
                json = mr.GetResponse();
            }
            catch (Exception ex)
            {

            }
            MainModel.List = JsonConvert.DeserializeObject<ObservableCollection<AjuroTemplate>>(json);

            uri = ("http://alfa.drops.jobit.io/json/sample");
            mr = new HttpHelper(uri, "GET");
            MainModel.Json = string.Empty;
            MainModel.Message = "Ready!";
            MainModel.Output = "Double click to evaluate!";
            try
            {
                json = mr.GetResponse();
            }
            catch (Exception ex)
            {
                json = File.ReadAllText(@"./Resources/JSON/demo.json");
            }
            MainModel.Json = json;
        }
    }
}