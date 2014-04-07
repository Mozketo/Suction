using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Janison.Suction.Extensions;
using Janison.Suction.Models;
using EnvDTE;
using System.Security.Principal;

namespace Janison.Suction.Infrastructure
{
    public sealed class CopiedFiles
    {
        private static List<RelativeFile> _relativeFilenames = new List<RelativeFile>();

        public static void Add(ProjectItem projectItem)
        {
            var startupProject = Infrastructure.Core.Instance.StartupProject;
            Add(startupProject, projectItem);
        }

        public static void Add(Project destination, ProjectItem projectItem)
        {
            if (!_relativeFilenames.Select(m => m.Name).Contains(projectItem.FilenameAsRelativePath()))
                _relativeFilenames.Add(new RelativeFile(projectItem.FilenameAsRelativePath()));

            var saveTo = destination.Combine(projectItem.FilenameAsRelativePath());
            Directory.CreateDirectory(Path.GetDirectoryName(saveTo));

            if (File.Exists(saveTo))
                File.Delete(saveTo);

            File.Copy(projectItem.FileNames[0], saveTo, true);
            if (File.GetAttributes(saveTo).HasFlag(FileAttributes.ReadOnly))
                File.SetAttributes(saveTo, File.GetAttributes(saveTo) & ~FileAttributes.ReadOnly);

            OutputWindow.Log(String.Format("Suctioning '{0}'", saveTo));
        }

        public static void Remove(string relativeFilename)
        {
            if (_relativeFilenames.Select(m => m.Name).Contains(relativeFilename))
                _relativeFilenames.Remove(new RelativeFile(relativeFilename));

            var startupProject = Infrastructure.Core.Instance.StartupProject;
            var existingFile = startupProject.Combine(relativeFilename);
            if (File.Exists(existingFile))
                File.Delete(existingFile);
        }

        public static void RemoveAll()
        {
            _relativeFilenames.ToList().ForEach(file => { Remove(file.Name); });
        }
    }
}
