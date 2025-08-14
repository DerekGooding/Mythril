using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Mythril.Controller
{
    public class ProcessManager
    {
        private Process? _gameProcess;
        private readonly string _gameExecutablePath;

        public ProcessManager(string gameExecutablePath)
        {
            _gameExecutablePath = gameExecutablePath;
        }

        public void StartGame()
        {
            if (!File.Exists(_gameExecutablePath))
            {
                throw new FileNotFoundException($"Game executable not found at: {_gameExecutablePath}");
            }

            _gameProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = _gameExecutablePath,
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            _gameProcess.Start();
            Console.WriteLine($"Game process started with PID: {_gameProcess.Id}");
        }

        public async Task StopGameAsync()
        {
            if (_gameProcess != null && !_gameProcess.HasExited)
            {
                Console.WriteLine($"Stopping game process with PID: {_gameProcess.Id}");
                _gameProcess.Kill(); // Forcefully kill for now
                await _gameProcess.WaitForExitAsync();
                Console.WriteLine("Game process stopped.");
            }
        }

        public StreamWriter? GetStandardInput()
        {
            return _gameProcess?.StandardInput;
        }

        public StreamReader? GetStandardOutput()
        {
            return _gameProcess?.StandardOutput;
        }
    }
}
