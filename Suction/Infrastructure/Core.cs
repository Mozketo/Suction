using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using EnvDTE;
using System.Globalization;
using EnvDTE80;

namespace Janison.Suction.Infrastructure
{
    public sealed class Core
    {
        private static readonly Core instance = new Core();
        public static Core Instance { get { return instance; } }

        static Core() { }

        private IVsOutputWindowPane lazyOutputWindowPane;
        private List<object> _events = new List<object>();

        public SolutionEventsListener SolutionEventsListener { get; private set; }
        public EnvDTE80.DTE2 Dte { get; private set; }
        public Events2 Events { get; private set; }

        public Project StartupProject
        {
            get
            {
                var startupProjects = (Array)Dte.Solution.SolutionBuild.StartupProjects;
                if (startupProjects == null)
                    return null;

                if (startupProjects.Length > 1)
                    throw new ApplicationException("The solution cannot contain more than one startup project. Active startup projects: " + String.Join(", ", startupProjects));

                Project startupProject = Dte.Solution.Projects.Item(startupProjects.GetValue(0));
                return startupProject;
            }
        }

        public Core()
        {
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Start Core(): {0}", this.ToString()));

            //SolutionEventsListener class from http://stackoverflow.com/questions/2525457/automating-visual-studio-with-envdte 
            // via Elisha http://stackoverflow.com/users/167149/elisha
            SolutionEventsListener = new SolutionEventsListener();

            SolutionEventsListener.OnQueryUnloadProject += () =>
            {
                CopiedFiles.RemoveAll();
            };

            Dte = Package.GetGlobalService(typeof(DTE)) as EnvDTE80.DTE2;
            Events = Dte.Events as Events2;
            
            var docEvents = Events.DocumentEvents;
            var projectEvents = Events.ProjectItemsEvents;
            var buildEvents = Events.BuildEvents;

            _events.Add(docEvents);
            _events.Add(projectEvents);
            _events.Add(buildEvents);

            docEvents.DocumentSaved += FileHandler.ItemSaved;

            projectEvents.ItemAdded += FileHandler.ItemAdded;
            projectEvents.ItemRemoved += FileHandler.ItemRemoved;
            projectEvents.ItemRenamed += FileHandler.ItemRenamed;

            buildEvents.OnBuildBegin += BuildHandler.BuildBegin;
            buildEvents.OnBuildDone += BuildHandler.BuildDone;
        }

        public void Notity(string message)
        {
            try
            {
                OutputWindowPane.OutputString(String.Format("Suction: {0}{1}", message, System.Environment.NewLine));
            }
            catch (Exception errorThrow)
            {
                var x = errorThrow;
                //MessageBox.Show(errorThrow.ToString());
            }
            //if (Dte == null || Dte.StatusBar == null)
            //    return;
            //Dte.StatusBar.Text = message;
        }

        #region Output Window Pane

        public IVsOutputWindowPane OutputWindowPane
        {
            get
            {
                if (lazyOutputWindowPane == null)
                {
                    lazyOutputWindowPane = GetGeneralOutputWindowPane();
                }
                return lazyOutputWindowPane;
            }
        }

        private IVsOutputWindowPane GetGeneralOutputWindowPane()
        {
            var outputWindow = Package.GetGlobalService(typeof(SVsOutputWindow)) as IVsOutputWindow;
            IVsOutputWindowPane pane;
            Guid guidGeneralPane = VSConstants.GUID_OutWindowGeneralPane;
            outputWindow.GetPane(ref guidGeneralPane, out pane);
            return pane;
        }

        public void OutputWindowWriteText(string message)
        {
            try
            {
                OutputWindowPane.OutputString(String.Format("Suction: {0}{1}", message, System.Environment.NewLine));
            }
            catch (Exception errorThrow)
            {
                //MessageBox.Show(errorThrow.ToString());
            }
        }

        #endregion
    }
}
