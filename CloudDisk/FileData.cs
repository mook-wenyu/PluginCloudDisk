using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudDisk
{
    /// <summary>
    /// 文件数据
    /// </summary>
    public class FileData : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public string Size { get; set; }
        public string Time { get; set; }

        private bool _isChecked;
        public bool IsCheck
        {
            set
            {
                _isChecked = value;
                OnPropertyChanged(nameof(IsCheck));
            }
            get
            {
                return _isChecked;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
