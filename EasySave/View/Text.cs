﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace View
{
    public enum Language
    {
        English,
        French
    }
    public static class Texts
    //This class has methods that return the text used by the View in one of each language proposed by the program
    {
        public static string ModeChoicePrompt(Language language)
        {
            switch (language)
            {
                case Language.French:
                    return ("Appuyez sur une touche pour effectuer une action." +
                        "\n1. Ajouter un travail" +
                        "\n2. Supprimer un travail" +
                        "\n3. Afficher les travaux" +
                        "\n4. Modifier un travail" +
                        "\n5. Exécuter un ou des travaux" +
                        "\n6. Paramétrer le programme" +
                        "\nÉchap. Quitter le programme");
                default:
                    return ("Press a key to choose a mode." +
                        "\n1. Add a job" +
                        "\n2. Delete a job" +
                        "\n3. Display jobs" +
                        "\n4. Modify a job" +
                        "\n5. Execute job(s)" +
                        "\n6. Adjust the program's settings" +
                        "\nEscape. Close the program");
            }
        }
        public static string ReturnToHomepage(Language language)
        {
            switch (language)
            {
                case Language.French:
                    return ("Retour à l'écran d'accueil");
                default:
                    return ("Return to homepage");
            }
        }
        public static string UnknownAction(Language language)
        {
            switch (language)
            {
                case Language.French:
                    return ("Touche non reconnue. Aucune action effectuée.");
                default:
                    return ("Unknown key, no actions were executed.");
            }
        }
        public static string JobAmountMaxed(Language language)
        {
            switch (language)
            {
                case Language.French:
                    return ("Impossible. Le nombre maximal de travaux est atteint.");
                default:
                    return ("The max amount of jobs was reached.");
            }
        }
        public static string NoJobs(Language language)
        {
            switch (language)
            {
                case Language.French:
                    return ("Aucun travail existant");
                default:
                    return ("No existing jobs");
            }
        }
        public static string PromptJobToDelete(Language language)
        {
            switch (language)
            {
                case Language.French:
                    return ("Entrez le nom du travail à supprimer :");
                default:
                    return ("Enter the name of the job to delete:");
            }
        }
        public static string ConfirmDeletion(Language language)
        {
            {
                switch (language)
                {
                    case Language.French:
                        return ("Le travail portant le nom '{0}' a été supprimé.");
                    default:
                        return ("The job named '{0}' was deleted.");
                }
            }
        }
        public static string PromptJobToModify(Language language)
        {
            switch (language)
            {
                case Language.French:
                    return ("Entrez le nom du travail à modifier :");
                default:
                    return ("Enter the name of the job to modify");
            }
        }
        public static string PromptJobName(Language language)
        {
            switch (language)
            {
                case Language.French:
                    return ("Nom de travail :");
                default:
                    return ("Job name:");
            }
        }
        public static string PromptJobSource(Language language)
        {
            switch (language)
            {
                case Language.French:
                    return ("Répertoire source :");
                default:
                    return ("Source path:");
            }
        }
        public static string PromptJobTarget(Language language)
        {
            switch (language)
            {
                case Language.French:
                    return ("Répertoire cible :");
                default:
                    return ("Target path:");
            }
        }
        public static string PromptJobType(Language language)
        {
            switch (language)
            {
                case Language.French:
                    return ("Type de sauvegarde (Complete/Differential) :");
                default:
                    return ("Save type (Complete/Differential) :");
            }
        }
        public static string PromptJobRange(Language language)
        {
            switch (language)
            {
                case Language.French:
                    return ("Entrez la plage d'indice des travaux à exécuter");
                default:
                    return ("Enter the index range of jobs to execute");
            }
        }
        public static string JobNameAlreadyUsed(Language language)
        {
            switch (language)
            {
                case Language.French:
                    return ("Un travail porte déjà ce nom");
                default:
                    return ("A job already has this name");
            }
        }
        public static string UnknownJob(Language language)
        {
            switch (language)
            {
                case Language.French:
                    return ("Ce travail n'existe pas");
                default:
                    return ("This job doesn't exist");
            }
        }
        public static string ExecutionEnd(Language language)
        {
            switch (language)
            {
                case Language.French:
                    return ("Fin de l'exécution des travaux");
                default:
                    return ("End of jobs execution");
            }
        }
        public static string ForbiddenProcessStarted(Language language)
        {
            switch (language)
            {
                case Language.French:
                    return ("La présence d'un ou plusieurs logiciels métiers a été détectée, veuillez le/les fermer, sinon le logiciel se fermera avant la prochaine exécution d'un travail");
                default:
                    return ("The execution of one or multiple forbidden software was detected, please close it/them, otherwise the logiciel will close itself before the next job execution");
            }
        }
        public static string ForbiddenProcessExited(Language language)
        {
            switch (language)
            {
                case Language.French:
                    return ("Le ou les logiciels métiers ont été fermés");
                default:
                    return ("No more forbidden software is running");
            }
        }
    }
}