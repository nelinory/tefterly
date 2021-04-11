using Prism.Regions;

namespace Tefterly.Business
{
    public interface INavigationItem
    {
        string NavigationRegion { get; set; }
        string NavigationPath { get; set; }
        NavigationParameters NavigationParameters { get; set; }
    }
}
