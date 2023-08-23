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
    /// Interaction logic for AddDeleteWindow.xaml
    /// </summary>
    public partial class AddDeleteWindow : Window
    {
        public AddDeleteWindow()
        {
            InitializeComponent();
            Loaded += AddDeleteWindow_Loaded;
        }

        private void AddDeleteWindow_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (var labelType in ProjectHelper.Instance.LabelTypes)
                LabelTypesListBox.Items.Add(CreateTextBlock(labelType));
        }

        #region Click Events

        private void AddLabelType_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(LabelTypeTextBox.Text))
            {
                MessageBox.Show("Lütfen bir etiket adı giriniz.", "Etiket Girilmedi");
                return;
            }

            foreach (var item in LabelTypesListBox.Items)
            {
                TextBlock textBox = (TextBlock)item;

                if (textBox.Text.Equals(HelperProcedures.RemoveDiacritics(LabelTypeTextBox.Text.ToUpper())))
                {
                    MessageBox.Show("Böyle bir etiket var, ekleyemezsiniz.","Benzer Etiket");
                    return;
                }
            }

            LabelTypesListBox.Items.Add(CreateTextBlock(HelperProcedures.RemoveDiacritics(LabelTypeTextBox.Text.ToUpper())));
        }

        private void SaveLabelTypesButton_Click(object sender, RoutedEventArgs e)
        {
            ChangeLabelTypes();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var textBlock = (((ContextMenu)((MenuItem)sender).Parent).PlacementTarget as TextBlock);
                var labelName = textBlock.Text;

                var labelledImageCount = ProjectHelper.Instance.ImageModels.Where(x => x.RegionModels.Where(y => y.LabelType.Equals(labelName)).Count() > 0).Count();

                if (labelledImageCount > 0)
                {
                    var result = MessageBox.Show($"Bu etiket {labelledImageCount} fotoğraf üzerinde kullanılmış silmek istediğinize emin misiniz?", "Etiket Kullanılıyor", MessageBoxButton.YesNo);

                    if (result == MessageBoxResult.No)
                        return;
                }

                LabelTypesListBox.Items.Remove(textBlock);

                foreach (var labelledImage in ProjectHelper.Instance.ImageModels)
                {
                    labelledImage.RegionModels.RemoveAll(x => x.LabelType.Equals(labelName));

                    var labelPath = System.IO.Path.Combine(Paths.ProjectPath, ProjectHelper.Instance.ProjectName, Paths.LabeledImages, labelledImage.GUID,
                                                    Paths.LabelsFolder, labelName);

                    if (Directory.Exists(labelPath))
                        Directory.Delete(labelPath, true);
                }

                ChangeLabelTypes();
            }
            catch { }
        }

        #endregion

        #region Helper Procedures

        private TextBlock CreateTextBlock(string text)
        {
            var textBlock = new TextBlock();

            textBlock.Text = text;
            textBlock.FontFamily = new FontFamily("Comic Sans MS");
            textBlock.FontSize = 11;
            textBlock.Width = 380;
            textBlock.FontWeight = FontWeights.DemiBold;

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = new MenuItem() { Header = "Sil" };
            menuItem.Click += MenuItem_Click;
            contextMenu.Items.Add(menuItem);
            textBlock.ContextMenu = contextMenu;

            return textBlock;
        }

        private void ChangeLabelTypes()
        {
            try
            {
                ProjectHelper.Instance.LabelTypes = new List<string>();

                foreach (var item in LabelTypesListBox.Items)
                {
                    TextBlock textBox = (TextBlock)item;

                    ProjectHelper.Instance.LabelTypes.Add(textBox.Text);
                }

                if (ProjectHelper.Instance.LabelTypes == null || ProjectHelper.Instance.LabelTypes.Count == 0)
                    return;

                var labelTypesText = Newtonsoft.Json.JsonConvert.SerializeObject(ProjectHelper.Instance.LabelTypes);

                var labelPath = System.IO.Path.Combine(ProjectHelper.Instance.ProjectPath, ProjectHelper.Instance.ProjectName, Paths.LabelsFolder);

                if (!Directory.Exists(labelPath))
                    Directory.CreateDirectory(labelPath);

                File.WriteAllText(System.IO.Path.Combine(labelPath, Paths.LabelsFile), labelTypesText);
            }
            catch { }
        }

        #endregion
    }
}
