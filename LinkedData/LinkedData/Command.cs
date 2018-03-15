#region Namespaces
using System;
using System.Diagnostics;
using System.Collections.Generic;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
#endregion

namespace ListLinks
{
    /// <summary>
    /// External command to list all DWG and RVT 
    /// links of a given document.
    /// </summary>
    [Transaction(TransactionMode.ReadOnly)]
    public class Command : IExternalCommand
    {
        #region Formatting
        /// <summary>
        /// Return an English plural suffix 's' or
        /// nothing for the given number of items.
        /// </summary>
        static string PluralSuffix(int n)
        {
            return 1 == n ? "" : "s";
        }

        /// <summary>
        /// Return a dot for zero items, or a colon for more.
        /// </summary>
        static string DotOrColon(int n)
        {
            return 0 < n ? ":" : ".";
        }
        #endregion // Formatting

        #region RevitAPI.chm sample code
        // sample code from RevitAPI.chm entry 
        // on the TransmissionData class:

        /// <summary>
        /// Unload all Revit links.
        /// This method will set all Revit links to be 
        /// unloaded the next time the document at the 
        /// given location is opened. 
        /// The TransmissionData for a given document 
        /// only contains top-level Revit links, not 
        /// nested links. 
        /// However, nested links will be unloaded if 
        /// their parent links are unloaded, so this 
        /// function only needs to look at the 
        /// document's immediate links. 
        /// </summary>
        void UnloadRevitLinks(ModelPath location)
        {
            // access transmission data in the given Revit file

            TransmissionData transData = TransmissionData
              .ReadTransmissionData(location);

            if (transData != null)
            {
                // collect all (immediate) external references in the model

                ICollection<ElementId> externalReferences
                  = transData.GetAllExternalFileReferenceIds();

                // find every reference that is a link

                foreach (ElementId refId in externalReferences)
                {
                    ExternalFileReference extRef
                      = transData.GetLastSavedReferenceData(refId);

                    if (extRef.ExternalFileReferenceType
                      == ExternalFileReferenceType.RevitLink)
                    {
                        // we do not want to change neither the 
                        // path nor the path-type; we only want 
                        // the links to be unloaded (shouldLoad 
                        // = false)

                        transData.SetDesiredReferenceData(refId,
                          extRef.GetPath(), extRef.PathType, false);
                    }
                }

                // make sure the IsTransmitted property is set 

                transData.IsTransmitted = true;

                // modified transmission data must be saved back to the model

                TransmissionData.WriteTransmissionData(
                  location, transData);
            }
            else
            {
                TaskDialog.Show(
                  "Unload Links",
                  "The document does not have any transmission data");
            }
        }
        #endregion // RevitAPI.chm sample code

        /// <summary>
        /// List all DWG, RVT and other links of a given document.
        /// </summary>
        void ListLinks(ModelPath location)
        {
            string path = ModelPathUtils
              .ConvertModelPathToUserVisiblePath(location);

            string content = string.Format(
              "The document at '{0}' ",
              path);

            List<string> links = null;

            // access transmission data in the given Revit file

            TransmissionData transData = TransmissionData
              .ReadTransmissionData(location);

            if (transData == null)
            {
                content += "does not have any transmission data";
            }
            else
            {
                // collect all (immediate) external references in the model

                ICollection<ElementId> externalReferences
                  = transData.GetAllExternalFileReferenceIds();

                int n = externalReferences.Count;

                content += string.Format(
                  "has {0} external reference{1}{2}",
                  n, PluralSuffix(n), DotOrColon(n));

                links = new List<string>(n);

                // find every reference that is a link

                foreach (ElementId refId in externalReferences)
                {
                    ExternalFileReference extRef
                      = transData.GetLastSavedReferenceData(refId);

                    links.Add(string.Format("{0} {1}",
                      extRef.ExternalFileReferenceType,
                      ModelPathUtils.ConvertModelPathToUserVisiblePath(
                        extRef.GetPath())));
                }
            }
            Debug.Print(content);

            TaskDialog dlg = new TaskDialog("List Links");

            dlg.MainInstruction = content;

            if (null != links && 0 < links.Count)
            {
                string s = string.Join("  \r\n",
                  links.ToArray());

                Debug.Print(s);

                dlg.MainContent = s;
            }
            dlg.Show();
        }

        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            View view = commandData.View;

            if (null == view)
            {
                message = "Please run this command in an active document.";
                return Result.Failed;
            }
            else
            {
                Document doc = view.Document;

                ModelPath modelPath = ModelPathUtils
                  .ConvertUserVisiblePathToModelPath(
                    doc.PathName);

                ListLinks(modelPath);

                return Result.Succeeded;
            }
        }
    }
}
