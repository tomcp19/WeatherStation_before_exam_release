using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Threading;
using WeatherApp.Commands;
using WeatherApp.Models;

namespace WeatherApp.ViewModels
{
    public class TemperatureViewModel : BaseViewModel
    {
        private TemperatureModel currentTemp;
        private string city;  //ok

        public ITemperatureService TemperatureService { get; private set; }  //ok

        public DelegateCommand<string> GetTempCommand { get; set; }  //ok

        public TemperatureModel CurrentTemp   //ok
        { 
            get => currentTemp;
            set
            {
                currentTemp = value;
                OnPropertyChanged();
                OnPropertyChanged("RawText");
            }
        }

        private ObservableCollection<TemperatureModel> temperatures;

        public ObservableCollection<TemperatureModel> Temperatures
        {
            get { return temperatures; }
            set { 
                temperatures = value;
                OnPropertyChanged();
            }
        }

        public string City  //ok
        {
            get { return city; }
            set
            {
                city = value;

                if (TemperatureService != null)
                {
                    TemperatureService.SetLocation(City);
                }

                OnPropertyChanged();
            }
        }

        private string _rawText; //ok

        public string RawText { //ok
            get {
                return _rawText;
            }
            set
            {
                _rawText = value;
                OnPropertyChanged();
            }
            
        }

        public TemperatureViewModel()
        {
            Name = this.GetType().Name;
            Temperatures = new ObservableCollection<TemperatureModel>();

            GetTempCommand = new DelegateCommand<string>(GetTemp, CanGetTemp);

        }


        public bool CanGetTemp(string obj) //ok
        {
            //return TemperatureService != null;

            if (Properties.Settings.Default.apiKey == "" || TemperatureService == null)
                return false;
            else
                return true;
        }

        public void GetTemp(string obj) //ok
        {
            if (TemperatureService == null) throw new NullReferenceException();

            _ = GetTempAsync();
        }

        private async Task GetTempAsync() //ok
        {
            try
            {
                CurrentTemp = await TemperatureService.GetTempAsync();
                //RawText = $"Time : {CurrentTemp.DateTime.ToLocalTime()} {Environment.NewLine}Temperature : {CurrentTemp.Temperature}";
                if (CurrentTemp != null)
                {
                    temperatures.Add(CurrentTemp);
                    //Save();
                    RawText = CurrentTemp.ToString() + Environment.NewLine + RawText;
                    Debug.WriteLine(CurrentTemp);
                }
            }
            catch (Exception e)
            {
                City = e.Message;
            }
        }

        public double CelsiusInFahrenheit(double c) //ok
        {
            return c * 9.0 / 5.0 + 32;
        }

        public double FahrenheitInCelsius(double f) //ok
        {
            return (f - 32) * 5.0 / 9.0;
        }

        public void SetTemperatureService(ITemperatureService srv) //ok
        {
            TemperatureService = srv;
        }

    }
}
