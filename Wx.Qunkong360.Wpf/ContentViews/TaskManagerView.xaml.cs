using Cj.EmbeddedAPP.BLL;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Wx.Qunkong360.Wpf.ViewModels;
using Xzy.EmbeddedApp.Model;
using Xzy.EmbeddedApp.Utils;
using Xzy.EmbeddedApp.WinForm.Tasks;

namespace Wx.Qunkong360.Wpf.ContentViews
{
    /// <summary>
    /// TaskManagerView.xaml 的交互逻辑
    /// </summary>
    public partial class TaskManagerView : UserControl
    {
        ResourceManager resourceManager;
        CultureInfo cultureInfo;
        AppOptViewModel _appOptViewModel;

        public TaskManagerView()
        {
            InitializeComponent();
            resourceManager = new ResourceManager("Wx.Qunkong360.Wpf.Languages.Res", typeof(TaskManagerView).Assembly);
            if (ConfigVals.Lang == 1)
            {
                cultureInfo = CultureInfo.CreateSpecificCulture("zh-cn");
                Res.Culture.Button = new Res_Zh_Button();
              //  ResA.Culture.Button = new ResA_Zh_Button();
                ResD.CultureD.Button = new ResD_Zh_Button();
            }
            else if (ConfigVals.Lang == 2)
            {
                cultureInfo = CultureInfo.CreateSpecificCulture("en-us");
                Res.Culture.Button = new Res_En_Button();
               // ResA.Culture.Button = new ResA_Zh_Button();
                ResD.CultureD.Button = new ResD_En_Button();
            }
            lblTaskManagement.Content= resourceManager.GetString("Task_Management", cultureInfo);
            /*btnSearchTask.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Search_Task", SystemLanguageManager.Instance.CultureInfo);
            btnRefreshTask.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Refresh_Task", SystemLanguageManager.Instance.CultureInfo);
            btnDeleteTask.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Delete_Task", SystemLanguageManager.Instance.CultureInfo);
            btnClearTask.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Clear_Task", SystemLanguageManager.Instance.CultureInfo);*/
            btnSearchTask.Content = resourceManager.GetString("Search_Task", cultureInfo);  
            btnRefreshTask.Content = resourceManager.GetString("Refresh_Task", cultureInfo);
            btnDeleteTask.Content = resourceManager.GetString("Delete_Task", cultureInfo);
            btnClearTask.Content = resourceManager.GetString("Clear_Task", cultureInfo);
        }

        /// <summary>
        /// 查询任务列表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearchTask_Click(object sender, RoutedEventArgs e)
        {
            getTasksList();
        }

        /// <summary>
        /// 刷新任务列表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRefreshTask_Click(object sender, RoutedEventArgs e)
        {
            getTasksList();
            if (ConfigVals.IsRunning != 1)
            {
                Task.Run(async () =>
                {
                    await ProessTask();
                });
            }
        }

        /// <summary>
        /// 删除任务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDeleteTask_Click(object sender, RoutedEventArgs e)
        {

        }

