using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace task_4.Model
{
    public class Logger
    {
        private static Logger? instance;

        private Logger()
            => Messages = new ObservableCollection<LogMessage>();

        public static Logger Instance
            => instance ??= new Logger();

        public ObservableCollection<LogMessage> Messages { get; }

        public void Log(string sender, string message)
        {
            App.Current?.Dispatcher.Invoke(() =>
            {
                Messages.Add(new LogMessage(sender, message));
            });
        }

        public void SaveToFile()
        {
            App.Current?.Dispatcher.Invoke(() =>
            {
                string docPath =
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                // Write the string array to a new file named "WriteLines.txt".
                using (StreamWriter outputFile = new StreamWriter(Path.Combine(docPath, "it_task4.log")))
                {
                    foreach (var line in Messages)
                        outputFile.WriteLine(line.Timestamp + " " + line.Sender + " " + line.Message);
                }
            });
        }
    }
}
