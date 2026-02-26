using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace VSYASGUI_WFP_App.MVVM.ViewModels.Base
{
    public abstract class Presenter : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Update a field with newValue, and notify everyone. Does not notify subscribers of updates if newValue is equal.
        /// </summary>
        /// <typeparam name="T">Type of the field (usually auto-inferred).</typeparam>
        /// <param name="field">The field.</param>
        /// <param name="newValue">The new newValue. <b>Note:</b> will not update <paramref name="field"/> if the same as the existing newValue.</param>
        /// <param name="propertyName">The name of the field.</param>
        protected void UpdateFieldWithValue<T>(ref T field, T newValue, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, newValue)) return;

            field = newValue;
            OnPropertyChanged(propertyName);
        }

        /// <summary>
        /// Notify that a field has been updated and its newValue has been assiged elsewhere.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        protected void NotifyFieldUpdated([CallerMemberName] string? propertyName = null)
        {
            if (propertyName == null)
                return;

            OnPropertyChanged(propertyName);
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
