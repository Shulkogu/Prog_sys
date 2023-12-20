using System;
using System.Diagnostics;
using System.Threading;

namespace Model
{
    public class ProcessWatcher
    {
        //Events declaration
        public event EventHandler ProcessStarted;
        public event EventHandler ProcessExited;

        private bool isWatching;
        private bool isRunning = false;

        //Method to start monitoring the presence of a certain process
        public void StartWatching()
        {
            if (!isWatching)
            {
                isWatching = true;
                //Create a new thread for the continuous monitoring of the process
                Thread watcherThread = new Thread(WatchProcess);
                watcherThread.Start();
            }
        }

        //Method to stop the monitoring
        public void StopWatching()
        {
            isWatching = false;
        }

        //Method executed in the thread to monitor the presence of the process and send events if needed
        private void WatchProcess()
        {
            while (isWatching) //While the monitoring is enabled
            {
                //Check if at least one of the monitored processes is running
                if (IsProcessRunning())
                {
                    //If it wasn't previously running
                    if (!isRunning)
                    {
                        OnProcessStarted(); //Trigger the event that states that at least one process is running
                    }
                    isRunning = true; //Set the value to true, in order not to trigger the event if the state doesn't change at next checking
                }
                else
                {
                    //If it was previously running
                    if (isRunning)
                    {
                        OnProcessExited(); //Trigger the event that states that no more process are running
                    }
                    isRunning = false; //Set the value to false, in order not to trigger the event next time if the state doesn't change
                }

                //Wait before cheking again
                Thread.Sleep(1000);
            }
        }

        //Method used to verify if at least one of the processes running has the looked for name.
        private bool IsProcessRunning()
        {
            if (Constants.Settings.ForbiddenSoftware != "")
            {
                Process[] processes = Process.GetProcessesByName(Constants.Settings.ForbiddenSoftware);
                return processes.Length > 0 && !processes[0].HasExited;
            }
            else
            {
                return false;
            }
        }

        //Method used to trigger the "ProcessStarted" event
        protected virtual void OnProcessStarted()
        {
            ProcessStarted?.Invoke(this, EventArgs.Empty);
        }
        //Method used to trigger the "ProcessExited" event
        protected virtual void OnProcessExited()
        {
            ProcessExited?.Invoke(this, EventArgs.Empty);
        }
    }
}