using System.Collections.ObjectModel;

namespace task_4.Model
{
    internal class Logger
    {
        private static Logger? instance;

        private Logger()
            => Messages = new ObservableCollection<LogMessage>();

        public static Logger Instance
            => instance ??= new Logger();

        public ObservableCollection<LogMessage> Messages { get; }

        public void Log(string sender, string message)
            => Messages.Add(new LogMessage(sender, message));
    }
}
