using System;

namespace Tefterly.Services
{
    public delegate void SearchEventHandler(object sender, EventArgs e);

    public interface ISearchService
    {
        event SearchEventHandler Search;
        string SearchText { get; set; }
        void ExecuteSearch(string searchText);
    }
}
