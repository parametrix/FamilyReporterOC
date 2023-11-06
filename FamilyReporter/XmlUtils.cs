using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace FamilyReporter
{
    class XmlUtils
    {
        internal static XNamespace spiceToolsNS = @"http://spicetools.co/namespaces/familyreporter";
        internal static string AbbreviatedNS = "STPL";
        internal static XName ReporterXName = spiceToolsNS + "ProjectReporter";

        public static bool HasStplNsElement(XDocument xDoc, string elementName)
        {
            // from: http://stackoverflow.com/questions/26952686/check-if-element-exists-in-xml-using-c-sharp
            return xDoc.Descendants(spiceToolsNS + elementName).Any();
        }

        /// <summary>
        /// this is not used
        /// IN FAVOR OF InsertSTPLElements(string filePath, XElement[] xmlElements)
        /// RESULTS ARE THE SAME
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="keyValues"></param>
        public static void InsertSTPLNodes(string filePath, KeyValuePair<string, object>[] keyValues)
        {
            XDocument xDoc = XDocument.Load(filePath);
            var nodes = xDoc.DescendantNodes();
            XElement root = xDoc.Root;

            // add STPL namespace to root if it does not exist already
            AddSTPLNamespaceToRoot(root);

            // get default namespace for additional elements with STPL namespace
            XNamespace defaultNS = root.GetDefaultNamespace();

            // check if Reporter Element exists
            XElement reporterElement = root.Descendants().Where(y => y.Name.LocalName == ReporterXName.LocalName).FirstOrDefault();

            if (reporterElement == null)
            {
                reporterElement = new XElement(ReporterXName,
                new XAttribute("updated", DateTime.Now.ToString())
                );
            }

            // add sub elements to reporter element
            if (keyValues != null)
            {
                foreach (var item in keyValues)
                {
                    reporterElement.Add(new XElement(defaultNS + item.Key, item.Value));
                }
            }

            root.AddFirst(reporterElement);
            xDoc.Save(filePath);
        }

        /// <summary>
        /// Insert an element directly rather than sending keyvalue pairs
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="xmlElements"></param>
        public static void InsertSTPLElements(string filePath, XElement[] xmlElements)
        {
            XDocument xDoc = XDocument.Load(filePath);
            var nodes = xDoc.DescendantNodes();
            XElement root = xDoc.Root;

            // add STPL namespace to root if it does not exist already
            AddSTPLNamespaceToRoot(root);

            // get default namespace for additional elements with STPL namespace
            XNamespace defaultNS = root.GetDefaultNamespace();

            // check if Reporter Element exists
            XElement reporterElement = root.Descendants().Where(y => y.Name.LocalName == ReporterXName.LocalName).FirstOrDefault();

            if (reporterElement == null)
            {
                reporterElement = new XElement(ReporterXName,
                new XAttribute("updated", DateTime.Now.ToString())
                );
            }

            // add sub elements to reporter element
            if (xmlElements != null)
            {
                foreach (var item in xmlElements)
                {
                    // assign default namespace to all elements
                    item.Name = defaultNS + item.Name.LocalName;
                    reporterElement.Add(item);
                }
            }

            root.AddFirst(reporterElement);
            xDoc.Save(filePath);
        }

        /// <summary>
        /// Add STPL Namespace to (root) element
        /// if the root element does not already exist
        /// </summary>
        /// <param name="root"></param>
        private static void AddSTPLNamespaceToRoot(XElement root)
        {
            // check if STPL namespace attribute exists
            if (root.HasAttributes)
            {
                // add attribute to doc if it does not exist
                if (!root.Attributes().Where(x => x.Name == XNamespace.Xmlns + AbbreviatedNS).Any())
                {
                    // adding STPL namespace
                    root.SetAttributeValue(XNamespace.Xmlns + AbbreviatedNS, spiceToolsNS);
                }
            }
        }

    }
}
