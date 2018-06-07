using System.Windows;

namespace Cod4ServerBrowser.Controls
{
    /// <summary>
    /// Interaction logic for SearchInput.xaml
    /// </summary>
    public partial class SearchInput
    {
        public static DependencyProperty WaterMarkDependencyProperty = DependencyProperty.Register("WaterMark", typeof(string), typeof(SearchInput));
        public string WaterMark
        {
            get => (string)GetValue(WaterMarkDependencyProperty);
            set => SetValue(WaterMarkDependencyProperty, value);
        }

        public static DependencyProperty ShowSearchIconDependencyProperty = DependencyProperty.Register("ShowSearchIcon", typeof(bool), typeof(SearchInput));
        public bool ShowSearchIcon
        {
            get => (bool)GetValue(ShowSearchIconDependencyProperty);
            set => SetValue(ShowSearchIconDependencyProperty, value);
        }

        public SearchInput()
        {
            InitializeComponent();
            DataContext = this;
        }
    }
}
