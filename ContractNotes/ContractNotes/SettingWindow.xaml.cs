using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ContractNotes
{
    /// <summary>
    /// Interaction logic for SettingWindow.xaml
    /// </summary>
    public partial class SettingWindow : Window
    {
        public SettingWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                StreamReader sr = new StreamReader("rule.dat");
                int ruleCount = int.Parse(sr.ReadLine());
                for (int i = 0; i < ruleCount; i++)
                {
                    Rule rule = new Rule();
                    rule.readRule(sr);
                    dataGrid.Items.Add(rule);
                }
                sr.Close();
            }
            catch { }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            int rightMargin = 30, bottomMargin = 30 + 23;
            Point windowPosition = PointToScreen(new Point(0, 0));
            Point dataGridPosition = dataGrid.PointToScreen(new Point(0, 0));
            dataGrid.Width = e.NewSize.Width - (dataGridPosition.X - windowPosition.X) - rightMargin;
            dataGrid.Height = e.NewSize.Height - (dataGridPosition.Y - windowPosition.Y) - bottomMargin;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (saveButton.IsEnabled)
            {
                MessageBoxResult messageBoxResult = MessageBox.Show("Do you want to save the changed Rule?", "Question", MessageBoxButton.YesNo);
                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    saveButton_Click(saveButton, null);
                }
                saveButton.IsEnabled = false;
            }
        }

        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            if (instanceTextBox.Text.Equals(""))
            {
                MessageBox.Show("Please set Instance");
                return;
            }
            if (sourceTextBox.Text.Equals(""))
            {
                MessageBox.Show("Please set Source Path");
                return;
            }
            if (destinationTextBox.Text.Equals(""))
            {
                MessageBox.Show("Please set Destination Path");
                return;
            }
            bool instanceIsRepeat = false, sourceIsRepeat = false;
            for (int i = 0; i < dataGrid.Items.Count; i++)
            {
                Rule rule = (Rule)dataGrid.Items[i];
                if (instanceTextBox.Text.Equals(rule.Instance))
                {
                    instanceIsRepeat = true;
                    break;
                }
                if (sourceTextBox.Text.Equals(rule.Source))
                {
                    sourceIsRepeat = true;
                    break;
                }
            }
            if (instanceIsRepeat)
            {
                MessageBox.Show("Please set OTHER Instance");
                instanceTextBox.Clear();
                return;
            }
            if (sourceIsRepeat)
            {
                MessageBox.Show("Please set OTHER Source Path");
                sourceTextBox.Clear();
                return;
            }
            dataGrid.Items.Add(new Rule() {
                Instance = instanceTextBox.Text,
                Source = sourceTextBox.Text,
                Destination = destinationTextBox.Text
            });
            instanceTextBox.Clear();
            sourceTextBox.Clear();
            dataGrid.SelectedIndex = -1;
            saveButton.IsEnabled = true;
        }

        private void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            int selectedIndex = dataGrid.SelectedIndex;
            if (selectedIndex < 0 || selectedIndex >= dataGrid.Items.Count) return;
            MessageBoxResult messageBoxResult = MessageBox.Show("Do you want to remove the selected Rule?", "Question", MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.No) return;
            dataGrid.Items.RemoveAt(selectedIndex);
            dataGrid.SelectedIndex = -1;
            saveButton.IsEnabled = true;
        }

        private void updateButton_Click(object sender, RoutedEventArgs e)
        {
            int selectedIndex = dataGrid.SelectedIndex;
            if (selectedIndex < 0 || selectedIndex >= dataGrid.Items.Count) return;
            if (instanceTextBox.Text.Equals(""))
            {
                MessageBox.Show("Please set Instance");
                return;
            }
            if (sourceTextBox.Text.Equals(""))
            {
                MessageBox.Show("Please set Source Path");
                return;
            }
            if (destinationTextBox.Text.Equals(""))
            {
                MessageBox.Show("Please set Destination Path");
                return;
            }
            bool instanceIsRepeat = false, sourceIsRepeat = false;
            for (int i = 0; i < dataGrid.Items.Count; i++)
            {
                if (i == selectedIndex) continue;
                Rule rule = (Rule)dataGrid.Items[i];
                if (instanceTextBox.Text.Equals(rule.Instance))
                {
                    instanceIsRepeat = true;
                    break;
                }
                if (sourceTextBox.Text.Equals(rule.Source))
                {
                    sourceIsRepeat = true;
                    break;
                }
            }
            if (instanceIsRepeat)
            {
                MessageBox.Show("Please set OTHER Instance");
                instanceTextBox.Clear();
                return;
            }
            if (sourceIsRepeat)
            {
                MessageBox.Show("Please set OTHER Source Path");
                sourceTextBox.Clear();
                return;
            }
            MessageBoxResult messageBoxResult = MessageBox.Show("Do you want to change the selected Rule?", "Question", MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.No) return;
            dataGrid.Items[selectedIndex] = new Rule()
            {
                Instance = instanceTextBox.Text,
                Source = sourceTextBox.Text,
                Destination = destinationTextBox.Text
            };
            dataGrid.SelectedIndex = -1;
            saveButton.IsEnabled = true;
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            StreamWriter sw = new StreamWriter("rule.dat");
            sw.WriteLine(dataGrid.Items.Count);
            for (int i = 0; i < dataGrid.Items.Count; i++)
            {
                Rule rule = (Rule)dataGrid.Items[i];
                rule.writeRule(sw);
            }
            sw.Close();
            saveButton.IsEnabled = false;
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void browseSourceButton_Click(object sender, RoutedEventArgs e)
        {
            var folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            folderBrowserDialog.SelectedPath = sourceTextBox.Text;
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                sourceTextBox.Text = folderBrowserDialog.SelectedPath;
            }
        }

        private void browseDestinationButton_Click(object sender, RoutedEventArgs e)
        {
            var folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            folderBrowserDialog.SelectedPath = destinationTextBox.Text;
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                destinationTextBox.Text = folderBrowserDialog.SelectedPath;
            }
        }

        private void dataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int selectedIndex = dataGrid.SelectedIndex;
            if (selectedIndex == -1)
            {
                deleteButton.IsEnabled = false;
                updateButton.IsEnabled = false;
                return;
            }
            deleteButton.IsEnabled = true;
            updateButton.IsEnabled = true;
            Rule rule = (Rule)dataGrid.Items[selectedIndex];
            instanceTextBox.Text = rule.Instance;
            sourceTextBox.Text = rule.Source;
            destinationTextBox.Text = rule.Destination;
        }
    }

    public class Rule
    {
        public string Instance { get; set; }
        public string Source { get; set; }
        public string Destination { get; set; }

        public Rule()
        {
            Instance = Source = Destination = "";
        }

        public void readRule(StreamReader sr)
        {
            Instance = sr.ReadLine();
            Source = sr.ReadLine();
            Destination = sr.ReadLine();
        }

        public void writeRule(StreamWriter sw)
        {
            sw.WriteLine(Instance);
            sw.WriteLine(Source);
            sw.WriteLine(Destination);
        }
    }
}
