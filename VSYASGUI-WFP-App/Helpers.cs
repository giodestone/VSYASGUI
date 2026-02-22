using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        /// <param name="navigationService">The service of the page.</param>
        public static void PreventNavigationBackToPage(NavigationService navigationService)
        {
            if (navigationService.CanGoBack)
                navigationService.RemoveBackEntry();
        }

        /// <summary>
        /// Remove ALL back history.
        /// </summary>
        /// <remarks>
        /// Fixes any issues to do with memory leaks.
        /// </remarks>
        /// <param name="navigationService"></param>
        public static void DestroyAllBackNavigation(NavigationService? navigationService)
        {
            while (navigationService.CanGoBack)
                navigationService.RemoveBackEntry();
        }
    }
}
