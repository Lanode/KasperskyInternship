namespace RetCodeTester
{  
    /// <summary>
    /// Основная программа
    /// </summary>
    public class Tester
    {
        /// <summary>
        /// Запуск основной программы
        /// </summary>
        /// <param name="args">Аргументы командной строки</param>
        /// <returns>Возвращает код который нужно вернуть в систему как exit code</returns>
        public int Run(string[] args) 
        {
            if (args.Length < 1) {
                Console.WriteLine("Argument error: no test script passed.");
                return -1;
            }
            if (args[0].Split('.').Last() != "dpl") {
                Console.WriteLine("Argument error: passed script is not in right format.");
                return -1;
            }
            if (!File.Exists(args[0])) {
                Console.WriteLine("Argument error: passed script doesn't exist.");
                return -1;
            }

            Parser parser = new Parser();
            parser.Placeholders = new Dictionary<string, string> {
                { "MyName", "Vasya" },
                { "Age",    "15" },
            };
            List<Command> commands;
            try {
                commands = parser.ParseScript(args[0]);
            } catch (Parser.InvalidLineFormatException e) {
                Console.WriteLine("Parsing error: {0}:\n{1}", e.Message, e.Line);
                return -2;
            }

            foreach (var command in commands) {
                Console.WriteLine("Testing \"{0}\" for [{1}] codes with {2} additional retries...", 
                    command.CommandText,
                    string.Join(", ", command.AcceptedCodes),
                    command.Retries);
                Command.ExecutionResult result = command.Execute();
                if (result.WrongCodes.Count > 0)
                    Console.WriteLine("Wrong exit codes: {0}", 
                        string.Join(", ", result.WrongCodes));
                switch (result.Error) {
                    case Command.ExecutionError.WrongCode:
                        Console.WriteLine("Execution error: command returns wrong code.");
                        return -3;
                    case Command.ExecutionError.Timeout:
                        Console.WriteLine("Execution error: command timeouts after 5 minutes.");
                        return -4;
                }
                Console.WriteLine("Command exits with accepted code \"{0}\" after {1} retry.", 
                    result.ExitCode,
                    result.RetriesCount);
            }

            return 0;
        }
    }
}