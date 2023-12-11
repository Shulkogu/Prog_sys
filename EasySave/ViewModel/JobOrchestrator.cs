using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using View;

namespace ViewModel
{
    internal class JobOrchestrator
    {
        public event EventHandler ForbiddenProcessStarted;
        public event EventHandler ForbiddenProcessExited;
        private ProcessWatcher processWatcher = new ProcessWatcher();
        private bool ForbiddenProcessRunning;
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
        public bool ExecuteJobs(List<Job> Jobs)
        //This methods execute given Jobs
        {
            processWatcher.StartWatching();
            this.processWatcher.ProcessStarted += ProcessStartedEventHandler;
            this.processWatcher.ProcessExited += ProcessExitedEventHandler;
            Logger Logger = new Logger(Constants.LogPath, Constants.StatePath);
            foreach (Job Job in Jobs)
            {
                if(ForbiddenProcessRunning)
                {
                    break;
                }
                //Test print, to be removed
                Console.WriteLine($"Executing Job:{Job.Name}");
                Saver saver = new Saver(Job, ref Logger);
                saver.SaveFiles();
            }
            Logger.FinalizeLogs();
            processWatcher.StopWatching();
            if(ForbiddenProcessRunning)
            {
                return false;
            }
            return true;
        }
        private void ProcessStartedEventHandler(object sender, EventArgs e)
        {
            this.ForbiddenProcessRunning = true;
            ForbiddenProcessStarted.Invoke(this, EventArgs.Empty);
        }
        private void ProcessExitedEventHandler(object sender, EventArgs e)
        {
            this.ForbiddenProcessRunning = false;
            ForbiddenProcessExited.Invoke(this, EventArgs.Empty);
        }
    }
}
