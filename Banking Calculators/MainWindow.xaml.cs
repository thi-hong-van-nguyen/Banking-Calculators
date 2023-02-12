using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Banking_Calculators
{
    public partial class MainWindow : Window
    {
        DataTable dt = new DataTable();

        public MainWindow()
        {
            InitializeComponent();
            Task task = GetCurrencyRatesAsync();
        }

        /*
         * GetCurrencyRatesAsync is used to get the list of currency rate from the API and pass into FROM and TO ComboBox 
         */
        private async Task GetCurrencyRatesAsync()
        {
            using (HttpClient client = new HttpClient())
            {
                //Fetch Currency Rates from API
                HttpResponseMessage response = await client.GetAsync("https://api.exchangerate.host/latest");
                response.EnsureSuccessStatusCode();
                string data = await response.Content.ReadAsStringAsync();
                ExchangeRates exchangeRates = JsonConvert.DeserializeObject<ExchangeRates>(data);  // Use ExchangeRates class to deserialize the JSON response from the API
                
                // Create columns for dataTable
                dt.Columns.Add("ID", typeof(int));
                dt.Columns.Add("Name", typeof(string));
                dt.Columns.Add("Rate", typeof(double));
                dt.PrimaryKey = new DataColumn[] { dt.Columns["ID"] };

                // Create Rows for dataTable
                // First row
                int id = 0;
                DataRow firstRow = dt.NewRow();
                firstRow["ID"] = id;
                firstRow["Name"] = "--Select--";
                firstRow["Rate"] = 0;
                dt.Rows.Add(firstRow);
                id++;

                // Insert the rates stored in exchangeRates object into each row of the DataTable
                foreach (var rate in exchangeRates.Rates)
                {
                    DataRow row = dt.NewRow();
                    row["ID"] = id;
                    row["Name"] = rate.Key;
                    row["Rate"] = rate.Value;
                    dt.Rows.Add(row);
                    id++;
                }

                // Set value, display name and default Index for the ComboBoxes
                fromCurrency.ItemsSource = dt.DefaultView;
                fromCurrency.DisplayMemberPath = "Name";
                fromCurrency.SelectedValuePath = "Rate";
                fromCurrency.SelectedIndex = 0;

                toCurrency.ItemsSource = dt.DefaultView;
                toCurrency.DisplayMemberPath = "Name";
                toCurrency.SelectedValuePath = "Rate";
                toCurrency.SelectedIndex = 0;
            }
        }


        public class ExchangeRates
        {
            public string Base { get; set; }
            public Dictionary<string, float> Rates { get; set; }
        }

        /*
         * Currency class is used to store the currency name and rate. Overide ToString method to return currency name
         */
        public class Currency
        {
            public string Name { get; set; }
            public float Rate { get; set; }

            public override string ToString()
            {
                return Name;
            }
        }

        private void amount_TextChanged(object sender, TextChangedEventArgs e)
        {
            
        }

        private void selectFromCurrency(object sender, SelectionChangedEventArgs e)
        {
            double val = (double)dt.Rows[fromCurrency.SelectedIndex]["Rate"];
            convertedAmount.Content = val;
            debugLabel.Content = fromCurrency.SelectedValue;
        }

        private void selectToCurrency(object sender, SelectionChangedEventArgs e)
        {

        }

        private void ConvertClick(object sender, RoutedEventArgs e)
        {
            debugLabel.Content = fromCurrency.SelectedValue;
        }
    }
}
