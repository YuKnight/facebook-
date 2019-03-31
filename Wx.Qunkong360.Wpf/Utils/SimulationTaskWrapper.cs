using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
using Xzy.EmbeddedApp.Model;
using Xzy.EmbeddedApp.Utils;
using Xzy.EmbeddedApp.WinForm.Socket;

namespace Wx.Qunkong360.Wpf.Utils
{
    public static class SimulationTaskWrapper
    {
        public const int MaxTryTimes = 10;
        public const uint TimeoutMilliseconds = 10000;

        public static Task<bool> FuzzySearch(int mobileIndex, string id)
        {
            int taskNum = Guid.NewGuid().GetHashCode();

            var task = new SimulationTask<bool>(taskNum, TimeoutMilliseconds);

            if (SimulationTaskManager.RegisterSingle(task))
            {
                var obj = new JObject()
                {
                    {"tasktype",(int)TaskType.FuzzySearch },
                    {"txtmsg","" },
                };

                var list = new JArray();
                list.Add($"{id}");

                obj.Add("list", list);

                SocketServer.SendTaskInstruct(mobileIndex, (int)TaskType.FuzzySearch, taskNum, obj.ToString(Newtonsoft.Json.Formatting.None));

                return task.Task as Task<bool>;
            }

            return Task.FromResult(false);
        }

        public static Task<bool> Search(int mobileIndex, string id)
        {
            int taskNum = Guid.NewGuid().GetHashCode();

            var task = new SimulationTask<bool>(taskNum, TimeoutMilliseconds);

            if (SimulationTaskManager.RegisterSingle(task))
            {
                var obj = new JObject()
                {
                    {"tasktype",(int)TaskType.ExactSearch },
                    {"txtmsg","" },
                };

                var list = new JArray();
                list.Add($"{id}");

                obj.Add("list", list);

                SocketServer.SendTaskInstruct(mobileIndex, (int)TaskType.ExactSearch, taskNum, obj.ToString(Newtonsoft.Json.Formatting.None));

                return task.Task as Task<bool>;
            }

            return Task.FromResult(false);
        }

        public static Task<bool> SearchClick(int mobileIndex, string id, int targetIndex = 0)
        {
            int taskNum = Guid.NewGuid().GetHashCode();

            var task = new SimulationTask<bool>(taskNum, TimeoutMilliseconds);

            if (SimulationTaskManager.RegisterSingle(task))
            {
                var obj = new JObject()
                {
                    {"tasktype",(int)TaskType.ExactSearchAndClick },
                    {"txtmsg","" },
                };

                var list = new JArray();
                list.Add($"{id}");
                list.Add($"{targetIndex}");

                obj.Add("list", list);

                SocketServer.SendTaskInstruct(mobileIndex, (int)TaskType.ExactSearchAndClick, taskNum, obj.ToString(Newtonsoft.Json.Formatting.None));

                return task.Task as Task<bool>;
            }

            return Task.FromResult(false);
        }

        public static Task<bool> FuzzySearchClick(int mobileIndex, string id, int targetIndex = 0)
        {
            int taskNum = Guid.NewGuid().GetHashCode();

            var task = new SimulationTask<bool>(taskNum, TimeoutMilliseconds);

            if (SimulationTaskManager.RegisterSingle(task))
            {
                var obj = new JObject()
                {
                    {"tasktype",(int)TaskType.FuzzySearchAndClick },
                    {"txtmsg","" },
                };

                var list = new JArray();
                list.Add($"{id}");
                list.Add($"{targetIndex}");

                obj.Add("list", list);

                SocketServer.SendTaskInstruct(mobileIndex, (int)TaskType.FuzzySearchAndClick, taskNum, obj.ToString(Newtonsoft.Json.Formatting.None));

                return task.Task as Task<bool>;
            }

            return Task.FromResult(false);
        }

        public static Task<bool> SearchClickParent(int mobileIndex, string id, int parentDepth, int targetChildIndex = 0)
        {
            int taskNum = Guid.NewGuid().GetHashCode();

            var task = new SimulationTask<bool>(taskNum, TimeoutMilliseconds);

            if (SimulationTaskManager.RegisterSingle(task))
            {
                var obj = new JObject()
                {
                    {"tasktype",(int)TaskType.ExactSearchAndClickParent },
                    {"txtmsg","" },
                };

                var list = new JArray();
                list.Add($"{id}");
                list.Add($"{targetChildIndex}");
                list.Add($"{parentDepth}");

                obj.Add("list", list);

                SocketServer.SendTaskInstruct(mobileIndex, (int)TaskType.ExactSearchAndClickParent, taskNum, obj.ToString(Newtonsoft.Json.Formatting.None));

                return task.Task as Task<bool>;
            }

            return Task.FromResult(false);
        }

