using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using EnvDTE;

namespace Janison.Suction.Extensions
{
    public static class ProjectExtensions
    {
        public static DirectoryInfo DirectoryInfo(this Project project)
        {
            var path = Path.GetDirectoryName(project.FullName);
            return new DirectoryInfo(path);
        }

        public static string Combine(this Project project, string filename)
        {
            var path = Path.Combine(project.DirectoryInfo().FullName, filename);
            return path;
        }

        public static string ProjectPath(this ProjectItem projectItem)
        {
            //If the document does not exist (projectItem was removed), fallback to using the project item's url
            var projectItemPath = projectItem.Document == null
                ? ((string)projectItem.Properties.Item("URL").Value).Substring("file:///".Length) 
                : Path.GetFullPath(projectItem.ContainingProject.FullName);
            return projectItemPath;
        }

        public static string FilenameAsRelativePath(this ProjectItem projectItem)
        {
            var relativePath = projectItem.FileNames[0].Replace(Path.GetDirectoryName(projectItem.ContainingProject.FullName), String.Empty).TrimStart("\\".First());
            return relativePath;
        }
    }
}
