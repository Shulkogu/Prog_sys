using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;

namespace Model
{
    internal class JobSaver
    {
        public static void SaveJob(Job job, string jobsFilePath)
        {
            List<Job> existingJobs = LoadExistingJobs(jobsFilePath);
            VerifyJobCount(existingJobs);
            existingJobs.Add(job);
            SaveJobs(existingJobs, jobsFilePath);
        }

        public static List<Job> LoadExistingJobs(string jobsFilePath)
        {
            List<Job> existingJobs = new List<Job>();

            if (System.IO.File.Exists(jobsFilePath))
            {
                string json = System.IO.File.ReadAllText(jobsFilePath);
                existingJobs = System.Text.Json.JsonSerializer.Deserialize<List<Job>>(json) ?? new List<Job>();
            }

            return existingJobs;
        }

        public static bool VerifyJobCount(List<Job> jobs)
        {
            const int maxJobAmount = 5;

            if (jobs.Count >= maxJobAmount)
            {
                return false;
            }
            return true;
        }

        public static bool DeleteAllJobsByName(List<Job> jobs, string nom, string jobsFilePath)
        {
            List<Job> jobsToDelete = jobs.Where(t => t.Name == nom).ToList();

            if (jobsToDelete.Any())
            {
                jobs.RemoveAll(t => t.Name == nom);
                SaveJobs(jobs, jobsFilePath);
                return true;
            }
            else
            {
                return false;
            }
        }

        public static void SaveJobs(List<Job> jobs, string jobsFilePath)
        {
            string json = System.Text.Json.JsonSerializer.Serialize(jobs, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            System.IO.File.WriteAllText(jobsFilePath, json);
        }
    }
}
