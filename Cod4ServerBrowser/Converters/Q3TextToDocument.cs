using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;

namespace Cod4ServerBrowser.Converters
{
    public class Q3TextToDocument : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return GetColoredDocumentText((string) value);
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return new TextRange(((FlowDocument)value).ContentStart, ((FlowDocument)value).ContentEnd).Text;
        }

        public FlowDocument GetColoredDocumentText(string text)
        {
            SolidColorBrush solidColorBrush = Brushes.White;
            FlowDocument doc = new FlowDocument();
            doc.Blocks.Clear();
            for (int index = 0; index < text.Length; ++index)
            {
                if ((int)text[index] == 94)
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
                    TextRange textRange = new TextRange(doc.ContentEnd,doc.ContentEnd);
                    textRange.Text = text[index].ToString();
                    textRange.ApplyPropertyValue(TextElement.ForegroundProperty, (object)solidColorBrush);
                    textRange.ApplyPropertyValue(TextElement.FontWeightProperty, (object)FontWeights.Regular);
                }
                else
                {
                    TextRange textRange = new TextRange(doc.ContentEnd, doc.ContentEnd);
                    textRange.Text = text[index].ToString();
                    textRange.ApplyPropertyValue(TextElement.ForegroundProperty, (object)solidColorBrush);
                    textRange.ApplyPropertyValue(TextElement.FontWeightProperty, (object)FontWeights.Regular);
                }
            }

            return doc;
        }
    }
}
