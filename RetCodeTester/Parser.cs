using System.Text.RegularExpressions;

namespace RetCodeTester
{
    /// <summary>
    /// Парсер dpl скрипта
    /// </summary>
    public class Parser
    {
        public class InvalidLineFormatException : System.FormatException
        {

            /// <summary>
            /// Линия с ошибкой
            /// </summary>
            public string Line { get; }

            public InvalidLineFormatException(string message, string line) : base(message) 
            {
                this.Line = line;
            }
        }

        private Regex regex = new Regex(@"^(?<command>(?:(?!;code|;retry).)+)(?:;code(?<code>(?:\d+,?)+))?(?:;retry(?<retry>\d+))?$");

        /// <summary>
        /// Словарь с плейсхолдерами (ключи) и их заменами (значения)
        /// </summary>
        public Dictionary<string, string> Placeholders { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Распарсить строку скрипта. Возвращает  
        /// </summary>
        public Command ParseLine(string line)
        {
            Match match = regex.Match(line);
            if (match.Success)
            {
                // Замена плейсхолдеров
                string placeholderedCommand = match.Groups["command"].Value;
                foreach (var placeholder in this.Placeholders) {
                    placeholderedCommand = placeholderedCommand.Replace('%'+placeholder.Key+'%', placeholder.Value);
                }

                // Создание объекта комманды с заменёнными плейсхолдерами
                Command command = new Command(placeholderedCommand);

                // Установить удволитворительные коды если указаны
                if (match.Groups["code"].Success) {
                    string[] codeSplitted = match.Groups["code"].Value.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    command.AcceptedCodes = Array.ConvertAll(codeSplitted, int.Parse);
                }

                // Установить количество повторов если указаны
                if (match.Groups["retry"].Success) {
                    command.Retries = int.Parse(match.Groups["retry"].Value);
                }

                return command;
            }
            else
            {
                throw new InvalidLineFormatException("Line cannot be parsed", line);
            }
        }

        /// <summary>
        /// Распарсить весь скрипт. Принимает аргументом путь до скрипта
        /// </summary>
        public List<Command> ParseScript(string path) 
        {
            var lines = File.ReadLines(path);
            List<Command> commands = new List<Command>();
            foreach (var line in lines) {
                commands.Add(this.ParseLine(line));
            }
            return commands;
        }
    }
}