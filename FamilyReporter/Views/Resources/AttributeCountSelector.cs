using System.Windows;
using System.Windows.Controls;
using System.Xml;

namespace FamilyReporter
{
    public class AttributeCountSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            FrameworkElement element = container as FrameworkElement;

            if (element != null && item != null)
            {
                XmlElement xmlElem = item as XmlElement;
                if (xmlElem.Attributes.Count == 3)
                {
                    return element.FindResource("ThreeAttributeTemplate") as DataTemplate;
                }
                else if (xmlElem.Attributes.Count == 2)
                {
                    return element.FindResource("TwoAttributeTemplate") as DataTemplate;
                }
                else
                {
                    return element.FindResource("SingleAttributeTemplate") as DataTemplate;
                }
            }
            return base.SelectTemplate(item, container);
        }
    }
}
