using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

namespace Visiomex.Projects.LabelingTool.Windows
{
    /// <summary>
    /// Interaction logic for LoadProjectWindow.xaml
    /// </summary>
    public partial class LoadProjectWindow : Window
    {
        private string searchKeyword;

        private string[] allItems;
        public string SelectedProject { get; set; }
        public string SelectedProjectForDelete { get; set; }
        public bool DeleteModel { get; set; }


        public ObservableCollection<string> Items { get; set; }

        protected void OnPropertyChanged(string name)
             => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public event PropertyChangedEventHandler PropertyChanged;

        public LoadProjectWindow(string[] items)
        {
            InitializeComponent();

            DataContext = this;

            allItems = items;
            Items = new ObservableCollection<string>();

            DeleteModelMenuItem.Visibility = Visibility.Visible;

            Filter();
        }

        private void Filter()
        {
            Items.Clear();

            foreach (string item in allItems)
                if (searchKeyword == null || item.Contains(searchKeyword.ToUpper())) Items.Add(item);
        }

        private void DemoItemsSearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            searchKeyword = DemoItemsSearchBox.Text;
            Filter();
        }

        private void ListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ListBox.SelectedIndex > -1)
            {
                SelectedProject = (string)ListBox.SelectedItem;
                Close();

            }
        }
        private void DeleteModelButton_Click(object sender, RoutedEventArgs e)
        {
            DeleteModel = true;
            Close();
        }
        private void ListBox_MouseRightClick(object sender, MouseButtonEventArgs e)
        {
            if (ListBox.SelectedIndex > -1)
            {
                SelectedProjectForDelete = (string)ListBox.SelectedItem;
            }
        }
    }
}
