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
using Visiomex.Projects.LabelingTool.Models;
using Visiomex.Projects.LabelingTool.Utils;

namespace Visiomex.Projects.LabelingTool.Windows
{
    /// <summary>
    /// Interaction logic for ModelNameWindow.xaml
    /// </summary>
    public partial class ProjectNameWindow : Window
    {
        public string projectName = "";
        public string projectPath = "";

        public ProjectNameWindow(string projectPath)
        {
            InitializeComponent();

            this.projectPath = projectPath;
            Loaded += ProjectNameWindow_Loaded;
        }

        private void ProjectNameWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ProjectPathTextBox.Text = Paths.ProjectPath;
        }

        #region Click Events

        private void CreateNewModelButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(ProjectNameTextBox.Text))
                {
                    MessageBox.Show("Lütfen bir proje adı giriniz.", "Proje Adı Girilmedi");
                    return;
                }

                if (string.IsNullOrEmpty(ProjectPathTextBox.Text))
                {
                    MessageBox.Show("Lütfen bir proje yolu giriniz.", "Proje Yolu Girilmedi");
                    return;
                }

                if (!Directory.Exists(ProjectPathTextBox.Text))
                {
                    MessageBox.Show("Lütfen geçerli bir proje yolu giriniz.", "Proje Yolu Bulunamadı");
                    return;
                }

                if (Directory.Exists(System.IO.Path.Combine(ProjectPathTextBox.Text, ProjectNameTextBox.Text)))
                {
                    var messageBoxResult = MessageBox.Show("Kaydetmek istediğiniz modele ait klasör var. Klasör silinsin mi?", "Mevcut Klasör Bulundu", MessageBoxButton.YesNo);

                    if (messageBoxResult == MessageBoxResult.No)
                        return;

                    if (messageBoxResult == MessageBoxResult.Yes)
                        Directory.Delete(System.IO.Path.Combine(ProjectPathTextBox.Text, ProjectNameTextBox.Text), true);
                }

                projectName = ProjectNameTextBox.Text;
                projectPath = ProjectPathTextBox.Text;



                Close();
            }
            catch { }
        }

        private void ProjectPathButton_Click(object sender, RoutedEventArgs e)
        {
            var folderDialog = new System.Windows.Forms.FolderBrowserDialog();
            {
                Title = "Dosya Yolu Seçiniz";
            };

            if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ProjectPathTextBox.Text = HelperProcedures.RemoveDiacritics(folderDialog.SelectedPath);
            }
        }

        #endregion

        public string GetProjectName()
        {
            return projectName.ToUpper();
        }

        public string GetProjectPath()
        {
            return projectPath;
        }
    }
}
