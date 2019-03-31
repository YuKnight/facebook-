using Cj.AppEmbeddedApp.DAL;
using Xzy.EmbeddedApp.Model;

namespace Cj.EmbeddedAPP.BLL
{
    public class TraceBLL
    {
        public static int CreateTaskTrace(TaskTrace taskTrace)
        {
            int result = 0;
            result = TraceDAL.InsertTaskTrace(taskTrace);

            return result;
        }

        public static int CountTaskTrace(TaskTrace taskTrace)
        {
            int result = 0;
            result = TraceDAL.CountTaskTrace(taskTrace);

            return result;
        }

        public static int UpdateTaskTrace(TaskTrace taskTrace)
        {
            int result = 0;
            result = TraceDAL.UpdateTaskTrace(taskTrace);

            return result;
        }

        public static int GetTaskTracePosition(TaskTrace taskTrace)
        {
            int position = 0;

            position = TraceDAL.GetTaskTracePosition(taskTrace);
            return position;
        }
    }
}
