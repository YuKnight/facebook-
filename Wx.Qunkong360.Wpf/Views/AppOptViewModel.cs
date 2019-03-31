using System.Collections.Generic;
using Xzy.EmbeddedApp.Model;

namespace Wx.Qunkong360.Wpf.ViewModels
{
    public class AppOptViewModel
    {
        public List<Simulators> simulators { get; set; } = new List<Simulators>();

        public List<Simulator> simulator { get; set; } = new List<Simulator>();

        public List<admins> admins { get; set; } = new List<admins>();
    }
}
