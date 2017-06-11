using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Ubrowser
{
    public class HelperRegistery
    {
        #region HelperInstance

        /// <summary>
        /// HelperInstance Attached Dependency Property
        /// </summary>
        public static readonly DependencyProperty HelperInstanceProperty =
            DependencyProperty.RegisterAttached("HelperInstance", typeof(WebBrowserHelper), typeof(HelperRegistery),
                new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Gets the HelperInstance property.  This dependency property 
        /// indicates ....
        /// </summary>
        public static WebBrowserHelper GetHelperInstance(DependencyObject d)
        {
            return (WebBrowserHelper)d.GetValue(HelperInstanceProperty);
        }

        /// <summary>
        /// Sets the HelperInstance property.  This dependency property 
        /// indicates ....
        /// </summary>
        public static void SetHelperInstance(DependencyObject d, WebBrowserHelper value)
        {
            d.SetValue(HelperInstanceProperty, value);
        }

        #endregion        
    }
}
