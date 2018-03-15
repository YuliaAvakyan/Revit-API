#region Namespaces
using System;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
#endregion

namespace Select3DPoint
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


        public void PickPoint(UIDocument uidoc, Application app)
        {
            
            XYZ startPoint = null;
            XYZ endPoint = null;

            try
            {
                PlanarFace face1 = SelectPlanarFace(uidoc);
                PlanarFace face2;
                //Выбор плоскости
                if (face1 != null)
                {
                    startPoint = CreatePoint(uidoc, face1);                    
                }

                face2 = SelectPlanarFace(uidoc);

                if (face2 != null)
                {
                    endPoint = CreatePoint(uidoc, face2);
                }
                
               /* double dist = startPoint.DistanceTo(endPoint);

                    string distance = "Distance is " + dist.ToString(); //Расстояние между точками
                    string strCoords = "Selected start point is " + startPoint.ToString() + "\nSelected end point is " + endPoint.ToString() + distance;
                    TaskDialog.Show("Revit", strCoords); //Вывод координат и расстояния
                    */
               /* double dist3D = Math.Sqrt(Math.Pow((endPoint.X - startPoint.X), 2) + Math.Pow((endPoint.Y - startPoint.Y), 2) + Math.Pow((endPoint.Z - startPoint.Z), 2));

                string distance3D = "Distance is " + dist3D.ToString();
                TaskDialog.Show("Revit", distance3D); */

                XYZ direction = startPoint - endPoint;

                double distance = direction.Normalize().DotProduct(startPoint.Subtract(endPoint));
                distance = UnitUtils.ConvertFromInternalUnits(distance, DisplayUnitType.DUT_METERS); //make sure it is m
                distance *= 1000;
                TaskDialog.Show("Revit", distance.ToString());

                //Line line = Line.CreateBound(startPoint, endPoint);
                //CreateLinearDimension1(uidoc.Document, startPoint, endPoint, sp, app); //Создание Dimension


            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return;
            }

        }

        public Dimension CreateLinearDimension1(Document doc, XYZ pt1, XYZ pt2, SketchPlane sketch, Application app)
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


        public XYZ CreatePoint(UIDocument uidoc, PlanarFace face)
        {
            ObjectSnapTypes snapTypes = ObjectSnapTypes.Points | ObjectSnapTypes.Nearest | ObjectSnapTypes.Perpendicular;
            XYZ point = null;
            Plane plane = Plane.CreateByNormalAndOrigin(
                          face.FaceNormal, face.Origin); // 2017
           
            SketchPlane sp = SketchPlane.Create(uidoc.Document, plane); // 2014

            uidoc.ActiveView.SketchPlane = sp;

            //Выбор точек
            try
            {
                point = uidoc.Selection.PickPoint(snapTypes, "Select start point");
                //return point;
            }

            catch (Autodesk.Revit.Exceptions.OperationCanceledException oc)
            {
                Console.WriteLine(oc.Message);
                //return;
            }
            catch (Autodesk.Revit.Exceptions.InvalidOperationException oe)
            {
                Console.WriteLine(oe.Message);
                TaskDialog.Show("Revit", "No work plane set in current view.");
               // return;

            }
            catch (Autodesk.Revit.Exceptions.ArgumentNullException n)
            {
                Console.WriteLine(n.Message);
               // return;
            }
            return point;
        }

        public PlanarFace SelectPlanarFace(UIDocument uidoc)
        {
            PlanarFace face = null;
            Reference r = uidoc.Selection.PickObject(
                 ObjectType.Face,
                 "Please select a planar face to define work plane");

            Element e = uidoc.Document.GetElement(r.ElementId);

            if (e != null)
            {
                face = e.GetGeometryObjectFromReference(r) as PlanarFace;
                //return face;
            }
            return face;
        }
    }

}
