using Prism.Regions;

namespace Tefterly.Core.Navigation
{
    public class NavigationItem : INavigationItem
    {
        public string NavigationRegion { get; set; }
        public string NavigationPath { get; set; }
        public NavigationParameters NavigationParameters { get; set; }
    }
}
