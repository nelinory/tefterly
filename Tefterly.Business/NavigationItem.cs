using Prism.Regions;

namespace Tefterly.Business
{
    public class NavigationItem : INavigationItem
    {
        public string NavigationRegion { get; set; }
        public string NavigationPath { get; set; }
        public NavigationParameters NavigationParameters { get; set; }
    }
}
