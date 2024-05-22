using System.Diagnostics;

namespace RetCodeTester
{
    /// <summary>
    /// Класс одной комманды (строки в файле) для выполнения тестирования
    /// Сначала должен быть создан объект, после чего могут быть переданы AcceptedCodes и Retries.
    /// И после этого запущен метод Execute() который вернёт класс с информацией о выполнении команды
    /// </summary>
    public class Command
    {
        /// <summary>
        /// Ошибки выполнения Execution()
        /// </summary>
        public enum ExecutionError { NoError, Timeout, WrongCode }

        public class ExecutionResult
        {
            /// <summary>
            /// Что послужило завершению выполнения метода Execute().
            /// </summary>
            public ExecutionError Error;

            /// <summary>
            /// Удволитворительный код
            /// </summary>
            public int ExitCode;

            /// <summary>
            /// Сколько было выполнено перезапусков.
            /// </summary>
            public int RetriesCount;

            /// <summary>
            /// Список кодов полученных в неудачных перезапусках 
            /// до окончания цикла или получения удволитворительного кода
            /// </summary>
            public List<int> WrongCodes = new List<int>();
        }

        /// <summary>
        /// Командная строка для проверки 
        /// </summary>
        public string CommandText { get; }

        /// <summary>
        /// Список кодов которые будут приняты как удволитворительный результат (по дефолту [0])
        /// </summary>
        public int[] AcceptedCodes { get; set; } = [0];

        /// <summary>
        /// Количество повторов перед выкидыванием ошибки (по дефолту 0)
        /// </summary>
        public int Retries { get; set; } = 0;

        public Command(string command)
        {
            this.CommandText = command;
        }

        /// <summary>
        /// Выполнить команду
        /// </summary>
        /// <returns>Возвращает класс с данными о выполнении команды</returns>
        public ExecutionResult Execute()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = string.Format("/c \"{0}\"", this.CommandText);

            ExecutionResult result = new ExecutionResult();
            int initialRetries = this.Retries + 1; // Каждому нужно дать один шанс, поэтому +1
            int attemptsRemain = initialRetries;
            bool isCodeAccepted = false;
            while (attemptsRemain > 0) {
                attemptsRemain--;

                Process process = new Process();
                process.StartInfo = startInfo;
                process.Start();

                int timeoutMilliseconds = 5*60*1000; // 5 минут
                bool finished = process.WaitForExit(timeoutMilliseconds); 

                if (!finished) {
                    process.Kill();
                    result.Error = ExecutionError.Timeout;
                    break;
                }

                if (this.AcceptedCodes.Contains(process.ExitCode)) {
                    result.Error = ExecutionError.NoError;
                    result.ExitCode = process.ExitCode;
                    isCodeAccepted = true;
                    break;
                } else {
                    result.WrongCodes.Add(process.ExitCode);
                }
            }
            if (!isCodeAccepted) {
                result.Error = ExecutionError.WrongCode;
            }
            result.RetriesCount = initialRetries-attemptsRemain;
            return result;
        }
    }
}