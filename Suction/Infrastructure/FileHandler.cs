using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using EnvDTE;
using Janison.Suction.Extensions;

namespace Janison.Suction.Infrastructure
{
    public class FileHandler
    {
        private enum ProjectItemBuildAction
        {
            None,
            Content,
            Compile,
            EmbeddedResource
        }

        protected static readonly List<string> ExportableFileTypes = new List<string> { "css", "js", "sass", "scss", "html", "htm" };

        public static void ItemSaved(Document document)
        {
            ItemAdded(document.ProjectItem);
        }

        public static void ItemAdded(ProjectItem projectItem)
        {
            if (projectItem.Properties == null)
                return;

            // Troubles loading F# projects.
            if (projectItem.GetType().ToString().Equals("Microsoft.VisualStudio.FSharp.ProjectSystem.Automation.OAFileItem", StringComparison.InvariantCultureIgnoreCase))
                return;

            // Only try and copy physical files (ignore folders, sub projects, virtual files.
            if (Guid.Parse(projectItem.Kind) != Guid.Parse("6bb5f8ee-4483-11d3-8bcf-00c04f8ec28c")) // GUID_ItemType_PhysicalFile
                return;

            // Only copy EmbeddedResources from projects into the Startup Project.
            var buildAction = (ProjectItemBuildAction)projectItem.Properties.Item("BuildAction").Value;
            if (buildAction != ProjectItemBuildAction.EmbeddedResource)
                return;

            // Also we're only going to copy selected file types (css, js) as other file types will probably need to be compiled
            if (!ExportableFileTypes.Contains(Path.GetExtension(projectItem.Name).TrimStart('.')))
                return;

            // If the file belongs to the start up application don't copy it anywhere
            var startupProject = Infrastructure.Core.Instance.StartupProject;
            if (projectItem.ContainingProject.Equals(startupProject))
                return;

            OutputWindow.Log(String.Format("Suctioning '{0}'", projectItem.FileNames[0]));
            CopiedFiles.Add(startupProject, projectItem);

            var ie = projectItem.ProjectItems.GetEnumerator();
            while (ie.MoveNext())
            {
                ProjectItem subProjectItem = projectItem.ProjectItems.Item(((dynamic)ie.Current).Name);
                ItemAdded(subProjectItem);
            }

            // Copy the file from the project into the same folder structure under the Startup Application
            // Have to be careful to keep folders like /content/styles/cool.css
            //var saveTo = startupProject.Combine(projectItem.FilenameAsRelativePath());
            //if (!Directory.Exists(Path.GetDirectoryName(saveTo)))
            //    Directory.CreateDirectory(Path.GetDirectoryName(saveTo));
            //File.Copy(projectItem.FileNames[0], saveTo, true);

            //Infrastructure.Core.Instance.Notity("Embedded resource exported to " + saveTo);
        }

        public static void ItemRenamed(ProjectItem projectItem, string oldName)
        {
            // Write the new file to disk and clean up the old.
            ItemAdded(projectItem);

            // Remove the old file, so we need to sub out the name name with the old.
            var oldRelativeFilePath = projectItem.FilenameAsRelativePath().Replace(Path.GetFileName(projectItem.FileNames[0]), oldName);
            CopiedFiles.Remove(oldRelativeFilePath);

            //var startupProject = Infrastructure.Core.Instance.StartupProject;
            //var existingFile = startupProject.Combine(projectItem.FilenameAsRelativePath());
            //var old = Path.Combine(Path.GetDirectoryName(existingFile), oldName);
            //if (File.Exists(old))
            //File.Delete(old);
        }

        public static void ItemRemoved(ProjectItem projectItem)
        {
            CopiedFiles.Remove(projectItem.FilenameAsRelativePath());
            //var startupProject = Infrastructure.Core.Instance.StartupProject;
            //var existingFile = startupProject.Combine(projectItem.FilenameAsRelativePath());
            //if (File.Exists(existingFile))
            //File.Delete(existingFile);
        }

        public static void MassExtraction(Solution solution)
        {
            foreach (Project project in solution.Projects)
            {
                MassExtraction(project);
            }
        }

        public static void MassExtraction(Project project)
        {
            if (project == null || project.ProjectItems == null)
                return;

            if (project.ProjectItems.Count < 1)
                return;

            var ie = project.ProjectItems.GetEnumerator();
            while (ie.MoveNext())
            {
                ProjectItem subProjectItem = project.ProjectItems.Item(((dynamic)ie.Current).Name);
                MassExtraction(subProjectItem);
            }
        }

        public static void MassExtraction(ProjectItem projectItem, bool displayOutput = false)
        {
            ItemAdded(projectItem);

            if ((projectItem.ProjectItems == null && projectItem.SubProject != null) && projectItem.SubProject.ProjectItems != null)
            {
                var ie = projectItem.SubProject.ProjectItems.GetEnumerator();
                while (ie.MoveNext())
                {
                    ProjectItem subProjectItem = projectItem.SubProject.ProjectItems.Item(((dynamic)ie.Current).Name);
                    MassExtraction(subProjectItem);
                }
            }

            // If this ProjectItem contains children they should be extracted too.
            if (projectItem.ProjectItems != null && projectItem.ProjectItems.Count > 0)
            {
                var ie = projectItem.ProjectItems.GetEnumerator();
                while (ie.MoveNext())
                {
                    ProjectItem subProjectItem = projectItem.ProjectItems.Item(((dynamic)ie.Current).Name);
                    MassExtraction(subProjectItem);
                }
            }
        }
    }
}
