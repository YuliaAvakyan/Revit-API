#region Namespaces
using System;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
#endregion

namespace PickObject
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

            using (Transaction tx = new Transaction(doc))
            {
                tx.Start("Start");

                PickPoint(uidoc, app);

            tx.Commit();
        }

            return Result.Succeeded;
               
        }


        public void PickPoint(UIDocument uidoc, Application app) {
            View activeView = uidoc.ActiveView;
            SketchPlane sketch = activeView.SketchPlane;
            
            ObjectSnapTypes snapTypes = ObjectSnapTypes.Points | ObjectSnapTypes.Nearest | ObjectSnapTypes.Perpendicular;
            XYZ startPoint;
            XYZ endPoint;
            
                Plane geometryPlane = Plane.CreateByNormalAndOrigin(XYZ.BasisZ, XYZ.Zero);
                sketch = SketchPlane.Create(uidoc.Document, geometryPlane);

                uidoc.Document.ActiveView.SketchPlane = sketch;
                uidoc.Document.ActiveView.ShowActiveWorkPlane();

            try //Выбор точек
            {
                startPoint = uidoc.Selection.PickPoint(snapTypes, "Select start point");
                endPoint = uidoc.Selection.PickPoint(snapTypes, "Select end point");
                
            }

            catch(Autodesk.Revit.Exceptions.OperationCanceledException oc)
            {
                Console.WriteLine(oc.Message);
                return;
            }
            catch(Autodesk.Revit.Exceptions.InvalidOperationException oe)
            {
                Console.WriteLine(oe.Message);
                TaskDialog.Show("Revit", "No work plane set in current view.");
                return;

            }
            catch (Autodesk.Revit.Exceptions.ArgumentNullException n)
            {
                Console.WriteLine(n.Message);
                return;
            } 

            double dist = startPoint.DistanceTo(endPoint);

            string distance = "Distance is " + dist.ToString();
            string strCoords = "Selected start point is " + startPoint.ToString() + "\nSelected end point is " + endPoint.ToString() + distance;
            Line line = Line.CreateBound(startPoint, endPoint);
            CreateLinearDimension1(uidoc.Document, startPoint, endPoint, sketch, app);
           // TaskDialog.Show("Revit", strCoords);
        
        }
        
        public Dimension CreateLinearDimension1(
       Document doc, XYZ pt1, XYZ pt2, SketchPlane sketch, Application app)
        {
            // first create line

            Line line = Line.CreateBound(pt1, pt2);
            ModelCurve modelcurve = doc.Create
            .NewModelCurve(line, sketch);

            ReferenceArray ra = new ReferenceArray();
            ra.Append(modelcurve.GeometryCurve.GetEndPointReference(0));
            ra.Append(modelcurve.GeometryCurve.GetEndPointReference(1));
            return doc.Create.NewDimension(doc.ActiveView, line, ra);

        }    



    }

}
