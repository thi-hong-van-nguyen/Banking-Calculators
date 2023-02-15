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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static Banking_Calculators.MainWindow;

namespace Banking_Calculators
{
    public partial class MainWindow : Window
    {
        DataTable dt = new DataTable();

        public MainWindow()
        {
            InitializeComponent();
            getCurrentDate();
            Task task = GetCurrencyRatesAsync();
            
        }

        private void getCurrentDate()
        {
            string currentDate = DateTime.Today.ToString("MM-dd-yyyy");
            exchangeRateDateLabel.Content = "Exchange Rate Date: " + currentDate;
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

        /*
         * If the user click Convert, validate inputs
         * Amount: not null or empty String AND must be numeric
         * From: not index 0
         * To: not index 0
         */
        private void ConvertClick(object sender, RoutedEventArgs e)
        {
            // If the amount is empty, throw error
            if (amountToConvert.Text == null || amountToConvert.Text.Trim() == "")
            {
                MessageBox.Show("Please enter an amount", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                amountToConvert.Focus();
                return;
            }

            // If amount is not numeric, throw error
            double i;
            if (!Double.TryParse(amountToConvert.Text, out i))
            {
                MessageBox.Show("Invalid input. Amount must be numeric", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // If currency is not selected (FROM or TO), throw error
            if (fromCurrency.SelectedIndex == 0 || toCurrency.SelectedIndex == 0)
            {
                MessageBox.Show("Please select a currency", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                fromCurrency.Focus();
                return;
            }

            // If all conditions are met, convert the amount
            // Formula to calculate Exchange Rate: TO currency / FROM currency
            // Conversion formula: converted amount = amount to convert * Exchange Rate
            double exchangeRate = (double)dt.Rows[toCurrency.SelectedIndex]["Rate"] / (double)dt.Rows[fromCurrency.SelectedIndex]["Rate"];
            double result = Double.Parse(amountToConvert.Text) * exchangeRate;
            string fromC = dt.Rows[fromCurrency.SelectedIndex]["Name"].ToString();
            string toC = dt.Rows[toCurrency.SelectedIndex]["Name"].ToString();
            convertedAmount.Content = result.ToString("N2") + " " + toC;
            exchangeRateLabel.Content = "1" + fromC + " = " + exchangeRate.ToString("N5") + " " + toC;
        }

        /*
         * When user clicks Cancel:
         * - reset amount and selections of currency
         * - Reset label convertedAmount to "Currency Converter"
         * - Remove label exchageRateLabel
         */
        private void CancelClick(object sender, RoutedEventArgs e)
        {
            amountToConvert.Clear();
            fromCurrency.SelectedIndex = 0; 
            toCurrency.SelectedIndex = 0;
            convertedAmount.Content = "Currency Converter";
            exchangeRateLabel.Content = "";
        }

        /*
         * When user clicks on Refresh button, fetch the rates data from API again
         * Update the date in exchangeRateDateLabel with the current Date
         */
        private void RefreshClick(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            DoubleAnimation da = new DoubleAnimation();
            da.From = 0;
            da.To = 360;
            da.Duration = new Duration(TimeSpan.FromSeconds(1));
            btn.RenderTransform = new RotateTransform();
            btn.RenderTransformOrigin = new Point(0.5, 0.5);
            btn.RenderTransform.BeginAnimation(RotateTransform.AngleProperty, da);

            Task task = GetCurrencyRatesAsync();
            getCurrentDate();
        }
    }
}

//test
