using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using EnvDTE;

namespace chrisbjohnson.TFS2010Interface
{
    class Common
    {
        internal static DTE GetApplicationReference()
        {
            return Package.GetGlobalService(typeof(SDTE)) as DTE;
        }

        internal static string GetSolutionDirectory()
        {
            string solutionFullname = GetApplicationReference().Solution.FullName;

            // If this fails, just return a blank string
            try { return System.IO.Path.GetDirectoryName(solutionFullname); }
            catch { return ""; }
        }
    }
}
