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

namespace Serial_Monitor_App
{
    public partial class MainWindow : Window
    {
        private SerialPort serialPort;

        public MainWindow()
        {
            InitializeComponent();
            LoadAvailablePorts();
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

        private void toggleConnectionBtn_Click(object sender, RoutedEventArgs e)
        {
            if (serialPort == null || !serialPort.IsOpen)
            {
                if (portsComboBox.SelectedItem != null && baudrateComboBox.SelectedItem != null)
                {
                    string selectedPort = portsComboBox.SelectedItem.ToString();
                    int baudRate = int.Parse(baudrateComboBox.SelectedItem.ToString());
                    Console.WriteLine("Port: " + selectedPort);
                    Console.WriteLine(baudRate);

                    // int selectedBaudRate = int.Parse(baudrateComboBox.SelectedItem.ToString());
                //    try
                //    {
                //        serialPort = new SerialPort(selectedPort, 9600);
                //        serialPort.DataReceived += SerialPort_DataReceived;
                //        serialPort.Open();
                //        toggleConnectionBtn.Content = "Disconnect";
                //        portsComboBox.IsEnabled = false;
                //        baudrateComboBox.IsEnabled = false;
                //        scanPortsBtn.IsEnabled = false;
                //    }
                //    catch (Exception ex)
                //    {
                //        MessageBox.Show($"Error al abrir el puerto: {ex.Message}");
                //    }
                }
                else
                {
                    MessageBox.Show("Selecciona un puerto válido.");
                }
            }
            else
            {
                serialPort.Close();
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
                Dispatcher.Invoke(() =>
                {
                    txtMonitor.AppendText("[Device]: " + data + Environment.NewLine);
                    txtMonitor.ScrollToEnd();
                });
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() => MessageBox.Show($"Error de lectura: {ex.Message}"));
            }
        }

        private void sendDataBtn_Click(object sender, RoutedEventArgs e)
        {
            if (serialPort != null && serialPort.IsOpen)
            {
                string command = txtCommand.Text;
                if (!string.IsNullOrWhiteSpace(command))
                {
                    serialPort.WriteLine(command);
                    txtMonitor.AppendText("[You]: " + command + Environment.NewLine);
                    txtMonitor.ScrollToEnd();
                    txtCommand.Clear();
                }
            }
            else
            {
                MessageBox.Show("No hay conexión con el puerto serial.");
            }
        }

        // Para limpiar el texto del textbox al hacer clic en él
        private void txtCommand_GotFocus(object sender, RoutedEventArgs e)
        {
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
                    // serialPort.Dispose();       // Libera los recursos del puerto serie
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
