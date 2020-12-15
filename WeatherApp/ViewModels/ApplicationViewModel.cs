using Newtonsoft.Json;
using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using WeatherApp.Commands;
using WeatherApp.Services;
using WeatherApp.Models;
using System.Windows;
using System.Diagnostics;

namespace WeatherApp.ViewModels
{
    public class ApplicationViewModel : BaseViewModel
    {
        #region Membres

        private BaseViewModel currentViewModel;
        private List<BaseViewModel> viewModels;
        private TemperatureViewModel tvm;
        private OpenWeatherService ows;
        private string filename;
        private string fileContent;

        private VistaSaveFileDialog saveFileDialog;
        private VistaOpenFileDialog openFileDialog;

        #endregion

        #region Propriétés
        /// <summary>
        /// Model actuellement affiché
        /// </summary>
        public BaseViewModel CurrentViewModel
        {
            get { return currentViewModel; }
            set {
                currentViewModel = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// String contenant le nom du fichier
        /// </summary>
        public string Filename
        {
            get
            {
                return filename;
            }
            set
            {
                filename = value;
            }
        }

        public string FileContent
        {
            get { return fileContent; }
            set
            {
                fileContent = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Commande pour changer la page à afficher
        /// </summary>
        public DelegateCommand<string> ChangePageCommand { get; set; }
        public DelegateCommand<string> SaveDataCommand { get; set; }
        public DelegateCommand<string> LoadDataCommand { get; set; }
        public DelegateCommand<string> ChangeLanguageCommand { get; set; }


        public List<BaseViewModel> ViewModels
        {
            get {
                if (viewModels == null)
                    viewModels = new List<BaseViewModel>();
                return viewModels;
            }
        }
        #endregion

        public ApplicationViewModel()
        {
            ChangePageCommand = new DelegateCommand<string>(ChangePage);
            SaveDataCommand = new DelegateCommand<string>(Export, CanExport);
            LoadDataCommand = new DelegateCommand<string>(Import);
            ChangeLanguageCommand = new DelegateCommand<string>(ChangeLanguage);

            initViewModels();

            CurrentViewModel = ViewModels[0];

        }

        #region Méthodes
        void initViewModels()
        {
            /// TemperatureViewModel setup
            tvm = new TemperatureViewModel();
            string apiKey = "";
            
            /*if (!string.IsNullOrEmpty(Properties.Settings.Default.apiKey))
                { 
                    apiKey = Properties.Settings.Default.apiKey;
                }*/

            if (Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") == "DEVELOPMENT")
            {
                apiKey = AppConfiguration.GetValue("OWApiKey");
            }

            if (string.IsNullOrEmpty(Properties.Settings.Default.apiKey) && apiKey == "")
            {
                //tvm.RawText = "Aucune clé API, veuillez la configurer";
                tvm.City = "Aucune clé API, veuillez la configurer";

            }
            else
            {
                if (apiKey == "")
                    apiKey = Properties.Settings.Default.apiKey;

                ows = new OpenWeatherService(apiKey);
            }

            tvm.SetTemperatureService(ows);
            ViewModels.Add(tvm);

            var cvm = new ConfigurationViewModel();
            ViewModels.Add(cvm);
        }



        private void ChangePage(string pageName)
        {
            if (CurrentViewModel is ConfigurationViewModel)
            {
                ows.SetApiKey(Properties.Settings.Default.apiKey);

                var vm = (TemperatureViewModel)ViewModels.FirstOrDefault(x => x.Name == typeof(TemperatureViewModel).Name);
                if (vm.TemperatureService == null)
                    vm.SetTemperatureService(ows);
            }

            CurrentViewModel = ViewModels.FirstOrDefault(x => x.Name == pageName);
        }

        private bool CanExport(string obj)
        {
            //throw new NotImplementedException();

            if (tvm.Temperatures != null) return true;
            else return false;
        }

        /// <summary>
        /// Méthode qui exécute l'exportation
        /// </summary>
        /// <param name="obj"></param>
        private void Export(string obj)
        {

            if (saveFileDialog == null)
            {
                saveFileDialog = new VistaSaveFileDialog();
                saveFileDialog.Filter = "Json file|*.json|All files|*.*";
                saveFileDialog.DefaultExt = "json";
            }

            if (saveFileDialog.ShowDialog() == true)
            {
                filename = saveFileDialog.FileName;
                saveToFile();
            }

        }

        private void saveToFile()
        {
            var resultat = JsonConvert.SerializeObject(tvm.Temperatures, Formatting.Indented);

            using (var tw = new StreamWriter(filename, false))
            {
                tw.WriteLine(resultat);
                tw.Close();
            }

        }

        private void openFromFile()
        {
            using (var sr = new StreamReader(Filename))
            {
                //FileContent = "-- FileContent --" + Environment.NewLine;
                FileContent += sr.ReadToEnd();

                if (FileContent != "")
                {

                    tvm.Temperatures = JsonConvert.DeserializeObject<ObservableCollection<TemperatureModel>>(FileContent);
                    //tvm.RawText = string.Join(Environment.NewLine, tvm.Temperatures);
                    FileContent = "";
                }

            }
        }

        private void Import(string obj)
        {
            if (openFileDialog == null)
            {
                openFileDialog = new VistaOpenFileDialog();
                openFileDialog.Filter = "Json file|*.json|All files|*.*";
                openFileDialog.DefaultExt = "json";
            }

            if (openFileDialog.ShowDialog() == true)
            {
                Filename = openFileDialog.FileName;
                openFromFile();
            }
        }

        private void ChangeLanguage(string language)
        {
            
            if(language != Properties.Settings.Default.Language)
            {
                MessageBoxResult result = MessageBox.Show($"{ Properties.Resources.TxtMsgBog_Lang}", Properties.Resources.TxtBoxMsg_Title, MessageBoxButton.YesNo);
                switch (result)
                {//Properties.Resources.TxtMsgBog_Lang
                    case MessageBoxResult.Yes:
                        Properties.Settings.Default.Language = language;
                        Properties.Settings.Default.Save();
                        restart();
                        break;

                    case MessageBoxResult.No:
                        break;
                }
            }
            else
            {
                MessageBox.Show($"{ Properties.Resources.TxtMsgBog_Already}", Properties.Resources.TxtBoxMsg_Title, MessageBoxButton.OK);
            }
        }

        void restart()
        {
            var filename = Application.ResourceAssembly.Location;
            var newFile = Path.ChangeExtension(filename, ".exe");
            Process.Start(newFile);
            Application.Current.Shutdown();
        }

        #endregion

    }
}
