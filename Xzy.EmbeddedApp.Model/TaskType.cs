namespace Xzy.EmbeddedApp.Model
{
    public enum TaskType
    {
        //[Description("导入通讯录")]
        /// <summary>
        /// 导入通讯录
        /// </summary>
        ImportContacts = 1,

        /// <summary>
        /// 添加通讯录好友
        /// </summary>
        AddListPhoneNums = 101,

        /// <summary>
        /// 发纯文本动态
        /// </summary>
        PostMessage = 2,

        /// <summary>
        /// 发纯图片动态
        /// </summary>
        PostPicture = 3,

        /// <summary>
        /// 发图文动态
        /// </summary>
        PostMessageAndPicture = 4,

        /// <summary>
        /// 发纯文本消息
        /// </summary>
        SendMessage = 5,

        /// <summary>
        /// 发纯图片消息
        /// </summary>
        SendPicture = 6,

        /// <summary>
        /// 发图文消息
        /// </summary>
        SendMessageAndPicture = 7,

        /// <summary>
        /// 发纯视频消息
        /// </summary>
        SendVideo = 8,

        /// <summary>
        /// 发视文消息
        /// </summary>
        SendMessageAndVideo = 9,

        /// <summary>
        /// 发音频消息
        /// </summary>
        SendAudio = 10,

        /// <summary>
        /// 发送短信验证码
        /// </summary>
        SendVerificationCode = 11,

        /// <summary>
        /// 修改昵称和个性说明
        /// </summary>
        UpdateNickName = 12,


        /// <summary>
        /// 清除通讯录
        /// </summary>
        ClearContact = 13,

        /// <summary>
        /// 创建群组
        /// </summary>
        CreateGroup = 14,

        /// <summary>
        /// 发送群组文本消息
        /// </summary>
        SendGroupTextMessage = 15,

        /// <summary>
        /// 发送群组图片消息
        /// </summary>
        SendGroupPictureMessage = 16,

        /// <summary>
        /// 发送群组视频消息
        /// </summary>
        SendGroupVideoMessage = 17,

        /// <summary>
        /// 添加好友的好友
        /// </summary>
        AddFriendByFriend = 20,

        /// <summary>
        /// 添加主页好友
        /// </summary>
        AddPageFriends = 21,

        /// <summary>
        /// 添加推荐好友
        /// </summary>
        AddRecommFriends = 22,

        /// <summary>
        /// 通过好友请求
        /// </summary>
        AllowRequestFriend = 23,


        /// <summary>
        /// 搜索添加好友
        /// </summary>
        SearchAndAddFriend =24,


        /// <summary>
        /// 搜索加入群组
        /// </summary>
        SearchAndJoinGroup =25,

        /// <summary>
        /// 关注主页
        /// </summary>
        AttentionHomePage = 26,

        /// <summary>
        /// 邀请好友进小组
        /// </summary>
        InvitingFriends = 27,

        /// <summary>
        /// 邀请好友点赞
        /// </summary>
        InviteFriendsLike = 28,

        /// <summary>
        /// 时间线点赞
        /// </summary>
        TimelineLike = 29,

        /// <summary>
        /// 好友时间线点赞
        /// </summary>
        FriendTimelineLike = 30,

        /// <summary>
        /// 发送主页
        /// </summary>
        SendHomepage = 31,
        /// <summary>
        /// 发布动态
        /// </summary>
        PublishPost = 32,

        /// <summary>
        /// 添加群组好友
        /// </summary>
        AddGroupUsers = 33,

        /// <summary>
        /// 好友群发消息
        /// </summary>
        SendFriendsMessage = 34,

        /// <summary>
        /// 小组群发消息
        /// </summary>
        SendGroupMessage = 35,


        /// <summary>
        /// 模糊查找
        /// </summary>
        FuzzySearch = 201,
        /// <summary>
        /// 精确查找
        /// </summary>
        ExactSearch = 202,
        /// <summary>
        /// 模糊查找并点击
        /// </summary>
        FuzzySearchAndClick = 203,
        /// <summary>
        /// 精确查找并点击
        /// </summary>
        ExactSearchAndClick = 204,
        /// <summary>
        /// 精确查找并清除
        /// </summary>
        ExactSearchAndClear = 205,
        /// <summary>
        /// 模糊查找并清除
        /// </summary>
        FuzzySearchAndClear = 206,

        /// <summary>
        /// 精确查找并赋值
        /// </summary>
        ExactSearchAndAssign = 207,

        /// <summary>
        /// 点击坐标
        /// </summary>
        ClickCoordinate = 208,

        /// <summary>
        /// 滑动
        /// </summary>
        Swipe = 209,

        /// <summary>
        /// 返回上一级页面
        /// </summary>
        GoBack = 210,


        /// <summary>
        /// 精确查找元素并点击父类
        /// </summary>
        ExactSearchAndClickParent=211,


        /// <summary>
        /// 精确查找元素并粘贴
        /// </summary>
        ExactSearchAndPaste=212,


        /// <summary>
        /// 删除图片
        /// </summary>
        DeletePictures=213,

        /// <summary>
        /// 更新图片
        /// </summary>
        UpdatePictures=214,        
    }
}
