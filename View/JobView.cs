using Model;
using System;
using System.Collections.Generic;
using System.Security.AccessControl;
using System.Xml.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using ViewModel;

namespace View
{
    internal class JobView
    {
        private JobOrchestrator jobOrchestrator = new JobOrchestrator();
        private Language lang;
        private List<Job> existingJobs;
        public JobView(ref List<Job> existingJobs, Language? language = null)
        //Constructor, needs a list of existingJobs, may take a language but if none is given, English will be used
        {
            this.lang = (language == null) ? Language.English : language.Value;
            this.existingJobs = existingJobs;
        }
        public void UpdateJobsList(List<Job> existingJobs)
        //Setter for the existingJobs jobs list
        {
            this.existingJobs = existingJobs;
        }
        public void UpdateLanguage(Language lang)
        //Setter for the language
        {
            this.lang = lang;
        }
        public bool UserInteraction()
        //Method used for the main user interactions
        {
            Console.WriteLine(Texts.ModeChoicePrompt(lang));
            //The user presses a key, according to the key that he pressed, one code block executes
            ConsoleKeyInfo key = Console.ReadKey(true);
            switch (key.Key)
            {
                case ConsoleKey.D1 or ConsoleKey.NumPad1:
                    //If the key pressed is 1, we verify that the job amount isn't max and if so, prompt the user for the new job's information
                    if (JobSaver.VerifyJobCount(this.existingJobs))
                    {
                        JobSaver.SaveJob(this.EnterDetailsFromConsole(), Constants.JobsFile);
                    }
                    else
                    {
                        Console.WriteLine(Texts.JobAmountMaxed(lang));
                        Console.ReadLine();
                    }
                    break;

                case ConsoleKey.D2 or ConsoleKey.NumPad2:
                    //If the key pressed is 2, we verify that there are existing Jobs and if so, prompt the user for the name of the job to delete
                    if (this.existingJobs.Count > 0)
                    {
                        Console.Write(Texts.PromptJobToDelete(lang));
                        string nameJobToDelete = Console.ReadLine();
                        if (JobSaver.DeleteAllJobsByName(this.existingJobs, nameJobToDelete, Constants.JobsFile))
                        {
                            Console.WriteLine(String.Format(Texts.ConfirmDeletion(lang), nameJobToDelete));
                        }
                        else
                        {
                            Console.WriteLine(Texts.UnknownJob(lang));
                        }
                    }
                    else
                    {
                        Console.Write(Texts.NoJobs(lang));
                    }
                    Console.ReadLine();
                    break;

                case ConsoleKey.D3 or ConsoleKey.NumPad3:
                    //If the key pressed is 3, we verify that there are existing jobs and if so, we display their information
                    if (this.existingJobs.Count > 0)
                    {
                        foreach (var existingJob in this.existingJobs)
                        {
                            Console.WriteLine(existingJob);
                        }
                    }
                    else
                    {
                        Console.WriteLine(Texts.NoJobs(lang));
                    }
                    Console.ReadLine();
                    break;


                case ConsoleKey.D4 or ConsoleKey.NumPad4:
                    //If the key pressed is 4, we verify that there are existing jobs and if so, we ask the user for the name of the job he wants to modify. If the name given corresponds to an existing save, we prompt the user for the new information
                    if (this.existingJobs.Count > 0)
                    {
                        Console.Write(Texts.PromptJobToModify(lang));
                        string jobName = Console.ReadLine();
                        int jobIndex = existingJobs.FindIndex(x => x.Name == jobName);
                        if (jobIndex >= 0)
                        {
                            existingJobs[jobIndex] = this.EnterDetailsFromConsole(existingJobs[jobIndex]);
                            JobSaver.SaveJobs(this.existingJobs, Constants.JobsFile);
                        }
                        else
                        {
                            Console.WriteLine(Texts.UnknownJob(lang));
                            Console.ReadLine();
                        }
                    }
                    else
                    {
                        Console.WriteLine(Texts.NoJobs(lang));
                        Console.ReadLine();
                    }
                    break;


                case ConsoleKey.D5 or ConsoleKey.NumPad5:
                    //If the key pressed is 5, we ask the user for a job range and we forward it to CallOrchestrator whose responsibility will be to execute the jobs
                    Console.WriteLine(Texts.PromptJobRange(lang));
                    this.CallOrchestrator(Console.ReadLine());
                    Console.ReadLine();
                    break;


                case ConsoleKey.D6 or ConsoleKey.NumPad6:
                    //If the key pressed is 6, we display the settings and allow the user to chose one to modify
                    List<dynamic> SettingsList = new List<dynamic>{Constants.Settings.Language,Constants.Settings.LogFileType};
                    int Choice = -1;
                    while (Choice != 0)
                    {
                        do
                        {
                            Console.WriteLine($"0. {Texts.ReturnToHomepage(lang)}");
                            for (int i = 0; i < SettingsList.Count; i++)
                            {
                                Console.WriteLine($"{i + 1}. {SettingsList[i].Value.GetType().Name} = {SettingsList[i].Value}");
                            }
                            try
                            {
                                Choice = Int32.Parse(Console.ReadLine());
                            }
                            catch
                            {
                                Choice = -1;
                            }
                        } while (Choice < 0 || Choice >= SettingsList.Count + 1);
                        if (Choice > 0)
                        {
                            MethodInfo method = typeof(JobView).GetMethod("EnterEnumElementFromConsole").MakeGenericMethod(SettingsList[Choice - 1].Value.GetType());
                            dynamic Setting = (method.Invoke(this, null));
                            SettingsList[Choice - 1].Value = Setting;
                            Constants.Settings.SaveSettings();
                        }
                    }
                    break;


                case ConsoleKey.Escape: //The escape key is used to close the program
                    return false;


                default:
                    //If another key is pressed, we ask the user to select an existing option next time
                    Console.WriteLine(Texts.UnknownAction(lang));
                    break;
            }
            Console.Clear();
            return true;
        }
        public void CallOrchestrator(string criteria)
        //Method used to call the Orchestrator with the list of jobs and the range of jobs to execute
        {
            this.jobOrchestrator.ExecuteJobs(this.jobOrchestrator.GetJobsByCriteria(criteria, this.existingJobs));
            Console.WriteLine(Texts.ExecutionEnd(lang));
        }
        private Job EnterDetailsFromConsole(Job? jobToModify = null)
        //Method used to ask the user for a job's details when creating/modifying one
        {
            Job job = new Job();
            do
            { //The user is asked for a job name until he enters a job name that isn't used by another job or that is the name of the job he's modifying
                Console.Write(Texts.PromptJobName(lang));
                string Name = Console.ReadLine();
                if (!this.existingJobs.Any(x => x.Name == Name) || (jobToModify != null && jobToModify.Name == Name))
                {
                    job.Name = Name;
                }
                else
                {
                    Console.WriteLine(Texts.JobNameAlreadyUsed(lang));
                }
            }
            while (job.Name == null);
            
            Console.Write(Texts.PromptJobSource(lang));
            job.SourcePath = Regex.Replace(Console.ReadLine(), Constants.SingleToDoubleBackslashRegex, @"\\"); //Uses a Regex that only captures backslashes in groups of 1 or 3+ and replaces them with 2 backslashes

            Console.Write(Texts.PromptJobTarget(lang));
            job.TargetPath = Regex.Replace(Console.ReadLine(), Constants.SingleToDoubleBackslashRegex, @"\\"); //Same as SourcePath

            Console.Write(Texts.PromptJobType(lang));
            string typeEntered = Console.ReadLine();
            job.SaveType = (Savetype)Enum.Parse(typeof(Savetype), typeEntered, true);
            return job;
        }
        public TEnum EnterEnumElementFromConsole<TEnum>()
        //Generic method used to ask the user for the element he wants to choose within a generic enum
        {
            int Choice;
            var EnumElements = Enum.GetValues(typeof(TEnum)).Cast<TEnum>().ToList();
            do
            {
                for (int i = 0; i < EnumElements.Count; i++)
                {
                    Console.WriteLine(i + "." + EnumElements[i]);
                }
                try
                {
                    Choice = Int32.Parse(Console.ReadLine());
                }
                catch
                {
                    Choice = -1;
                }
            } while (Choice < 0 || Choice >= EnumElements.Count);
            Console.Clear();
            return (EnumElements[Choice]);
        }
    }
}
