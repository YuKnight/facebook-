using Cj.AppEmbeddedApp.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xzy.EmbeddedApp.Model;

namespace Cj.EmbeddedAPP.BLL
{
    public class RecordStageBLL
    {
        private static RecordStageDAL recorddal = new RecordStageDAL();

        public static IList<RecordStage> GetRecordstages(int mobileindex, int typeid)
        {
            return recorddal.GetRecordstages(mobileindex, typeid);
        }
    }
}
