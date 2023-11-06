using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyReporter
{
    internal class DocQueryUtils
    {
        /// <summary>
        /// Deletes all import instances within familyDocument
        /// and reloads family into project while overwriting existing family
        /// </summary>
        /// <param name="familyDocument"></param>
        /// <returns></returns>
        internal static List<string> DeleteImportsAndReloadFamily(Document familyDocument, Document parentDocument)
        {
            // delete import instance from family document
            ElementClassFilter classFilter = new ElementClassFilter(typeof(ImportInstance));
            FilteredElementCollector collector = new FilteredElementCollector(familyDocument);

            List<string> importInstancesDeleted = collector.WherePasses(classFilter).Select(x => x.get_Parameter(BuiltInParameter.ELEM_FAMILY_AND_TYPE_PARAM).AsValueString()).ToList();
            IList<ElementId> importInstanceIdList = new FilteredElementCollector(familyDocument).OfClass(typeof(ImportInstance)).Select(x => x.Id).ToList();
            using (Transaction t1 = new Transaction(familyDocument, "Delete Import Instance"))
            {
                t1.Start();
                familyDocument.Delete(importInstanceIdList);
                t1.Commit();
            }

            // save family in temporary location
            string familyName = familyDocument.Title;
            string filePath = Path.Combine(Path.GetTempPath(), familyName);

            SaveAsOptions saveAsOptions = new SaveAsOptions();
            saveAsOptions.OverwriteExistingFile = true;

            familyDocument.SaveAs(filePath, saveAsOptions);
            familyDocument.Close(false);

            // reload family to project
            FamilyReporter.RequestHandlerFunctions.FamilyLoadOptions familyLoadOptions = new FamilyReporter.RequestHandlerFunctions.FamilyLoadOptions();
            using (Transaction t2 = new Transaction(UIEventApp.DbDoc, "Reload Modified Family"))
            {
                t2.Start();

                Family family = null;

                FilteredElementCollector familyCollector = new FilteredElementCollector(parentDocument).OfClass(typeof(Family));

                UIEventApp.DbDoc.LoadFamily(filePath, familyLoadOptions, out family);

                t2.Commit();
            }

            return importInstancesDeleted;
        }


        /// <summary>
        /// Gets a list of family documents containing Import instances
        /// </summary>
        /// <returns></returns>
        internal static List<Document> GetFamilyDocumentsWithImports()
        {
            Document dbDoc = UIEventApp.DbDoc;
            Options geometryOptions = dbDoc.Application.Create.NewGeometryOptions();

            List<Document> familyDocsWithImportInstances = new List<Document>();

            ElementClassFilter filter = new ElementClassFilter(typeof(FamilyInstance));
            FilteredElementCollector collector = new FilteredElementCollector(dbDoc)
                .WherePasses(filter);

            IEnumerable<Element> elementsWithValidGeometry = collector
                                    .Where(i => { try { return i.get_Geometry(geometryOptions) != null; } catch { return false; } })
                                    .Select(anon => new { param = anon.get_Parameter(BuiltInParameter.ELEM_FAMILY_PARAM), anon })
                                    .GroupBy(x => x.param.AsValueString())
                                    .Select(y => y.Select(o => o.anon).FirstOrDefault());


            foreach (Element element in elementsWithValidGeometry)
            {
                GeometryElement geometryElement = element.get_Geometry(geometryOptions);

                foreach (GeometryObject geometryObject in geometryElement)
                {
                    GeometryInstance geometryInstance = geometryObject as GeometryInstance;

                    if (null != geometryInstance)
                    {
                        GeometryElement instanceGeometryElement = geometryInstance.GetSymbolGeometry();

                        foreach (GeometryObject o in instanceGeometryElement)
                        {
                            if (o.GetType() == typeof(GeometryInstance))
                            {
                                GeometryInstance gi = o as GeometryInstance;
                                if (gi.Symbol.GetType() == typeof(CADLinkType))
                                {
                                    FamilyInstance familyInstance = element as FamilyInstance;
                                    familyDocsWithImportInstances.Add(dbDoc.EditFamily(familyInstance.Symbol.Family));
                                }
                            }
                        }
                    }
                }
            }
            return familyDocsWithImportInstances;
        }
    }
}
