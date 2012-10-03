using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Microsoft.Win32;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;

namespace Janison.Suction
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    ///
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the 
    /// IVsPackage interface and uses the registration attributes defined in the framework to 
    /// register itself and its components with the shell.
    /// </summary>
    // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
    // a package.
    [PackageRegistration(UseManagedResourcesOnly = true)]
    // This attribute is used to register the informations needed to show the this package
    // in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(GuidList.guidSuctionPkgString)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExists_string)]
    public sealed class SuctionPackage : Package
    {
        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public SuctionPackage()
        {
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
        }



        /////////////////////////////////////////////////////////////////////////////
        // Overriden Package Implementation
        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initilaization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            Trace.WriteLine (string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
            base.Initialize();

            var core = Infrastructure.Core.Instance; // Init the core.

            // Add our command handlers for menu (commands must exist in the .vsct file)
            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (null != mcs)
            {
                // Create the command for the menu item.
                CommandID menuCommandID = new CommandID(GuidList.guidSuctionCmdSet, (int)PkgCmdIDList.cmdidSuctionSolution);
                MenuCommand menuItem = new MenuCommand(MenuItemCallback_Solution, menuCommandID);
                mcs.AddCommand(menuItem);

                menuCommandID = new CommandID(GuidList.guidSuctionCmdProjectSet, (int)PkgCmdIDList.cmdidSuctionProject);
                menuItem = new MenuCommand(MenuItemCallback_Project, menuCommandID);
                mcs.AddCommand(menuItem);

                menuCommandID = new CommandID(GuidList.guidSuctionCmdItemSet, (int)PkgCmdIDList.cmdidSuctionItem);
                menuItem = new MenuCommand(MenuItemCallback_Item, menuCommandID);
                mcs.AddCommand(menuItem);                
            }
        }
        #endregion

        /// <summary>
        /// This function is the callback used to execute a command when the a menu item is clicked.
        /// See the Initialize method to see how the menu item is associated to this function using
        /// the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        private void MenuItemCallback_Solution(object sender, EventArgs e)
        {
            var stopwatch = Stopwatch.StartNew();

            // Show a Message Box to prove we were here
            IVsUIShell uiShell = (IVsUIShell)GetService(typeof(SVsUIShell));
            Infrastructure.FileHandler.MassExtraction(Infrastructure.Core.Instance.Dte.Solution);
            OutputWindow.Log(String.Format("Done with Solution! In {0}s", Math.Round(stopwatch.Elapsed.TotalSeconds, 1)));
            //Guid clsid = Guid.Empty;
            //int result;
            //Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(uiShell.ShowMessageBox(
            //           0,
            //           ref clsid,
            //           "Suction",
            //           string.Format(CultureInfo.CurrentCulture, "Inside {0}.MenuItemCallback_Solution()", this.ToString()),
            //           string.Empty,
            //           0,
            //           OLEMSGBUTTON.OLEMSGBUTTON_OK,
            //           OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST,
            //           OLEMSGICON.OLEMSGICON_INFO,
            //           0,        // false
            //           out result));
        }

        private void MenuItemCallback_Project(object sender, EventArgs e)
        {
            var stopwatch = Stopwatch.StartNew();

            // Show a Message Box to prove we were here
            IVsUIShell uiShell = (IVsUIShell)GetService(typeof(SVsUIShell));

            EnvDTE.UIHierarchy solutionExplorer = Infrastructure.Core.Instance.Dte.ToolWindows.SolutionExplorer;
            var selectedItems = solutionExplorer.SelectedItems as Array;
            if (selectedItems == null || selectedItems.Length > 1)
                return;

            EnvDTE.UIHierarchyItem item1 = selectedItems.GetValue(0) as EnvDTE.UIHierarchyItem;
            var project = item1.Object as EnvDTE.Project;
            if (project != null)
            {
                Infrastructure.FileHandler.MassExtraction(project);
            }
            else
            {
                var projectItem = item1.Object as EnvDTE.ProjectItem;
                Infrastructure.FileHandler.MassExtraction(projectItem);
            }

            OutputWindow.Log(String.Format("Done with Project! In {0}s", Math.Round(stopwatch.Elapsed.TotalSeconds, 1)));
        }

        private void MenuItemCallback_Item(object sender, EventArgs e)
        {
            // Show a Message Box to prove we were here
            IVsUIShell uiShell = (IVsUIShell)GetService(typeof(SVsUIShell));

            EnvDTE.UIHierarchy solutionExplorer = Infrastructure.Core.Instance.Dte.ToolWindows.SolutionExplorer;
            var selectedItems = solutionExplorer.SelectedItems as Array;
            if (selectedItems == null || selectedItems.Length > 1)
                return;

            EnvDTE.UIHierarchyItem item1 = selectedItems.GetValue(0) as EnvDTE.UIHierarchyItem;
            var projectItem = item1.Object as EnvDTE.ProjectItem;
            Infrastructure.FileHandler.MassExtraction(projectItem);

            OutputWindow.Log(String.Format("Done with File!"));
        }
    }
}
