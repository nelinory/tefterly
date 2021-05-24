using Prism.Mvvm;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Documents;

namespace Tefterly.Business
{
    public class ModelChangeTrackingBase : BindableBase, IRevertibleChangeTracking
    {
        private static readonly IEnumerable<string> _propertiesToIgnore = new List<string> { "IsChanged" };

        public event EventHandler ModelChanged;
        public ConcurrentDictionary<string, object> Changes { get; private set; }

        private DateTime _createdDateTime;
        public DateTime CreatedDateTime
        {
            get { return _createdDateTime; }
            set { SetProperty(ref _createdDateTime, value); }
        }

        private DateTime _updatedDateTime;
        public DateTime UpdatedDateTime
        {
            get { return _updatedDateTime; }
            set { SetProperty(ref _updatedDateTime, value); }
        }

        private bool _trackChanges;
        public bool TrackChanges
        {
            get { return _trackChanges; }
            set
            {
                _trackChanges = value;

                if (value == true)
                    PropertyChanged += ModelChangeTrackingBase_PropertyChanged;
                else
                    PropertyChanged -= ModelChangeTrackingBase_PropertyChanged;
            }
        }

        public ModelChangeTrackingBase()
        {
            Changes = new ConcurrentDictionary<string, object>();

            TrackChanges = false;
            IsChanged = false;
        }

        private void ModelChangeTrackingBase_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (TrackChanges == true && IsChanged == false && _propertiesToIgnore.Contains(e.PropertyName) == false)
            {
                OnModelChanged(EventArgs.Empty);
                IsChanged = true;
            }
        }

        protected virtual void OnModelChanged(EventArgs e)
        {
            EventHandler eventHandler = ModelChanged;

            if (eventHandler != null)
                eventHandler(this, e);
        }

        #region BindableBase Implementation

        protected override bool SetProperty<T>(ref T fieldValue, T newValue, [CallerMemberName] string propertyName = null)
        {
            if ((fieldValue as FlowDocument) != null && (newValue as FlowDocument) != null)
            {
                TextRange oldDoc = new TextRange((fieldValue as FlowDocument).ContentStart, (fieldValue as FlowDocument).ContentEnd);
                TextRange newDoc = new TextRange((newValue as FlowDocument).ContentStart, (newValue as FlowDocument).ContentEnd);
                if (oldDoc.GetHashCode() == newDoc.GetHashCode())
                    return false;
            }
            else if (EqualityComparer<T>.Default.Equals(fieldValue, newValue))
                return false;

            // change tracking support
            if (TrackChanges == true)
            {
                // store field initial value
                StoreInitialValue(fieldValue, propertyName);

                // updatedDateTime support
                StoreInitialValue(_updatedDateTime, nameof(UpdatedDateTime));
                _updatedDateTime = DateTime.Now;
                RaisePropertyChanged(nameof(UpdatedDateTime));
            }

            fieldValue = newValue;
            RaisePropertyChanged(propertyName);

            return true;
        }

        private void StoreInitialValue<T>(T fieldValue, string propertyName)
        {
            if (Changes.ContainsKey(propertyName) == false)
                Changes[propertyName] = fieldValue;
        }

        #endregion

        #region IRevertibleChangeTracking Implementation

        public bool IsChanged { get; private set; }

        public void AcceptChanges()
        {
            Changes.Clear();

            IsChanged = false;
        }

        public void RejectChanges()
        {
            foreach (var property in Changes)
                GetType().GetRuntimeProperty(property.Key).SetValue(this, property.Value);

            AcceptChanges();
        }

        #endregion
    }
}