        public static Task<bool> SearchPaste(int mobileIndex, string id, int targetIndex = 0)
        {
            int taskNum = Guid.NewGuid().GetHashCode();
            
            var task = new SimulationTask<bool>(taskNum, TimeoutMilliseconds);

            if (SimulationTaskManager.RegisterSingle(task))
            {
                var obj = new JObject()
                {
                    { "tasktype",(int)TaskType.ExactSearchAndPaste },
                    {"txtmsg","" },
                };

                var list = new JArray();
                list.Add($"{id}");
                list.Add($"{targetIndex}");

                obj.Add("list", list);

                SocketServer.SendTaskInstruct(mobileIndex, (int)TaskType.ExactSearchAndPaste, taskNum, obj.ToString(Newtonsoft.Json.Formatting.None));

                return task.Task as Task<bool>;
            }

            return Task.FromResult(false);
        }


        public static Task<bool> Assign(int mobileIndex, string id, string value, int targetIndex = 0)
        {
            int taskNum = Guid.NewGuid().GetHashCode();

            var task = new SimulationTask<bool>(taskNum, TimeoutMilliseconds);

            if (SimulationTaskManager.RegisterSingle(task))
            {
                var obj = new JObject()
                {
                    {"tasktype",(int)TaskType.ExactSearchAndAssign},
                    {"txtmsg","" },
                };

                var list = new JArray();
                list.Add($"{id}");
                list.Add($"{value}");
                list.Add($"{targetIndex}");

                obj.Add("list", list);

                SocketServer.SendTaskInstruct(mobileIndex, (int)TaskType.ExactSearchAndAssign, taskNum, obj.ToString(Newtonsoft.Json.Formatting.None));

                return task.Task as Task<bool>;
            }

            return Task.FromResult(false);

        }

        public static Task<bool> Clear(int mobileIndex, string id, int targetIndex = 0)
        {
            int taskNum = Guid.NewGuid().GetHashCode();

            var task = new SimulationTask<bool>(taskNum, TimeoutMilliseconds);

            if (SimulationTaskManager.RegisterSingle(task))
            {
                var obj = new JObject()
                {
                    {"tasktype",(int)TaskType.ExactSearchAndClear },
                    {"txtmsg","" },
                };

                var list = new JArray();
                list.Add($"{id}");
                list.Add($"{targetIndex}");

                obj.Add("list", list);

                SocketServer.SendTaskInstruct(mobileIndex, (int)TaskType.ExactSearchAndClear, taskNum, obj.ToString(Newtonsoft.Json.Formatting.None));

                return task.Task as Task<bool>;
            }

            return Task.FromResult(false);

        }

        /// <summary>
        /// 点击坐标
        /// </summary>
        /// <param name="mobileIndex"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static Task<bool> ClickCoordinate(int mobileIndex, int x, int y)
        {
            int taskNum = Guid.NewGuid().GetHashCode();

            var task = new SimulationTask<bool>(taskNum, TimeoutMilliseconds);

            if (SimulationTaskManager.RegisterSingle(task))
            {
                var obj = new JObject()
                {
                    {"tasktype",(int)TaskType.ClickCoordinate },
                    {"txtmsg","" },
                };

                var list = new JArray();
                list.Add($"{x}");
                list.Add($"{y}");

                obj.Add("list", list);

                SocketServer.SendTaskInstruct(mobileIndex, (int)TaskType.ClickCoordinate, taskNum, obj.ToString(Newtonsoft.Json.Formatting.None));

                return task.Task as Task<bool>;
            }

            return Task.FromResult(false);
        }

