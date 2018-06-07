using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Cod4ServerBrowser
{
    public class Student : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public ObservableCollection<int> Grades { get; set; }
        public string Name { get; set; }
        public int Grade { get; set; }
        public bool GoodStudent { get; set; }
        public Student ()
        {
            Grades = new ObservableCollection<int> { 30, 40, 90, 100 };
            Grade = 100;
        }
    }
}
