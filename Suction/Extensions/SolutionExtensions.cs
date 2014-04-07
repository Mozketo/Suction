using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EnvDTE80;
using System.IO;

namespace Janison.Suction.Extensions
{
    public static class SolutionExtensions
    {
        public static EnvDTE.Project FindProjectEx(this EnvDTE.Solution solution, string projectName)
        {
            foreach (EnvDTE.Project project in solution.Projects)
            {
                if (project.Name.Equals(projectName))
                    return project;
            }
            return null;
        }

        public static EnvDTE.ProjectItem FindProjectItemEx(this EnvDTE.Solution solution, string projectItemName)
        {
            if (projectItemName.Contains(@"\"))
            {
                var f = new FileInfo(projectItemName);
                projectItemName = f.Name.Replace(f.Extension, String.Empty);
            }
            foreach (EnvDTE.Project project in solution.Projects)
            {
                foreach (EnvDTE.ProjectItem subProject in project.ProjectItems)
                {
                    if (subProject.Name.Equals(projectItemName))
                        return subProject;
                }
            }
            return null;
        }
    }
}
