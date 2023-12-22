using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    internal class JobOrchestrator
    {
        public event EventHandler StatesUpdated;
        private ProcessWatcher processWatcher = new ProcessWatcher();
        private ManualResetEvent ManualResetEvent = new ManualResetEvent(true);
        public Logger? Logger = null;
        public bool Ready = true;
        public List<Job> GetJobsByCriteria(string criteria, List<Job> Jobs)
        {
            //Takes a list of jobs and returns the jobs whose indexes are in the criteria
            //Example : a list of 5 jobs is sent along with the criteria 1-3;5 : The first, second, third and fifth jobs are returned
            List<Job> selectedJobs = new List<Job>();
            string[] criteriaArray = criteria.Split(';');
            foreach (string range in criteriaArray)
            {
                string[] bounds = range.Split('-');
                if (bounds.Length == 1)
                {
                    //If a single number is specifcied
                    if (int.TryParse(bounds[0], out int singleIndex))
                    {
                        if (singleIndex >= 1 && singleIndex <= Jobs.Count)
                        {
                            selectedJobs.Add(Jobs[singleIndex - 1]);
                        }
                        else
                        {
                            Console.WriteLine($"Index {singleIndex} hors des limites. Ignoré.");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Critère non valide : {bounds[0]}. Ignoré.");
                    }
                }
                else if (bounds.Length == 2)
                {
                    //If a range is specified
                    if (int.TryParse(bounds[0], out int startIndex) && int.TryParse(bounds[1], out int endIndex))
                    {
                        startIndex = Math.Max(startIndex, 1);
                        endIndex = Math.Min(endIndex, Jobs.Count);

                        for (int i = startIndex; i <= endIndex; i++)
                        {
                            selectedJobs.Add(Jobs[i - 1]);
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Critère non valide : {bounds[0]}-{bounds[1]}. Ignoré.");
                    }
                }
                else
                {
                    Console.WriteLine($"Critère non valide : {range}. Ignoré.");
                }
            }
            return selectedJobs;
        }
        public async void ExecuteJobs(List<Job> Jobs)
        //This methods execute given Jobs
        {
            if (Ready)
            {
                Ready = false;
                if (this.Logger == null)
                {
                    this.Logger = new Logger(Constants.LogPath, Constants.StatePath);
                }
                processWatcher.StartWatching();
                this.processWatcher.ProcessStarted += ProcessStartedEventHandler;
                this.processWatcher.ProcessExited += ProcessExitedEventHandler;
                this.Logger.StatesUpdated += LoggerUpdatedStates;
                try
                {
                    List<Saver> Savers = new List<Saver>();
                    foreach (Job Job in Jobs)
                    {
                        Savers.Add(new Saver(Job, ref Logger, ref ManualResetEvent));
                    }
                    foreach(Saver Saver in Savers)
                    {
                        Task task = Task.Run(() =>
                        {
                            Saver.SaveFiles();
                        });
                        await Task.WhenAll(task);
                        task.Dispose();
                    }
                }
                catch
                {
                }
                finally
                {
                    Ready = true;
                }
            }
        }
        private void ProcessStartedEventHandler(object sender, EventArgs e)
        {
            ManualResetEvent.Reset();
        }
        private void ProcessExitedEventHandler(object sender, EventArgs e)
        {
            ManualResetEvent.Set();
        }
        private void LoggerUpdatedStates(object sender, EventArgs e) 
        {
            StatesUpdated?.Invoke(this, EventArgs.Empty);
        }
    }
}
