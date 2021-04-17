using Prism.Mvvm;
using System;

namespace Tefterly.Business.Models
{
    public class Notebook: BindableBase
    {
        private Guid _id;
        public Guid Id
        {
            get { return _id; }
            set { SetProperty(ref _id, value); }
        }

        private string _title;
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        private string _iconFont;
        public string IconFont
        {
            get { return _iconFont; }
            set { SetProperty(ref _iconFont, value); }
        }

        private int _totalItems;
        public int TotalItems
        {
            get { return _totalItems; }
            set { SetProperty(ref _totalItems, value); }
        }

        private bool _isSystem;
        public bool IsSystem
        {
            get { return _isSystem; }
            set { SetProperty(ref _isSystem, value); }
        }
    }
}