using System.Diagnostics;

namespace RetCodeAPI
{
    public interface ITaskService
    {
        public Task CreateTask(string path);
        public Task GetTask(int taskId);
    }

    public class Task 
    {
        private Process process;

        public int TaskId { get; private set; }
        public bool Completed { get; private set; }
        public int ExitCode { get; private set;}
        public string Status 
        { 
            get 
            {
                if (Completed)
                    return "Completed";
                else 
                    return "In Progress";
            } 
        }

        public Task(int taskId, string path) 
        {
            string testerPath = "..\\RetCodeTester\\bin\\Debug\\net8.0\\RetCodeTester.exe";

            this.TaskId = taskId;

            this.process = new Process();
            this.process.StartInfo.FileName = testerPath;
            this.process.StartInfo.Arguments = path;
            this.process.StartInfo.RedirectStandardOutput = true;
            this.process.StartInfo.UseShellExecute = false;
            this.process.StartInfo.CreateNoWindow = true;
            this.process.EnableRaisingEvents = true;
            this.process.Exited += new EventHandler(this.process_Exited);
        }

        public void Start() 
        {
            if (!this.Completed)
                this.process.Start();
        }

        private void process_Exited(object sender, System.EventArgs e)
        {
            this.Completed = true;
            this.ExitCode = process.ExitCode;
            Console.WriteLine(process.StandardOutput.ReadToEnd());
        }
    }

    public class TaskService: ITaskService
    {
        private Dictionary<int, Task> tasks = new Dictionary<int, Task>();

        public Task CreateTask(string path)
        {
            Task task;
            lock (this.tasks) {
                int taskId = this.tasks.Count;
                task = new Task(taskId, path);
                task.Start();
                this.tasks.Add(taskId, task);
            }
            return task;
        }

        public Task GetTask(int taskId)
        {
            Task task;
            lock (this.tasks) {
                task = this.tasks[taskId];
            }
            return task;
        }
    }
}