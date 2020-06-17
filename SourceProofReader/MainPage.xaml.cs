using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SourceProofReader
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Windows.Storage.StorageFile fileToProofread;
        public MainPage()
        {
            this.InitializeComponent();
        }

        private string checksum(string text)
        {
            int checksum = 0;
            var chars = text.ToCharArray();

            for (int i = 0; i < chars.Length; ++i)
            {
                checksum += (chars[i] * (i + 1));
                checksum &= 0xff;
            }

            var c1 = (char) (65 + (checksum / 16.0));
            var c2 = (char) (65 + (checksum & 0xf));
            return string.Empty + c1 + c2 + " " + text + '\n';
        }

        private async void ProofreadFile_Click(object sender, RoutedEventArgs e)
        {
            if (fileToProofread == null) return;
            proofread.Text = string.Empty;
            var fileStream = await fileToProofread.OpenStreamForReadAsync();
            using (var reader = new StreamReader(fileStream))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    proofread.Text += checksum(line);
                }
            }
        }

        private async void filePicker_Click(object sender, RoutedEventArgs e)
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
                filename.Text = file.Path;
                fileToProofread = file;
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
