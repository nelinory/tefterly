﻿using Prism.Mvvm;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Windows.Documents;

namespace Tefterly.Core.Models
{
    public class ModelChangeTrackingBase : BindableBase, IRevertibleChangeTracking
    {
        private static readonly IList<string> _propertiesToIgnore = new List<string> { "IsChanged" };

        [JsonIgnore]
        public ConcurrentDictionary<string, object> Changes { get; private set; }
        public event EventHandler ModelChanged;

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
        [JsonIgnore]
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
            ModelChanged?.Invoke(this, e);
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
                StoreInitialValue(fieldValue, propertyName);

                // updatedDateTime support
                StoreInitialValue(_updatedDateTime, nameof(UpdatedDateTime));
                // update updatedDateTime when every other property change
                if (propertyName != "UpdatedDateTime")
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

        [JsonIgnore]
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
