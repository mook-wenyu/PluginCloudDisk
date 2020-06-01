using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudDisk
{
    public class FileDataViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<FileData> fileDataList;
        public ObservableCollection<FileData> FileDataList
        {
            get
            {
                return this.fileDataList;
            }
            set
            {
                fileDataList = value;
                OnPropertyChanged(nameof(FileDataList));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
