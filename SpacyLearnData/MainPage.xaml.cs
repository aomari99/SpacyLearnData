using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Text.RegularExpressions;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x407 dokumentiert.

namespace SpacyLearnData
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

       

        private ObservableCollection<string> entitys = new ObservableCollection<string>();
        public ObservableCollection<string> Entitys
        {
            get { return entitys; }

            set
            {
                entitys = value;
                NotifyPropertyChanged("Entitys");
            }
        }

         

        public MainPage()
        {
            this.InitializeComponent();
            
            data.AddHandler(PointerReleasedEvent, new PointerEventHandler(data_RightTapped), true);
          
        }

        private async void  loadpdfbtn_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".pdf");

            Windows.Storage.StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                // Application now has read/write access to the picked file
               ExtractTextFromPDF( file.OpenStreamForReadAsync().Result);
            }
            else
            {
               Debug.WriteLine( "Operation cancelled.");
            }
        }
        public void ExtractTextFromPDF(Stream filePath)
        {
      
        
                StringBuilder text = new StringBuilder();
                PdfReader pdfReader = new PdfReader(filePath);
                PdfDocument pdfDoc = new PdfDocument(pdfReader);
                for (int page = 1; page <= pdfDoc.GetNumberOfPages(); page++)
                {
                    ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
                    string pageContent = PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(page), strategy);
                    text.Append(pageContent);
                }
                pdfDoc.Close();
                pdfReader.Close();
                string pattern = "/.*?[?!.]/g";
                Regex rg = new Regex(pattern);

            pdf = Regex.Split(text.ToString().Replace("\n", ""), @"(?<=[\. !\?])\s+").ToList();
            pdf.RemoveAll(y => y == "");
            addentity.IsEnabled = true;
                next();
            
        }

        private void addentity_Click(object sender, RoutedEventArgs e)
        {
            data.IsEnabled = true;
            Entitys.Add(entittyaddtxt.Text);
            EntityList.SelectedIndex = Entitys.Count - 1;
            entittyaddtxt.Text = "";
        }

        public List<Trainig> trainigs = new List<Trainig>();
 

        public List<string> pdf = new List<string>();

        int i = 0;

        public void next()
        {
            if (pdf.Count > i)
            {
                data.Text = pdf[i];
                actualentities.Clear();
                i++;
            }
            else
            {
                //disable next btn and show that u finished
            }
        }
        public void next2()
        {
            if (pdf.Count > i)
            {
                data.Text += " "+ pdf[i];              
                i++;
            }
            else
            {
                //disable next btn and show that u finished
            }
        }
        public ObservableCollection<Entity> actualentities = new ObservableCollection<Entity>();
        Entity actualentity = new Entity { value = ""};
        private void TextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (data.SelectedText != "")
            {
                int startpos = data.SelectedText[0] == ' ' ? data.SelectionStart + 1 : data.SelectionStart;
                int endpos = startpos + data.SelectedText[data.SelectionLength-1] == ' ' ? startpos + data.SelectionLength - 1 : startpos + data.SelectionLength;

                actualentity = new Entity { start = startpos, value = data.SelectedText.TrimStart().TrimEnd(), end = endpos, Entityname = EntityList.SelectedItem.ToString() };
            }else
                actualentity = new Entity { start = 0, value = data.SelectedText , end = 0, Entityname = "" };

        }
        public void finished()
        {
            trainigs.Add(new Trainig { Data = data.Text , entities = actualentities.ToList() });
            next();
        }
        private void data_RightTapped(object sender, PointerRoutedEventArgs e)
        {
            if(actualentity.value != "")
                actualentities.Add(actualentity);


        }

        private void nextbtn_Click(object sender, RoutedEventArgs e)
        {
            finished();
        }

        private void resetbtn_Click(object sender, RoutedEventArgs e)
        {
            actualentities.Clear();
        }

        private async void savebtn_Click(object sender, RoutedEventArgs e)
        {
            //save as traindata

            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append("[");
            foreach(var item in trainigs.Select((x, i) => new { Value = x, Index = i }))
            {
                stringBuilder.Append($"( \"{item.Value.Data}\" , {{ \"entities\" : [ { entitiesToString(item.Value.entities)}] }} ) ");
                if (item.Index < trainigs.Count - 1)
                    stringBuilder.Append(",");
            }
            stringBuilder.Append("]");

            var savePicker = new Windows.Storage.Pickers.FileSavePicker();
            savePicker.SuggestedStartLocation =
                Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            // Dropdown of file types the user can save the file as
            savePicker.FileTypeChoices.Add("Plain Text", new List<string>() { ".txt" });
            // Default file name if the user does not type one in or select a file to replace
            savePicker.SuggestedFileName = "TrainData";
            Windows.Storage.StorageFile file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                // Prevent updates to the remote version of the file until
                // we finish making changes and call CompleteUpdatesAsync.
                Windows.Storage.CachedFileManager.DeferUpdates(file);
                // write to file
                await Windows.Storage.FileIO.WriteTextAsync(file, stringBuilder.ToString(),Windows.Storage.Streams.UnicodeEncoding.Utf8);
                // Let Windows know that we're finished changing the file so
                // the other app can update the remote version of the file.
                // Completing updates may require Windows to ask for user input.
                Windows.Storage.Provider.FileUpdateStatus status =
                    await Windows.Storage.CachedFileManager.CompleteUpdatesAsync(file);
                if (status == Windows.Storage.Provider.FileUpdateStatus.Complete)
                {
                    //this.textBlock.Text = "File " + file.Name + " was saved.";
                }
                else
                {
                  //  this.textBlock.Text = "File " + file.Name + " couldn't be saved.";
                }
            }
            else
            {
              //  this.textBlock.Text = "Operation cancelled.";
            }
        }

        private string entitiesToString(List<Entity> entities)
        {
            StringBuilder stringBuilder = new StringBuilder();

            foreach(var item in entities.Select((x, i) => new { Value = x, Index = i }))
            {
                var item2 = item.Value;
                stringBuilder.Append($"({item2.start}, {item2.end}), \"{item2.Entityname}\"");
                if (item.Index < entities.Count - 1)
                    stringBuilder.Append(",");
            }

            return stringBuilder.ToString();
        }

        private void next2btn_Click(object sender, RoutedEventArgs e)
        {
            next2();
        }
    }
}
