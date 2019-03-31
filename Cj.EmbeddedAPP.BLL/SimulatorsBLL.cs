using Cj.AppEmbeddedApp.DAL;
using System.Collections.Generic;
using Xzy.EmbeddedApp.Model;

namespace Cj.EmbeddedAPP.BLL
{
    public class SimulatorsBLL
    {
        private static SimulatorsDAL objSimulatorsDAL = new SimulatorsDAL();

        /// <summary>
        /// 查询模拟器号码
        /// </summary>
        /// <returns></returns>
        public static List<Simulators> GetSimulatorsList()
        {
            return objSimulatorsDAL.GetSimulatorsList();
        }

        /// <summary>
        /// 添加模拟器号码
        /// </summary>
        /// <param name="objSimulators"></param>
        /// <returns></returns>
        public int UpdateSimulators(Simulators objSimulators)
        {
            return objSimulatorsDAL.UpdateSimulators(objSimulators);
        }
    }
}
