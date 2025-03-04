using System;
using System.Collections.Generic;
using System.Linq;
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
using System.IO.Ports;
using LiveCharts;
using LiveCharts.Wpf;

namespace Serial_Monitor_App
{
    public partial class MainWindow : Window
    {
        private SerialPort serialPort;
        public SeriesCollection ChartSeries { get; set; }
        public string[] baudrate_options { get; set; } 
        public MainWindow()
        {
            InitializeComponent();
            LoadAvailablePorts();

            baudrate_options = new string[] { "300", "600", "750", "1200", "2400", "4800", "9600", "19200", "31250", "38400", "57600", "74880", "115200" };
            baudrateComboBox.SelectedIndex = 6;

            ChartSeries = new SeriesCollection();
            chartControl.Series = ChartSeries;
            DataContext = this;
        }

        private void LoadAvailablePorts()
        {
            // Funcion para escanear los puerto disponibles y agregarlo al combobox
            // Obtenemos la lista de puertos disponibles
            string[] availablePorts = SerialPort.GetPortNames();

            // Limpia y actualiza la lista del combobox
            portsComboBox.Items.Clear();
            if (availablePorts.Length > 0)
            {
                foreach (string port in availablePorts)
                {
                    portsComboBox.Items.Add(port);
                }
                portsComboBox.SelectedIndex = 0;
            }
        }

        private void scanPortsBtn_Click(object sender, RoutedEventArgs e)
        {
            LoadAvailablePorts();
        }

        private async void toggleConnectionBtn_Click(object sender, RoutedEventArgs e)
        {
            if (serialPort == null || !serialPort.IsOpen)
            {
                if (portsComboBox.SelectedItem != null && baudrateComboBox.SelectedItem != null)
                {
                    string selectedPort = portsComboBox.SelectedItem.ToString();
                    int selectedBaudRate = int.Parse(baudrateComboBox.SelectedItem.ToString());
                    try
                    {
                        serialPort = new SerialPort(selectedPort, selectedBaudRate);
                        serialPort.DataReceived += SerialPort_DataReceived;

                        //serialPort.Open();
                        await Task.Run(() => serialPort.Open()); // Abrir en segundo plano
                        
                        // Cambios en la interfaz
                        toggleConnectionBtn.Content = "Disconnect";
                        portsComboBox.IsEnabled = false;
                        baudrateComboBox.IsEnabled = false;
                        scanPortsBtn.IsEnabled = false;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error al abrir el puerto: {ex.Message}");
                    }
                }
                else MessageBox.Show("Selecciona un puerto válido.");
            }
            else
            {
                await Task.Run(() => serialPort.Close());

                // Cambios en la interfaz
                toggleConnectionBtn.Content = "Connect";
                portsComboBox.IsEnabled = true;
                baudrateComboBox.IsEnabled = true;
                scanPortsBtn.IsEnabled = true;
            }
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                string data = serialPort.ReadLine();
                string[] values = data.Split(',');

                Dispatcher.Invoke(() =>
                {
                    // Imprimir en el monitor serial
                    txtMonitor.AppendText("[Device]: " + data);
                    txtMonitor.ScrollToEnd();

                    // Graficacion de los valores
                    int count = values.Length;

                    // Verificar cuántas series hay y ajustar dinámicamente
                    while (ChartSeries.Count < count)
                    {
                        ChartSeries.Add(new LineSeries
                        {
                            Title = "Valor " + (ChartSeries.Count + 1),
                            Values = new ChartValues<double>()
                        });
                    }

                    while (ChartSeries.Count > count)
                    {
                        ChartSeries.RemoveAt(ChartSeries.Count - 1);
                    }

                    // Agregar los valores recibidos a las series correspondientes
                    for (int i = 0; i < count; i++)
                    {
                        if (double.TryParse(values[i], out double parsedValue))
                        {
                            ChartSeries[i].Values.Add(parsedValue);

                            // Limitar datos para evitar saturación
                            if (ChartSeries[i].Values.Count > 6)
                            {
                                ChartSeries[i].Values.RemoveAt(0);
                            }
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    txtMonitor.AppendText($"[Error]: {ex.Message}" + Environment.NewLine);
                    txtMonitor.ScrollToEnd();
                });
            }
        }

        private void sendDataBtn_Click(object sender, RoutedEventArgs e)
        {
            if (serialPort != null && serialPort.IsOpen)
            {
                string command = txtCommand.Text.Trim();    // Elimina espacios en blanco
                if (!string.IsNullOrWhiteSpace(command))
                {
                    try
                    {
                        serialPort.WriteLine(command);
                        txtMonitor.AppendText("[You]: " + command + Environment.NewLine);
                        txtMonitor.ScrollToEnd();
                        txtCommand.Clear();
                    }
                    catch (Exception ex)
                    {
                        txtMonitor.AppendText($"[Error]: {ex.Message}" + Environment.NewLine);
                        txtMonitor.ScrollToEnd();
                    }
                }
                else MessageBox.Show("El mensaje no puede estar vacío");
            }
            else MessageBox.Show("No hay conexión con el puerto serial.");
        }

        private void txtCommand_GotFocus(object sender, RoutedEventArgs e)
        {
            // Para limpiar el texto del textbox al hacer clic en él
            txtCommand.Clear(); 
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            try
            {
                if (serialPort != null)
                {
                    if (serialPort.IsOpen)
                    {
                        serialPort.Close();     // Cierra el puerto serie si está abierto
                    }
                    serialPort.Dispose();       // Libera los recursos del puerto serie
                }
            } 
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cerrar el puerto: {ex.Message}");
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
            Window_Closed(this, null); // Llama al evento Window_Closed manualmente
        }
    }
}
