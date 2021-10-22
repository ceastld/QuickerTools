using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace QuickerTools.Domain
{
    public class Variable : INotifyPropertyChanged, IComparer, IEqualityComparer
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if (propertyChanged == null)
                return;
            propertyChanged((object)this, new PropertyChangedEventArgs(name));
        }

        public int Compare(object x, object y)
        {
            if (x == null || y == null) return 0;
            else
            {
                return Compare((x as Variable).Key, (y as Variable).Key);
            }
        }

        public new bool Equals(object x, object y)
        {
            if (x == null)
            {
                if (y == null)
                    return true;
                else
                    return false;
            }
            else
            {
                if (y == null)
                    return false;
                else
                    return (x as Variable).Key == (y as Variable).Key;
            }
        }

        public int GetHashCode(object obj)
        {
            throw new NotImplementedException();
        }

        private string _key;

        public string Key
        {
            get { return _key; }
            set
            {
                _key = value;
                OnPropertyChanged(nameof(Key));
            }
        }
        private string _defaultVale;
        public Type Type { get; set; }
        public string Tips
        {
            get
            {
                if (string.IsNullOrEmpty(DefaultValue))
                {
                    return null;
                }
                else
                {
                    return DefaultValue.Length > 500 ? DefaultValue.Substring(500) : DefaultValue;
                }
            }
        }
        public string DefaultValue
        {
            get { return _defaultVale; }
            set
            {
                _defaultVale = value;
                OnPropertyChanged(nameof(Tips));
                OnPropertyChanged(nameof(DefaultValue));
            }
        }


    }
}
