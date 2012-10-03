using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EnvDTE;
using System.Diagnostics;

namespace Janison.Suction.Infrastructure
{
    public class BuildHandler
    {
        public static void BuildBegin(vsBuildScope buildScope, vsBuildAction buildAction)
        {
            Trace.WriteLine("Build began");
        }

        public static void BuildDone(vsBuildScope buildScope, vsBuildAction buildAction)
        {
            Trace.WriteLine("Build done");
			
            // Once the project has built then try and clean up the files copied into the Bootstrapper
            CopiedFiles.RemoveAll();
        }
    }
}
