using Prism.Events;

namespace Wx.Qunkong360.Wpf.Events
{
    public class TaskUpdatedEvent : PubSubEvent<string>
    {

    }

    //public class AdbsConnectedEvent : PubSubEvent
    //{

    //}

    //public class AdbConnectedEvent : PubSubEvent
    //{

    //}

    public class NewDeviceAttachedEvent : PubSubEvent
    {

    }

    public class NewSocketConnectedEvent : PubSubEvent<int>
    {

    }

    public class VmClosedEvent : PubSubEvent
    {

    }
}
