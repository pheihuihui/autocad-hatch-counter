using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;

[assembly: CommandClass(typeof(HatchCounter.Counter))]
namespace HatchCounter
{
    public class Counter
    {
        [CommandMethod("hello")]
        public void HelloCAD()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor edt = doc.Editor;
            edt.WriteMessage("hello cad");
        }

        [CommandMethod("CountHatches")]
        public void CountHatches()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            var selected = ed.GetSelection().Value;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                var blockTable = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                var blockTableRecord = (BlockTableRecord)tr.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForRead);
                var hatches = GetAllHatches(blockTableRecord, tr);
                foreach(var ht in hatches)
                {
                    if(selected != null)
                    {
                        var selectedids = selected.GetObjectIds();
                        if (selectedids.Length > 0 && selectedids.Contains(ht.ObjectId))
                        {
                            ed.WriteMessage(ht.PatternName);
                            ed.WriteMessage(": ");
                            ed.WriteMessage(ht.NumberOfLoops.ToString());
                            ed.WriteMessage(Environment.NewLine);
                            ed.WriteMessage(Environment.NewLine);
                        }
                    }
                }
            }
        }

        public IEnumerable<Hatch> GetAllHatches(BlockTableRecord btr, Transaction tr)
        {
            return btr.Cast<ObjectId>()
               .Select(id => tr.GetObject(id, OpenMode.ForRead))
               .OfType<Hatch>();
               //.Select(hatch => hatch.Area)
               //.Sum();
        }

        [CommandMethod("ListAllBlocks")]
        public void ListAllBlocks()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                var blockTable = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                var blockTableRecord = (BlockTableRecord)tr.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForRead);

                foreach (ObjectId id in blockTableRecord)
                {
                    if (id.ObjectClass.DxfName == "INSERT")
                    {
                        var blockReference = (BlockReference)tr.GetObject(id, OpenMode.ForRead);
                        ed.WriteMessage("\n" + blockReference.Name);
                    }
                }
                
                tr.Commit();
            }
        }
    }
}
