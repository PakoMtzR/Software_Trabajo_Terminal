using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;

namespace tkdScoreboard.Services
{
    public class SerialConnection
    {
        private SerialPort _serialPort;
        public event Action<string> DataReceived;

        public SerialConnection(string portName, int baudRate = 115200)
        {
            _serialPort = new SerialPort(portName, baudRate)
            {
                Parity = Parity.None,
                DataBits = 8,
                StopBits = StopBits.One,
                Handshake = Handshake.None
            };

            _serialPort.DataReceived += OnDataReceived;
        }

        public void Open()
        {
            if (!_serialPort.IsOpen)
            {
                _serialPort.Open();
            }
        }

        public void Close()
        {
            if (_serialPort.IsOpen)
            {
                _serialPort.Close();
            }
        }

        public void SendData(string data)
        {
            if (_serialPort.IsOpen)
            {
                _serialPort.WriteLine(data);
            }
        }

        private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                string data = _serialPort.ReadLine(); // Lee una línea completa
                DataReceived?.Invoke(data); // Lanza el evento con los datos recibidos
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al recibir datos: {ex.Message}");
            }
        }
    }
}
