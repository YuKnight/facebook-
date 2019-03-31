using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
//using System.Windows.Forms;
using Cj.EmbeddedAPP.BLL;
using Newtonsoft.Json.Linq;
using Wx.Qunkong360.Wpf.Utils;
using Wx.Qunkong360.Wpf.Views;
using Xzy.EmbeddedApp.Model;
using Xzy.EmbeddedApp.Utils;
using Xzy.EmbeddedApp.WinForm.Socket;

namespace Xzy.EmbeddedApp.WinForm.Tasks
{
    public class TasksSchedule
    {
        public static ConcurrentDictionary<int,Timer> AllTimersKey = new ConcurrentDictionary<int, Timer>();
        public static Dictionary<int, int> timerstatus=new Dictionary<int, int>();

        /// <summary>
        /// 创建对应的timer个数
        /// </summary>
        /// <param name="maxnums"></param>
        public static bool StartTimer(int mobileindex,int iswhole)
        {
            Console.WriteLine("准备启动Timer"+ mobileindex);            
            List<int> mobilelist = new List<int>();
            mobilelist.Add(mobileindex);
            mobilelist.Add(iswhole);

            int isexist = 0;
            try
            {
                //已经存在则不创建
                if(!AllTimersKey.IsNull() && AllTimersKey.Count > 0)
                {
                    foreach(var item in AllTimersKey)
                    {
                        if(item.Key == mobileindex)
                        {
                            isexist = 1;
                        }
                    }
                }

                if (isexist == 0)
                {
                    TimerCallback timerTask = new TimerCallback(Timer_Elapsed);
                    //Timer timer = new Timer(timerTask, mobileindex, 30000, Timeout.Infinite);
                    Timer timer = new Timer(timerTask, mobilelist, 5000, Timeout.Infinite);

                    if (timerstatus.ContainsKey(mobileindex))
                    {
                        timerstatus[mobileindex] = 1;
                    }
                    else
                    {
                        timerstatus.Add(mobileindex, 1);
                    }

                    Console.WriteLine("Timer—{0}创建成功", mobileindex);
                    AllTimersKey.TryAdd(mobileindex, timer);
                }
                else
                {
                    //运行中直接启动
                    if(timerstatus.ContainsKey(mobileindex))
                    {
                        if(timerstatus[mobileindex]==0) //挂起状态
                        {
                            foreach (var item in AllTimersKey)
                            {
                                if (item.Key == mobileindex)
                                {
                                    item.Value.Change(3000, Timeout.Infinite);
                                }
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                LogUtils.Information(string.Format("任务执行Timer异常_{0}；信息：{1}",mobileindex,ex.Message));
                Console.WriteLine("Timer—{0}启动失败上", mobileindex);
                return false;
            }

            return true;
        }

        public static async Task<int> ProessTask()
        {
            ConfigVals.IsRunning = 1;
            int res = 0;
            //执行普通任务
            int resnum = TasksBLL.CountByStatus("waiting",0);
            int intval = 0;
            if (resnum > 0)
            {
                intval++;
                List<TaskSch> taskindex = TasksBLL.GetTasksForSendGroup("waiting");
                if (taskindex.Count > 0)
                {                    
                    for (int t = 0; t < taskindex.Count; t++)
                    {
                        //TimerCallback timerTask = new TimerCallback(Timer_Elapsed);
                        StartTimer(taskindex[t].MobileIndex,0);
                        await Task.Delay(2000);
                    }
                }                
            }

            //执行全局任务
            int resnum2= TasksBLL.CountByStatus("waiting", 1);
            intval = 0;
            if (resnum2 > 0)
            {
                intval++;
                List<TaskSch> taskindex = TasksBLL.GetTasksForSendGroup("waiting");
                if (taskindex.Count > 0)
                {
                    for (int t = 0; t < taskindex.Count; t++)
                    {
                        StartTimer(taskindex[t].MobileIndex, 1);
                        await Task.Delay(2000);
                    }
                }
            }

            ConfigVals.IsRunning = 0;
            return res;
        }

        /// <summary>
        /// timer的回调处理
        /// </summary>
        /// <param name="state"></param>
        public static async void Timer_Elapsed(object state)
        {
            await Task.Delay(2000);
            //int mobileIndex0 = Int32.Parse(state.ToString());
            List<int> mobilelist = (List<int>)state;
            int mobileIndex0 = mobilelist[0];
            int iswhole= mobilelist[1]; //iswhole=1:全局任务
            /*if (timerstatus.ContainsKey(mobileIndex0))
            {
                timerstatus[mobileIndex0] = 1;
            }
            else
            {
                timerstatus.Add(mobileIndex0, 1);
            }*/
            
            int resnum = TasksBLL.CountByStatus("waiting", mobileIndex0, iswhole);
            Random r = new Random();
            while (resnum > 0)
            {
                List<TaskSch> list = TasksBLL.GetTasksList("waiting", mobileIndex0, 1, 10);
                if (list != null && list.Count > 0)
                {
                    foreach (var item in list)
                    {
                        string mobileIndex = "";
                        mobileIndex = item.MobileIndex.ToString();  //需要匹配出对应的客户端标识

                        int flag = -1;
                        //bool flag2 = false;
                        if (item.IsStep == 0)
                        {
                            for (int i = 0; i < item.RepeatNums; i++)
                            {
                                flag = SocketServer.SendTaskInstruct(Int32.Parse(mobileIndex), item.TypeId, item.Id, item.Bodys);
                                int waittime = r.Next(item.RandomMins, item.RandomMaxs);
                                Thread.Sleep(waittime);
                            }
                        }
                        else
                        {
                            if (item.TypeId == 22)
                            {
                                var flag2 = await AddRecommFriend(Int32.Parse(mobileIndex), item.RepeatNums);
                                if (flag2)
                                {
                                    flag = 1;
                                }
                            }
                            else if (item.TypeId == 23)
                            {
                                var flag2 = await AllowRequestFriend(Int32.Parse(mobileIndex), item.RepeatNums);
                                if (flag2)
                                {
                                    flag = 1;
                                }
                            }
                            else if (item.TypeId == 101)
                            {
                                var flag2 = await AddListPhoneFriend(Int32.Parse(mobileIndex), item.RepeatNums);
                                if (flag2)
                                {
                                    flag = 1;
                                }
                            }
                            else if (item.TypeId == 21)   //添加主页好友
                            {
                                var flag2 = await AddHomePageFriend(Int32.Parse(mobileIndex), item.RepeatNums, item.InputName);
                                if (flag2)
                                {
                                    flag = 1;
                                }
                            }
                            else if (item.TypeId == 20)
                            {
                                //添加好友的好友
                                var flag2 = await AddFriendByFriend2(Int32.Parse(mobileIndex), item.RepeatNums);
                                if (flag2)
                                {
                                    flag = 1;
                                }
                            }
                            else if (item.TypeId == (int)TaskType.SearchAndAddFriend)
                            {
                                //搜索并添加好友
                                bool result = await SearchAndAddFriendAsync(int.Parse(mobileIndex), item.InputName, item.RepeatNums);
                                flag = result ? 1 : -1;
                            }
                            else if (item.TypeId == (int)TaskType.SearchAndJoinGroup)
                            {
                                //加入小组
                                bool result = await JoinGroupAsync(int.Parse(mobileIndex), item.InputName, item.RepeatNums);
                                flag = result ? 1 : -1;
                            }
                            else if (item.TypeId == (int)TaskType.TimelineLike)
                            {
                                //时间线点赞
                                bool result = await TimelineLike(int.Parse(mobileIndex), item.SlideNums, item.RepeatNums, item.RandomMins, item.RandomMaxs);
                                flag = result ? 1 : -1;
                            }
                            else if (item.TypeId == (int)TaskType.FriendTimelineLike)
                            {
                                //好友时间线点赞
                                bool result = await FriendTimelineLike(int.Parse(mobileIndex), item.SlideNums, item.RepeatNums, item.FriendNums, item.RandomMins, item.RandomMaxs);
                                flag = result ? 1 : -1;
                            }
                            else if (item.TypeId == (int)TaskType.PublishPost)
                            {
                                //发送动态
                                bool result = await PublishPost(int.Parse(mobileIndex), item.Bodys);
                                flag = result ? 1 : -1;
                            }
                            else if (item.TypeId == (int)TaskType.SendHomepage)
                            {
                                //发送主页
                                bool result = await SendHomepageAsync(int.Parse(mobileIndex), item.InputName);
                                flag = result ? 1 : -1;
                            }
                            else if (item.TypeId == (int)TaskType.AttentionHomePage)
                            {
                                //关注主页
                                bool result = await SubscribeHomepageAsync(int.Parse(mobileIndex), item.InputName, item.RepeatNums);
                                flag = result ? 1 : -1;
                            }
                            else if (item.TypeId == (int)TaskType.InvitingFriends)
                            {
                                //邀请好友进小组
                                bool result = await InviteFriendsToGroupAsync(int.Parse(mobileIndex), item.InputName, item.RepeatNums);
                                flag = result ? 1 : -1;
                            }
                            else if (item.TypeId == (int)TaskType.InviteFriendsLike)
                            {
                                //邀请好友点赞
                                bool result = await InviteFriendsToLikeAsync(int.Parse(mobileIndex), item.InputName, item.RepeatNums);
                                flag = result ? 1 : -1;
                            }
                            else if (item.TypeId == (int)TaskType.AddGroupUsers)
                            {
                                //添加小组内的好友
                                bool result = await AddGroupFriends(int.Parse(mobileIndex), item.RepeatNums, item.InputName);
                            }
                            else if (item.TypeId == (int)TaskType.SendFriendsMessage)
                            {
                                bool result = await SendFrinedsMsgLite(int.Parse(mobileIndex), item.Bodys);
                                flag = result ? 1 : -1;
                            }
                            else if (item.TypeId == (int)TaskType.SendGroupMessage)
                            {
                                bool result = await SendGroupMsg(int.Parse(mobileIndex), item.Bodys);
                                flag = result ? 1 : -1;
                            }
                        }

                        TasksBLL.UpdateTaskStatus(item.Id, flag);
                        resnum--;
                    }
                }

                if (resnum <= 0)
                {
                    resnum = TasksBLL.CountByStatus("waiting", mobileIndex0,0);
                    foreach (var item in AllTimersKey)
                    {
                        if (item.Key == mobileIndex0)
                        {
                            item.Value.Change(Timeout.Infinite, Timeout.Infinite);
                        }
                    }
                    timerstatus[mobileIndex0] = 0;
                }
            }
        }                

        /// <summary>
        /// 任务处理
        /// </summary>
        /// <returns></returns>
        public static async Task<int> ProessTask2()
        {
            int res = 0;
            //TasksBLL tasksbll = new TasksBLL();
            int resnum = TasksBLL.CountByStatus("waiting",0);
            int intval = 0;
            Random r = new Random();
            while (resnum > 0)
            {
                intval++;
                List<TaskSch> list = TasksBLL.GetTasksList("waiting", 1);
                if (list != null && list.Count > 0)
                {
                    foreach (var item in list)
                    {
                        string mobileIndex = "";
                        mobileIndex = item.MobileIndex.ToString();  //需要匹配出对应的客户端标识

                        int flag = 0;
                        //bool flag2 = false;
                        if (item.IsStep == 0)
                        {
                            for (int i = 0; i < item.RepeatNums; i++)
                            {
                                flag = SocketServer.SendTaskInstruct(Int32.Parse(mobileIndex), item.TypeId, item.Id, item.Bodys);
                                int waittime = r.Next(item.RandomMins, item.RandomMaxs);
                                Thread.Sleep(waittime);
                            }
                        }
                        else
                        {
                            if (item.TypeId == 22)
                            {
                                var flag2 = await AddRecommFriend(Int32.Parse(mobileIndex), item.RepeatNums);
                                if (flag2)
                                {
                                    flag = 1;
                                }
                            }
                            else if (item.TypeId == 23)
                            {
                                var flag2 = await AllowRequestFriend(Int32.Parse(mobileIndex), item.RepeatNums);
                                if (flag2)
                                {
                                    flag = 1;
                                }
                            }
                            else if (item.TypeId == 101)
                            {
                                var flag2 = await AddListPhoneFriend(Int32.Parse(mobileIndex), item.RepeatNums);
                                if (flag2)
                                {
                                    flag = 1;
                                }
                            }
                            else if (item.TypeId == 21)   //添加主页好友
                            {
                                var flag2 = await AddHomePageFriend(Int32.Parse(mobileIndex), item.RepeatNums, item.InputName);
                                if (flag2)
                                {
                                    flag = 1;
                                }
                            }
                            else if (item.TypeId == 20)
                            {
                                //添加好友的好友
                                var flag2 = await AddFriendByFriend2(Int32.Parse(mobileIndex), item.RepeatNums);
                                if (flag2)
                                {
                                    flag = 1;
                                }
                            }
                            else if (item.TypeId == (int)TaskType.SearchAndAddFriend)
                            {
                                //搜索并添加好友
                                bool result = await SearchAndAddFriendAsync(int.Parse(mobileIndex), item.InputName, item.RepeatNums);
                                flag = result ? 1 : -1;
                            }
                            else if (item.TypeId == (int)TaskType.SearchAndJoinGroup)
                            {
                                //加入小组
                                bool result = await JoinGroupAsync(int.Parse(mobileIndex), item.InputName, item.RepeatNums);
                                flag = result ? 1 : -1;
                            }
                            else if (item.TypeId == (int)TaskType.TimelineLike)
                            {
                                //时间线点赞
                                bool result = await TimelineLike(int.Parse(mobileIndex), item.SlideNums, item.RepeatNums, item.RandomMins, item.RandomMaxs);
                                flag = result ? 1 : -1;
                            }
                            else if (item.TypeId == (int)TaskType.FriendTimelineLike)
                            {
                                //好友时间线点赞
                                bool result = await FriendTimelineLike(int.Parse(mobileIndex), item.SlideNums, item.RepeatNums, item.FriendNums, item.RandomMins, item.RandomMaxs);
                                flag = result ? 1 : -1;
                            }
                            else if (item.TypeId == (int)TaskType.PublishPost)
                            {
                                //发送动态
                                bool result = await PublishPost(int.Parse(mobileIndex), item.Bodys);
                                flag = result ? 1 : -1;
                            }
                            else if (item.TypeId == (int)TaskType.SendHomepage)
                            {                          
                                //发送主页
                                bool result = await SendHomepageAsync(int.Parse(mobileIndex), item.InputName);
                                
                                //if (result)
                                //{
                                //    result = await SendHomepageAsyncPosting(int.Parse(mobileIndex), item.InputName);
                                //}

                                //if (result)
                                //{
                                //    result = await SendHomepageAsyncMessaging(int.Parse(mobileIndex), item.InputName);
                                //}

                                flag = result ? 1 : -1;
                                
                            }
                            else if (item.TypeId == (int)TaskType.AttentionHomePage)
                            {
                                //关注主页
                                bool result = await SubscribeHomepageAsync(int.Parse(mobileIndex), item.InputName, item.RepeatNums);
                                flag = result ? 1 : -1;
                            }
                            else if (item.TypeId == (int)TaskType.InvitingFriends)
                            {
                                //邀请好友进小组
                                bool result = await InviteFriendsToGroupAsync(int.Parse(mobileIndex), item.InputName, item.RepeatNums);
                                flag = result ? 1 : -1;
                            }
                            else if (item.TypeId == (int)TaskType.InviteFriendsLike)
                            {
                                //邀请好友点赞
                                bool result = await InviteFriendsToLikeAsync(int.Parse(mobileIndex), item.InputName, item.RepeatNums);
                                flag = result ? 1 : -1;
                                
                            }
                        }

                        TasksBLL.UpdateTaskStatus(item.Id, flag);
                        resnum--;
                    }
                }
                if (resnum <= 0)
                {
                    resnum = TasksBLL.CountByStatus("waiting",0);
                }
            }
            ConfigVals.IsRunning = 0;
            return res;
        }

        public static async Task<bool> SearchAndAddFriendAsync(int mobileIndex, string friendName, int addTimes)
        {
            try
            {
                MonitorView.ClearLog();

                // 1 => 回到主页
                if (await SimulationTaskWrapper.GotoHomepage(mobileIndex))
                {
                    MonitorView.WriteLog(mobileIndex, SystemLanguageManager.Instance.ResourceManager.GetString("GotoHomePage_Success", SystemLanguageManager.Instance.CultureInfo));

                    await Task.Delay(3000);

                    // 2 => 点击坐标(280,73)
                    if (await SimulationTaskWrapper.ClickCoordinate(mobileIndex, 280, 73))
                    {
                        MonitorView.WriteLog(mobileIndex, SystemLanguageManager.Instance.ResourceManager.GetString("GotoPersonalHomePage_Success", SystemLanguageManager.Instance.CultureInfo));

                        await Task.Delay(3000);

                        int findFriendsTimes = 0;
                        // 3 => 点击"搜索好友"
                        while (!await SimulationTaskWrapper.SearchClick(mobileIndex, SystemLanguageManager.Instance.ResourceManager.GetString("Find_Friends", SystemLanguageManager.Instance.CultureInfo)))
                        {
                            await Task.Delay(3000);
                            findFriendsTimes++;

                            if (await SimulationTaskWrapper.SearchClick(mobileIndex, SystemLanguageManager.Instance.ResourceManager.GetString("Friends", SystemLanguageManager.Instance.CultureInfo)))
                            {
                                break;
                            }

                            if (findFriendsTimes > SimulationTaskWrapper.MaxTryTimes)
                            {
                                break;
                            }
                        }

                        if (findFriendsTimes > SimulationTaskWrapper.MaxTryTimes)
                        {
                            MonitorView.WriteLog(mobileIndex, SystemLanguageManager.Instance.ResourceManager.GetString("GotoSearchFriends_Failure", SystemLanguageManager.Instance.CultureInfo));
                            return false;
                        }

                        MonitorView.WriteLog(mobileIndex, SystemLanguageManager.Instance.ResourceManager.GetString("GotoSearchFriends_Success", SystemLanguageManager.Instance.CultureInfo));

                        await Task.Delay(3000);

                        int searchTimes = 0;
                        // 4 => 点击"搜索"
                        while (!await SimulationTaskWrapper.SearchClick(mobileIndex, SystemLanguageManager.Instance.ResourceManager.GetString("Search", SystemLanguageManager.Instance.CultureInfo)))
                        {
                            await Task.Delay(3000);
                            searchTimes++;

                            if (searchTimes > SimulationTaskWrapper.MaxTryTimes)
                            {
                                break;
                            }
                        }

                        if (searchTimes > SimulationTaskWrapper.MaxTryTimes)
                        {
                            MonitorView.WriteLog(mobileIndex, SystemLanguageManager.Instance.ResourceManager.GetString("GotoSearchFriendTab_Failure", SystemLanguageManager.Instance.CultureInfo));

                            return false;
                        }

                        MonitorView.WriteLog(mobileIndex, SystemLanguageManager.Instance.ResourceManager.GetString("GotoSearchFriendTab_Success", SystemLanguageManager.Instance.CultureInfo));

                        await Task.Delay(3000);

                        // 5 => 在"按姓名搜索"中输入好友名称
                        if (await SimulationTaskWrapper.Assign(mobileIndex, SystemLanguageManager.Instance.ResourceManager.GetString("Search_By_Name", SystemLanguageManager.Instance.CultureInfo), friendName))
                        {
                            MonitorView.WriteLog(mobileIndex, SystemLanguageManager.Instance.ResourceManager.GetString("Assignment_Success", SystemLanguageManager.Instance.CultureInfo));

                            await Task.Delay(2000);

                            // 6 => 点击"加为好友"
                            for (int j = 0; j < addTimes; j++)
                            {
                                int loopTimes = 0;

                                while (!await SimulationTaskWrapper.SearchClick(mobileIndex, SystemLanguageManager.Instance.ResourceManager.GetString("Add_Friend", SystemLanguageManager.Instance.CultureInfo)))
                                {
                                    await Task.Delay(1000);

                                    await SimulationTaskWrapper.Swipe(mobileIndex, 160, 350, 160, 300, 5);

                                    await Task.Delay(2000);

                                    loopTimes++;
                                    if (loopTimes > SimulationTaskWrapper.MaxTryTimes)
                                    {
                                        MonitorView.WriteLog(mobileIndex, SystemLanguageManager.Instance.ResourceManager.GetString("AddFriend_Failure", SystemLanguageManager.Instance.CultureInfo));
                                        break;
                                    }
                                }

                                if (loopTimes <= SimulationTaskWrapper.MaxTryTimes)
                                {
                                    MonitorView.WriteLog(mobileIndex, SystemLanguageManager.Instance.ResourceManager.GetString("AddFriend_Success", SystemLanguageManager.Instance.CultureInfo));
                                }
                            }

                            return true;
                        }
                        else
                        {
                            MonitorView.WriteLog(mobileIndex, SystemLanguageManager.Instance.ResourceManager.GetString("Assignment_Failure", SystemLanguageManager.Instance.CultureInfo));
                        }
                    }
                    else
                    {
                        MonitorView.WriteLog(mobileIndex, SystemLanguageManager.Instance.ResourceManager.GetString("GotoPersonalHomePage_Failure", SystemLanguageManager.Instance.CultureInfo));
                    }
                }
                else
                {
                    MonitorView.WriteLog(mobileIndex, SystemLanguageManager.Instance.ResourceManager.GetString("GotoHomePage_Failure", SystemLanguageManager.Instance.CultureInfo));
                }

                return false;
            }
            catch (Exception ex)
            {
                LogUtils.Error($"{ex.Message}");
                return false;
            }
        }

        public static async Task<bool> JoinGroupAsync(int mobileIndex, string groupName, int addTimes)
        {
            try
            {
                // 1 => 回到主页
                if (await SimulationTaskWrapper.GotoHomepage(mobileIndex))
                {
                    await Task.Delay(3000);

                    // 2 => 点击坐标（280,73）
                    if (await SimulationTaskWrapper.ClickCoordinate(mobileIndex, 280, 73))
                    {
                        await Task.Delay(3000);


                        int clickGroupTimes = 0;
                        // 3 => 搜索点击“小组”
                        while (!await SimulationTaskWrapper.SearchClick(mobileIndex, SystemLanguageManager.Instance.ResourceManager.GetString("Groups", SystemLanguageManager.Instance.CultureInfo)))
                        {
                            await Task.Delay(3000);
                            clickGroupTimes++;

                            if (clickGroupTimes > SimulationTaskWrapper.MaxTryTimes)
                            {
                                break;
                            }
                        }

                        if (clickGroupTimes > SimulationTaskWrapper.MaxTryTimes)
                        {
                            return false;
                        }

                        await Task.Delay(3000);


                        int clickSearchGroupTimes = 0;
                        // 4 => 搜索点击“搜索小组”
                        while (!await SimulationTaskWrapper.SearchClick(mobileIndex, SystemLanguageManager.Instance.ResourceManager.GetString("Search_Group", SystemLanguageManager.Instance.CultureInfo)))
                        {
                            await Task.Delay(3000);
                            clickSearchGroupTimes++;

                            if (clickSearchGroupTimes > SimulationTaskWrapper.MaxTryTimes)
                            {
                                break;
                            }
                        }

                        if (clickSearchGroupTimes > SimulationTaskWrapper.MaxTryTimes)
                        {
                            return false;
                        }

                        await Task.Delay(3000);


                        int assignSearchGroupTimes = 0;
                        // 5 => 在“搜索小组”文本框中输入小组名称
                        while (!await SimulationTaskWrapper.Assign(mobileIndex, SystemLanguageManager.Instance.ResourceManager.GetString("Search_Group", SystemLanguageManager.Instance.CultureInfo), groupName))
                        {
                            await Task.Delay(3000);
                            assignSearchGroupTimes++;

                            if (assignSearchGroupTimes > SimulationTaskWrapper.MaxTryTimes)
                            {
                                break;
                            }
                        }

                        if (assignSearchGroupTimes > SimulationTaskWrapper.MaxTryTimes)
                        {
                            return false;
                        }

                        await Task.Delay(3000);


                        // 6 => 点击目标小组
                        if (await SimulationTaskWrapper.SearchClick(mobileIndex, groupName))
                        {
                            await Task.Delay(5000);

                            // 7 => 点击“加入小组”
                            bool result = await SimulationTaskWrapper.SearchClick(mobileIndex, SystemLanguageManager.Instance.ResourceManager.GetString("Join_Group", SystemLanguageManager.Instance.CultureInfo));
                            return result;
                        }
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                LogUtils.Error($"{ex.Message}");
                return false;
            }
        }

        public static async Task<bool> TimelineLike(int mobileIndex, int slideTimes, int likeTimes, int minInterval, int maxInterval)
        {
            try
            {
                bool result = false;

                // 1 => 回到主页
                if (await SimulationTaskWrapper.GotoHomepage(mobileIndex))
                {
                    await Task.Delay(1000);

                    // 2 => 点击坐标（40,73）
                    if (await SimulationTaskWrapper.ClickCoordinate(mobileIndex, 40, 73))
                    {
                        await Task.Delay(2000);

                        // 3 => 下滑刷新slideTimes次
                        for (int i = 0; i < slideTimes; i++)
                        {
                            await SimulationTaskWrapper.Swipe(mobileIndex, 160, 150, 160, 200, 20);
                            await Task.Delay(5000);
                        }

                        for (int i = 0; i < likeTimes; i++)
                        {
                            //await SimulationTaskWrapper.Swipe(mobileIndex, 160, 350, 160, 200, 30);

                            //await Task.Delay(3000);

                            int loopTimes = 0;

                            while (!await SimulationTaskWrapper.SearchClick(mobileIndex, SystemLanguageManager.Instance.ResourceManager.GetString("Like", SystemLanguageManager.Instance.CultureInfo)))
                            {
                                await Task.Delay(1000);

                                await SimulationTaskWrapper.Swipe(mobileIndex, 160, 350, 160, 250, 50);

                                loopTimes++;
                                if (loopTimes > SimulationTaskWrapper.MaxTryTimes)
                                {
                                    break;
                                }
                            }

                            await Task.Delay(1000);
                        }

                        result = true;
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                LogUtils.Error($"{ex.Message}");
                return false;
            }
        }

        public static async Task<bool> FriendTimelineLike(int mobileIndex, int slideTimes, int likeTimes, int friendNum, int minInterval, int maxInterval)
        {
            try
            {
                bool result = false;

                // 1 => 回到主页
                if (await SimulationTaskWrapper.GotoHomepage(mobileIndex))
                {
                    await Task.Delay(3000);

                    // 2 => 点击坐标（280,73）
                    if (await SimulationTaskWrapper.ClickCoordinate(mobileIndex, 280, 73))
                    {
                        await Task.Delay(3000);

                        // 3 => 点击"搜索好友"
                        if (await SimulationTaskWrapper.SearchClick(mobileIndex, SystemLanguageManager.Instance.ResourceManager.GetString("Find_Friends", SystemLanguageManager.Instance.CultureInfo)))
                        {
                            await Task.Delay(3000);

                            // 4 => 点击"好友"
                            if (await SimulationTaskWrapper.SearchClick(mobileIndex, SystemLanguageManager.Instance.ResourceManager.GetString("Friends", SystemLanguageManager.Instance.CultureInfo)))
                            {
                                //好友多，加载需要一定时间

                                await Task.Delay(2000);

                                int checkedTimes = 0;
                                while (!await SimulationTaskWrapper.Search(mobileIndex, SystemLanguageManager.Instance.ResourceManager.GetString("Remove_Friend", SystemLanguageManager.Instance.CultureInfo)))
                                {
                                    await Task.Delay(3000);
                                    checkedTimes++;
                                    if (checkedTimes > SimulationTaskWrapper.MaxTryTimes)
                                    {
                                        return false;
                                    }
                                }

                                for (int i = 0; i < friendNum; i++)
                                {
                                    if (await SimulationTaskWrapper.SearchClickParent(mobileIndex, SystemLanguageManager.Instance.ResourceManager.GetString("Remove_Friend", SystemLanguageManager.Instance.CultureInfo), 1, i))
                                    {
                                        for (int j = 0; j < slideTimes; j++)
                                        {
                                            await SimulationTaskWrapper.Swipe(mobileIndex, 160, 300, 160, 400, 5);
                                            await Task.Delay(5000);
                                        }

                                        for (int k = 0; k < likeTimes; k++)
                                        {
                                            //await SimulationTaskWrapper.Swipe(mobileIndex, 160, 300, 160, 200, 30);

                                            await Task.Delay(3000);

                                            int loopTimes = 0;
                                            while (!await SimulationTaskWrapper.SearchClick(mobileIndex, SystemLanguageManager.Instance.ResourceManager.GetString("Like", SystemLanguageManager.Instance.CultureInfo)))
                                            {
                                                await Task.Delay(2000);

                                                await SimulationTaskWrapper.Swipe(mobileIndex, 160, 300, 160, 200, 30);

                                                loopTimes++;
                                                if (loopTimes > SimulationTaskWrapper.MaxTryTimes)
                                                {
                                                    break;
                                                }
                                            }

                                            await Task.Delay(3000);
                                        }

                                        await SimulationTaskWrapper.GoBack(mobileIndex);
                                    }
                                }

                                result = true;
                            }
                        }
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                LogUtils.Error($"{ex.Message}");
                return false;
            }
        }

        public static async Task<bool> PublishPost(int mobileIndex, string jsonBody)
        {
            try
            {
                MonitorView.ClearLog();

                JObject jObject = JObject.Parse(jsonBody);

                string text = jObject.SelectToken("txtmsg").ToString();
                JArray list = (JArray)jObject.SelectToken("list");


                bool result = true;
                // 1.回到主页
                if (await SimulationTaskWrapper.GotoHomepage(mobileIndex))
                {
                    MonitorView.WriteLog(mobileIndex, SystemLanguageManager.Instance.ResourceManager.GetString("GotoHomePage_Success", SystemLanguageManager.Instance.CultureInfo));

                    await Task.Delay(3000);

                    // 点击坐标（40,73）
                    if (await SimulationTaskWrapper.ClickCoordinate(mobileIndex, 40, 73))
                    {
                        MonitorView.WriteLog(mobileIndex, SystemLanguageManager.Instance.ResourceManager.GetString("GotoMyTimeline_Success", SystemLanguageManager.Instance.CultureInfo));

                        await Task.Delay(3000);

                        if (await SimulationTaskWrapper.ClickCoordinate(mobileIndex, 40, 73))
                        {
                            MonitorView.WriteLog(mobileIndex, SystemLanguageManager.Instance.ResourceManager.GetString("GotoMyTimeline_Success", SystemLanguageManager.Instance.CultureInfo));

                            await Task.Delay(5000);

                            List<string> pics = new List<string>();
                            bool updatePicsSucceeded = false;

                            if (list != null && list.Count > 0)
                            {
                                //首先push图片到手机
                                for (int i = 0; i < list.Count; i++)
                                {
                                    string mobilePicPath = $"/sdcard/a{DateTime.Now.ToString("mmssffff")}{Path.GetExtension(list[i].ToString())}";
                                    pics.Add(mobilePicPath);

                                    string mobileId = DeviceConnectionManager.Instance.GetDeviceNameByMobileIndex(mobileIndex);

                                    ProcessUtils.PushFileToVm(mobileId, list[i].ToString(), mobilePicPath);

                                    MonitorView.WriteLog(mobileIndex, SystemLanguageManager.Instance.ResourceManager.GetString("UploadPhoto_Success", SystemLanguageManager.Instance.CultureInfo));
                                }

                                if (await SimulationTaskWrapper.UpdatePictures(mobileIndex, pics.ToArray()))
                                {

                                    MonitorView.WriteLog(mobileIndex, SystemLanguageManager.Instance.ResourceManager.GetString("UpdatePhoto_Success", SystemLanguageManager.Instance.CultureInfo));

                                    updatePicsSucceeded = true;

                                    await Task.Delay(5000);

                                    if (await SimulationTaskWrapper.SearchClick(mobileIndex, SystemLanguageManager.Instance.ResourceManager.GetString("Photo", SystemLanguageManager.Instance.CultureInfo)))
                                    {

                                        MonitorView.WriteLog(mobileIndex, SystemLanguageManager.Instance.ResourceManager.GetString("GotoPhoto_Success", SystemLanguageManager.Instance.CultureInfo));

                                        await Task.Delay(3000);

                                        //image size 80*80, first image center: (39,94)
                                        int x = 0, y = 0;
                                        for (int i = 0; i < list.Count; i++)
                                        {
                                            x = 39 + (i * 80);
                                            y = 94 + (i * 80);

                                            bool selectPicResult = await SimulationTaskWrapper.ClickCoordinate(mobileIndex, x, y);
                                            await Task.Delay(2000);
                                        }

                                        if (await SimulationTaskWrapper.SearchClick(mobileIndex, SystemLanguageManager.Instance.ResourceManager.GetString("Done", SystemLanguageManager.Instance.CultureInfo)))
                                        {
                                            await Task.Delay(3000);

                                            if (await SimulationTaskWrapper.Assign(mobileIndex, SystemLanguageManager.Instance.ResourceManager.GetString("SaySomething", SystemLanguageManager.Instance.CultureInfo), text))
                                            {
                                                await Task.Delay(3000);

                                                if (await SimulationTaskWrapper.ClickCoordinate(mobileIndex, 297, 37))
                                                {
                                                    await Task.Delay(3000);

                                                    if (await SimulationTaskWrapper.SearchClick(mobileIndex, SystemLanguageManager.Instance.ResourceManager.GetString("Share_Now", SystemLanguageManager.Instance.CultureInfo)))
                                                    {
                                                        if (updatePicsSucceeded)
                                                        {
                                                            await Task.Delay(30000);

                                                            await SimulationTaskWrapper.DeletePictures(mobileIndex, pics.ToArray());
                                                        }

                                                        return true;
                                                    }
                                                    else
                                                    {
                                                        result = false;
                                                    }
                                                }
                                                else
                                                {
                                                    result = false;
                                                }
                                            }
                                            else
                                            {
                                                result = false;
                                            }
                                        }
                                        else
                                        {
                                            result = false;
                                        }
                                    }
                                    else
                                    {
                                        MonitorView.WriteLog(mobileIndex, SystemLanguageManager.Instance.ResourceManager.GetString("GotoPhoto_Failure", SystemLanguageManager.Instance.CultureInfo));
                                    }
                                }
                                else
                                {
                                    MonitorView.WriteLog(mobileIndex, SystemLanguageManager.Instance.ResourceManager.GetString("UpdatePhoto_Failure", SystemLanguageManager.Instance.CultureInfo));
                                }

                                if (!result)
                                {
                                    if (updatePicsSucceeded)
                                    {
                                        await SimulationTaskWrapper.DeletePictures(mobileIndex, pics.ToArray());
                                    }
                                    return false;
                                }
                            }
                            else
                            {
                                //发文字动态

                                // 2.点击“分享新鲜事”（不生效），使用点击坐标（160,120）代替。
                                //if (await SimulationTaskWrapper.SearchClick(mobileIndex, SystemLanguageManager.Instance.ResourceManager.GetString("Post", SystemLanguageManager.Instance.CultureInfo)))
                                if (await SimulationTaskWrapper.ClickCoordinate(mobileIndex, 160, 120))
                                {
                                    await Task.Delay(5000);

                                    // 3.粘贴文本
                                    if (await SimulationTaskWrapper.Assign(mobileIndex, SystemLanguageManager.Instance.ResourceManager.GetString("Post", SystemLanguageManager.Instance.CultureInfo), text))
                                    {
                                        await Task.Delay(3000);


                                        //if (await SimulationTaskWrapper.SearchClick(mobileIndex, SystemLanguageManager.Instance.ResourceManager.GetString("Red_Background", SystemLanguageManager.Instance.CultureInfo)))
                                        //{//不是所有facebook app当前页都有Red_Background

                                        //}

                                        if (await SimulationTaskWrapper.ClickCoordinate(mobileIndex, 108, 427))
                                        {
                                            //挑选一种背景色
                                        }

                                        await Task.Delay(3000);

                                        if (await SimulationTaskWrapper.ClickCoordinate(mobileIndex, 297, 37))
                                        {
                                            await Task.Delay(3000);

                                            if (await SimulationTaskWrapper.SearchClick(mobileIndex, SystemLanguageManager.Instance.ResourceManager.GetString("Share_Now", SystemLanguageManager.Instance.CultureInfo)))
                                            {
                                                return true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            MonitorView.WriteLog(mobileIndex, SystemLanguageManager.Instance.ResourceManager.GetString("GotoMyTimeline_Failure", SystemLanguageManager.Instance.CultureInfo));
                        }
                    }
                    else
                    {
                        MonitorView.WriteLog(mobileIndex, SystemLanguageManager.Instance.ResourceManager.GetString("GotoMyTimeline_Failure", SystemLanguageManager.Instance.CultureInfo));
                    }
                }
                else
                {
                    MonitorView.WriteLog(mobileIndex, SystemLanguageManager.Instance.ResourceManager.GetString("GotoHomePage_Failure", SystemLanguageManager.Instance.CultureInfo));
                }

                return false;
            }
            catch (Exception ex)
            {
                LogUtils.Error($"{ex.Message}");
                return false;
            }
        }

        public static async Task<bool> SendFrinedsMsg(int mobileIndex, string jsonBody)
        {
            try
            {
                string messenger = "com.facebook.orca";

                // 关闭messenger
                ProcessUtils.AdbCloseApps(mobileIndex, messenger);

                await Task.Delay(2000);

                // 开启messenger
                ProcessUtils.AdbOpenApps(mobileIndex, messenger);

                JObject jObject = JObject.Parse(jsonBody);

                string text = jObject.SelectToken("txtmsg").ToString();
                JArray list = (JArray)jObject.SelectToken("list");
                string str_friendNum = jObject.SelectToken("friendnums").ToString();
                string str_position = jObject.SelectToken("position").ToString();


                int friendNum = int.Parse(str_friendNum);
                int position = int.Parse(str_position);

                // maybe it takes longer to show page
                await Task.Delay(6000);

                bool gotoReadyPageResult = true;

                // 以“”的身份继续，坐标（160. 294）；
                if (await SimulationTaskWrapper.Search(mobileIndex, SystemLanguageManager.Instance.ResourceManager.GetString("WelcomeToMessenger", SystemLanguageManager.Instance.CultureInfo)))
                {
                    await Task.Delay(2000);

                    if (await SimulationTaskWrapper.ClickCoordinate(mobileIndex, 160, 294))
                    {
                        await Task.Delay(2000);
                        int searchTurnOnTimes = 0;
                        while (!await SimulationTaskWrapper.SearchClick(mobileIndex, SystemLanguageManager.Instance.ResourceManager.GetString("TurnOn", SystemLanguageManager.Instance.CultureInfo)))
                        {
                            await Task.Delay(2000);
                            searchTurnOnTimes++;

                            if (searchTurnOnTimes > SimulationTaskWrapper.MaxTryTimes)
                            {
                                gotoReadyPageResult = false;
                                break;
                            }
                        }
                    }
                    else
                    {
                        gotoReadyPageResult = false;
                    }
                }

                if (!gotoReadyPageResult)
                {
                    return false;
                }

                if (await SimulationTaskWrapper.Search(mobileIndex, SystemLanguageManager.Instance.ResourceManager.GetString("TurnOn", SystemLanguageManager.Instance.CultureInfo)))
                {
                    await Task.Delay(2000);
                    if (await SimulationTaskWrapper.SearchClick(mobileIndex, SystemLanguageManager.Instance.ResourceManager.GetString("TurnOn", SystemLanguageManager.Instance.CultureInfo)))
                    {
                        await Task.Delay(2000);
                    }
                    else
                    {
                        gotoReadyPageResult = false;
                    }
                }

                if (!gotoReadyPageResult)
                {
                    return false;
                }

                List<string> pics = new List<string>();
                bool updatePicsSucceeded = false;

                if (list != null && list.Count > 0)
                {
                    //首先push图片到手机
                    for (int i = 0; i < list.Count; i++)
                    {
                        string mobilePicPath = $"/sdcard/a{DateTime.Now.ToString("mmssffff")}{Path.GetExtension(list[i].ToString())}";
                        pics.Add(mobilePicPath);

                        string mobileId = DeviceConnectionManager.Instance.GetDeviceNameByMobileIndex(mobileIndex);

                        ProcessUtils.PushFileToVm(mobileId, list[i].ToString(), mobilePicPath);
                    }

                    if (await SimulationTaskWrapper.UpdatePictures(mobileIndex, pics.ToArray()))
                    {
                        updatePicsSucceeded = true;
                    }
                }

                if (await SimulationTaskWrapper.SearchClick(mobileIndex, SystemLanguageManager.Instance.ResourceManager.GetString("Users", SystemLanguageManager.Instance.CultureInfo)))
                {
                    await Task.Delay(2000);

                    if (await SimulationTaskWrapper.ClickCoordinate(mobileIndex, 160, 148))
                    {
                        await Task.Delay(3000);

                        int swiped = 0;
                        while (swiped < position)
                        {
                            if (swiped == 0)
                            {
                                await SimulationTaskWrapper.Swipe(mobileIndex, 160, 130, 160, 100, 20);
                                swiped += 30;
                            }
                            else
                            {
                                await SimulationTaskWrapper.Swipe(mobileIndex, 160, 142, 160, 100, 20);
                                swiped += 42;
                            }

                            await Task.Delay(2000);
                        }

                        int newSwiped = 0;
                        for (int f = 0; f < friendNum; f++)
                        {
                            if (await SimulationTaskWrapper.Swipe(mobileIndex, 160, swiped == 0 ? 130 : 142, 160, 100, 20))
                            {
                                newSwiped += (swiped == 0 ? 30 : 42);

                                await Task.Delay(2000);

                                if (await SimulationTaskWrapper.ClickCoordinate(mobileIndex, 160, 100))
                                {
                                    await Task.Delay(4000);

                                    int triedTimes = 0;
                                    while (!await SimulationTaskWrapper.Search(mobileIndex, SystemLanguageManager.Instance.ResourceManager.GetString("DialogDetails", SystemLanguageManager.Instance.CultureInfo)))
                                    {
                                        await Task.Delay(2000);
                                        await SimulationTaskWrapper.Swipe(mobileIndex, 160, 142, 160, 100, 20);
                                        newSwiped += 42;
                                        triedTimes++;

                                        if (triedTimes > 3)
                                        {
                                            return false;
                                        }
                                    }

                                    await Task.Delay(1000);

                                    if (string.IsNullOrEmpty(text))
                                    {
                                        //仅发图片
                                        if (await SimulationTaskWrapper.SearchClick(mobileIndex, "Choose photo"))
                                        {
                                            await Task.Delay(2000);

                                            if (await SimulationTaskWrapper.SearchClick(mobileIndex, "Media Gallery"))
                                            {
                                                for (int i = 0; i < list.Count; i++)
                                                {
                                                    await Task.Delay(2000);

                                                    int x = 55 + i * 106;
                                                    int y = 97 + i * 106;

                                                    await SimulationTaskWrapper.ClickCoordinate(mobileIndex, x, y);

                                                }

                                                if (await SimulationTaskWrapper.ClickCoordinate(mobileIndex, 290, 450))
                                                {

                                                }
                                            }
                                        }
                                    }
                                    else if (list == null || list.Count == 0)
                                    {
                                        //仅发文字
                                        if (await SimulationTaskWrapper.Assign(mobileIndex, "Aa", text))
                                        {
                                            await Task.Delay(2000);

                                            if (await SimulationTaskWrapper.SearchClick(mobileIndex, "Send"))
                                            {
                                            }
                                        }
                                    }
                                    else
                                    {
                                        //发图文
                                        if (await SimulationTaskWrapper.Assign(mobileIndex, "Aa", text))
                                        {
                                            await Task.Delay(2000);

                                            if (await SimulationTaskWrapper.SearchClick(mobileIndex, "Send"))
                                            {
                                                await Task.Delay(2000);

                                                if (await SimulationTaskWrapper.SearchClick(mobileIndex, "Choose photo"))
                                                {
                                                    await Task.Delay(2000);

                                                    if (await SimulationTaskWrapper.SearchClick(mobileIndex, "Media Gallery"))
                                                    {
                                                        for (int i = 0; i < list.Count; i++)
                                                        {
                                                            await Task.Delay(2000);

                                                            int x = 55 + i * 106;
                                                            int y = 97 + i * 106;

                                                            await SimulationTaskWrapper.ClickCoordinate(mobileIndex, x, y);

                                                        }

                                                        await Task.Delay(3000);

                                                        if (await SimulationTaskWrapper.ClickCoordinate(mobileIndex, 290, 450))
                                                        {
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                    }


                                }
                            }

                            await Task.Delay(3000);

                            await SimulationTaskWrapper.GoBack(mobileIndex);

                            await Task.Delay(3000);

                            await SimulationTaskWrapper.GoBack(mobileIndex);

                            await Task.Delay(3000);

                        }

                        // 存储本次结束的位置
                        TaskTrace taskTrace = new TaskTrace() { MobileIndex = mobileIndex, TypeId = (int)TaskType.SendFriendsMessage, Position = swiped + newSwiped };
                        if (TraceBLL.CountTaskTrace(taskTrace) > 0)
                        {
                            TraceBLL.UpdateTaskTrace(taskTrace);
                        }
                        else
                        {
                            TraceBLL.CreateTaskTrace(taskTrace);
                        }


                        if (updatePicsSucceeded)
                        {
                            await Task.Delay(30000);
                            await SimulationTaskWrapper.DeletePictures(mobileIndex, pics.ToArray());
                        }


                        return true;
                    }
                }

                if (updatePicsSucceeded)
                {
                    await Task.Delay(30000);
                    await SimulationTaskWrapper.DeletePictures(mobileIndex, pics.ToArray());
                }

                return false;

                //// 右下角 用户 坐标（280，459）；
                //// 所有联系人 坐标（160. 148）；
                //// fuzzy search “Users”
                //// 点击 “Aa”
                //// 发送  都是 “Send"，坐标（302.461）； 
                //// 选择照片 都是”Choose photo“  坐标（84，461）；
                //// 点击左下方 Media Gallery，坐标（27，453）；
                //// 第一张照片的 中心坐标（55.97）；， 每张照片的宽度和高度：（106.106）；
                //// 点击右下角发送 坐标，（290.450）；
                //return false;           
            }
            catch (Exception ex)
            {
                LogUtils.Error($"{ex.Message}");
                return false;
            }
        }

        public static async Task<bool> SendFrinedsMsgLite(int mobileIndex, string jsonBody)
        {
            try
            {
                string messenger = "com.facebook.mlite";

                // 关闭messenger
                ProcessUtils.AdbCloseApps(mobileIndex, messenger);

                await Task.Delay(2000);

                // 开启messenger
                ProcessUtils.AdbOpenApps(mobileIndex, messenger);

                JObject jObject = JObject.Parse(jsonBody);

                string text = jObject.SelectToken("txtmsg").ToString();
                JArray list = (JArray)jObject.SelectToken("list");
                string str_friendNum = jObject.SelectToken("friendnums").ToString();
                string str_position = jObject.SelectToken("position").ToString();


                int friendNum = int.Parse(str_friendNum);
                int position = int.Parse(str_position);

                // maybe it takes longer to show page
                await Task.Delay(6000);

                bool gotoReadyPageResult = true;

                // 以“”的身份继续，坐标（160. 294）；
                if (await SimulationTaskWrapper.Search(mobileIndex, SystemLanguageManager.Instance.ResourceManager.GetString("WelcomeToMessengerLite", SystemLanguageManager.Instance.CultureInfo)))
                {
                    await Task.Delay(2000);

                    if (await SimulationTaskWrapper.ClickCoordinate(mobileIndex, 160, 340))
                    {
                        await Task.Delay(2000);
                        int searchTurnOnTimes = 0;
                        while (!await SimulationTaskWrapper.SearchClick(mobileIndex, SystemLanguageManager.Instance.ResourceManager.GetString("TurnOn", SystemLanguageManager.Instance.CultureInfo)))
                        {
                            await Task.Delay(2000);
                            searchTurnOnTimes++;

                            if (searchTurnOnTimes > SimulationTaskWrapper.MaxTryTimes)
                            {
                                gotoReadyPageResult = false;
                                break;
                            }
                        }
                    }
                    else
                    {
                        gotoReadyPageResult = false;
                    }
                }

                if (!gotoReadyPageResult)
                {
                    return false;
                }

                if (await SimulationTaskWrapper.Search(mobileIndex, SystemLanguageManager.Instance.ResourceManager.GetString("TurnOn", SystemLanguageManager.Instance.CultureInfo)))
                {
                    await Task.Delay(2000);
                    int searchTurnOnTimes = 0;

                    while (!await SimulationTaskWrapper.SearchClick(mobileIndex, SystemLanguageManager.Instance.ResourceManager.GetString("TurnOn", SystemLanguageManager.Instance.CultureInfo)))
                    {
                        await Task.Delay(2000);
                        searchTurnOnTimes++;

                        if (searchTurnOnTimes > SimulationTaskWrapper.MaxTryTimes)
                        {
                            gotoReadyPageResult = false;
                            break;
                        }

                    }
                }

                if (!gotoReadyPageResult)
                {
                    return false;
                }

                List<string> pics = new List<string>();
                bool updatePicsSucceeded = false;

                if (list != null && list.Count > 0)
                {
                    //首先push图片到手机
                    for (int i = 0; i < list.Count; i++)
                    {
                        string mobilePicPath = $"/sdcard/a{DateTime.Now.ToString("mmssffff")}{Path.GetExtension(list[i].ToString())}";
                        pics.Add(mobilePicPath);

                        string mobileId = DeviceConnectionManager.Instance.GetDeviceNameByMobileIndex(mobileIndex);

                        ProcessUtils.PushFileToVm(mobileId, list[i].ToString(), mobilePicPath);
                    }

                    if (await SimulationTaskWrapper.UpdatePictures(mobileIndex, pics.ToArray()))
                    {
                        updatePicsSucceeded = true;
                    }
                }

                if (await SimulationTaskWrapper.SearchClick(mobileIndex, SystemLanguageManager.Instance.ResourceManager.GetString("ContactsTab", SystemLanguageManager.Instance.CultureInfo)))
                {
                    await Task.Delay(2000);

                    //if (await SimulationTaskWrapper.ClickCoordinate(mobileIndex, 160, 148))
                    //{
                    //await Task.Delay(3000);

                    int swiped = 0;
                    while (swiped < position)
                    {
                        if (swiped == 0)
                        {
                            await SimulationTaskWrapper.Swipe(mobileIndex, 160, 130, 160, 100, 20);
                            swiped += 30;
                        }
                        else
                        {
                            await SimulationTaskWrapper.Swipe(mobileIndex, 160, 142, 160, 100, 20);
                            swiped += 42;
                        }

                        await Task.Delay(2000);
                    }

                    int newSwiped = 0;
                    for (int f = 0; f < friendNum; f++)
                    {
                        if (await SimulationTaskWrapper.Swipe(mobileIndex, 160, swiped + newSwiped == 0 ? 169 : 142, 160, 100, 20))
                        {
                            newSwiped += (swiped + newSwiped == 0 ? 69 : 42);

                            await Task.Delay(2000);

                            if (await SimulationTaskWrapper.ClickCoordinate(mobileIndex, 160, 100))
                            {
                                await Task.Delay(4000);

                                int triedTimes = 0;
                                while (!await SimulationTaskWrapper.Search(mobileIndex, SystemLanguageManager.Instance.ResourceManager.GetString("More_Options", SystemLanguageManager.Instance.CultureInfo)))
                                {
                                    await Task.Delay(2000);
                                    await SimulationTaskWrapper.Swipe(mobileIndex, 160, 142, 160, 100, 20);
                                    newSwiped += 42;
                                    triedTimes++;

                                    if (triedTimes > 3)
                                    {
                                        return false;
                                    }
                                }

                                await Task.Delay(1000);


                                if (!string.IsNullOrEmpty(text))
                                {
                                    //仅发文字
                                    if (await SimulationTaskWrapper.Assign(mobileIndex, SystemLanguageManager.Instance.ResourceManager.GetString("WriteAMessage", SystemLanguageManager.Instance.CultureInfo), text))
                                    {
                                        await Task.Delay(2000);

                                        if (await SimulationTaskWrapper.SearchClick(mobileIndex, SystemLanguageManager.Instance.ResourceManager.GetString("Send", SystemLanguageManager.Instance.CultureInfo)))
                                        {
                                        }
                                    }

                                }

                                if (list != null && list.Count > 0)
                                {
                                    for (int i = 0; i < list.Count; i++)
                                    {

                                        if (await SimulationTaskWrapper.SearchClick(mobileIndex, SystemLanguageManager.Instance.ResourceManager.GetString("Back", SystemLanguageManager.Instance.CultureInfo)))
                                        {
                                            await Task.Delay(2000);
                                        }

                                        if (await SimulationTaskWrapper.SearchClick(mobileIndex, SystemLanguageManager.Instance.ResourceManager.GetString("Camera", SystemLanguageManager.Instance.CultureInfo)))
                                        {
                                            await Task.Delay(3000);

                                            int x = 80 + i * 80;
                                            int y = 118 + i * 80;

                                            await SimulationTaskWrapper.ClickCoordinate(mobileIndex, x, y);

                                            await Task.Delay(3000);
                                        }
                                    }

                                }
                            }
                        }

                        await Task.Delay(3000);

                        await SimulationTaskWrapper.GoBack(mobileIndex);

                        await Task.Delay(3000);

                        //await SimulationTaskWrapper.GoBack(mobileIndex);

                        //await Task.Delay(3000);

                    }

                    // 存储本次结束的位置
                    TaskTrace taskTrace = new TaskTrace() { MobileIndex = mobileIndex, TypeId = (int)TaskType.SendFriendsMessage, Position = swiped + newSwiped };
                    if (TraceBLL.CountTaskTrace(taskTrace) > 0)
                    {
                        TraceBLL.UpdateTaskTrace(taskTrace);
                    }
                    else
                    {
                        TraceBLL.CreateTaskTrace(taskTrace);
                    }


                    if (updatePicsSucceeded)
                    {
                        await Task.Delay(30000);
                        await SimulationTaskWrapper.DeletePictures(mobileIndex, pics.ToArray());
                    }


                    return true;
                    //}
                }

                if (updatePicsSucceeded)
                {
                    await Task.Delay(30000);
                    await SimulationTaskWrapper.DeletePictures(mobileIndex, pics.ToArray());
                }

                return false;

            }
            catch (Exception ex)
            {
                LogUtils.Error($"{ex}");
                return false;
            }

            //// 右下角 用户 坐标（280，459）；
            //// 所有联系人 坐标（160. 148）；
            //// fuzzy search “Users”
            //// 点击 “Aa”
            //// 发送  都是 “Send"，坐标（302.461）； 
            //// 选择照片 都是”Choose photo“  坐标（84，461）；
            //// 点击左下方 Media Gallery，坐标（27，453）；
            //// 第一张照片的 中心坐标（55.97）；， 每张照片的宽度和高度：（106.106）；
            //// 点击右下角发送 坐标，（290.450）；
            //return false;
        }

        public static async Task<bool> SendGroupMsg(int mobileIndex, string jsonBody)
        {
            try
            {
                JObject jObject = JObject.Parse(jsonBody);

                string title = jObject.SelectToken("title").ToString();
                int price = int.Parse(jObject.SelectToken("price").ToString());
                string group_name = jObject.SelectToken("group_name").ToString();
                string address = jObject.SelectToken("address").ToString();
                string detail = jObject.SelectToken("detail").ToString();

                JArray pic_list = (JArray)jObject.SelectToken("list");

                if (await SimulationTaskWrapper.GotoHomepage(mobileIndex))
                {
                    await Task.Delay(2000);

                    if (await SimulationTaskWrapper.ClickCoordinate(mobileIndex, 280, 73))
                    {
                        int clickGroupTimes = 0;
                        // 3 => 搜索点击“小组”
                        while (!await SimulationTaskWrapper.SearchClick(mobileIndex, SystemLanguageManager.Instance.ResourceManager.GetString("Groups", SystemLanguageManager.Instance.CultureInfo)))
                        {
                            await Task.Delay(3000);
                            clickGroupTimes++;

                            if (clickGroupTimes > SimulationTaskWrapper.MaxTryTimes)
                            {
                                break;
                            }
                        }

                        if (clickGroupTimes > SimulationTaskWrapper.MaxTryTimes)
                        {
                            return false;
                        }

                        await Task.Delay(3000);

                        if (await SimulationTaskWrapper.SearchClick(mobileIndex, SystemLanguageManager.Instance.ResourceManager.GetString("Search_Group", SystemLanguageManager.Instance.CultureInfo)))
                        {
                            await Task.Delay(2000);

                            if (await SimulationTaskWrapper.Assign(mobileIndex, SystemLanguageManager.Instance.ResourceManager.GetString("Search_Group", SystemLanguageManager.Instance.CultureInfo), group_name))
                            {
                                await Task.Delay(3000);

                                if (await SimulationTaskWrapper.SearchClick(mobileIndex, group_name))
                                {
                                    await Task.Delay(4000);

                                    if (await SimulationTaskWrapper.FuzzySearchClick(mobileIndex, SystemLanguageManager.Instance.ResourceManager.GetString("More_Options", SystemLanguageManager.Instance.CultureInfo)))
                                    {
                                        await Task.Delay(2000);

                                        if (await SimulationTaskWrapper.ClickCoordinate(mobileIndex, 160, 333))
                                        {
                                            await Task.Delay(2000);

                                            await SimulationTaskWrapper.Assign(mobileIndex, SystemLanguageManager.Instance.ResourceManager.GetString("SellingSomething", SystemLanguageManager.Instance.CultureInfo), title);

                                            await Task.Delay(2000);
                                            await SimulationTaskWrapper.Assign(mobileIndex, SystemLanguageManager.Instance.ResourceManager.GetString("GoodsPrice", SystemLanguageManager.Instance.CultureInfo), price.ToString());
                                            await Task.Delay(2000);
                                            await SimulationTaskWrapper.Assign(mobileIndex, SystemLanguageManager.Instance.ResourceManager.GetString("GoodsDescription", SystemLanguageManager.Instance.CultureInfo), detail);
                                            await Task.Delay(2000);

                                            if (await SimulationTaskWrapper.SearchClick(mobileIndex, SystemLanguageManager.Instance.ResourceManager.GetString("Location", SystemLanguageManager.Instance.CultureInfo)))
                                            {
                                                await Task.Delay(2000);

                                                if (await SimulationTaskWrapper.Assign(mobileIndex, SystemLanguageManager.Instance.ResourceManager.GetString("SearchPlaces", SystemLanguageManager.Instance.CultureInfo), address))
                                                {
                                                    await Task.Delay(3000);

                                                    await SimulationTaskWrapper.ClickCoordinate(mobileIndex, 160, 111);

                                                    await Task.Delay(2000);
                                                }
                                            }

                                            List<string> pics = new List<string>();
                                            bool updatePicsSucceeded = false;

                                            if (pic_list != null && pic_list.Count > 0)
                                            {
                                                for (int i = 0; i < pic_list.Count; i++)
                                                {
                                                    string mobilePicPath = $"/sdcard/a{DateTime.Now.ToString("mmssffff")}{Path.GetExtension(pic_list[i].ToString())}";
                                                    pics.Add(mobilePicPath);

                                                    string mobileId = DeviceConnectionManager.Instance.GetDeviceNameByMobileIndex(mobileIndex);

                                                    ProcessUtils.PushFileToVm(mobileId, pic_list[i].ToString(), mobilePicPath);

                                                }

                                                if (await SimulationTaskWrapper.UpdatePictures(mobileIndex, pics.ToArray()))
                                                {
                                                    await Task.Delay(4000);
                                                    updatePicsSucceeded = true;
                                                }

                                                if (await SimulationTaskWrapper.SearchClick(mobileIndex, SystemLanguageManager.Instance.ResourceManager.GetString("Photo", SystemLanguageManager.Instance.CultureInfo)))
                                                {
                                                    await Task.Delay(5000);

                                                    int x = 0, y = 0;
                                                    for (int i = 0; i < pic_list.Count; i++)
                                                    {
                                                        x = 39 + (i * 80);
                                                        y = 94 + (i * 80);

                                                        bool selectPicResult = await SimulationTaskWrapper.ClickCoordinate(mobileIndex, x, y);
                                                        await Task.Delay(2000);
                                                    }

                                                    await SimulationTaskWrapper.SearchClick(mobileIndex, SystemLanguageManager.Instance.ResourceManager.GetString("Done", SystemLanguageManager.Instance.CultureInfo));
                                                }

                                            }

                                            await Task.Delay(2000);

                                            if (await SimulationTaskWrapper.SearchClick(mobileIndex, SystemLanguageManager.Instance.ResourceManager.GetString("Next", SystemLanguageManager.Instance.CultureInfo)))
                                            {
                                                await Task.Delay(2000);

                                                if (await SimulationTaskWrapper.SearchClick(mobileIndex, SystemLanguageManager.Instance.ResourceManager.GetString("PostGoods", SystemLanguageManager.Instance.CultureInfo)))
                                                {
                                                    if (updatePicsSucceeded)
                                                    {
                                                        await Task.Delay(25000);

                                                        await SimulationTaskWrapper.DeletePictures(mobileIndex, pics.ToArray());
                                                    }

                                                    return true;
                                                }
                                            }


                                            if (updatePicsSucceeded)
                                            {
                                                await Task.Delay(25000);

                                                await SimulationTaskWrapper.DeletePictures(mobileIndex, pics.ToArray());
                                            }

                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                LogUtils.Error($"{ex}");
                return false;
            }
        }


        /// <summary>
        /// 更新任务执行结果
        /// </summary>
        /// <param name="tasknum"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public int UpdateTaskResval(int tasknum, string status)
        {
            int flag = TasksBLL.UpdateTaskRes(tasknum, status);


            return flag;
        }

        /// <summary>
        /// 更新号码发送结果
        /// </summary>
        /// <param name="phone"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        public int UpdatePhoneStatus(int phone, int response)
        {
            int res = PhonenumBLL.updatePhoneStatus(phone, response);

            return res;
        }

        /// <summary>
        /// 添加好友的好友2
        /// </summary>
        /// <param name="index"></param>
        /// <param name="nums"></param>
        /// <returns></returns>
        public static async Task<bool> AddFriendByFriend2(int index, int nums)
        {
            MonitorView.WriteThreadLog(index, SystemLanguageManager.Instance.ResourceManager.GetString("AddFriendByFriend", SystemLanguageManager.Instance.CultureInfo));
            try
            {
                if (await SimulationTaskWrapper.GotoHomepage(index))
                {
                    MonitorView.WriteThreadLog(index, SystemLanguageManager.Instance.ResourceManager.GetString("GotoHomePage_Success", SystemLanguageManager.Instance.CultureInfo));
                    await Task.Delay(3000);
                    //点击坐标
                    if (await SimulationTaskWrapper.ClickCoordinate(index, 120, 73))
                    {
                        //MonitorView.WriteThreadLog(index, "点击了坐标X:120-Y:73");
                        await Task.Delay(3000);
                        //是否存在打开按钮
                        bool openbut = await SimulationTaskWrapper.Search(index, SystemLanguageManager.Instance.ResourceManager.GetString("Facebook_Open", SystemLanguageManager.Instance.CultureInfo));
                        await Task.Delay(3000);
                        if (openbut)
                        {
                            MonitorView.WriteThreadLog(index, "找到了打开窗口");
                            if (!await SimulationTaskWrapper.ClickCoordinate(index, 14, 460))
                            {
                                MonitorView.WriteThreadLog(index, "关闭打开窗口失败");
                                return false;
                            }
                            else
                            {
                                MonitorView.WriteThreadLog(index, "关闭打开窗口成功");
                            }
                            await Task.Delay(3000);
                        }
                        //点击搜索好友 
                        string searchname = SystemLanguageManager.Instance.ResourceManager.GetString("Find_Friends", SystemLanguageManager.Instance.CultureInfo); //搜索好友 
                        string searchname2 = SystemLanguageManager.Instance.ResourceManager.GetString("Friends", SystemLanguageManager.Instance.CultureInfo); //好友
                        
                        if (!await SimulationTaskWrapper.SearchClick(index, searchname, 0))
                        {
                            await Task.Delay(1000);
                            if (!await SimulationTaskWrapper.SearchClick(index, searchname2, 0))
                            {
                                return false;
                            }
                        }
                        await Task.Delay(5000);
                        //点击全部好友
                        if (await SimulationTaskWrapper.SearchClick(index, SystemLanguageManager.Instance.ResourceManager.GetString("WhatsApp_AllFriends", SystemLanguageManager.Instance.CultureInfo), 0))
                        {
                            Console.WriteLine("点击了全部好友");
                            await Task.Delay(5000);
                            //循环添加好友的好友 
                            int i = 0;
                            string clickname1 = SystemLanguageManager.Instance.ResourceManager.GetString("More_Options", SystemLanguageManager.Instance.CultureInfo);
                            string clickname2 = SystemLanguageManager.Instance.ResourceManager.GetString("See_Friends", SystemLanguageManager.Instance.CultureInfo);
                            string clickname3 = SystemLanguageManager.Instance.ResourceManager.GetString("All", SystemLanguageManager.Instance.CultureInfo);
                            string clickname4 = SystemLanguageManager.Instance.ResourceManager.GetString("Add_Friend", SystemLanguageManager.Instance.CultureInfo);
                            string clickname5 = SystemLanguageManager.Instance.ResourceManager.GetString("Find_Friends", SystemLanguageManager.Instance.CultureInfo);


                            for (i = 0; i < nums;)
                            {
                                int flag = 0;
                                // 点击 更多信息…
                                await Task.Delay(5000);
                                if (await SimulationTaskWrapper.SearchClick(index, clickname1, i))//无法找到
                                {
                                    Console.WriteLine("点击每个好友的更多信息");
                                    await Task.Delay(2000);

                                    //点击查看好友
                                    if (await SimulationTaskWrapper.SearchClick(index, clickname2, 0))
                                    {
                                        await Task.Delay(5000);
                                        //点击全部
                                        if (await SimulationTaskWrapper.SearchClick(index, clickname3, 0))
                                        {
                                            await Task.Delay(5000);
                                            //判断是否没有好友


                                            //循环加为好友    
                                            int aw = 0;
                                            for (int j = 0; j <= 1;)
                                            {
                                                bool restmp = await SimulationTaskWrapper.SearchClick(index, clickname4);
                                                await Task.Delay(2000);
                                                if (!restmp)
                                                {
                                                    if (aw == 0)
                                                    {
                                                        await SimulationTaskWrapper.Swipe(index, 150, 450, 150, 300, 50);
                                                        aw = 1;
                                                    }
                                                    else
                                                    {
                                                        await SimulationTaskWrapper.Swipe(index, 150, 400, 150, 300, 50);
                                                    }

                                                    //判断是否没有好友                                                    
                                                    bool isnone = await SimulationTaskWrapper.Search(index, clickname5);
                                                    if (isnone)
                                                    {
                                                        break;
                                                    }
                                                }
                                                if (restmp)
                                                {
                                                    flag = 1;
                                                    j++;
                                                }
                                                await Task.Delay(3000);
                                                MonitorView.WriteThreadLog(index, SystemLanguageManager.Instance.ResourceManager.GetString("Add_Friend", SystemLanguageManager.Instance.CultureInfo) + i);
                                            }

                                            //回退
                                            await Task.Delay(2000);
                                            await SimulationTaskWrapper.GoBack(index);
                                            await Task.Delay(2000);
                                            //下滑
                                            if (i == 0)
                                            {
                                                await SimulationTaskWrapper.Swipe(index, 150, 200, 150, 89, 0);
                                            }
                                            else
                                            {
                                                await SimulationTaskWrapper.Swipe(index, 150, 200, 150, 123, 0);
                                            }
                                        }
                                    }

                                    
                                }
                                else
                                {
                                    if (i == 0)
                                    {
                                        await SimulationTaskWrapper.Swipe(index, 150, 200, 150, 89, 0);
                                    }
                                    else
                                    {
                                        await SimulationTaskWrapper.Swipe(index, 150, 200, 150, 123, 0);
                                    }
                                }
                                if (flag != 0)
                                    i++;
                            }
                            return true;
                        }
                    }
                    else
                    {
                        MonitorView.WriteThreadErrorLog(index, SystemLanguageManager.Instance.ResourceManager.GetString("AddFriendByFriend", SystemLanguageManager.Instance.CultureInfo) + "#101");                       
                    }                    
                }
                else
                {
                    MonitorView.WriteThreadLog(index, SystemLanguageManager.Instance.ResourceManager.GetString("GotoHomePage_Failure", SystemLanguageManager.Instance.CultureInfo));                    
                }
            }
            catch(Exception ex)
            {
                MonitorView.WriteThreadErrorLog(index, SystemLanguageManager.Instance.ResourceManager.GetString("AddFriendByFriend", SystemLanguageManager.Instance.CultureInfo) + "#103");                
            }
            return false;
        }

        /// <summary>
        /// 添加好友的好友
        /// </summary>
        /// <param name="index"></param>
        /// <param name="nums"></param>
        /// <returns></returns>
        public static async Task<bool> AddFriendByFriend(int index, int nums)
        {
            int flag = -1;
            try
            {
                if (await SimulationTaskWrapper.GotoHomepage(index))
                {
                    await Task.Delay(3000);
                    //点击坐标
                    if (await SimulationTaskWrapper.ClickCoordinate(index, 280, 73))
                    {
                        await Task.Delay(5000);

                        //点击搜索好友
                        //搜索点击“搜索好友”
                        string searchname = SystemLanguageManager.Instance.ResourceManager.GetString("Find_Friends", SystemLanguageManager.Instance.CultureInfo); //搜索好友 Find_Friends
                        string searchname2 = SystemLanguageManager.Instance.ResourceManager.GetString("Friends", SystemLanguageManager.Instance.CultureInfo); //好友 Find_Friends
                        if (!await SimulationTaskWrapper.SearchClick(index, searchname, 0))
                        {
                            await Task.Delay(1000);
                            if (!await SimulationTaskWrapper.SearchClick(index, searchname2, 0))
                            {
                                return false;
                            }
                        }

                        if (await SimulationTaskWrapper.SearchClick(index, SystemLanguageManager.Instance.ResourceManager.GetString("Find_Friends", SystemLanguageManager.Instance.CultureInfo), 0))
                        {
                            await Task.Delay(5000);

                            //点击好友                        
                            await Task.Delay(5000);

                            //添加好友的好友
                            //循环添加好友的好友
                            string clickname1 = SystemLanguageManager.Instance.ResourceManager.GetString("Remove_Friend", SystemLanguageManager.Instance.CultureInfo);//删除好友
                            string clickname3 = SystemLanguageManager.Instance.ResourceManager.GetString("WhatsApp_AllFriends", SystemLanguageManager.Instance.CultureInfo);
                            string clickname4 = SystemLanguageManager.Instance.ResourceManager.GetString("Add_Friend", SystemLanguageManager.Instance.CultureInfo);
                            int i = 0;
                            for (i = 0; i < nums; i++)
                            {
                                //点击Imageview
                                if (await SimulationTaskWrapper.SearchClickParent(index, clickname1, 1, i))
                                {
                                    await Task.Delay(5000);

                                    int swflag = 0;
                                    while (!await SimulationTaskWrapper.SearchClick(index, clickname3))
                                    {
                                        swflag++;
                                        await Task.Delay(2000);
                                        //下滑
                                        await SimulationTaskWrapper.Swipe(index, 100, 220, 100, 160, 4);
                                        if (swflag > 6)
                                        {
                                            swflag = -1;
                                            break;
                                        }
                                    }
                                    await Task.Delay(3000);
                                    if (swflag == -1)
                                    {
                                        //返回上一步
                                        await SimulationTaskWrapper.GoBack(index);
                                        await Task.Delay(3000);
                                        continue;
                                    }
                                    else
                                    {
                                        //点击加为好友 
                                        int clnums = 0;
                                        while (!await SimulationTaskWrapper.SearchClick(index, clickname4))
                                        {
                                            clnums++;
                                            await Task.Delay(2000);
                                            await SimulationTaskWrapper.Swipe(index, 150, 350, 150, 300, 5);

                                            await Task.Delay(3000);
                                            if (clnums >= 500)
                                            {
                                                break;
                                            }
                                        }
                                        return true;
                                    }
                                }
                                else
                                {
                                    return false;
                                }
                                //返回上二步
                                for (int j = 0; j < 2; j++)
                                {
                                    await SimulationTaskWrapper.GoBack(index);
                                    await Task.Delay(3000);
                                }
                            }
                        }
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch(Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// 添加主页好友
        /// </summary>
        /// <param name="index"></param>
        /// <param name="nums"></param>
        /// <param name="pagename"></param>
        /// <returns></returns>
        public static async Task<bool> AddHomePageFriend(int index, int nums, string pagename)
        {
            MonitorView.WriteThreadLog(index, SystemLanguageManager.Instance.ResourceManager.GetString("AddHomePageFriend", SystemLanguageManager.Instance.CultureInfo));
            try
            {
                //回到主页
                if (await SimulationTaskWrapper.GotoHomepage(index))
                {
                    MonitorView.WriteThreadLog(index, SystemLanguageManager.Instance.ResourceManager.GetString("GotoHomePage_Success", SystemLanguageManager.Instance.CultureInfo));
                    await Task.Delay(2000);

                    await System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        for(int j=0; j<2;j++)
                        {
                            System.Windows.Forms.Clipboard.SetText(pagename);
                            Thread.Sleep(2000);
                        }
                        
                    }));

                    await Task.Delay(6000);

                    //点击搜索
                    string clickname1 = SystemLanguageManager.Instance.ResourceManager.GetString("Search", SystemLanguageManager.Instance.CultureInfo);
                    if(await SimulationTaskWrapper.SearchClick(index, clickname1, 0))
                    {
                        await Task.Delay(2000);

                        //在搜索中粘贴主页名称
                        if(await SimulationTaskWrapper.SearchPaste(index, clickname1, 0))
                        {
                            await Task.Delay(3000);
                            //点击搜索
                            string searchname2 = SystemLanguageManager.Instance.ResourceManager.GetString("Facebook_SearchResult", SystemLanguageManager.Instance.CultureInfo);  //"查看{0}的搜索结果";
                            if(await SimulationTaskWrapper.SearchClick(index, string.Format(searchname2, pagename.ToLower()), 0))
                            {
                                MonitorView.WriteThreadLog(index, string.Format("点击：点击查看{0}的搜索结果 成功", pagename.ToLower()));
                                await Task.Delay(3000);
                                Console.WriteLine("点击查看{0}的搜索结果", pagename.ToLower());

                                //点击主页
                                string clickname2 = SystemLanguageManager.Instance.ResourceManager.GetString("Facebook_MainPage", SystemLanguageManager.Instance.CultureInfo);//主页 Facebook_MainPage
                                if(await SimulationTaskWrapper.SearchClick(index, clickname2, 0))
                                {
                                    await Task.Delay(5000);

                                    //点击搜索出来的第一个坐标
                                    if (await SimulationTaskWrapper.ClickCoordinate(index, 170, 120))
                                    {
                                        await Task.Delay(5000);

                                        //点击帖子
                                        string clickname3 = SystemLanguageManager.Instance.ResourceManager.GetString("Facebook_Post", SystemLanguageManager.Instance.CultureInfo); // 帖子
                                        if(await SimulationTaskWrapper.SearchClick(index, clickname3, 0))
                                        {
                                            await Task.Delay(5000);

                                            //点击评论
                                            //选择评论
                                            string clickname4 = SystemLanguageManager.Instance.ResourceManager.GetString("Add_PageFriend_Comment", SystemLanguageManager.Instance.CultureInfo);
                                            int tryclick = 0;
                                            while (!await SimulationTaskWrapper.SearchClick(index, clickname4))
                                            {
                                                tryclick++;
                                                await Task.Delay(2000);
                                                //下滑
                                                await SimulationTaskWrapper.Swipe(index, 100, 260, 100, 160, 4);
                                                if(tryclick>=15)
                                                {
                                                    return false;
                                                }
                                            }
                                            await Task.Delay(3000);
                                            //点击左上角
                                            if(await SimulationTaskWrapper.ClickCoordinate(index, 160, 38))
                                            {
                                                await Task.Delay(3000);

                                                //循环添加好友
                                                int i = 1;
                                                string clickname5 = SystemLanguageManager.Instance.ResourceManager.GetString("Add_Friend", SystemLanguageManager.Instance.CultureInfo);

                                                for (i = 1; i <= nums; i++)
                                                {
                                                    while (!await SimulationTaskWrapper.SearchClick(index, clickname5))
                                                    {
                                                        await Task.Delay(2000);

                                                        await SimulationTaskWrapper.Swipe(index, 150, 350, 150, 300, 5);

                                                        await Task.Delay(2000);
                                                    }
                                                    await Task.Delay(3000);
                                                    MonitorView.WriteThreadLog(index, SystemLanguageManager.Instance.ResourceManager.GetString("Add_Friend", SystemLanguageManager.Instance.CultureInfo) + i);
                                                }
                                                return true;
                                            }                                            
                                        }
                                    }                                    
                                }                                
                            }  
                            else
                            {
                                MonitorView.WriteThreadLog(index, string.Format("点击：点击查看{0}的搜索结果 失败", pagename.ToLower()));
                            }
                        }                      
                    }                 
                }
                else
                {
                    MonitorView.WriteThreadLog(index, SystemLanguageManager.Instance.ResourceManager.GetString("GotoHomePage_Failure", SystemLanguageManager.Instance.CultureInfo));                    
                }
            }
            catch(Exception ex)
            {
                MonitorView.WriteThreadErrorLog(index, SystemLanguageManager.Instance.ResourceManager.GetString("AddHomePageFriend", SystemLanguageManager.Instance.CultureInfo));                
            }
            return false;
        }

        /// <summary>
        /// 添加通讯录好友
        /// </summary>
        /// <param name="index"></param>
        /// <param name="nums"></param>
        /// <returns></returns>
        public static async Task<bool> AddListPhoneFriend(int index, int nums)
        {
            MonitorView.WriteThreadLog(index, SystemLanguageManager.Instance.ResourceManager.GetString("AddListPhoneNums", SystemLanguageManager.Instance.CultureInfo));
            try
            {
                if (await SimulationTaskWrapper.GotoHomepage(index))
                {
                    MonitorView.WriteThreadLog(index, SystemLanguageManager.Instance.ResourceManager.GetString("GotoHomePage_Success", SystemLanguageManager.Instance.CultureInfo));
                    await Task.Delay(2000);
                    //进入我的好友界面
                    if (await SimulationTaskWrapper.ClickCoordinate(index, 280, 73))
                    {
                        await Task.Delay(6000);
                        //搜索点击“搜索好友”
                        string searchname = SystemLanguageManager.Instance.ResourceManager.GetString("Find_Friends", SystemLanguageManager.Instance.CultureInfo); //搜索好友 Find_Friends
                        string searchname2 = SystemLanguageManager.Instance.ResourceManager.GetString("Friends", SystemLanguageManager.Instance.CultureInfo); //好友 Find_Friends
                        if (!await SimulationTaskWrapper.SearchClick(index, searchname, 0))
                        {
                            await Task.Delay(1000);
                            if (!await SimulationTaskWrapper.SearchClick(index, searchname2, 0))
                            {
                                return false;
                            }
                        }

                        await Task.Delay(5000);
                        //点击通讯录
                        string clickname = SystemLanguageManager.Instance.ResourceManager.GetString("Facebook_PhoneListFriend", SystemLanguageManager.Instance.CultureInfo); //通讯录 
                        if (await SimulationTaskWrapper.SearchClick(index, clickname, 0))
                        {
                            await Task.Delay(5000);
                            //判断是否存在通讯录好友                            

                            /*string clickname4 = SystemLanguageManager.Instance.ResourceManager.GetString("Facebook_ManageContacts", SystemLanguageManager.Instance.CultureInfo); //管理已导入的联系人
                            string clickname5 = SystemLanguageManager.Instance.ResourceManager.GetString("Facebook_HaveNoneImport", SystemLanguageManager.Instance.CultureInfo); //你还没有导入任何联系人信息。
                            if (!await SimulationTaskWrapper.Search(index, clickname4))
                            {
                                await Task.Delay(10000);
                                if (await SimulationTaskWrapper.Search(index, clickname4))
                                {
                                    Console.WriteLine("点击了你还没有导入任何联系人信息。");
                                    await Task.Delay(5000);
                                    return true;
                                }
                                else
                                {
                                    Console.WriteLine("点击了你还没有导入任何联系人信息失败");
                                    tns = 0;
                                    //回退
                                    await SimulationTaskWrapper.GoBack(index);
                                }
                            }*/

                            //检查是否存在 以后再说 

                            string clickname3 = SystemLanguageManager.Instance.ResourceManager.GetString("Facebook_Talk_later", SystemLanguageManager.Instance.CultureInfo);
                            if (!await SimulationTaskWrapper.Search(index, clickname3))
                            {
                                return true;    //通讯录中没有好友
                            }
                            //循环添加好友
                            string clickname2 = SystemLanguageManager.Instance.ResourceManager.GetString("Add_Friend", SystemLanguageManager.Instance.CultureInfo); //加为好友 

                            /* if (tns != 0)
                                nums = tns;*/
                            int i = 1;
                            int tns = 0;
                            for (i = 1; i <= nums; i++)
                            {
                                while (true)
                                {
                                    tns++;
                                    bool findflag = await SimulationTaskWrapper.Search(index, clickname3);
                                    await Task.Delay(3000);
                                    if (findflag)
                                    {
                                        await SimulationTaskWrapper.SearchClick(index, clickname2);
                                        await Task.Delay(2000);
                                        break;
                                    }
                                    else
                                    {
                                        await Task.Delay(2000);
                                        await SimulationTaskWrapper.Swipe(index, 150, 350, 150, 300, 5);
                                    }
                                    if (tns > 10)
                                    {
                                        break;
                                    }

                                }
                                MonitorView.WriteThreadLog(index, SystemLanguageManager.Instance.ResourceManager.GetString("Add_Friend", SystemLanguageManager.Instance.CultureInfo) + i);
                            }
                            return true;
                        }
                        else
                        {
                            MonitorView.WriteThreadErrorLog(index, SystemLanguageManager.Instance.ResourceManager.GetString("AddListPhoneNums", SystemLanguageManager.Instance.CultureInfo) + "#101");
                            return false;
                        }
                    }
                    else
                    {
                        MonitorView.WriteThreadErrorLog(index, SystemLanguageManager.Instance.ResourceManager.GetString("AddListPhoneNums", SystemLanguageManager.Instance.CultureInfo)+"#100");
                        return false;
                    }
                }
                else
                {
                    MonitorView.WriteThreadLog(index, SystemLanguageManager.Instance.ResourceManager.GetString("GotoHomePage_Failure", SystemLanguageManager.Instance.CultureInfo));
                    return false;
                }
            }
            catch(Exception ex)
            {
                MonitorView.WriteThreadErrorLog(index, SystemLanguageManager.Instance.ResourceManager.GetString("AddListPhoneNums", SystemLanguageManager.Instance.CultureInfo)+"#102");
                return false;
            }
        }

        /// <summary>
        /// 通过好友请求
        /// </summary>
        /// <param name="index"></param>
        /// <param name="nums"></param>
        /// <returns></returns>
        public static async Task<bool> AllowRequestFriend(int index, int nums)
        {
            MonitorView.WriteThreadLog(index, SystemLanguageManager.Instance.ResourceManager.GetString("Facebook_AllowFriend", SystemLanguageManager.Instance.CultureInfo));
            try
            {
                if (await SimulationTaskWrapper.GotoHomepage(index))
                {
                    MonitorView.WriteThreadLog(index, SystemLanguageManager.Instance.ResourceManager.GetString("GotoHomePage_Success", SystemLanguageManager.Instance.CultureInfo));
                    await Task.Delay(2000);
                    //进入我的好友界面
                    if (await SimulationTaskWrapper.ClickCoordinate(index, 280, 73))
                    {
                        await Task.Delay(6000);
                        //搜索点击“搜索好友”
                        string searchname = SystemLanguageManager.Instance.ResourceManager.GetString("Find_Friends", SystemLanguageManager.Instance.CultureInfo); //搜索好友 Find_Friends
                        string searchname2 = SystemLanguageManager.Instance.ResourceManager.GetString("Friends", SystemLanguageManager.Instance.CultureInfo); //好友 Find_Friends
                        if (!await SimulationTaskWrapper.SearchClick(index, searchname, 0))
                        {
                            await Task.Delay(1000);
                            if (!await SimulationTaskWrapper.SearchClick(index, searchname2, 0))
                            {
                                return false;
                            }
                        }

                        await Task.Delay(6000);
                        //点击请求
                        string clickname1 = SystemLanguageManager.Instance.ResourceManager.GetString("Facebook_Request", SystemLanguageManager.Instance.CultureInfo); //请求 Facebook_Request
                        if (await SimulationTaskWrapper.SearchClick(index, clickname1, 0))
                        {
                            await Task.Delay(5000);

                            //判断是否存在新请求 
                            string clickname3 = SystemLanguageManager.Instance.ResourceManager.GetString("Facebook_HaveNoneRequest", SystemLanguageManager.Instance.CultureInfo); //没有新请求
                            if (await SimulationTaskWrapper.Search(index, clickname3))
                            {
                                Console.WriteLine("没有新的待确认请求");
                                return true;
                            }

                            //循环确认请求
                            string clickname2 = SystemLanguageManager.Instance.ResourceManager.GetString("Facebook_ConfirmFriends", SystemLanguageManager.Instance.CultureInfo); //确认  
                            int i = 1;
                            for (i = 1; i <= nums; i++)
                            {
                                int trynums = 0;
                                while (!await SimulationTaskWrapper.SearchClick(index, clickname2))
                                {
                                    trynums++;
                                    await Task.Delay(2000);

                                    await SimulationTaskWrapper.Swipe(index, 150, 350, 150, 300, 5);

                                    await Task.Delay(3000);
                                    if(trynums>=10)
                                    {
                                        break;
                                    }
                                }
                                await Task.Delay(3000);
                                MonitorView.WriteThreadLog(index, SystemLanguageManager.Instance.ResourceManager.GetString("Add_Friend", SystemLanguageManager.Instance.CultureInfo) + i);
                            }
                            return true;
                        }
                        else
                        {
                            MonitorView.WriteThreadErrorLog(index, SystemLanguageManager.Instance.ResourceManager.GetString("Facebook_AllowFriend", SystemLanguageManager.Instance.CultureInfo) + "#101");
                            return false;
                        }
                    }
                    else
                    {
                        MonitorView.WriteThreadErrorLog(index, SystemLanguageManager.Instance.ResourceManager.GetString("Facebook_AllowFriend", SystemLanguageManager.Instance.CultureInfo) + "#102");
                        return false;
                    }
                }
                else
                {
                    MonitorView.WriteThreadLog(index, SystemLanguageManager.Instance.ResourceManager.GetString("GotoHomePage_Failure", SystemLanguageManager.Instance.CultureInfo));
                    return false;
                }
            }
            catch(Exception ex)
            {
                MonitorView.WriteThreadErrorLog(index, SystemLanguageManager.Instance.ResourceManager.GetString("Facebook_AllowFriend", SystemLanguageManager.Instance.CultureInfo)+"#103");
                return false;
            }
        }

        /// <summary>
        /// 添加推荐的好友
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static async Task<bool> AddRecommFriend(int index, int nums)
        {
            try
            {
                MonitorView.WriteThreadLog(index, SystemLanguageManager.Instance.ResourceManager.GetString("AddRecommFriend", SystemLanguageManager.Instance.CultureInfo));
                if (await SimulationTaskWrapper.GotoHomepage(index))
                {
                    MonitorView.WriteThreadLog(index, SystemLanguageManager.Instance.ResourceManager.GetString("GotoHomePage_Success", SystemLanguageManager.Instance.CultureInfo));
                    await Task.Delay(2000);
                    //进入我的好友界面
                    if (await SimulationTaskWrapper.ClickCoordinate(index, 280, 73))
                    {                        
                        await Task.Delay(6000);

                        //搜索点击“搜索好友”
                        string searchname = SystemLanguageManager.Instance.ResourceManager.GetString("Find_Friends", SystemLanguageManager.Instance.CultureInfo); //搜索好友 Find_Friends
                        string searchname2 = SystemLanguageManager.Instance.ResourceManager.GetString("Friends", SystemLanguageManager.Instance.CultureInfo); //好友 Find_Friends
                        if (!await SimulationTaskWrapper.SearchClick(index, searchname, 0))
                        {
                            await Task.Delay(1000);
                            if(!await SimulationTaskWrapper.SearchClick(index, searchname2, 0))
                            {
                                return false;
                            }
                        }
                        await Task.Delay(6000);

                        //点击推荐

                        await Task.Delay(2000);
                        string clickname2 = SystemLanguageManager.Instance.ResourceManager.GetString("Facebook_Friend_Advise", SystemLanguageManager.Instance.CultureInfo); //建议
                        if(await SimulationTaskWrapper.SearchClick(index, clickname2))
                        {
                            await Task.Delay(3000);

                            MonitorView.WriteThreadLog(index, SystemLanguageManager.Instance.ResourceManager.GetString("Facebook_Friend_Advise", SystemLanguageManager.Instance.CultureInfo));

                            //点击加为好友
                            string clickname = SystemLanguageManager.Instance.ResourceManager.GetString("Add_Friend", SystemLanguageManager.Instance.CultureInfo); //加为好友
                            int i = 1;
                            for (i = 1; i <= nums; i++)
                            {
                                while (!await SimulationTaskWrapper.SearchClick(index, clickname))
                                {
                                    await Task.Delay(2000);

                                    await SimulationTaskWrapper.Swipe(index, 150, 350, 150, 300, 5);

                                    await Task.Delay(3000);
                                }
                                await Task.Delay(3000);
                                MonitorView.WriteThreadLog(index, SystemLanguageManager.Instance.ResourceManager.GetString("Add_Friend", SystemLanguageManager.Instance.CultureInfo) + i);
                            }
                            return true;
                        }
                    }
                    else
                    {
                        MonitorView.WriteThreadLog(index, SystemLanguageManager.Instance.ResourceManager.GetString("GotoHomePage_Failure", SystemLanguageManager.Instance.CultureInfo));                        
                    }
                }
                else
                {
                    MonitorView.WriteThreadErrorLog(index, SystemLanguageManager.Instance.ResourceManager.GetString("AddRecommFriend", SystemLanguageManager.Instance.CultureInfo) + "#102");
                }
            }
            catch(Exception ex)
            {
                MonitorView.WriteThreadErrorLog(index, SystemLanguageManager.Instance.ResourceManager.GetString("AddRecommFriend", SystemLanguageManager.Instance.CultureInfo)+"#103");                
            }
            return false;
        }

        /// <summary>
        /// 关注主页
        /// </summary>
        /// <param name="mobileIndex"></param>
        /// <param name="homePageName"></param>
        /// <param name="nums"></param>
        /// <returns></returns>  
        public static async Task<bool> SubscribeHomepageAsync(int mobileIndex, string homePageName, int addTimes)
        {
            //回到主页   
            if (await SimulationTaskWrapper.GotoHomepage(mobileIndex))
            {
                await Task.Delay(3000);

                //点击坐标（70,30）
                if (await SimulationTaskWrapper.ClickCoordinate(mobileIndex, 70, 30))
                {
                    await Task.Delay(2000);
                    //点击搜索结果列表
                    if (await SimulationTaskWrapper.SearchPaste(mobileIndex, homePageName, 0))
                    {
                        await Task.Delay(2000);
                        //点击搜索列表第一个列
                        if (await SimulationTaskWrapper.ClickCoordinate(mobileIndex, 100, 75))
                        {
                            await Task.Delay(2000);
                        }
                    }
                }
            }
            //点击“主页”
            if (await SimulationTaskWrapper.ClickCoordinate(mobileIndex, 200, 75))
            {
                await Task.Delay(2000);
                //点击筛选条件列表第一列
                if (await SimulationTaskWrapper.ClickCoordinate(mobileIndex, 170, 125))
                {
                    await Task.Delay(2000);
                    //点击“...”
                    if (await SimulationTaskWrapper.ClickCoordinate(mobileIndex, 300, 270))
                    {
                        await Task.Delay(2000);
                        //点击“关注”
                        if (await SimulationTaskWrapper.ClickCoordinate(mobileIndex, 50, 300))
                        {
                            await Task.Delay(2000);
                            //返回主页
                            if (await SimulationTaskWrapper.GotoHomepage(mobileIndex))
                            {
                                await Task.Delay(3000);
                            }
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 邀请好友进小组
        /// </summary>
        /// <param name="mobileIndex"></param>
        /// <param name="homePageName"></param>
        /// <param name="addTimes"></param>
        /// <returns></returns>
        public static async Task<bool> InviteFriendsToGroupAsync(int mobileIndex, string homePageName, int addTimes)
        {
            ////回到主页   
            if (await SimulationTaskWrapper.GotoHomepage(mobileIndex))
            {
                await Task.Delay(2000);

                //点击坐标（70,30）
                if (await SimulationTaskWrapper.ClickCoordinate(mobileIndex, 70, 30))
                {
                    await Task.Delay(3000);
                    //搜索粘贴
                    if (await SimulationTaskWrapper.SearchPaste(mobileIndex, homePageName,0))
                    {
                        await Task.Delay(3000);
                        //点击搜索结果
                        if(await SimulationTaskWrapper.SearchClick(mobileIndex, string.Format(SystemLanguageManager.Instance.ResourceManager.GetString("Search_Result",
                             SystemLanguageManager.Instance.CultureInfo), homePageName.ToLower()), 0))
                        {
                        }
                     }
                }
            }
            await Task.Delay(3000);
            //点击添加
            if (await SimulationTaskWrapper.ClickCoordinate(mobileIndex, 300, 225))
                {
                      await Task.Delay(2000);
                if (await SimulationTaskWrapper.SearchClick(mobileIndex, SystemLanguageManager.Instance.ResourceManager.GetString("Add", SystemLanguageManager.Instance.CultureInfo)))
                {                
                }
            }
            await Task.Delay(2000);
            int X = 295;
            int Y = 150;
            int length = 7;
            int swflag = 0;
            for (int i =swflag; i < length; i++)
            {
                swflag++;
                if (await SimulationTaskWrapper.ClickCoordinate(mobileIndex, X, Y))
                {
                    await Task.Delay(2000);
                    Y = Y + 40;
                }
                //if (swflag==6)
                //{
                //    await SimulationTaskWrapper.Swipe(mobileIndex, 100, 470, 100, 0, 20);
                //}
            }
            await Task.Delay(2000);
            if (await SimulationTaskWrapper.SearchClick(mobileIndex, SystemLanguageManager.Instance.ResourceManager.GetString("Complete", SystemLanguageManager.Instance.CultureInfo)))
            {
                await Task.Delay(2000);
            }
            return false;
        }

        /// <summary>
        /// 邀请好友点赞
        /// </summary>
        /// <param name="mobileIndex"></param>
        /// <param name="homePageName"></param>
        /// <param name="addTimes"></param>
        /// <returns></returns>
        public static async Task<bool> InviteFriendsToLikeAsync(int mobileIndex, string homePageName, int addTimes)
        {
            //回到主页   
            if (await SimulationTaskWrapper.GotoHomepage(mobileIndex))
            {
                await Task.Delay(2000);

                //点击坐标（70,30）
                if (await SimulationTaskWrapper.ClickCoordinate(mobileIndex, 70, 30))
                {
                    await Task.Delay(1500);
                    if (await SimulationTaskWrapper.Assign(mobileIndex, SystemLanguageManager.Instance.ResourceManager.GetString("Search", SystemLanguageManager.Instance.CultureInfo), homePageName.ToLower()))
                    {

                    }
                }
            }

             await Task.Delay(3000);
            //点击搜索结果
            string searchname2 = SystemLanguageManager.Instance.ResourceManager.GetString("Search", SystemLanguageManager.Instance.CultureInfo);  //"查看{0}的搜索结果";
            await SimulationTaskWrapper.SearchClick(mobileIndex, string.Format(searchname2, homePageName.ToLower()), 0);
            if (await SimulationTaskWrapper.ClickCoordinate(mobileIndex, 80, 106)) 
            {
                await Task.Delay(2000);
                //点击列表第一行“主页”
                if (await SimulationTaskWrapper.ClickCoordinate(mobileIndex, 200, 70))
                {
                    await Task.Delay(2000);
                    //点击搜索结果第一列
                    if (await SimulationTaskWrapper.ClickCoordinate(mobileIndex, 150, 128))
                    {
                    }
                }
            }

            await Task.Delay(2000);
            //搜索点击“社群”
            if (await SimulationTaskWrapper.SearchClick(mobileIndex, SystemLanguageManager.Instance.ResourceManager.GetString("Community",
                        SystemLanguageManager.Instance.CultureInfo)))
            {
                await Task.Delay(3000);

            //搜索点击“邀请好友赞主页”
            if (await SimulationTaskWrapper.SearchClick(mobileIndex, SystemLanguageManager.Instance.ResourceManager.GetString("Invite_Friends_Praise",
                    SystemLanguageManager.Instance.CultureInfo)))
            {
                await Task.Delay(3000);  

            //点击“邀请”
            await Task.Delay(4000);
            int X = 300;
            int Y = 140;
            int length = 7;
            int swflag = 0;
            for (int i = swflag; i < length; i++)
            {
                swflag++;
                if (await SimulationTaskWrapper.ClickCoordinate(mobileIndex, X, Y))
                {
                    await Task.Delay(2000);
                    Y = Y + 40;
                }
            }
            }
            }
            return false;
        }

        /// <summary>
        /// 发送主页-快转
        /// </summary>
        /// <param name="mobileIndex"></param>
        /// <param name="homePageName"></param>
        /// <returns></returns>
        public static async Task<bool> SendHomepageAsync(int mobileIndex, string homePageName)
        {
            // 1.回到主页
            if (await SimulationTaskWrapper.GotoHomepage(mobileIndex))
            {
                await Task.Delay(3000);
                    //点击主页搜索框
                    if (await SimulationTaskWrapper.ClickCoordinate(mobileIndex, 70, 30))
                    {
                        await Task.Delay(1500);
                       // 在搜索框填写关键字
                        if (await SimulationTaskWrapper.SearchPaste(mobileIndex, homePageName))
                        {
                            await Task.Delay(2000);
                            //点击搜索结果列表
                            if (await SimulationTaskWrapper.ClickCoordinate(mobileIndex, 87, 50))
                            {
                                await Task.Delay(2000);
                                //点击已搜索到的小组
                                if (await SimulationTaskWrapper.ClickCoordinate(mobileIndex, 70, 150))
                                {
                                }
                            }
                        }
                    }
            }

            await Task.Delay(2000);
            //向上滑动
            if (await SimulationTaskWrapper.Swipe(mobileIndex, 100, 470, 100, 0, 20))
            {
                await Task.Delay(2000);
                //搜索点击"分享"
                string searchname = SystemLanguageManager.Instance.ResourceManager.GetString("Share", SystemLanguageManager.Instance.CultureInfo);
                if (await SimulationTaskWrapper.SearchClick(mobileIndex, searchname, 0))
                {
                    await Task.Delay(5000);
                    //搜索点击"快转"
                    string searchQuickTurn = SystemLanguageManager.Instance.ResourceManager.GetString("Quick_Turn", SystemLanguageManager.Instance.CultureInfo);
                    if (await SimulationTaskWrapper.SearchClick(mobileIndex, searchQuickTurn, 0))
                    {
                        await Task.Delay(2000);
                    }
                    if (await SimulationTaskWrapper.ClickCoordinate(mobileIndex, 270, 390))
                    {
                        await Task.Delay(2000);
                        if (await SimulationTaskWrapper.ClickCoordinate(mobileIndex, 236, 308))
                        {
                            await Task.Delay(2000);
                            if (await SimulationTaskWrapper.ClickCoordinate(mobileIndex, 236, 362))
                            {
                                await Task.Delay(2000);
                                if (await SimulationTaskWrapper.ClickCoordinate(mobileIndex, 289, 40))
                                {
                                    await Task.Delay(2000);
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 发送主页-发帖
        /// </summary>
        /// <param name="mobileIndex"></param>
        /// <param name="homePageName"></param>
        /// <returns></returns>
        public static async Task<bool> SendHomepageAsyncPosting(int mobileIndex, string homePageName)
        {
            // 1.回到主页
            if (await SimulationTaskWrapper.GotoHomepage(mobileIndex))
            {
                await Task.Delay(3000);
                //点击主页搜索框
                if (await SimulationTaskWrapper.ClickCoordinate(mobileIndex, 70, 30))
                {
                    await Task.Delay(1500);
                    //在搜索框填写关键字
                    if (await SimulationTaskWrapper.SearchPaste(mobileIndex, homePageName))
                    {
                        await Task.Delay(2000);
                        //点击搜索结果列表
                        if (await SimulationTaskWrapper.ClickCoordinate(mobileIndex, 87, 50))
                        {
                            await Task.Delay(3000);
                            //点击已搜索到的小组
                            if (await SimulationTaskWrapper.ClickCoordinate(mobileIndex, 70, 150))
                            {
                            }
                        }
                    }
                }
            }

            await Task.Delay(3000);
            //向上滑动
            if (await SimulationTaskWrapper.Swipe(mobileIndex, 100, 470, 100, 0, 20))
            {
                await Task.Delay(2000);
                //搜索点击"分享"
                string searchname = SystemLanguageManager.Instance.ResourceManager.GetString("Share", SystemLanguageManager.Instance.CultureInfo);
                if (await SimulationTaskWrapper.SearchClick(mobileIndex, searchname, 0))
                {
                    await Task.Delay(4000);
                    //搜索点击"发帖"
                    string searchPosting = SystemLanguageManager.Instance.ResourceManager.GetString("Posting", SystemLanguageManager.Instance.CultureInfo);
                    if (await SimulationTaskWrapper.SearchClick(mobileIndex, searchPosting, 0))
                    {
                       
                    }
                }
            }
            await Task.Delay(2000);
            //搜索点击"发布"
            string searchRelease = SystemLanguageManager.Instance.ResourceManager.GetString("Release", SystemLanguageManager.Instance.CultureInfo);
            if (await SimulationTaskWrapper.SearchClick(mobileIndex, searchRelease, 0))
            {
                await Task.Delay(3000);
            }
            return false;
        }

        /// <summary>
        /// 发送主页-分组通过消息发送
        /// </summary>
        /// <param name="mobileIndex"></param>
        /// <param name="homePageName"></param>
        /// <returns></returns>
        public static async Task<bool> SendHomepageAsyncMessaging(int mobileIndex, string homePageName)
        {
            // 1.回到主页
            if (await SimulationTaskWrapper.GotoHomepage(mobileIndex))
            {
                await Task.Delay(3000);
                //点击主页搜索框
                if (await SimulationTaskWrapper.ClickCoordinate(mobileIndex, 70, 30))
                {
                    await Task.Delay(1500);
                    //在搜索框填写关键字
                    if (await SimulationTaskWrapper.SearchPaste(mobileIndex, homePageName))
                    {
                        await Task.Delay(3000);
                        //点击搜索结果列表
                        if (await SimulationTaskWrapper.ClickCoordinate(mobileIndex, 87, 50))
                        {
                            await Task.Delay(3000);
                            //点击已搜索到的小组
                            if (await SimulationTaskWrapper.ClickCoordinate(mobileIndex, 70, 150))
                            {
                            }
                        }
                    }
                }
            }

            await Task.Delay(2000);
            //向上滑动
            if (await SimulationTaskWrapper.Swipe(mobileIndex, 100, 470, 100, 0, 20))
            {
                await Task.Delay(2000);
                //搜索点击"分享"
                string searchname = SystemLanguageManager.Instance.ResourceManager.GetString("Share", SystemLanguageManager.Instance.CultureInfo);
                if (await SimulationTaskWrapper.SearchClick(mobileIndex, searchname, 0))
                {
                    await Task.Delay(4000);
                    //搜索点击"通过消息发送"
                    string searchPosting = SystemLanguageManager.Instance.ResourceManager.GetString("Messaging", SystemLanguageManager.Instance.CultureInfo);
                    if (await SimulationTaskWrapper.SearchClick(mobileIndex, searchPosting, 0))
                    {
                        await Task.Delay(2000);
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 添加小组内的好友
        /// </summary>
        /// <param name="mobileIndex"></param>
        /// <param name="nums"></param>
        /// <param name="groupname"></param>
        /// <returns></returns>
        public static async Task<bool> AddGroupFriends(int mobileIndex,int nums, string groupname)
        {
            MonitorView.WriteThreadLog(mobileIndex, SystemLanguageManager.Instance.ResourceManager.GetString("Facebook_AddGroupUser", SystemLanguageManager.Instance.CultureInfo));

            try
            {
                if (await SimulationTaskWrapper.GotoHomepage(mobileIndex))
                {
                    //点击坐标
                    MonitorView.WriteThreadLog(mobileIndex, SystemLanguageManager.Instance.ResourceManager.GetString("GotoHomePage_Success", SystemLanguageManager.Instance.CultureInfo));
                    await Task.Delay(2000);
                    //进入我的好友界面
                    if (await SimulationTaskWrapper.ClickCoordinate(mobileIndex, 280, 73))
                    {
                        await Task.Delay(2000);
                        //点击小组 
                        string clickname1 = SystemLanguageManager.Instance.ResourceManager.GetString("Facebook_Group", SystemLanguageManager.Instance.CultureInfo);
                        if (await SimulationTaskWrapper.SearchClick(mobileIndex, clickname1))
                        {
                            await Task.Delay(2000);
                            //模拟匹配小组名称 FuzzySearchClick
                            int trynums = 0;
                            while (!await SimulationTaskWrapper.SearchClick(mobileIndex, groupname,0))
                            {
                                await Task.Delay(2000);
                                await SimulationTaskWrapper.Swipe(mobileIndex, 150, 300, 150, 200, 5);
                                trynums++;
                                if(trynums>=5)
                                {
                                    MonitorView.WriteThreadLog(mobileIndex, SystemLanguageManager.Instance.ResourceManager.GetString("Facebook_NotFoundGroup", SystemLanguageManager.Instance.CultureInfo));
                                    return false;
                                }
                            }
                            //点击成员
                            string clickname2 = SystemLanguageManager.Instance.ResourceManager.GetString("Facebook_Member", SystemLanguageManager.Instance.CultureInfo);
                            if(await SimulationTaskWrapper.SearchClick(mobileIndex, clickname2))
                            {
                                MonitorView.WriteThreadLog(mobileIndex, "点击成员成功");
                                await Task.Delay(2000);
                                //循环添加
                                string clickname3 = SystemLanguageManager.Instance.ResourceManager.GetString("Add_Friend", SystemLanguageManager.Instance.CultureInfo);
                                int i = 1;
                                for (i = 1; i <= nums; i++)
                                {
                                    int trynum2 = 0;
                                    while (!await SimulationTaskWrapper.SearchClick(mobileIndex, clickname3))
                                    {
                                        MonitorView.WriteThreadLog(mobileIndex, "界面中没好友，执行下滑");
                                        trynum2++;
                                        await Task.Delay(2000);

                                        await SimulationTaskWrapper.Swipe(mobileIndex, 150, 300, 150, 200, 5);

                                        await Task.Delay(3000);
                                        if(trynum2>=10)
                                        {
                                            MonitorView.WriteThreadLog(mobileIndex, "没有可以添加的好友了");
                                            return false;
                                        }
                                    }
                                    await Task.Delay(3000);
                                    MonitorView.WriteThreadLog(mobileIndex, SystemLanguageManager.Instance.ResourceManager.GetString("Facebook_AddGroupUser", SystemLanguageManager.Instance.CultureInfo) + i);
                                }
                                return true;
                            }
                            else
                            {
                                MonitorView.WriteThreadLog(mobileIndex, "点击成员失败.");
                            }
                        }
                    }
                }
                else
                {
                    MonitorView.WriteThreadErrorLog(mobileIndex, SystemLanguageManager.Instance.ResourceManager.GetString("Facebook_AddGroupUser", SystemLanguageManager.Instance.CultureInfo) + "#102");
                }
            }
            catch (Exception ex)
            {
                LogUtils.Error("添加群组好友失败："+ex.ToString());
                MonitorView.WriteThreadErrorLog(mobileIndex, SystemLanguageManager.Instance.ResourceManager.GetString("Facebook_AddGroupUser", SystemLanguageManager.Instance.CultureInfo) + "#103");
            }
            return false;
        }
    }

}