        public static Task<bool> Swipe(int mobileIndex, int x1, int y1, int x2, int y2, int steps)
        {
            string mobileId = DeviceConnectionManager.Instance.GetDeviceNameByMobileIndex(mobileIndex);

            return Task.Run(() =>
           {
               ProcessUtils.AdbSwipe(mobileId, x1, y1, x2, y2);

               return true;
           });

            #region uitest implementation
            //int taskNum = Guid.NewGuid().GetHashCode();

            //var task = new SimulationTask<bool>(taskNum, TimeoutMilliseconds);

            //if (SimulationTaskManager.RegisterSingle(task))
            //{
            //    var obj = new JObject()
            //    {
            //        {"tasktype",(int)TaskType.Swipe },
            //        {"txtmsg","" },
            //    };

            //    var list = new JArray();
            //    list.Add($"{x1}");
            //    list.Add($"{y1}");
            //    list.Add($"{x2}");
            //    list.Add($"{y2}");
            //    list.Add($"{steps}");

            //    obj.Add("list", list);

            //    SocketServer.SendTaskInstruct(mobileIndex, (int)TaskType.Swipe, taskNum, obj.ToString(Newtonsoft.Json.Formatting.None));

            //    return task.Task as Task<bool>;
            //}

            //return Task.FromResult(false); 
            #endregion
        }

        public static Task<bool> GoBack(int mobileIndex)
        {
            int taskNum = Guid.NewGuid().GetHashCode();

            var task = new SimulationTask<bool>(taskNum, TimeoutMilliseconds);

            if (SimulationTaskManager.RegisterSingle(task))
            {
                var obj = new JObject()
                {
                    {"tasktype",(int)TaskType.GoBack },
                    {"txtmsg","" },
                };

                var list = new JArray();

                obj.Add("list", list);

                SocketServer.SendTaskInstruct(mobileIndex, (int)TaskType.GoBack, taskNum, obj.ToString(Newtonsoft.Json.Formatting.None));

                return task.Task as Task<bool>;
            }

            return Task.FromResult(false);

        }

        public static async Task<bool> GotoHomepage(int mobileIndex)
        {
            int triedTimes = 0;

            string camera = SystemLanguageManager.Instance.ResourceManager.GetString("Camera", SystemLanguageManager.Instance.CultureInfo);
            string stories = SystemLanguageManager.Instance.ResourceManager.GetString("Stories", SystemLanguageManager.Instance.CultureInfo);
            string photo = SystemLanguageManager.Instance.ResourceManager.GetString("Photo", SystemLanguageManager.Instance.CultureInfo);

            bool result = false;

            while (true)
            {
                await Task.Delay(1000);

                await Swipe(mobileIndex, 150, 200, 150, 250, 20);

                await Task.Delay(1000);

                if (await Search(mobileIndex, camera))
                {
                    result = true;
                    break;
                }
                else if (await Search(mobileIndex, stories))
                {
                    result = true;
                    break;
                }
                else if (await Search(mobileIndex, photo))
                {
                    result = true;
                    break;
                }
                else
                {
                    await GoBack(mobileIndex);
                }

                triedTimes++;
                if (triedTimes > MaxTryTimes)
                {
                    break;
                }
            }

            return result;
        }

        public static Task<bool> DeletePictures(int mobileIndex, string[] pictures)
        {
            if (pictures == null || pictures.Length == 0)
            {
                return Task.FromResult(false);
            }

            int taskNum = Guid.NewGuid().GetHashCode();

            var task = new SimulationTask<bool>(taskNum, TimeoutMilliseconds * 2);

            if (SimulationTaskManager.RegisterSingle(task))
            {
                var obj = new JObject()
                {
                    { "tasktype",(int)TaskType.DeletePictures},
                    { "txtmsg",""},
                };

                var list = new JArray();

                foreach (var picture in pictures)
                {
                    list.Add(picture);
                }

                obj.Add("list", list);

                SocketServer.SendTaskInstruct(mobileIndex, (int)TaskType.DeletePictures, taskNum, obj.ToString(Newtonsoft.Json.Formatting.None));

                return task.Task as Task<bool>;
            }

            return Task.FromResult(false);
        }

        public static Task<bool> UpdatePictures(int mobileIndex, string[] pictures)
        {
            if (pictures == null || pictures.Length == 0)
            {
                return Task.FromResult(false);
            }

            int taskNum = Guid.NewGuid().GetHashCode();

            var task = new SimulationTask<bool>(taskNum, TimeoutMilliseconds * 2);

            if (SimulationTaskManager.RegisterSingle(task))
            {
                var obj = new JObject()
                {
                    { "tasktype",(int)TaskType.UpdatePictures},
                    { "txtmsg",""},
                };

                var list = new JArray();

                foreach (var picture in pictures)
                {
                    list.Add(picture);
                }

                obj.Add("list", list);

                SocketServer.SendTaskInstruct(mobileIndex, (int)TaskType.UpdatePictures, taskNum, obj.ToString(Newtonsoft.Json.Formatting.None));

                return task.Task as Task<bool>;
            }

            return Task.FromResult(false);
        }
    }
}
