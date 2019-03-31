using Prism.Events;

namespace Wx.Qunkong360.Wpf.Events
{
    public class EventAggregatorManager
    {
        private EventAggregatorManager()
        {
        }

        public static readonly EventAggregatorManager Instance = new EventAggregatorManager();

        public EventAggregator EventAggregator { get; private set; }

        public void Initialize()
        {
            EventAggregator = new EventAggregator();
        }
    }
}
