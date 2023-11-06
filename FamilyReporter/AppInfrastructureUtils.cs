using System.IO;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace FamilyReporter
{
    internal class AppInfrastructureUtils
    {
        /// <summary>
        /// This does not work for External Event Functions
        /// </summary>
        /// <param name="embeddedResource"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        internal static System.Windows.Media.ImageSource BmpImageSource(string embeddedResource, object obj)
        {
            Stream stream = obj.GetType().Assembly.GetManifestResourceStream(embeddedResource);
            var decoder = new System.Windows.Media.Imaging.PngBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
            return decoder.Frames[0];
        }


        #region RIBBON HELPER FUNCTIONS modified from http://spiderinnet.typepad.com/blog/2011/03/ribbon-panel-ribbonpanel-and-items-ribbonitem-of-revit-api-part-1-push-button-pushbutton-and-text-box-textbox.html
        internal static string AssemblyFullName
        {
            get
            {
                return Assembly.GetExecutingAssembly().Location;
            }
        }

        internal static string AssemblyPath
        {
            get
            {
                return Path.GetDirectoryName(AssemblyFullName);
            }
        }
        #endregion
    }
}
