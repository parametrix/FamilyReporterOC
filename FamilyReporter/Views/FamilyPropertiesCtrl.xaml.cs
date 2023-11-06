using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;

namespace FamilyReporter
{
    /// <summary>
    /// Interaction logic for FamilyPropertiesCtrl.xaml
    /// </summary>
    public partial class FamilyPropertiesCtrl : UserControl
    {
        public FamilyPropertiesCtrl(string filePath)
        {
            InitializeComponent();

            if (filePath != null)
            {

                // data.xml must be replaced with new document
                // somewhat painful - probable opportunity for optimization
                XmlDocument xDoc = new XmlDocument();
                xDoc.Load(filePath);

                var entryXdp = this.FindResource("entry") as XmlDataProvider;
                entryXdp.Document = xDoc;

                var categoryXdp = this.FindResource("category") as XmlDataProvider;
                categoryXdp.Document = xDoc;

                var featureXdp = this.FindResource("feature") as XmlDataProvider;
                featureXdp.Document = xDoc;

                var groupXdp = this.FindResource("group") as XmlDataProvider;
                groupXdp.Document = xDoc;

                var familyXdp = this.FindResource("family") as XmlDataProvider;
                familyXdp.Document = xDoc;

                var partXdp = this.FindResource("part") as XmlDataProvider;
                partXdp.Document = xDoc;

                var stplXdp = this.FindResource("projectReporter") as XmlDataProvider;
                stplXdp.Document = xDoc;


                // load image
                string imageFilePath = System.IO.Path.ChangeExtension(filePath, ".png");
                if (File.Exists(imageFilePath))
                {
                    BitmapImage bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.UriSource = new Uri(imageFilePath);
                    bmp.EndInit();
                    imageViewer.Source = bmp;
                }
            }
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
            e.Handled = true;
        }
    }
}
