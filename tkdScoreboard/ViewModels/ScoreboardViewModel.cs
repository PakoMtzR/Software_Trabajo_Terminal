using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using tkdScoreboard.Commands;
using tkdScoreboard.Models;
using tkdScoreboard.Services;
using tkdScoreboard.Services.Interfaces;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;
using System.Threading;
using System.Windows.Forms;

namespace tkdScoreboard.ViewModels
{
    public class ScoreboardViewModel : BaseViewModel
    {
        public Match CurrentMatch { get; }
        private SerialConnection _serialConnection;
        public string ReceivedData { get; set; }
        private readonly IDialogService _dialogService;

        // Commandos para abrir ventanas flotantes
        public ICommand OpenSettingsCommand { get; }
        public ICommand OpenEditScoreboardCommand { get; }

        // Propiedades expuestas para la vista
        public string TimerDisplay => CurrentMatch.TimerDisplay;
        public string RoundDisplay => CurrentMatch.RoundDisplay;

        // Comandos del timer
        public ICommand ResumeRoundCommand { get; }
        public ICommand PauseRoundCommand { get; }
        public ICommand NextRoundCommand { get; }
        public ICommand ResetMatchCommand { get; }
        public ICommand FinishRoundCommand { get; }


        // Comandos atajos para el jugador 1 (Azul)
        // ----------------------------------------------------------
        // Comandos para agregar puntos al jugador 1
        public ICommand Add1PointToPlayer1Command { get; }
        public ICommand Add2PointsToPlayer1Command { get; }
        public ICommand Add3PointsToPlayer1Command { get; }
        public ICommand Add4PointsToPlayer1Command { get; }
        public ICommand Add5PointsToPlayer1Command { get; }
        // Comandos para quitar puntos al jugador 1
        public ICommand Remove1PointToPlayer1Command { get; }
        public ICommand Remove2PointsToPlayer1Command { get; }
        public ICommand Remove3PointsToPlayer1Command { get; }
        public ICommand Remove4PointsToPlayer1Command { get; }
        public ICommand Remove5PointsToPlayer1Command { get; }
        // Comandos para agregar/quitar penalizaciones al jugador 1
        public ICommand AddPenaltyToPlayer1Command { get; }
        public ICommand RemovePenaltyToPlayer1Command { get; }


        // Comandos atajos para el jugador 2 (Rojo)
        // ----------------------------------------------------------
        public ICommand Add1PointToPlayer2Command { get; }
        public ICommand Add2PointsToPlayer2Command { get; }
        public ICommand Add3PointsToPlayer2Command { get; }
        public ICommand Add4PointsToPlayer2Command { get; }
        public ICommand Add5PointsToPlayer2Command { get; }
        // Comandos para quitar puntos al jugador 2
        public ICommand Remove1PointToPlayer2Command { get; }
        public ICommand Remove2PointsToPlayer2Command { get; }
        public ICommand Remove3PointsToPlayer2Command { get; }
        public ICommand Remove4PointsToPlayer2Command { get; }
        public ICommand Remove5PointsToPlayer2Command { get; }
        // Comandos para agregar/quitar penalizaciones al jugador 2
        public ICommand AddPenaltyToPlayer2Command { get; }
        public ICommand RemovePenaltyToPlayer2Command { get; }

        // Comandos para la conexión serial
        public ICommand ConnectSerialCommand { get; }
        public ICommand DisconnectSerialCommand { get; }

        public ICommand RunPythonScriptCommand { get; }

