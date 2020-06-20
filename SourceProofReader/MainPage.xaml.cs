using System;
using System.Collections.ObjectModel;
using System.IO;
using Windows.UI.Xaml;

namespace SourceProofReader
{
    public class ProofreadSourceInfo
    {
        public ProofreadSourceInfo(string checksum, string line)
        {
            Checksum = checksum;
            _sourceLine = line;
        }

        private readonly string _sourceLine;
        public string Checksum { get; }
        public ushort LineNumber => ushort.Parse(_sourceLine.Substring(0, _sourceLine.IndexOf(' ')));
        public string Line => _sourceLine.Substring(_sourceLine.IndexOf(' ') + 1);
    }
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage
    {
        private Windows.Storage.StorageFile _fileToProofread;
        public ObservableCollection<ProofreadSourceInfo> ProofreadSourceCodeDataSource = new ObservableCollection<ProofreadSourceInfo>();
        public MainPage()
        {
            this.InitializeComponent();
            this.ProofreadSourceCodeListView.ItemsSource = ProofreadSourceCodeDataSource;
        }

        private static string Checksum(string text)
        {
            var checksum = 0;
            var chars = text.ToCharArray();

            for (var i = 0; i < chars.Length; ++i)
            {
                checksum += (chars[i] * (i + 1));
                checksum &= 0xff;
            }

            var c1 = (char) (65 + (checksum >> 4));
            var c2 = (char) (65 + (checksum & 0xf));
            return $"{c1}{c2}";
        }

        private async void ProofreadFile_Click(object sender, RoutedEventArgs e)
        {
            if (_fileToProofread == null) return;
            ProofreadSourceCodeDataSource.Clear();
            var fileStream = await _fileToProofread.OpenStreamForReadAsync();
            using (var reader = new StreamReader(fileStream))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    ProofreadSourceCodeDataSource.Add(new ProofreadSourceInfo(Checksum(line), line));
                }
            }
        }

        private async void FilePicker_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker
            {
                ViewMode = Windows.Storage.Pickers.PickerViewMode.List
                ,SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary
                ,FileTypeFilter = { ".bas" }
            };

            Windows.Storage.StorageFile file = await picker.PickSingleFileAsync();

            if (file != null)
            {
                Filename.Text = file.Path;
                _fileToProofread = file;
                ProofreadFile.IsEnabled = true;
                Windows.Storage.AccessCache.StorageApplicationPermissions
                    .FutureAccessList.Add(file);
            }
            else
            {
                ProofreadFile.IsEnabled = false;
            }
        }
    }
}
