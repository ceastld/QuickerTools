using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace QuickerTools.Domain.X
{
    public class Variable : INotifyPropertyChanged
    {
        private string _desc;
        private string _key;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Key
        {
            get => this._key;
            set
            {
                this._key = value;
                this.OnPropertyChanged(nameof(Key));
            }
        }

        public VarType Type { get; set; }

        public string Desc
        {
            get => this._desc;
            set
            {
                this._desc = value;
                this.OnPropertyChanged(nameof(Desc));
            }
        }

        public string DefaultValue { get; set; }

        //public Type ActualType;
        //public static Type GetAcualType(VarType varType)
        //{
        //    switch (varType)
        //    {

        //    }
        //} 
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if (propertyChanged == null)
                return;
            propertyChanged((object)this, new PropertyChangedEventArgs(name));
        }

        public override string ToString() => this.Key ?? "";
    }

}
