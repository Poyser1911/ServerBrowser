using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Cod4ServerBrowser.Controls
{
    /// <summary>
    /// Interaction logic for BindableDocument.xaml
    /// </summary>
    public partial class BindableDocument : INotifyPropertyChanged
    {
        public static DependencyProperty TextDependencyProperty = DependencyProperty.Register("Text", typeof(string),
            typeof(BindableDocument), new PropertyMetadata("", OnTextChanged));

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BindableDocument control = d as BindableDocument;
            control.OnSetTextChanged(e);
        }

        private bool AlreadySet = false;

        private void OnSetTextChanged(DependencyPropertyChangedEventArgs e)
        {
            if (!AlreadySet)
               RichTextBox.AppendText(Text);
            AlreadySet = true;
        }

        public string Text
        {
            get => (string)GetValue(TextDependencyProperty);
            set => SetValue(TextDependencyProperty, value);
        }

        public BindableDocument()
        {
            InitializeComponent();
        }

        public void SetColoredText(RichTextBox box, string text)
        {
            Task.Run(() =>
            {
                Dispatcher.Invoke(() =>
                {
                    SolidColorBrush solidColorBrush = Brushes.White;
                    box.Document.Blocks.Clear();
                    for (int index = 0; index < text.Length; ++index)
                    {
                        if (text[index] == 94)
                        {
                            switch (text[index + 1])
                            {
                                case '0':
                                    solidColorBrush = Brushes.Black;
                                    break;
                                case '1':
                                    solidColorBrush = Brushes.Red;
                                    break;
                                case '2':
                                    solidColorBrush = Brushes.Green;
                                    break;
                                case '3':
                                    solidColorBrush = Brushes.Yellow;
                                    break;
                                case '4':
                                    solidColorBrush = Brushes.Blue;
                                    break;
                                case '5':
                                    solidColorBrush = Brushes.Aqua;
                                    break;
                                case '6':
                                    solidColorBrush = Brushes.Purple;
                                    break;
                                case '7':
                                    solidColorBrush = Brushes.White;
                                    break;
                                case '8':
                                    solidColorBrush = Brushes.Gray;
                                    break;
                                case '9':
                                    solidColorBrush = Brushes.Brown;
                                    break;
                            }

                            index += 2;
                            TextRange textRange = new TextRange(box.Document.ContentEnd, box.Document.ContentEnd);
                            if (index >= text.Length)
                                return;
                            textRange.Text = text[index].ToString();
                            textRange.ApplyPropertyValue(TextElement.ForegroundProperty, solidColorBrush);
                            textRange.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Regular);
                        }
                        else
                        {
                            TextRange textRange = new TextRange(box.Document.ContentEnd, box.Document.ContentEnd);
                            textRange.Text = text[index].ToString();
                            textRange.ApplyPropertyValue(TextElement.ForegroundProperty, solidColorBrush);
                            textRange.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Regular);
                        }
                    }
                });
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;

    }
}