        /// <summary>
        /// 清空全部任务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClearTask_Click(object sender, RoutedEventArgs e)
        {
            if (System.Windows.Forms.MessageBox.Show(resourceManager.GetString("Delete_Task_Tips", cultureInfo), resourceManager.GetString("Delete_Task_Confirmation", cultureInfo), System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
            {
                TasksBLL.DeleteTasks(-1);
                getTasksList();
            }
        }

        /// <summary>
        /// 加载数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tabTaskManagement_Loaded(object sender, RoutedEventArgs e)
        {

        }

        /// <summary>
        /// 启动已提交任务
        /// </summary>
        public async Task ProessTask()
        {
            ConfigVals.IsRunning = 1;
            //TasksSchedule tasks = new TasksSchedule();
            await TasksSchedule.ProessTask();
        }

        /// <summary>
        /// 查询任务
        /// </summary>
        public void getTasksList()
        {
            colNo.Header = resourceManager.GetString("Col_Id", cultureInfo);  
            colTaskType.Header = resourceManager.GetString("Col_TaskType", cultureInfo);
            colPhone.Header = resourceManager.GetString("Col_Phone", cultureInfo);
            colParameter.Header = resourceManager.GetString("Col_Parameter", cultureInfo);
            colStatus.Header = resourceManager.GetString("Col_Status", cultureInfo);
            colResult.Header = resourceManager.GetString("Col_Result", cultureInfo);
            colTime.Header = resourceManager.GetString("Col_Time", cultureInfo);

            List<TaskSch> list = TasksBLL.GetTasksList("-1", 2, 1000);

            if (list != null && list.Count() > 0)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    list[i].MobileIndex = list[i].MobileIndex + 1;
                    if (list[i].TypeId == 1)
                    {
                        list[i].TypeDescripton = resourceManager.GetString("Import_Address_List", cultureInfo);
                    }
                    else if (list[i].TypeId == 2)
                    {
                        list[i].TypeDescripton = resourceManager.GetString("MomentType_Text", cultureInfo);
                    }
                    else if (list[i].TypeId == 3)
                    {
                        list[i].TypeDescripton = resourceManager.GetString("MomentType_Picture", cultureInfo);
                    }
                    else if (list[i].TypeId == 4)
                    {
                        list[i].TypeDescripton = resourceManager.GetString("MomentType_Picture_Text", cultureInfo);
                    }
                    else if (list[i].TypeId == 5)
                    {
                        list[i].TypeDescripton = resourceManager.GetString("MsgType_Text", cultureInfo);
                    }
                    else if (list[i].TypeId == 6)
                    {
                        list[i].TypeDescripton = resourceManager.GetString("MsgType_Picture", cultureInfo);
                    }
                    else if (list[i].TypeId == 7)
                    {
                        list[i].TypeDescripton = resourceManager.GetString("MsgType_Picture_Text", cultureInfo);
                    }
                    else if (list[i].TypeId == 8)
                    {
                        list[i].TypeDescripton = resourceManager.GetString("MsgType_Video", cultureInfo);
                    }
                    else if (list[i].TypeId == 9)
                    {
                        list[i].TypeDescripton = resourceManager.GetString("MsgType_Video_Text", cultureInfo);
                    }
                    else if (list[i].TypeId == 11)
                    {
                        list[i].TypeDescripton = resourceManager.GetString("WhatsApp_Op_TaskType_UpNickName", cultureInfo);
                    }
                    else if (list[i].TypeId == 15)
                    {
                        list[i].TypeDescripton = resourceManager.GetString("Group_MsgType_Picture_Text", cultureInfo);
                    }
                    else if (list[i].TypeId == 16)
                    {
                        list[i].TypeDescripton = resourceManager.GetString("Group_MsgType_Text", cultureInfo);
                    }
                    else if (list[i].TypeId == 17)
                    {
                        list[i].TypeDescripton = resourceManager.GetString("Group_MsgType_Video", cultureInfo);
                    }
                    else if (list[i].TypeId == 101)
                    {
                        list[i].TypeDescripton = resourceManager.GetString("AddListPhoneNums", cultureInfo);
                    }
                    else if (list[i].TypeId == 20)
                    {
                        list[i].TypeDescripton = resourceManager.GetString("AddFriendByFriend", cultureInfo);
                    }
                    else if (list[i].TypeId == 21)
                    {
                        list[i].TypeDescripton = resourceManager.GetString("AddHomePageFriend", cultureInfo);
                    }
                    else if (list[i].TypeId == 22)
                    {
                        list[i].TypeDescripton = resourceManager.GetString("AddRecommFriend", cultureInfo);
                    }
                    else if (list[i].TypeId == 23)
                    {
                        list[i].TypeDescripton = resourceManager.GetString("Facebook_AllowFriend", cultureInfo);
                    }
                    else if (list[i].TypeId == 24)
                    {
                        list[i].TypeDescripton = resourceManager.GetString("Facebook_SearchAndAddFriend", cultureInfo);
                    }
                    else if (list[i].TypeId == 25)
                    {
                        list[i].TypeDescripton = resourceManager.GetString("Facebook_SearchAndAddGroup", cultureInfo);
                    }
                    else if (list[i].TypeId == 26)
                    {
                        list[i].TypeDescripton = resourceManager.GetString("Facebook_FocusHomePage", cultureInfo);
                    }
                    else if (list[i].TypeId == 27)
                    {
                        list[i].TypeDescripton = resourceManager.GetString("Facebook_InvitingFriends", cultureInfo);
                    }
                    else if (list[i].TypeId == 28)
                    {
                        list[i].TypeDescripton = resourceManager.GetString("Facebook_InviteFriendsLike", cultureInfo);
                    }
                    else if (list[i].TypeId == 29)
                    {
                        list[i].TypeDescripton = resourceManager.GetString("Facebook_TimelineLike", cultureInfo);
                    }
                    else if (list[i].TypeId == 30)
                    {
                        list[i].TypeDescripton = resourceManager.GetString("Facebook_FriendTimelineLike", cultureInfo);
                    }
                    else if (list[i].TypeId == 31)
                    {
                        list[i].TypeDescripton = resourceManager.GetString("Facebook_SendHomepage", cultureInfo);
                    }
                    else if (list[i].TypeId == 32)
                    {
                        list[i].TypeDescripton = resourceManager.GetString("Facebook_PublishPost", cultureInfo);
                    }
                    else if (list[i].TypeId == (int)TaskType.AddGroupUsers)
                    {
                        list[i].TypeDescripton = resourceManager.GetString("Facebook_AddGroupUser", cultureInfo);
                    }
                    else
                    {
                        list[i].TypeDescripton = resourceManager.GetString("WhatsApp_Op_Other", cultureInfo);
                    }
                }
            }
            datagrid.ItemsSource = list;
        }
    }
}
