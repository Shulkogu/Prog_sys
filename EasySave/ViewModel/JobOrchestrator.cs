using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using View;

namespace ViewModel
{
    internal class JobOrchestrator
    {
        private ProcessWatcher processWatcher = new ProcessWatcher();
        private bool ForbiddenProcessRunning;
        List<(Saver, Task)> Tasks = new List<(Saver, Task)>();
        Logger? Logger = null;
        bool JobsExecuting = false;
        object LockPriorityEvent = new object();
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
            if(this.Logger == null)
            {
                this.Logger = new Logger(Constants.LogPath, Constants.StatePath);
            }
            processWatcher.StartWatching();
            this.processWatcher.ProcessStarted += ProcessStartedEventHandler;
            this.processWatcher.ProcessExited += ProcessExitedEventHandler;
            try
            {
                foreach (Job Job in Jobs)
                {
                    if (!Tasks.Any(x => x.Item1.Job.Name == Job.Name))
                    {
                        Saver saver = new Saver(Job, ref Logger, ref LockPriorityEvent);
                        saver.PrioritizedCopyStarted += JobStartedPriority;
                        saver.PrioritizedCopyEnded += JobEndedPriority;
                        if(ForbiddenProcessRunning)
                        {
                            saver.Workstate = Workstate.PAUSED_FORBIDDENSOFTWARE;
                            saver.ManualResetEvent.Reset();
                        }
                        Tasks.Add((saver, Task.Run(() => {
                            saver.SaveFiles();
                            DeleteFinishedJobTask(saver);
                            }
                        )));
                    }
                }
                if (!JobsExecuting)
                {
                    JobsExecuting = true;
                    await Task.WhenAll(Tasks.Select(x => x.Item2));
                    //Console.WriteLine("all jobs finished");
                    Logger.FinalizeLogs();
                    Logger = null;
                    processWatcher.StopWatching();
                    JobsExecuting = false;
                }
            }
            catch
            {
            }
        }
        private void DeleteFinishedJobTask(Saver saver)
        {
            Tasks.RemoveAll(x => x.Item1 == saver);
        }
        private void JobStartedPriority(object sender, EventArgs e)
        {
            if ((sender is Saver saver)  && (saver.CurrentPriority==FilePriority.HIGH))
            {
                //Console.WriteLine($"The job {saver.Job.Name} is copying prioritized files.");
                foreach ((Saver, Task) NotPrioritizedTask in Tasks.Where(x => x.Item1 != sender))
                {
                    NotPrioritizedTask.Item1.Workstate = Workstate.PAUSED_UNPRIORITIZED;
                    NotPrioritizedTask.Item1.ManualResetEvent.Reset();
                    Logger.PauseState(NotPrioritizedTask.Item1.Job.Name, Workstate.PAUSED_UNPRIORITIZED);
                    //Console.WriteLine($"Job : {NotPrioritizedTask.Item1.Job.Name} is paused.");
                }
            }
        }
        private void JobEndedPriority(object sender, EventArgs e)
        {
            if ((sender is Saver saver) && (saver.CurrentPriority == FilePriority.HIGH))
            {
                //Console.WriteLine($"The job {saver.Job.Name} finished copying prioritized files.");
                foreach ((Saver, Task) NotPrioritizedTask in Tasks.Where(x => x.Item1 != sender))
                {
                    NotPrioritizedTask.Item1.Workstate = Workstate.ACTIVE;
                    NotPrioritizedTask.Item1.ManualResetEvent.Set();
                    Logger.ResumeState(NotPrioritizedTask.Item1.Job.Name, Workstate.ACTIVE);
                    //Console.WriteLine($"Job : {NotPrioritizedTask.Item1.Job.Name} is resumed.");
                }
            }
        }
        public void PauseJob(Job Job)
        {
            Saver saver = Tasks.Where(x => x.Item1.Job == Job).First().Item1;
            if( saver != null)
            {
                saver.Workstate = Workstate.PAUSED_USER;
                saver.ManualResetEvent.Reset();
                Logger.PauseState(saver.Job.Name, Workstate.PAUSED_USER);
            }
        }
        public void ResumeJob(Job Job)
        {
            Saver saver = Tasks.Where(x => x.Item1.Job == Job).First().Item1;
            if (saver != null && !this.ForbiddenProcessRunning && saver.Workstate == Workstate.PAUSED_USER)
            {
                saver.Workstate = Workstate.ACTIVE;
                saver.ManualResetEvent.Set();
                Logger.ResumeState(saver.Job.Name, Workstate.ACTIVE);
            }
        }
        private void ProcessStartedEventHandler(object sender, EventArgs e)
        {
            this.ForbiddenProcessRunning = true;
            foreach ((Saver, Task) NotPrioritizedTasks in Tasks.Where(x => x.Item1.Workstate != Workstate.PAUSED_UNPRIORITIZED && x.Item1.Workstate != Workstate.PAUSED_USER))
            {
                NotPrioritizedTasks.Item1.Workstate = Workstate.PAUSED_FORBIDDENSOFTWARE;
                NotPrioritizedTasks.Item1.ManualResetEvent.Reset();
                Logger.PauseState(NotPrioritizedTasks.Item1.Job.Name, Workstate.PAUSED_FORBIDDENSOFTWARE);
            }

        }
        private void ProcessExitedEventHandler(object sender, EventArgs e)
        {
            this.ForbiddenProcessRunning = false;
            foreach ((Saver, Task) NotPrioritizedTasks in Tasks.Where(x => x.Item1.Workstate != Workstate.PAUSED_UNPRIORITIZED && x.Item1.Workstate != Workstate.PAUSED_USER))
            {
                NotPrioritizedTasks.Item1.Workstate = Workstate.ACTIVE;
                NotPrioritizedTasks.Item1.ManualResetEvent.Set();
                Logger.ResumeState(NotPrioritizedTasks.Item1.Job.Name, Workstate.ACTIVE);
            }
        }
    }
}
