using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Model;
using View;

class Program
{
    static void Main(string[] args)
    {
        //A jobview is created along with its list of existing jobs
        List<Job> existingJobs = JobSaver.LoadExistingJobs(Constants.JobsFile);
        bool SettingsExisted = Constants.Settings.LoadSettings();
        View.JobView jobView = new View.JobView(ref existingJobs, (SettingsExisted) ? Constants.Settings.Language.Value : null);
        //If two arguments were passed when starting the .EXE, the first one being "-e", the second argument is sent as range to be used by the orchestrator to start jobs
        if (args.Length == 2 && args[0] == "-e")
        {
            jobView.CallOrchestrator(args[1]);
        }
        //If no "-e" argument was passed when starting the .EXE, the CLI mode starts
        else
        {
            if(!SettingsExisted)
            {
                MethodInfo method = typeof(JobView).GetMethod("EnterEnumElementFromConsole").MakeGenericMethod(typeof(Language));
                dynamic language = (method.Invoke(jobView, null));
                Constants.Settings.Language.Value= language;
                Constants.Settings.SaveSettings();
                jobView.UpdateLanguage(language);
            }
            //Until the user closes the program, the home page is displayed after he finishes interacting with one of the modes. The list of existing jobs is refreshed each time
            while (jobView.UserInteraction())
            {
                existingJobs = JobSaver.LoadExistingJobs(Constants.JobsFile);
                jobView.UpdateJobsList(existingJobs);
            }
        }
    }
}