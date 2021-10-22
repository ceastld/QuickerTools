using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace QuickerTools.Domain
{
    public class CodeData
    {
        public CodeData()
        {
            
        }
        public Dictionary<string, CodeItem> CodeItemDict { get; set; } = new Dictionary<string, CodeItem>();
        public Dictionary<string, CodeItem> TrashDict { get; set; } = new Dictionary<string, CodeItem>();
        public CodeItem GetItem(string id)
        {
            if (!CodeItemDict.ContainsKey(id))
                CodeItemDict[id] = (new CodeItem() { Id = id }); ;
            return CodeItemDict[id];
        }
        /// <summary>
        /// 会自动向列表中添加
        /// </summary>
        /// <param name="codeitem"></param>
        public void AddNewItem(CodeItem codeitem)
        {
            if (codeitem != null && codeitem.Id != null)
                CodeItemDict[codeitem.Id] = codeitem;
            if (AppState.CodeItems == null)
                AppState.CodeItems = new SmartCollection<CodeItem>();
            AppState.CodeItems.Add(codeitem);
        }
        public void UpdateId(CodeItem codeItem,string id)
        {
            CodeItemDict.Remove(codeItem.Id);
            codeItem.Id = id;
            CodeItemDict[id] = codeItem;
        }
        public void UpdateItem(CodeItem item)
        {
            if (CodeItemDict.ContainsKey(item.Id))
            {
                CodeItemDict[item.Id] = item;
            }
            else
            {
                AddNewItem(item);
            }
        }
        public void Remove(CodeItem item)
        {
            if (CodeItemDict.Remove(item.Id))
            {
                AppState.CodeItems.Remove(item);
                TrashDict[item.Id] = item;
            }
            else
            {
                TrashDict.Remove(item.Id);
            }
        }
        public bool IsEmpty() => CodeItemDict.Count == 0;
        public int Count => CodeItemDict.Count;
        public void Init()
        {
            InitCodeItemList();
        }
        public void InitCodeItemList()
        {
            if (AppState.CodeItems.Count > 0)
            {
                AppState.CodeItems.Clear();
            }
            foreach(var x in CodeItemDict.Values.ToList())
            {
                AppState.CodeItems.Add(x);
            }
        }
        public void CleanUpTrash()
        {
            var now = DateTime.Now;
            foreach(var x in TrashDict.Keys.ToArray())
            {
                if (now.Subtract(TrashDict[x].EditTime).TotalDays > 1)
                    TrashDict.Remove(x);
            }
        }
        public bool ContainsItem(CodeItem item) => CodeItemDict.ContainsKey(item.Id);
    }
    [JsonObject(MemberSerialization.OptOut)]
    public class CodeItem : IComparable<CodeItem>, INotifyPropertyChanged
    {
        private string title;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        public string Id { get; set; }
        public string Icon { get; set; }
        public static string defaultTitle = "未命名";
        public string Title
        {
            get => title; 
            set
            {
                title = value; OnPropertyChanged(nameof(Title));
            }
        }
        [JsonIgnore]
        public string Tips
        {
            get
            {
                return Code.Length > 500 ? Code.Substring(0, 500) : Code;
            }
        }
        public string Description { get; set; }
        public string Code { get; set; } = "";
        public string Parent { get; set; }
        public List<string> Children { get; set; }
        public DateTime CreatTime { get; set; }
        public DateTime EditTime { get; set; }

        public CodeItem(string title = "")
        {
            Title = title;
            Id = Guid.NewGuid().ToString();
            CreatTime = EditTime = DateTime.Now;
        }
        public int CompareTo(CodeItem other)
        {
            return Title.CompareTo(other.Title);
        }
        public void UpdateId(string id) => AppState.ExpressionData.UpdateId(this, id);
        public CodeItem Clone() => JsonConvert.DeserializeObject<CodeItem>(JsonConvert.SerializeObject(this));
        public void UpdateEditTime() => EditTime = DateTime.Now;
    }
}
