using System;
using System.Windows.Threading;

namespace Tefterly.Services
{
    public class SearchService : ISearchService
    {
        private readonly DispatcherTimer _searchTimer = new DispatcherTimer();

        // services
        private readonly ISettingsService _settingsService;

        public event SearchEventHandler Search;
        public string SearchTerm { get; set; }

        public int Subscribers
        {
            get { return Search == null ? 0 : Search.GetInvocationList().Length; }
        }

        public SearchService(ISettingsService settingsService)
        {
            // attach all required services
            _settingsService = settingsService;

            _searchTimer = new DispatcherTimer();
            _searchTimer.Interval = TimeSpan.FromMilliseconds(300);
            _searchTimer.Tick += (sender, e) =>
            {
                _searchTimer.Stop();

                Search?.Invoke(this, null);
            };

            SearchTerm = String.Empty;
        }

        public void ExecuteSearch(string searchText)
        {
            SearchTerm = searchText;

            _searchTimer.Start();
        }

        public bool IsSearchInProgress()
        {
            return (String.IsNullOrEmpty(SearchTerm) == false && SearchTerm.Length > _settingsService.Settings.Search.TermMinimumLength);
        }
    }
}
