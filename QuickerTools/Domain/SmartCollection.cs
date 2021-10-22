using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickerTools.Domain
{
    public class SmartCollection<T> : ObservableCollection<T>
    {
        public SmartCollection()
        {
        }

        public SmartCollection(IEnumerable<T> collection)
          : base(collection)
        {
        }

        public SmartCollection(List<T> list)
          : base(list)
        {
        }
        public void Changed()
        {
            this.OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            this.OnPropertyChanged(new PropertyChangedEventArgs("Count"));
        }
        public void AddRange(IEnumerable<T> range)
        {
            foreach (T obj in range)
                this.Items.Add(obj);
            this.OnPropertyChanged(new PropertyChangedEventArgs("Count"));
            this.OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public void Reset(IEnumerable<T> range)
        {
            this.ClearItems();
            if (range == null)
                return;
            this.AddRange(range);
        }
        /// <summary>
        /// <strong>列表自截取</strong>
        /// 保留从第 0 项开始的指定长度项
        /// </summary>
        /// <param name="_length">要截取的长度</param>
        public void TakeSelf(int _length)
        {
            int Length = this.Count;
            if (_length > Length || _length < 1) return;
            else
            {
                for (int i = Length - 1; i >= _length; i--)
                {
                    this.RemoveAt(i);
                }
            }

        }
        private bool IndexCheck(int index) => index >= 0 && index < this.Count;
    }


    public class SmartListDict : ObservableCollection<CodeItem>
    {
        public SmartListDict()
        {
        }
        public HashSet<string> ItemIds = new HashSet<string>();
        public void AddNewItem(CodeItem item)
        {
            string id = item.Id;
            if (!ItemIds.Contains(id))
                ItemIds.Add(id);
                this.Add(item);
        }
        public void UpdateItemId(CodeItem item)
        {
            string id = Guid.NewGuid().ToString();
            
        }
    }
}
