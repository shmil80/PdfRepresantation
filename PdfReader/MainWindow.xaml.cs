using PdfRepresantation;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;

namespace PdfReader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static readonly RoutedCommand LoadCommand = new RoutedCommand
        {
            InputGestures = {new KeyGesture(Key.L, ModifierKeys.Control)}
        };
        public static readonly RoutedCommand ZoomInCommand = new RoutedCommand
        {
            InputGestures = {new KeyGesture(Key.Add, ModifierKeys.Control)},
        };
        public static readonly RoutedCommand ZoomOutCommand = new RoutedCommand
        {
            InputGestures = {new KeyGesture(Key.Subtract, ModifierKeys.Control)},
        };

        readonly PdfWpfBuilder pdfBuilder = new PdfWpfBuilder();

        public MainWindow()
        {
            InitializeComponent();
        }

        public MainWindow(string fileName)
        {
            InitializeComponent();
            ShowPdf(fileName);
        }

        private void ShowPdf(string fileName)
        {
            Cursor originalCursor = Cursor;
            try
            {
                Cursor = Cursors.Wait;
                PdfDetails details = PdfDetailsFactory.Create(fileName);
                pdfBuilder.AddPdf(RootContainer, details);
                StatusBarText.Text = fileName;
                Cursor = originalCursor;
            }
            catch (Exception e)
            {
                Cursor = originalCursor;
                MessageBox.Show("cannot open this file", "Pdf reader");
                Console.WriteLine(e);
            }
        }

        private void LoadFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Pdf Files|*.pdf"
            };
            if (openFileDialog.ShowDialog() == true)
                ShowPdf(openFileDialog.FileName);
        }

        private void IncreaseZoom(object sender, ExecutedRoutedEventArgs e)
        {
            var scale = (ScaleTransform) Scale.LayoutTransform;
        
            scale.ScaleX += 0.1;
            scale.ScaleY += 0.1;
        }
        private void DecreaseZoom(object sender, ExecutedRoutedEventArgs e)
        {
            var scale = (ScaleTransform) Scale.LayoutTransform;
            
            scale.ScaleX -= 0.1;
            scale.ScaleY -= 0.1;

        }

        private void CanZoomIn(object sender, CanExecuteRoutedEventArgs e)
        {
            var scale = (ScaleTransform) Scale.LayoutTransform;
            e.CanExecute = scale.ScaleX <= 1.999;
        }
        private void CanZoomOut(object sender, CanExecuteRoutedEventArgs e)
        {
            var scale = (ScaleTransform) Scale.LayoutTransform;
            e.CanExecute = scale.ScaleX >= 0.101;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Application.Current.Shutdown();
        }
    }
}