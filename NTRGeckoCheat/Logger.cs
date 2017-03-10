using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace NTRGeckoCheat
{
    public enum LoggerEnum
    {
        Info,
        Alert,
        Error,
        Debug
    }
    public class Logger
    {
        private readonly Brush _alertColor = (Brush)((new BrushConverter()).ConvertFrom("#d35400"));
        private readonly Brush _debugColor = (Brush)((new BrushConverter()).ConvertFrom("#7f8c8d"));
        private readonly Brush _errorColor = (Brush)((new BrushConverter()).ConvertFrom("#c0392b"));
        private readonly Brush _infoColor = (Brush)((new BrushConverter()).ConvertFrom("#27ae60"));

        public static Logger Instance { get; private set; }

        public Logger(RichTextBox richTextBox)
        {
            RichTextBox = richTextBox;
            IsFlowDocument = false;
            Instance = this;
        }

        public RichTextBox RichTextBox { get; set; }
        public bool IsFlowDocument { get; set; }
        public void Add(string text, LoggerEnum loggerEnum = LoggerEnum.Info)
        {
            if (RichTextBox.Dispatcher == null)
                return;

            if (RichTextBox.Dispatcher.CheckAccess())
            {
                if (!IsFlowDocument)
                {
                    RichTextBox.Document = new FlowDocument();
                    IsFlowDocument = true;
                }

                Paragraph paragraph = new Paragraph();

                string log = String.Empty;
                Brush color = Brushes.Black;
                switch (loggerEnum)
                {
                    case LoggerEnum.Alert:
                        color = _alertColor;
                        break;
                    case LoggerEnum.Debug:
                        color = _debugColor;
                        break;
                    case LoggerEnum.Error:
                        color = _errorColor;
                        break;
                    case LoggerEnum.Info:
                        color = _infoColor;
                        break;
                }

                string time = DateTime.Now.ToShortTimeString();
                Inline timeInline = new Bold(new Italic(new Run($"[{time}]"))) { Foreground = color };
                Inline separatorInline = new Run(" - ") { Foreground = color };
                Inline logInline = new Run(text) { Foreground = color };

                paragraph.Inlines.Add(timeInline);
                paragraph.Inlines.Add(separatorInline);
                paragraph.Inlines.Add(logInline);
                paragraph.Margin = new Thickness(0, 2, 0, 2);

                RichTextBox.Document.Blocks.Add(paragraph);
                RichTextBox.ScrollToEnd();
            }
            else
                RichTextBox.Dispatcher.Invoke(() =>Add(text, loggerEnum));
        }

        public void Error(Exception ex)
        {
            string message = $"{ex.Message}{Environment.NewLine}{ex.ToString()}";
            Add(message, LoggerEnum.Error);
        }

        public void Clear()
        {
            if (RichTextBox.Dispatcher.CheckAccess())
                RichTextBox.Document.Blocks.Clear();
            else
                RichTextBox.Dispatcher.Invoke(Clear);
        }
    }
}