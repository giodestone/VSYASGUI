using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;

namespace VSYASGUI_WFP_App
{
    /// <summary>
    /// Class for helpers functions.
    /// </summary>
    internal static class Helpers
    {
        /// <summary>
        /// Prevent any further naviation back to this page. Should be called in <see cref="System.Windows.FrameworkElement.OnInitialized(EventArgs)"/> after it has been initalised.
        /// </summary>
        /// <remarks>
        /// Will not remove all navigation is navigation service is null.
        /// </remarks>
        /// <returns>True if possible to delete. False if not.</returns>
        public static bool TryPreventNavigationBackToPage(NavigationService? navigationService)
        {
            if (navigationService == null)
            {
                Console.WriteLine("WARNING: Back navigation has not been destroyed, as navigation service is null.");
                return false;
            }

            if (navigationService.CanGoBack)
                navigationService.RemoveBackEntry();

            return true;
        }

        /// <summary>
        /// Remove ALL back history, if possible.
        /// </summary>
        /// <remarks>
        /// Fixes any issues to do with memory leaks.
        /// <br/><br/>
        /// Will not remove all navigation is navigation service is null.
        /// </remarks>
        /// <returns>True if possible to delete. False if not.</returns>
        public static bool TryDestroyAllBackNavigation(NavigationService? navigationService)
        {
            if (navigationService == null)
            {
                Console.WriteLine("WARNING: Back navigation has not been destroyed, as navigation service is null.");
                return false;
            }

            while (navigationService.CanGoBack)
                navigationService.RemoveBackEntry();

            return true;
        }
    }
}