        // Constructor
        public ScoreboardViewModel(IDialogService dialogService)
        {
            // Inicializamos el modelo
            CurrentMatch = new Match();
            _dialogService = dialogService;
            _serialConnection = new SerialConnection("COM3", 115200);
            _serialConnection.DataReceived += OnDataReceived;

            RunPythonScriptCommand = new RelayCommand(RunPythonScript);

            // Configuramos los comandos para abrir ventanas flotantes
            OpenSettingsCommand = new RelayCommand(OpenSettings);
            OpenEditScoreboardCommand = new RelayCommand(OpenEditScoreboard);

            // Configuramos los comandos del timer
            ResumeRoundCommand = new RelayCommand(CurrentMatch.ResumeRound, CanResumeRound);
            PauseRoundCommand = new RelayCommand(CurrentMatch.PauseRound, CanPauseRound);
            NextRoundCommand = new RelayCommand(CurrentMatch.NextRound, CanNextRound);
            ResetMatchCommand = new RelayCommand(CurrentMatch.ResetMatch);
            FinishRoundCommand = new RelayCommand(CurrentMatch.FinishRound);

            // Comandos para la conexión serial
            ConnectSerialCommand = new RelayCommand(_serialConnection.Open, () => !_serialConnection.IsOpen);
            DisconnectSerialCommand = new RelayCommand(_serialConnection.Close, () => _serialConnection.IsOpen);

            // Comandos para el jugador 1
            Add1PointToPlayer1Command = new RelayCommand(() => CurrentMatch.Player1.AddPoints(1));
            Add2PointsToPlayer1Command = new RelayCommand(() => CurrentMatch.Player1.AddPoints(2));
            Add3PointsToPlayer1Command = new RelayCommand(() => CurrentMatch.Player1.AddPoints(3));
            Add4PointsToPlayer1Command = new RelayCommand(() => CurrentMatch.Player1.AddPoints(4));
            Add5PointsToPlayer1Command = new RelayCommand(() => CurrentMatch.Player1.AddPoints(5));
            Remove1PointToPlayer1Command = new RelayCommand(() => CurrentMatch.Player1.RemovePoints(1));
            Remove2PointsToPlayer1Command = new RelayCommand(() => CurrentMatch.Player1.RemovePoints(2));
            Remove3PointsToPlayer1Command = new RelayCommand(() => CurrentMatch.Player1.RemovePoints(3));
            Remove4PointsToPlayer1Command = new RelayCommand(() => CurrentMatch.Player1.RemovePoints(4));
            Remove5PointsToPlayer1Command = new RelayCommand(() => CurrentMatch.Player1.RemovePoints(5));
            AddPenaltyToPlayer1Command = new RelayCommand(() => CurrentMatch.PenalizePlayer1());
            RemovePenaltyToPlayer1Command = new RelayCommand(() => CurrentMatch.DespenalizePlayer1());

            // Comandos para el jugador 2
            Add1PointToPlayer2Command = new RelayCommand(() => CurrentMatch.Player2.AddPoints(1));
            Add2PointsToPlayer2Command = new RelayCommand(() => CurrentMatch.Player2.AddPoints(2));
            Add3PointsToPlayer2Command = new RelayCommand(() => CurrentMatch.Player2.AddPoints(3));
            Add4PointsToPlayer2Command = new RelayCommand(() => CurrentMatch.Player2.AddPoints(4));
            Add5PointsToPlayer2Command = new RelayCommand(() => CurrentMatch.Player2.AddPoints(5));
            Remove1PointToPlayer2Command = new RelayCommand(() => CurrentMatch.Player2.RemovePoints(1));
            Remove2PointsToPlayer2Command = new RelayCommand(() => CurrentMatch.Player2.RemovePoints(2));
            Remove3PointsToPlayer2Command = new RelayCommand(() => CurrentMatch.Player2.RemovePoints(3));
            Remove4PointsToPlayer2Command = new RelayCommand(() => CurrentMatch.Player2.RemovePoints(4));
            Remove5PointsToPlayer2Command = new RelayCommand(() => CurrentMatch.Player2.RemovePoints(5));
            AddPenaltyToPlayer2Command = new RelayCommand(() => CurrentMatch.PenalizePlayer2());
            RemovePenaltyToPlayer2Command = new RelayCommand(() => CurrentMatch.DespenalizePlayer2());


            CurrentMatch.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(CurrentMatch.TimerDisplay))
                    OnPropertyChanged(nameof(TimerDisplay));
                if (e.PropertyName == nameof(CurrentMatch.RoundDisplay))
                    OnPropertyChanged(nameof(RoundDisplay));
            };
        }

        private void OpenSettings()
        {
            var settingsViewModel = new SettingsViewModel(
            CurrentMatch.Player1.Name,
            CurrentMatch.Player2.Name,
            CurrentMatch.RoundTime,
            CurrentMatch.RestTime,
            CurrentMatch.PenaltiesLimit,
            null); // El callback lo pone DialogService

            if (_dialogService.ShowSettingsDialog(settingsViewModel))
            {
                // Aquí los valores ya están actualizados por DialogService
                CurrentMatch.Player1.Name = settingsViewModel.Player1Name;
                CurrentMatch.Player2.Name = settingsViewModel.Player2Name;
                CurrentMatch.RoundTime = settingsViewModel.RoundTime;
                CurrentMatch.RestTime = settingsViewModel.RestTime;
                CurrentMatch.PenaltiesLimit = settingsViewModel.PenaltyLimit;

                CurrentMatch._timer.RemainingTime = CurrentMatch.RoundTime;
                OnPropertyChanged(nameof(CurrentMatch.TimerDisplay));
            }
        }

        private void OpenEditScoreboard()
        {
            var editScoreboardViewModel = new EditScoreboardViewModel(
            CurrentMatch.Player1.Points,
            CurrentMatch.Player1.Penalties,
            CurrentMatch.Player2.Points,
            CurrentMatch.Player2.Penalties,
            CurrentMatch._timer.RemainingTime,
            null); // El callback lo pone DialogService

            if (_dialogService.ShowEditScoreboardDialog(editScoreboardViewModel))
            {
                // Aquí los valores ya están actualizados por DialogService
                CurrentMatch.Player1.Points = editScoreboardViewModel.Player1Points;
                CurrentMatch.Player1.Penalties = editScoreboardViewModel.Player1Penalties;
                CurrentMatch.Player2.Points = editScoreboardViewModel.Player2Points;
                CurrentMatch.Player2.Penalties = editScoreboardViewModel.Player2Penalties;
                CurrentMatch._timer.RemainingTime = editScoreboardViewModel.CurrentTime;

                OnPropertyChanged(nameof(CurrentMatch.TimerDisplay));
            }
        }

        private void OnDataReceived(string data)
        {
            if (CurrentMatch.MatchState != Match.MatchStateEnum.Combate) 
                return;

            // Actualiza la propiedad con los datos recibidos
            ReceivedData = data;
            if (int.TryParse(data.Trim(), out int dataValue)) 
            { 
                if (dataValue <= 5) CurrentMatch.Player1.AddPoints(dataValue);
                else CurrentMatch.Player2.AddPoints(dataValue - 5); // Asumiendo que los valores mayores a 5 son para el jugador 2
            }
            else Console.WriteLine("Error al procesar los datos recibidos.");
            OnPropertyChanged(nameof(ReceivedData));
        }

        public void RunPythonScript()
        {
            // Configuración de parámetros
            string condaPath = @"D:\miniConda\Scripts\conda.exe";
            string environmentName = "yolo_env";
            string pythonScriptPath = @"D:\UPIIH\10_Semestre\TrabajoTerminal_II\Programacion\csharp_projects\Solution_tt\tkdScoreboard\Scripts\taekwondoKickCounter.py";
            string resultsJsonPath = @"D:\UPIIH\10_Semestre\TrabajoTerminal_II\Programacion\csharp_projects\Solution_tt\tkdScoreboard\Scripts\taekwondo_results.json";

            try
            {
                // 1. Mostrar mensaje de inicio
                MessageBox.Show("El sistema de conteo de patadas se está iniciando. Por favor espere...",
                              "Procesando", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // 2. Eliminar archivo de resultados previo si existe
                if (File.Exists(resultsJsonPath))
                    File.Delete(resultsJsonPath);

                // 3. Configurar proceso Python
                ProcessStartInfo start = new ProcessStartInfo
                {
                    FileName = condaPath,
                    Arguments = $"run -n {environmentName} python \"{pythonScriptPath}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true, // Ocultar ventana de consola
                    WorkingDirectory = Path.GetDirectoryName(pythonScriptPath)
                };

                // 4. Ejecutar y esperar bloqueando la UI
                using (Process process = new Process { StartInfo = start })
                {
                    // Configurar eventos para capturar salida
                    StringBuilder output = new StringBuilder();
                    StringBuilder error = new StringBuilder();

                    process.OutputDataReceived += (sender, e) => {
                        if (!string.IsNullOrEmpty(e.Data)) output.AppendLine(e.Data);
                    };
                    process.ErrorDataReceived += (sender, e) => {
                        if (!string.IsNullOrEmpty(e.Data)) error.AppendLine(e.Data);
                    };

                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    // Esperar bloqueando hasta que termine
                    process.WaitForExit();

                    // Mostrar resultados de la ejecución
                    if (output.Length > 0)
                        Console.WriteLine("Salida Python:\n" + output.ToString());
                    if (error.Length > 0)
                        Console.WriteLine("Errores Python:\n" + error.ToString());
                }

                // 5. Esperar y verificar archivo JSON
                int attempts = 0;
                while (!File.Exists(resultsJsonPath) && attempts < 10)
                {
                    attempts++;
                    Thread.Sleep(500);
                }

                if (!File.Exists(resultsJsonPath))
                    throw new FileNotFoundException("No se generó el archivo de resultados");

                // 6. Procesar JSON
                string jsonString = File.ReadAllText(resultsJsonPath);
                dynamic jsonData = JsonConvert.DeserializeObject(jsonString);

                foreach (var player in jsonData.players)
                {
                    int kicks = Convert.ToInt32(player.kick_count);
                    if (player.color == "blue")
                        CurrentMatch.Player1.AddPoints(kicks);
                    else if (player.color == "red")
                        CurrentMatch.Player2.AddPoints(kicks);
                }

                // 7. Notificar finalización
                MessageBox.Show("Conteo de patadas completado!",
                               "Proceso terminado", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}",
                               "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
        }

        private bool CanResumeRound()
        {
            return CurrentMatch.MatchState == Match.MatchStateEnum.Pausa;
        }
        private bool CanPauseRound()
        {
            return CurrentMatch.MatchState == Match.MatchStateEnum.Combate;
        }
        private bool CanNextRound()
        {
            return CurrentMatch.MatchState == Match.MatchStateEnum.Descanso;
        }

        ~ScoreboardViewModel()
        {
            _serialConnection.Close();
        }
    }
}
