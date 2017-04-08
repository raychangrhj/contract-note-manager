using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ContractNotes
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string versionString = "ContractNoteManager 1.0";
        TextBox[] fieldTextBoxes = new TextBox[ContractNoteItemManager.FIELD_COUNT];
        ComboBox fieldComboBox;
        List<string> pdfFilePathList = new List<string>();
        ContractNote contractNote;
        List<ContractNoteItemManager> contractNoteList = new List<ContractNoteItemManager>();
        List<List<BitmapSource>> bitmapSourceList = new List<List<BitmapSource>>();
        int currentParsingPdfNo, previousSelectedIndexOfListBox = -1;
        int zoomValue = 1;
        bool isProcessing;

        public MainWindow()
        {
            InitializeComponent();
            createFieldTextBoxes();

            string applicationName = AppDomain.CurrentDomain.FriendlyName;
            string processName = applicationName.Substring(0, applicationName.Length - 4);
            if (Process.GetProcessesByName(processName).Length > 1)
            {
                MessageBox.Show(string.Format("{0} is already running", processName));
                Application.Current.Shutdown();
            }

            DateTime expireDate = new DateTime(2017, 3, 29, 12, 0, 0);
            DateTime today = DateTime.Now;
            if (today > expireDate)
            {
                //MessageBox.Show(@"Please Contact 'liuqiang199119@gmail.com'", "Demo Expired");
                //Application.Current.Shutdown();
            }

            SplashWindow splashWindow = new SplashWindow();
            splashWindow.Show();

            TemporaryDataManager.cleanTemporaryDirectory();
            contractNote = new ContractNote(this);
            setEnable(false);
            scanButton.IsEnabled = true;
            settingButton.IsEnabled = true;
            quitButton.IsEnabled = true;

            splashWindow.Close();
        }

        private void scanButton_Click(object sender, RoutedEventArgs e)
        {
            int maxFileCount = 20;
            pdfFilePathList.Clear();
            listBox.Items.Clear();
            Dictionary<string, bool> dictionary = new Dictionary<string, bool>();
            for(int i = 0; i < contractNote.ruleManager.ruleList.Count; i++)
            {
                string sourceFolderPath = contractNote.ruleManager.ruleList[i].Source;
                if (!Directory.Exists(sourceFolderPath) || dictionary.ContainsKey(sourceFolderPath)) continue;
                dictionary.Add(sourceFolderPath, true);
                foreach (string f in Directory.GetFiles(contractNote.ruleManager.ruleList[i].Source))
                {
                    if (!System.IO.Path.GetExtension(f).ToLower().Equals(".pdf")) continue;
                    pdfFilePathList.Add(f);
                    listBox.Items.Add(f);
                    if (pdfFilePathList.Count >= maxFileCount) break;
                }
                if (pdfFilePathList.Count >= maxFileCount) break;
            }
            if (pdfFilePathList.Count == 0)
            {
                MessageBox.Show("There is not Contract Note to process");
                return;
            }
            TemporaryDataManager.createNewTemporaryDirectory();
            contractNote.contractNoteItemManager.masterListManager.initializeMasterList();
            setEnable(false);
            contractNoteList.Clear();
            bitmapSourceList.Clear();
            currentParsingPdfNo = 0;
            parseContractNote();
        }

        private void approveButton_Click(object sender, RoutedEventArgs e)
        {
            int contractNoteNo = listBox.SelectedIndex;
            if (contractNoteNo == -1) return;
            backupContractNote(contractNoteNo);
            displayContractNote(contractNoteNo);
            if (!contractNoteList[contractNoteNo].success)
            {
                MessageBox.Show("Please fill in mandatory fields");
            }
        }

        private void skipButton_Click(object sender, RoutedEventArgs e)
        {
            int contractNoteNo = listBox.SelectedIndex;
            if (contractNoteNo == -1) return;
            backupContractNote(contractNoteNo);
            displayContractNote(contractNoteNo, true);
        }

        private void exportButton_Click(object sender, RoutedEventArgs e)
        {
            if (listBox.Items.Count == 0) return;
            Dictionary<string, List<ContractNoteItemManager>> dictionary = new Dictionary<string, List<ContractNoteItemManager>>();
            for (int i = listBox.Items.Count - 1; i >= 0; i--)
            {
                if (!contractNoteList[i].success) continue;
                Rule rule = contractNote.ruleManager.getRuleWithSource(Path.GetDirectoryName(pdfFilePathList[i]));
                string destinationPath = rule.Destination;
                if (!Directory.Exists(destinationPath))
                {
                    try
                    {
                        Directory.CreateDirectory(destinationPath);
                    }
                    catch
                    {
                        MessageBox.Show(string.Format("Can't create directory:\n{0}", destinationPath));
                        return;
                    }
                }
                string fileNameWithoutExtension = contractNoteList[i].contractNoteItems[23].Value;
                string pdfFilePath = string.Format(@"{0}\{1}.pdf", destinationPath, fileNameWithoutExtension);
                try
                {
                    File.Copy(pdfFilePathList[i], pdfFilePath, true);
                }
                catch { }
                try
                {
                    File.Delete(pdfFilePathList[i]);
                }
                catch { }
                string contractNoteInstance = contractNoteList[i].contractNoteItems[0].Value;
                if (!dictionary.ContainsKey(contractNoteInstance))
                {
                    dictionary.Add(contractNoteInstance, new List<ContractNoteItemManager>());
                }
                dictionary[contractNoteInstance].Add(contractNoteList[i]);
                pdfFilePathList.RemoveAt(i);
                contractNoteList.RemoveAt(i);
                bitmapSourceList.RemoveAt(i);
                listBox.Items.RemoveAt(i);
            }
            DateTime now = DateTime.Now;
            foreach (KeyValuePair<string, List<ContractNoteItemManager>> pair in dictionary)
            {
                try
                {
                    Rule rule = contractNote.ruleManager.getRuleWithInstance(pair.Key);
                    string csvFilePath = string.Format(@"{0}\{1}-{2}{3}{4}{5}{6}.csv", rule.Destination, pair.Key, now.Year, now.Month.ToString("00"), now.Day.ToString("00"), now.Hour.ToString("00"), now.Minute.ToString("00"));
                    StreamWriter sw = new StreamWriter(csvFilePath);
                    for (int i = 0; i < ContractNoteItemManager.FIELD_COUNT; i++)
                    {
                        sw.Write(string.Format("{0}{1}", ContractNoteItemManager.fields[i], i == ContractNoteItemManager.FIELD_COUNT - 1 ? "\r\n" : ","));
                    }
                    for (int i = 0; i < pair.Value.Count; i++)
                    {
                        ContractNoteItemManager cnim = pair.Value[i];
                        for (int j = 0; j < ContractNoteItemManager.FIELD_COUNT; j++)
                        {
                            sw.Write(string.Format(@"{0}{1}", cnim.contractNoteItems[j].Value.Replace(",", ""), j == ContractNoteItemManager.FIELD_COUNT - 1 ? "\r\n" : ","));
                        }
                    }
                    sw.Close();
                }
                catch { }
            }
            listBox.SelectedIndex = previousSelectedIndexOfListBox = -1;
            if (listBox.Items.Count > 0)
            {
                listBox.SelectedIndex = 0;
            }
            else
            {
                displayContractNote(-1);
                showImage(true);
            }
            scanButton.IsEnabled = allIsExported();
            exportButton.IsEnabled = false;

            if(scanButton.IsEnabled)
            {
                Process.Start(Application.ResourceAssembly.Location);
                Application.Current.Shutdown();
            }
            MessageBox.Show("Exporting Finished");
        }

        private void zoomInButton_Click(object sender, RoutedEventArgs e)
        {
            zoomValue--;
            showImage();
        }

        private void zoomOutButton_Click(object sender, RoutedEventArgs e)
        {
            zoomValue++;
            showImage();
        }

        private void prevPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (listBox.Items.Count == 0) return;
            if (listBox.SelectedIndex < 1) return;
            listBox.SelectedIndex--;
        }

        private void nextPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (listBox.Items.Count == 0) return;
            if (listBox.SelectedIndex > listBox.Items.Count - 2) return;
            listBox.SelectedIndex++;
        }

        private void settingButton_Click(object sender, RoutedEventArgs e)
        {
            SettingWindow window = new SettingWindow();
            window.ShowDialog();
            contractNote.ruleManager.loadRule();
        }

        private void helpButton_Click(object sender, RoutedEventArgs e)
        {
            HelpWindow window = new HelpWindow();
            window.ShowDialog();
        }

        private void quitButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listBox.SelectedIndex == -1) return;
            Title = string.Format("{0}    [{1}/{2}] {3}[{4}]", versionString, listBox.SelectedIndex + 1, pdfFilePathList.Count, isProcessing ? "Processing... " : "", listBox.SelectedValue.ToString());
            if (isProcessing || listBox.SelectedIndex == previousSelectedIndexOfListBox) return;
            if (previousSelectedIndexOfListBox != -1 && contractNoteList[previousSelectedIndexOfListBox].templateIsFound && !contractNoteList[previousSelectedIndexOfListBox].success)
            {
                MessageBox.Show("Please check and approve current Contract Note");
                listBox.SelectedIndex = previousSelectedIndexOfListBox;
                return;
            }
            showImage();
            backupContractNote(previousSelectedIndexOfListBox);
            displayContractNote(listBox.SelectedIndex);
            previousSelectedIndexOfListBox = listBox.SelectedIndex;
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            int contractNoteNo = listBox.SelectedIndex;
            if (contractNoteNo == -1) return;
            switch (e.Key)
            {
                case System.Windows.Input.Key.F1:
                    fieldTextBoxes[5].Text = "SELL";
                    break;
                case System.Windows.Input.Key.F2:
                    fieldTextBoxes[5].Text = "BUY";
                    break;
                case System.Windows.Input.Key.F5:
                    approveButton_Click(approveButton, null);
                    break;
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            System.Windows.Point listBoxPosition = new System.Windows.Point(11, 628);
            System.Windows.Point scrollViewerBorderPosition = new System.Windows.Point(398, 48);
            double newWidth = e.NewSize.Width, newHeight = e.NewSize.Height;
            double rightMargin = 30, bottomMargin = 30 + 23;
            listBox.Height = newHeight - listBoxPosition.Y - bottomMargin;
            scrollViewerBorder.Width = newWidth - scrollViewerBorderPosition.X - rightMargin;
            scrollViewerBorder.Height = newHeight - scrollViewerBorderPosition.Y - bottomMargin;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (!allIsExported())
            {
                MessageBoxResult messageBoxResult = MessageBox.Show("Do you want to continue without exporting opened Contract Note?", "Question", MessageBoxButton.YesNo);
                if (messageBoxResult == MessageBoxResult.No)
                {
                    e.Cancel = true;
                }
            }
        }

        void createFieldTextBoxes()
        {
            int rowHeight = 22;
            int labelWidth = 130;
            int textBoxWidth = 250;
            for(int i = 0; i < ContractNoteItemManager.FIELD_COUNT; i++)
            {
                Label fieldLabel = new Label();
                fieldLabel.Content = string.Format("{0}", ContractNoteItemManager.fields[i]);
                fieldLabel.FontSize = 11;
                fieldLabel.Width = labelWidth;
                fieldLabel.Height = rowHeight + 1;
                resultCanvas.Children.Add(fieldLabel);
                Canvas.SetLeft(fieldLabel, 0);
                Canvas.SetTop(fieldLabel, i * rowHeight);
                if (i == 24)
                {
                    fieldComboBox = new ComboBox();
                    fieldComboBox.Items.Add("Cancellation");
                    fieldComboBox.Items.Add("Reversal");
                    fieldComboBox.FontSize = 13;
                    fieldComboBox.Width = textBoxWidth;
                    fieldComboBox.Height = rowHeight + 1;
                    resultCanvas.Children.Add(fieldComboBox);
                    Canvas.SetLeft(fieldComboBox, labelWidth);
                    Canvas.SetTop(fieldComboBox, i * rowHeight);
                }
                else
                {
                    fieldTextBoxes[i] = new TextBox();
                    fieldTextBoxes[i].FontSize = 13;
                    fieldTextBoxes[i].Width = textBoxWidth;
                    fieldTextBoxes[i].Height = rowHeight + 1;
                    resultCanvas.Children.Add(fieldTextBoxes[i]);
                    Canvas.SetLeft(fieldTextBoxes[i], labelWidth);
                    Canvas.SetTop(fieldTextBoxes[i], i * rowHeight);
                }
            }
        }

        void backupContractNote(int contractNoteNo)
        {
            if (contractNoteNo == -1) return;
            for (int i = 0; i < ContractNoteItemManager.FIELD_COUNT; i++)
            {
                if (i == 24)
                {
                    contractNoteList[contractNoteNo].contractNoteItems[i].Value = fieldComboBox.Text;
                }
                else
                {
                    contractNoteList[contractNoteNo].contractNoteItems[i].Value = fieldTextBoxes[i].Text;
                }
            }
        }

        void displayContractNote(int contractNoteNo, bool skip = false)
        {
            if (contractNoteNo == -1)
            {
                for (int i = 0; i < ContractNoteItemManager.FIELD_COUNT; i++)
                {
                    if (i == 24)
                    {
                        fieldComboBox.Text = "";
                    }
                    else
                    {
                        fieldTextBoxes[i].Clear();
                    }
                }
                return;
            }
            contractNoteList[contractNoteNo].validateContractNoteItems(skip);
            for(int i = 0; i < ContractNoteItemManager.FIELD_COUNT; i++)
            {
                if (i == 24)
                {
                    fieldComboBox.Text = contractNoteList[contractNoteNo].contractNoteItems[i].Value;
                }
                else
                {
                    fieldTextBoxes[i].Text = contractNoteList[contractNoteNo].contractNoteItems[i].Value;
                }
                if (ContractNoteItemManager.mandatory[i])
                {
                    fieldTextBoxes[i].Background = contractNoteList[contractNoteNo].contractNoteItems[i].Valid ? new SolidColorBrush(System.Windows.Media.Color.FromRgb(150, 230, 150)) : new SolidColorBrush(System.Windows.Media.Color.FromRgb(230, 150, 150));
                }
            }
            if (contractNoteList[contractNoteNo].success)
            {
                exportButton.IsEnabled = true;
            }
        }

        void setEnable(bool enable)
        {
            scanButton.IsEnabled = enable;
            approveButton.IsEnabled = enable;
            exportButton.IsEnabled = enable;
            zoomInButton.IsEnabled = enable;
            zoomOutButton.IsEnabled = enable;
            prevPageButton.IsEnabled = enable;
            nextPageButton.IsEnabled = enable;
            settingButton.IsEnabled = enable;
            quitButton.IsEnabled = enable;
            listBox.IsEnabled = enable;
            for (int i = 0; i < ContractNoteItemManager.FIELD_COUNT; i++)
            {
                if (i == 24)
                {
                    fieldComboBox.IsEnabled = enable;
                }
                else
                {
                    fieldTextBoxes[i].IsEnabled = enable;
                }
            }
            isProcessing = !enable;
        }

        void parseContractNote()
        {
            listBox.SelectedIndex = currentParsingPdfNo;
            contractNote.parseContractNote(pdfFilePathList[currentParsingPdfNo]);
        }

        public void parsingCompleted(bool templateIsFound, string pngFilePath)
        {
            if (templateIsFound)
            {
                contractNote.contractNoteItemManager.completeContractNoteItems(pdfFilePathList[currentParsingPdfNo]);
            }
            else
            {
                contractNote.contractNoteItemManager.notFoundTemplate();
            }
            contractNote.contractNoteItemManager.validateContractNoteItems();
            contractNoteList.Add(contractNote.contractNoteItemManager.getContractNoteItemManagerCopy());
            GC.Collect();
            Console.WriteLine(string.Format("Garbage Collect [{0}]", DateTime.Now.ToString()));
            Bitmap originalBitmap = new Bitmap(System.Drawing.Image.FromFile(pngFilePath));
            int[] zoomScale = new int[] { 40, 30, 25 };
            List<BitmapSource> bitmapSources = new List<BitmapSource>();
            for (int j = 0; j < 3; j++)
            {
                int width = originalBitmap.Width * zoomScale[j] / 100;
                int height = originalBitmap.Height * zoomScale[j] / 100;
                Bitmap bitmap = new Bitmap(width, height);
                Graphics graphic = Graphics.FromImage(bitmap);
                graphic.DrawImage(originalBitmap, 0, 0, width, height);
                bitmapSources.Add(getBitmapSourceFromBitmap(bitmap));
            }
            bitmapSourceList.Add(bitmapSources);
            currentParsingPdfNo++;
            if (currentParsingPdfNo < pdfFilePathList.Count)
            {
                parseContractNote();
                return;
            }
            zoomValue = 1;
            setEnable(true);
            scanButton.IsEnabled = allIsExported();
            listBox.SelectedIndex = previousSelectedIndexOfListBox = -1;
            listBox.SelectedIndex = 0;
            MessageBox.Show("Parsing Completed");
        }

        BitmapSource getBitmapSourceFromBitmap(Bitmap bitmap)
        {
            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    bitmap.GetHbitmap(),
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions()
                );
        }

        void showImage(bool clear = false)
        {
            if (clear)
            {
                image.Source = null;
                return;
            }
            if (listBox.Items.Count == 0) return;
            zoomInButton.IsEnabled = (zoomValue != 0);
            zoomOutButton.IsEnabled = (zoomValue != 2);
            BitmapSource bitmapSource = null;
            try {
                bitmapSource=bitmapSourceList[listBox.SelectedIndex][zoomValue];
            }
            catch { }
            if (bitmapSource == null) return;
            image.Width = bitmapSource.Width;
            image.Height = bitmapSource.Height;
            image.Source = bitmapSource;
        }

        bool allIsExported()
        {
            for(int i = 0; i < Math.Min(listBox.Items.Count, contractNoteList.Count); i++)
            {
                if (contractNoteList[i].templateIsFound && !contractNoteList[i].isExported) return false;
            }
            return true;
        }
    }

    public class TemporaryDataManager
    {
        public static string temporaryBaseDirectoryPath = @"C:\Users\Public\ContractNotes";

        public static void cleanTemporaryDirectory()
        {
            try
            {
                Directory.Delete(temporaryBaseDirectoryPath, true);
            }
            catch { }
            Directory.CreateDirectory(temporaryBaseDirectoryPath);
        }

        public static void createNewTemporaryDirectory()
        {
            string tempDirPath = string.Format(@"{0}\{1}", temporaryBaseDirectoryPath, getDirectoryCount());
            Directory.CreateDirectory(tempDirPath);
        }

        public static string getTemporaryDirectoryPath()
        {
            return string.Format(@"{0}\{1}", temporaryBaseDirectoryPath, getDirectoryCount() - 1);
        }

        public static string getFilePath(string fileNameWithoutExtension, string extension)
        {
            return string.Format(@"{0}\{1}.{2}", getTemporaryDirectoryPath(), fileNameWithoutExtension, extension);
        }

        public static int getDirectoryCount()
        {
            if (!Directory.Exists(temporaryBaseDirectoryPath))
            {
                Directory.CreateDirectory(temporaryBaseDirectoryPath);
            }
            return Directory.GetDirectories(temporaryBaseDirectoryPath).Length;
        }
    }
}
