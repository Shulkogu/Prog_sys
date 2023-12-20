using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;

namespace Model
{
    internal class JobSaver
    {
        public static void SaveJob(Job job)
        {
            List<Job> existingJobs = LoadExistingJobs();
            existingJobs.Add(job);
            SaveJobs(existingJobs);
        }

        public static List<Job> LoadExistingJobs()
        {
            List<Job> existingJobs = new List<Job>();

            if (System.IO.File.Exists(Constants.JobsFile))
            {
                string json = System.IO.File.ReadAllText(Constants.JobsFile);
                existingJobs = System.Text.Json.JsonSerializer.Deserialize<List<Job>>(json) ?? new List<Job>();
            }

            return existingJobs;
        }

        public static bool DeleteAllJobsByName(List<Job> jobs, string nom)
        {
            List<Job> jobsToDelete = jobs.Where(t => t.Name == nom).ToList();

            if (jobsToDelete.Any())
            {
                jobs.RemoveAll(t => t.Name == nom);
                SaveJobs(jobs);
                return true;
            }
            else
            {
                return false;
            }
        }

        public static void SaveJobs(List<Job> jobs)
        {
            string json = System.Text.Json.JsonSerializer.Serialize(jobs, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            System.IO.File.WriteAllText(Constants.JobsFile, json);
        }
    }
}
