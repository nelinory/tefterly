using System;
using System.Windows.Threading;

namespace Tefterly.Services
{
    public class SearchService : ISearchService
    {
        private DispatcherTimer _searchTimer = new DispatcherTimer();

        public event SearchEventHandler Search;
        public string SearchTerm { get; set; }

        public int Subscribers
        {
            get { return Search == null ? 0 : Search.GetInvocationList().Length; }
        }

        public SearchService()
        {
            _searchTimer = new DispatcherTimer();
            _searchTimer.Interval = TimeSpan.FromMilliseconds(300);
            _searchTimer.Tick += (sender, e) =>
            {
                _searchTimer.Stop();

                if (Search != null)
                    Search(this, null);
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
            return (String.IsNullOrEmpty(SearchTerm) == false && SearchTerm.Length > 2); // TODO: Read from settings
        }
    }
}
