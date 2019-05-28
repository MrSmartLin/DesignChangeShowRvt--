using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Diagnostics;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using System.IO;

namespace DesignChangeShowRvt
{
    [TransactionAttribute(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]
    
    public class Command : IExternalCommand
    {
        public List<Element> winSelectElements { get; set; }
        MainWindow mainWin = null;

        

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {

            mainWin = new MainWindow();
            IntPtr rvtPtr = Process.GetCurrentProcess().MainWindowHandle;
            WindowInteropHelper helper = new WindowInteropHelper(mainWin);
            helper.Owner = rvtPtr;

            pageExtract page = new pageExtract(mainWin.txtMainSelectEles.Text,mainWin);
            mainWin.page = page;

            mainWin.Show();


            

            return Result.Succeeded;
        }
    }

    public class ExternalCommand : IExternalEventHandler
    {
        public void Execute(UIApplication app)
        {
            UIDocument uiDoc = app.ActiveUIDocument;
            Document revitDoc = uiDoc.Document;
            Selection s1 = uiDoc.Selection;
            IList<Reference> refs = s1.PickObjects(ObjectType.Element);


            List<Element> eles = new List<Element>();



            foreach (Reference item in refs)
            {
                eles.Add(revitDoc.GetElement(item));
            }


            string elesStr = "";
            foreach (Element item in eles)
            {
                elesStr += item.Name + " ID:" + item.Id + "\n";
            }

            File.WriteAllText(@"C:\selectElementIds.txt", elesStr);

            

        }

        public string GetName()
        {
            return "显示录信息窗口";
        }
    }
}
