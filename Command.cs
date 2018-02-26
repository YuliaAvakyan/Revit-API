#region Namespaces
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB.PointClouds;
#endregion

namespace PointCloudHighlights
{
    [Transaction(TransactionMode.Manual)]
    public class Command : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;

            // Access current selection

            /*   Selection sel = uidoc.Selection;

               // Retrieve elements from database

               FilteredElementCollector col
                 = new FilteredElementCollector(doc)
                   .WhereElementIsNotElementType()
                   .OfCategory(BuiltInCategory.INVALID)
                   .OfClass(typeof(Wall));

               // Filtered element collector is iterable

               foreach (Element e in col)
               {
                   Debug.Print(e.Name);
               }

               // Modify document within a transaction

               using (Transaction tx = new Transaction(doc))
               {
                   tx.Start("Transaction Name");
                   tx.Commit();
               }*/
            using (Transaction tx = new Transaction(doc))
            {
                tx.Start("Transaction Name");
                PointCloudInstance pcInst = CreatePointCloud(doc);

         /*   var coor = pcInst.GetScans();

            Debug.Print(coor.Count.ToString());

            PointCloudFilter pcf;*/

            return Result.Succeeded;
                tx.Commit();
            }
        }

        private PointCloudInstance CreatePointCloud(Document doc)
        {
            PointCloudType type = PointCloudType.Create(doc, "rcs", "D:\\trimetari\\Sochi\\pts\\roof.rcs");
            return (PointCloudInstance.Create(doc, type.Id, Transform.Identity));
        }


        /*
                private PointCloudInstance CreatePointCloud(Document doc)
                {
                    PointCloudType type = PointCloudType.Create(doc, "rcs", "D:\\trimetari\\Sochi\\pts\\roof.rcs");
                    return (PointCloudInstance.Create(doc, type.Id, Transform.Identity));
                }

                private void GetPointCloudDataByIteration(PointCloudInstance pcInstance, PointCloudFilter pointCloudFilter)
                {
                    // read points by iteration
                    double averageDistance = 0.001;
                    PointCollection points = pcInstance.GetPoints(pointCloudFilter, averageDistance, 10000); // Get points.  Number of points is determined by the needs of the client
                    foreach (CloudPoint point in points)
                    {
                        // Process each point
                        System.Drawing.Color color = System.Drawing.ColorTranslator.FromWin32(point.Color);
                        String pointDescription = String.Format("({0}, {1}, {2}, {3}", point.X, point.Y, point.Z, color.ToString());
                    }
                }
                */

    }
}
