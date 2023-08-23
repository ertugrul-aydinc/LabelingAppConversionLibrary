using HalconDotNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Visiomex.Projects.LabelingTool.Models;
using Visiomex.Projects.LabelingTool.Models.Enums;
using Visiomex.Projects.LabelingTool.Utils;
using Visiomex.Projects.LabelingTool.Windows;

namespace Visiomex.Projects.LabelingTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Control Properties

        private DrawTypeEnum drawType = DrawTypeEnum.FreeHand;
        private RectangleDirection rectangleDirection;

        private bool isLabelTypeHandle = false;
        private bool isAllImagesHandle = false;

        private bool isDraw { get; set; } = false;
        private bool isMouseDown { get; set; } = false;

        private bool isRectangleChange { get; set; } = false;
        private bool isRectangleMove { get; set; } = true;

        private bool isCircleMove { get; set; } = false;

        private bool isCtrlDown { get; set; } = false;


        private Point oldPoint = new Point(-1, -1);
        private Point currentPoint = new Point();

        private Point rectangleStartPoint = new Point(-1, -1);
        private Point rectangleStopPoint = new Point();
        private Point rectangleCurrentPoint = new Point();

        private Point circleCenterPoint = new Point(-1, -1);
        private double circleRadius = 0;
        private Point circleCurrentPoint = new Point();

        private List<Point> selectedPoints = new List<Point>();
        private List<Point> undoStack = new List<Point>();

        private HTuple imageWidthSize;
        private HTuple imageHeightSize;


        #endregion

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Init();

            KeyDown += MainWindow_KeyDown;
            KeyUp += MainWindow_KeyUp;
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl)
                isCtrlDown = true;

            if (e.Key == Key.Delete && !isDraw)
                DeleteRegion();

            if (isCtrlDown && e.Key == Key.Z)
                UndoFreeDrawing();

            if (isCtrlDown && e.Key == Key.Y)
                FastForwardFreeDrawing();
        }

        private void MainWindow_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl)
                isCtrlDown = false;
        }


        private void Init()
        {
            if (!Directory.Exists(Paths.GeneralPath))
                Directory.CreateDirectory(Paths.GeneralPath);

            if (!Directory.Exists(Paths.ProjectPath))
                Directory.CreateDirectory(Paths.ProjectPath);

            DrawTypeComboBox.SelectedIndex = 0;
            HOperatorSet.SetSystem("clip_region", "false");
        }

        #region Click Events

        private void AddDeleteLabelTypeButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(ProjectHelper.Instance.ProjectName))
            {
                MessageBox.Show("Lütfen ilk olarak bir proje oluşturunuz.", "Proje Bulunamadı");
                return;
            }

            if (isDraw)
            {
                MessageBox.Show("Çizim devam ediyor, lütfen bitiriniz.", "Etiket Eklenemedi");
                return;
            }

            var addDeleteWindow = new AddDeleteWindow();

            addDeleteWindow.ShowDialog();

            LabelTypeComboBox.Items.Clear();

            foreach (var labelType in ProjectHelper.Instance.LabelTypes)
                LabelTypeComboBox.Items.Add(labelType);

            if (LabelTypeComboBox.SelectedIndex > 0 || LabelTypeComboBox.Items.Count < 1)
                return;

            LabelTypeComboBox.SelectedIndex = 0;
        }

        private void CloseApplicationButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void CreateNewProjectButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(ProjectHelper.Instance.ProjectName))
            {
                var messageBoxResult = MessageBox.Show("Şu an bir proje yüklü. Yine de yeni projeye geçilsin mi?", "Yeni Proje", MessageBoxButton.YesNo);

                if (messageBoxResult == MessageBoxResult.No)
                    return;
            }

            if (isDraw)
            {
                MessageBox.Show("Çizim devam ediyor, lütfen bitiriniz.", "Yeni Proje Oluşturulamadı");
                return;
            }

            ClearProject();

            var projectNameWindow = new ProjectNameWindow(ProjectHelper.Instance.ProjectPath);
            projectNameWindow.ShowDialog();

            ProjectHelper.Instance.ProjectName = HelperProcedures.RemoveDiacritics(projectNameWindow.GetProjectName());
            ProjectHelper.Instance.ProjectPath = projectNameWindow.GetProjectPath();

            if (string.IsNullOrEmpty(ProjectHelper.Instance.ProjectName) || string.IsNullOrEmpty(ProjectHelper.Instance.ProjectPath))
                return;

            Directory.CreateDirectory(Path.Combine(ProjectHelper.Instance.ProjectPath, ProjectHelper.Instance.ProjectName));
        }

        private void LoadProjectButton_Click(object sender, RoutedEventArgs e)
        {
            var projects = Directory.GetDirectories(Paths.ProjectPath);

            if (!projects.Any())
            {
                MessageBox.Show("Hiç kayıtlı model bulunamadı.", "Model Bulunamadı");
                return;
            }

            if (isDraw)
            {
                MessageBox.Show("Çizim devam ediyor, lütfen bitiriniz.", "Model Yüklenemedi");
                return;
            }

            projects = projects.Select(x => Path.GetFileName(x)).ToArray();

            var loadProjectWindow = new LoadProjectWindow(projects);
            loadProjectWindow.ShowDialog();

            if (loadProjectWindow.DeleteModel)
            {
                HelperProcedures.DeleteModel(loadProjectWindow.SelectedProjectForDelete);

                if (loadProjectWindow.SelectedProjectForDelete == ProjectHelper.Instance.ProjectName)
                    ProjectHelper.Instance.ClearAll();

                MessageBox.Show("Proje başarılı bir şekilde silindi.", "Proje Sil");

                return;
            }

            if (string.IsNullOrEmpty(loadProjectWindow.SelectedProject))
            {
                MessageBox.Show("Lütfen model seçimi yapınız.", "Model Seçilmedi");
                return;
            }

            ClearProject();

            ProjectHelper.Instance.ProjectName = loadProjectWindow.SelectedProject;
            ProjectHelper.Instance.ProjectPath = Paths.ProjectPath;

            if (File.Exists(Path.Combine(Paths.ProjectPath, ProjectHelper.Instance.ProjectName, Paths.LabelsFolder, Paths.LabelsFile)))
            {
                var labelTypesText = File.ReadAllText(Path.Combine(Paths.ProjectPath, ProjectHelper.Instance.ProjectName, Paths.LabelsFolder, Paths.LabelsFile));

                ProjectHelper.Instance.LabelTypes = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(labelTypesText);

                foreach (var labelType in ProjectHelper.Instance.LabelTypes)
                    LabelTypeComboBox.Items.Add(labelType);

                if (LabelTypeComboBox.Items.Count > 0)
                    LabelTypeComboBox.SelectedIndex = 0;
            }

            if (Directory.Exists(Path.Combine(Paths.ProjectPath, ProjectHelper.Instance.ProjectName, Paths.LabeledImages)))
            {
                var labeledImageFolders = Directory.GetDirectories(Path.Combine(Paths.ProjectPath, ProjectHelper.Instance.ProjectName, Paths.LabeledImages));

                foreach (var labeledImageFolder in labeledImageFolders)
                {
                    var imagePath = "";

                    if (File.Exists(Path.Combine(labeledImageFolder, Paths.ImagePath)))
                        imagePath = File.ReadAllText(Path.Combine(labeledImageFolder, Paths.ImagePath));

                    ProjectHelper.Instance.ImageModels.Add(new ImageModel(imagePath, Path.GetFileName(labeledImageFolder)));
                }

                foreach (var imageModel in ProjectHelper.Instance.ImageModels)
                    AllImagesListView.Items.Add(CreateTextBlock(imageModel.GUID));
            }
        }

        private void AddROIMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(ProjectHelper.Instance.ProjectName))
            {
                MessageBox.Show("Çizim yapmak için proje oluşturunuz/açınız.", "Etiket Çizilemedi");
                return;
            }

            if (ProjectHelper.Instance.SelectedImage == null || ProjectHelper.Instance.SelectedImage.Image == null || !ProjectHelper.Instance.SelectedImage.Image.IsInitialized())
            {
                MessageBox.Show("Etiketlenecek fotoğrafı seçiniz.", "Fotoğraf Seçilmedi");
                return;
            }

            if (LabelTypeComboBox.SelectedIndex < 0)
            {
                MessageBox.Show("Lütfen bir etiket seçiniz.", "Etiket Seçilmedi");
                return;
            }

            if (isDraw)
            {
                MessageBox.Show("Region çizimi devam ediyor. Lütfen çizimi bitiriniz.", "Region Çizim");
                return;
            }

            LabellingHWindow.HMoveContent = false;

            DisplayImageAndRegions();

            var contextMenu = sender as MenuItem;

            LabellingHWindow.HalconWindow.SetColor(RegionColors.GetColor(LabelTypeComboBox.SelectedIndex));
            LabellingHWindow.HalconWindow.SetDraw("margin");

            int selectedTypeIndex = DrawTypeComboBox.SelectedIndex;

            drawType = selectedTypeIndex == 0 ? DrawTypeEnum.FreeHand : selectedTypeIndex == 1 ? DrawTypeEnum.Rectangle : DrawTypeEnum.Cicle;
            isDraw = true;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var imageGUID = (((ContextMenu)((MenuItem)sender).Parent).PlacementTarget as TextBlock).Text;

            var imageModel = ProjectHelper.Instance.ImageModels.FirstOrDefault(x => x.GUID.Equals(imageGUID));

            var imageModelPath = Path.Combine(Paths.ProjectPath, ProjectHelper.Instance.ProjectName, Paths.LabeledImages, imageGUID);

            if (imageModel != null)
                ProjectHelper.Instance.ImageModels.Remove(imageModel);

            if (Directory.Exists(imageModelPath))
                Directory.Delete(imageModelPath, true);

            AllImagesListView.Items.Clear();

            foreach (var modelImage in ProjectHelper.Instance.ImageModels)
            {
                AllImagesListView.Items.Add(CreateTextBlock(modelImage.GUID));
            }
        }

        #endregion

        #region Change Events

        private void DrawTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var drawTypeCombobox = (ComboBox)sender;

            switch (drawTypeCombobox.SelectedIndex)
            {
                case 1:
                    ProjectHelper.Instance.DrawType = DrawTypeEnum.FreeHand;
                    break;
                case 2:
                    ProjectHelper.Instance.DrawType = DrawTypeEnum.Rectangle;
                    break;
                case 3:
                    ProjectHelper.Instance.DrawType = DrawTypeEnum.Cicle;
                    break;
                default:
                    ProjectHelper.Instance.DrawType = DrawTypeEnum.FreeHand;
                    break;
            }
        }

        private void AllImagesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isDraw)
            {
                if (isAllImagesHandle)
                {
                    isAllImagesHandle = false;
                    return;
                }

                MessageBox.Show("Çizim devam ederken başka fotoğraf seçilemez.", "Çizim Devam Eiyor");
                isAllImagesHandle = true;
                AllImagesListView.SelectedItem = e.RemovedItems[0];

                return;
            }

            ListView listBox = (ListView)sender;

            TextBlock textBlock = (TextBlock)listBox.SelectedItem;

            if (textBlock == null)
                return;

            ProjectHelper.Instance.SelectedImage = ProjectHelper.Instance.ImageModels.FirstOrDefault(x => x.GUID.Equals(textBlock.Text));

            if (ProjectHelper.Instance.SelectedImage == null)
                return;

            HelperProcedures.DisplayImageRegion(LabellingHWindow.HalconWindow);
            DisplayRegions();

            if (ProjectHelper.Instance.SelectedImage.Image != null && ProjectHelper.Instance.SelectedImage.Image.IsInitialized())
                ProjectHelper.Instance.SelectedImage.Image.GetImageSize(out imageWidthSize, out imageHeightSize);
        }

        private void LabelTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isDraw && !isLabelTypeHandle)
            {
                isLabelTypeHandle = true;
                LabelTypeComboBox.SelectedItem = e.RemovedItems[0];
                return;
            }

            isLabelTypeHandle = false;
        }

        #endregion

        #region Drag Drop Events

        private void DropFilesPathBorder_Drop(object sender, DragEventArgs e)
        {
            DropFilesPathBorder.Background = Brushes.White;

            if (string.IsNullOrEmpty(ProjectHelper.Instance.ProjectName))
            {
                Task.Run(() => { MessageBox.Show("Fotoğraf yüklemek için lütfen yeni proje oluşturunuz.", "Proe Bulunamadı"); });
                return;
            }

            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                return;

            string[] allFilePath = (string[])e.Data.GetData(DataFormats.FileDrop);

            if (allFilePath == null)
                return;

            var allImageModels = ProjectHelper.Instance.ImageModels;

            foreach (var filePath in allFilePath)
            {
                var extention = Path.GetExtension(filePath);

                if (!extention.Contains("tiff") && !extention.Contains("tif") && !extention.Contains("png") && !extention.Contains("jpg") && !extention.Contains("jpeg") && !extention.Contains("bmp"))
                    continue;

                if (allImageModels.Exists(x => x.ImagePath.Equals(filePath)))
                    continue;

                allImageModels.Add(new ImageModel(filePath, Guid.NewGuid().ToString()));
            }

            AllImagesListView.Items.Clear();

            foreach (var imageModel in allImageModels)
            {
                AllImagesListView.Items.Add(CreateTextBlock(imageModel.GUID));

                var imageModelPath = Path.Combine(Paths.ProjectPath, ProjectHelper.Instance.ProjectName, Paths.LabeledImages, imageModel.GUID);

                if (!Directory.Exists(imageModelPath))
                    Directory.CreateDirectory(imageModelPath);

                File.WriteAllText(Path.Combine(imageModelPath, Paths.ImagePath), imageModel.ImagePath);
            }
        }

        private void DropFilesPathBorder_DragOver(object sender, DragEventArgs e)
        {
            var converter = new System.Windows.Media.BrushConverter();
            var brush = (Brush)converter.ConvertFromString("#C0C0C0");

            DropFilesPathBorder.Background = brush;
        }

        private void DropFilesPathBorder_DragLeave(object sender, DragEventArgs e)
        {
            DropFilesPathBorder.Background = Brushes.White;
        }

        #endregion

        #region Mouse Events

        private void LabellingHWindow_HMouseDown(object sender, HSmartWindowControlWPF.HMouseEventArgsWPF e)
        {
            currentPoint.X = e.Row;
            currentPoint.Y = e.Column;

            Console.WriteLine("a " + e.Button.ToString() );

            if (e.Button == MouseButton.Right || isMouseDown)
            {
                if (!isDraw)
                    isMouseDown = false;
                else
                {
                    isDraw = false;

                    Task.Delay(50);

                    if ((selectedPoints == null || !selectedPoints.Any()) && (drawType == DrawTypeEnum.FreeHand))
                        return;

                    CreateRegion();

                    LabellingHWindow.HMoveContent = true;
                }

                return;
            }

            isMouseDown = true;

            if (!isDraw && ProjectHelper.Instance.SelectedImage != null && e.Button == MouseButton.Left)
            {
                ProjectHelper.Instance.RegionForMove = ProjectHelper.Instance.SelectedImage.RegionModels.Find(x => x.Region.TestRegionPoint(e.Row, e.Column) == 1);

                if (ProjectHelper.Instance.RegionForMove != null)
                    LabellingHWindow.HMoveContent = false;

                DisplayImageAndRegions();
            }

            if (!isDraw)
                return;

            if (drawType == DrawTypeEnum.FreeHand)
            {
                DrawLine();

                if (undoStack.Count > 0)
                    undoStack = new List<Point>();

                return;
            }
            else if (drawType == DrawTypeEnum.Rectangle)
            {

                if (rectangleStartPoint.X == -1 || rectangleStartPoint.Y == -1)
                {
                    rectangleStartPoint.X = e.Row;
                    rectangleStartPoint.Y = e.Column;

                    return;
                }

                isRectangleChange = true;

                if (e.Row > rectangleStartPoint.X + 10 && e.Row < rectangleStopPoint.X - 10 && e.Column > rectangleStartPoint.Y + 10 && e.Column < rectangleStopPoint.Y - 10)
                {
                    isRectangleMove = true;
                    rectangleCurrentPoint = new Point(e.Row, e.Column);
                }
                else
                {
                    isRectangleMove = false;
                    rectangleDirection = FindRectangleDirection(e, rectangleStartPoint, rectangleStopPoint);
                    rectangleCurrentPoint = new Point(e.Row, e.Column);
                }
            }
            else if (drawType == DrawTypeEnum.Cicle)
            {
                if (circleCenterPoint.X == -1 || circleCenterPoint.Y == -1)
                {
                    circleCenterPoint.X = e.Row;
                    circleCenterPoint.Y = e.Column;

                    circleRadius = 0;
                    circleCurrentPoint = new Point(e.Row, e.Column);

                    isCircleMove = false;

                    return;
                }

                if (Math.Sqrt(Math.Pow(e.Row - circleCenterPoint.X, 2) + Math.Pow(e.Column - circleCenterPoint.Y, 2)) < circleRadius)
                {
                    isCircleMove = true;
                    circleCurrentPoint = new Point(e.Row, e.Column);
                }
                else
                {
                    isCircleMove = false;
                    circleCurrentPoint = new Point(e.Row, e.Column);
                }
            }

        }

        private void LabellingHWindow_HMouseMove(object sender, HSmartWindowControlWPF.HMouseEventArgsWPF e)
        {
            if (!isDraw && isMouseDown)
            {
                if (ProjectHelper.Instance.RegionForMove != null && ProjectHelper.Instance.RegionForMove.Region != null && ProjectHelper.Instance.RegionForMove.Region.IsInitialized())
                    ProjectHelper.Instance.RegionForMove.ChangeRegion(MoveRegion(ProjectHelper.Instance.RegionForMove.Region, e));

                DisplayImageAndRegions();

                return;
            }

            if (!isDraw || !isMouseDown)
                return;

            if (drawType == DrawTypeEnum.FreeHand)
            {
                currentPoint.X = e.Row;
                currentPoint.Y = e.Column;

                DrawLine();
                return;
            }
            else if (drawType == DrawTypeEnum.Rectangle)
            {

                if (e.Row < 0 || e.Column < 0 || e.Row >= imageHeightSize - 1 || e.Column >= imageWidthSize - 1)
                    return;

                if (isRectangleChange)
                {
                    if (isRectangleMove)
                    {
                        var differenceRow = e.Row - rectangleCurrentPoint.X;
                        var differenceColumn = e.Column - rectangleCurrentPoint.Y;

                        rectangleStartPoint.X += differenceRow;
                        rectangleStartPoint.Y += differenceColumn;
                        rectangleStopPoint.X += differenceRow;
                        rectangleStopPoint.Y += differenceColumn;

                        rectangleCurrentPoint = new Point(e.Row, e.Column);
                    }
                    else
                    {
                        ChangeRectanglePoints(rectangleDirection, e, rectangleCurrentPoint, rectangleStartPoint, rectangleStopPoint, out rectangleStartPoint, out rectangleStopPoint);
                        rectangleCurrentPoint = new Point(e.Row, e.Column);
                    }
                }
                else
                {
                    rectangleStopPoint.X = e.Row;
                    rectangleStopPoint.Y = e.Column;
                }

                DisplayImageAndRegions();

                GetMinMax(rectangleStartPoint.X, rectangleStopPoint.X, out var minRow, out var maxRow);
                GetMinMax(rectangleStartPoint.Y, rectangleStopPoint.Y, out var minColumn, out var maxColumn);

                try
                {
                    HOperatorSet.GenRectangle1(out var rectangle, minRow, minColumn, maxRow, maxColumn);
                    LabellingHWindow.HalconWindow.DispObj(rectangle);
                    rectangle.Dispose();
                }
                catch { }
            }
            else if (drawType == DrawTypeEnum.Cicle)
            {
                if (e.Row < 0 || e.Column < 0 || e.Row >= imageHeightSize - 1 || e.Column >= imageWidthSize - 1)
                    return;

                if (isCircleMove)
                {
                    var differenceRow = e.Row - circleCurrentPoint.X;
                    var differenceColumn = e.Column - circleCurrentPoint.Y;

                    circleCenterPoint.X += differenceRow;
                    circleCenterPoint.Y += differenceColumn;

                    circleCurrentPoint = new Point(e.Row, e.Column);
                }
                else
                {
                    var oldDistance = Math.Sqrt(Math.Pow(circleCenterPoint.X - circleCurrentPoint.X, 2) + Math.Pow(circleCenterPoint.Y - circleCurrentPoint.Y, 2));
                    var newDistance = Math.Sqrt(Math.Pow((circleCenterPoint.X - e.Row), 2) + Math.Pow((circleCenterPoint.Y - e.Column), 2));

                    circleRadius += newDistance - oldDistance;

                    if (circleRadius < 0)
                        circleRadius = 0;

                    circleCurrentPoint = new Point(e.Row, e.Column);
                }

                DisplayImageAndRegions();

                try
                {
                    HOperatorSet.GenCircle(out var circle, circleCenterPoint.X, circleCenterPoint.Y, circleRadius);
                    LabellingHWindow.HalconWindow.DispObj(circle);
                    circle.Dispose();
                }
                catch(Exception ex) 
                { }
            }

        }

        private void LabellingHWindow_HMouseUp(object sender, HSmartWindowControlWPF.HMouseEventArgsWPF e)
        {
            isMouseDown = false;

            if (!isDraw)
                LabellingHWindow.HMoveContent = true;

            if (ProjectHelper.Instance.SelectedImage != null && ProjectHelper.Instance.SelectedImage.Image != null && ProjectHelper.Instance.SelectedImage.Image.IsInitialized())
                ProjectHelper.Instance.SelectedImage.SelectedRegion = ProjectHelper.Instance.SelectedImage.RegionModels.Find(x => x.Region.TestRegionPoint(e.Row, e.Column) == 1);

            if (ProjectHelper.Instance.RegionForMove != null)
            {
                var regionPath = Path.Combine(Paths.ProjectPath, ProjectHelper.Instance.ProjectName, Paths.LabeledImages, ProjectHelper.Instance.SelectedImage.GUID, Paths.LabelsFolder, ProjectHelper.Instance.RegionForMove.LabelType, ProjectHelper.Instance.RegionForMove.GUID);

                if (File.Exists(Path.Combine(regionPath, Paths.Region)))
                    File.Delete(Path.Combine(regionPath, Paths.Region));

                HOperatorSet.WriteObject(ProjectHelper.Instance.RegionForMove.Region, Path.Combine(regionPath, Paths.Region));

                ProjectHelper.Instance.RegionForMove = null;

                DisplayImageAndRegions();

                if (ProjectHelper.Instance.SelectedImage.SelectedRegion != null)
                    DisplaySingleRegion(ProjectHelper.Instance.SelectedImage.SelectedRegion.Region, $"{RegionColors.GetColor(ProjectHelper.Instance.LabelTypes.FindIndex(x => x.Equals(ProjectHelper.Instance.SelectedImage.SelectedRegion.LabelType)))}60");
            }

            if (!isDraw)
                return;

            if (drawType == DrawTypeEnum.Rectangle)
            {
                GetMinMax(rectangleStartPoint.X, rectangleStopPoint.X, out var minRow, out var maxRow);
                GetMinMax(rectangleStartPoint.Y, rectangleStopPoint.Y, out var minColumn, out var maxColumn);

                rectangleStartPoint = new Point(minRow, minColumn);
                rectangleStopPoint = new Point(maxRow, maxColumn);
            }

            return;
        }

        private void LabellingHWindow_HMouseDoubleClick(object sender, HSmartWindowControlWPF.HMouseEventArgsWPF e)
        {
            if (e.Button == MouseButton.Right)
                return;

            if (isDraw)
            {
                isDraw = false;

                Task.Delay(50);

                if ((selectedPoints == null || !selectedPoints.Any()) && (drawType == DrawTypeEnum.FreeHand))
                    return;

                CreateRegion();

                LabellingHWindow.HMoveContent = true;

                return;
            }

            DisplayImageAndRegions();

            if (ProjectHelper.Instance.SelectedImage == null || ProjectHelper.Instance.SelectedImage.Image == null || !ProjectHelper.Instance.SelectedImage.Image.IsInitialized())
                return;

            ProjectHelper.Instance.SelectedImage.SelectedRegion = ProjectHelper.Instance.SelectedImage.RegionModels.Find(x => x.Region.TestRegionPoint(e.Row, e.Column) == 1);

            if (ProjectHelper.Instance.SelectedImage.SelectedRegion != null)
                DisplaySingleRegion(ProjectHelper.Instance.SelectedImage.SelectedRegion.Region, $"{RegionColors.GetColor(ProjectHelper.Instance.LabelTypes.FindIndex(x => x.Equals(ProjectHelper.Instance.SelectedImage.SelectedRegion.LabelType)))}60");

            if (ProjectHelper.Instance.SelectedImage.SelectedRegion == null)
                LabellingHWindow.HalconWindow.SetPart(0, 0, -2, 2);
        }

        private void LabellingHWindow_HMouseWheel(object sender, HSmartWindowControlWPF.HMouseEventArgsWPF e)
        {
            DisplayImageAndRegions();

            if (!isDraw)
                return;

            if (drawType == DrawTypeEnum.FreeHand)
            {
                var rows = new HTuple(selectedPoints.Select(x => x.X).ToArray());
                var cols = new HTuple(selectedPoints.Select(x => x.Y).ToArray());

                HOperatorSet.GenRegionPolygon(out var region, rows, cols);
                LabellingHWindow.HalconWindow.DispObj(region);
                region.Dispose();
            }
            else if (drawType == DrawTypeEnum.Rectangle)
            {
                try
                {
                    HOperatorSet.GenRectangle1(out var region, rectangleStartPoint.X, rectangleStartPoint.Y, rectangleStopPoint.X, rectangleStopPoint.Y);
                    LabellingHWindow.HalconWindow.DispObj(region);
                    region.Dispose();
                }
                catch { }
            }
            else if(drawType == DrawTypeEnum.Cicle)
            {
                try
                {
                    HOperatorSet.GenCircle(out var region, circleCenterPoint.X, circleCenterPoint.Y, circleRadius);
                    LabellingHWindow.HalconWindow.DispObj(region);
                    region.Dispose();
                }
                catch { }
            }
        }

        #endregion

        #region Helper Procedures

        private void ClearProject()
        {
            ProjectHelper.Instance.ClearAll();
            AllImagesListView.Items.Clear();
            LabelTypeComboBox.Items.Clear();
        }

        private void DrawLine()
        {
            if (currentPoint.X < 0 || currentPoint.Y < 0 || currentPoint.X >= imageHeightSize - 1 || currentPoint.Y >= imageWidthSize - 1)
                return;

            if (oldPoint.X == -1 && oldPoint.Y == -1)
            {
                oldPoint.X = currentPoint.X;
                oldPoint.Y = currentPoint.Y;
            }

            HOperatorSet.GenRegionLine(out var regionLines, oldPoint.X, oldPoint.Y, currentPoint.X, currentPoint.Y);
            LabellingHWindow.HalconWindow.DispObj(regionLines);

            oldPoint.X = currentPoint.X;
            oldPoint.Y = currentPoint.Y;

            selectedPoints.Add(new Point(currentPoint.X, currentPoint.Y));
        }

        private void DisplayImageAndRegions()
        {
            DisplayImage();
            DisplayRegions();
        }

        private void DisplayRegions()
        {
            if (ProjectHelper.Instance.SelectedImage == null)
                return;

            LabellingHWindow.HalconWindow.SetDraw("margin");

            foreach (var regionModel in ProjectHelper.Instance.SelectedImage.RegionModels)
            {
                if (regionModel.Region == null || !regionModel.Region.IsInitialized())
                    continue;

                var color = RegionColors.GetColor(ProjectHelper.Instance.LabelTypes.FindIndex(x => x.Equals(regionModel.LabelType)));

                LabellingHWindow.HalconWindow.SetColor(color);
                LabellingHWindow.HalconWindow.DispObj(regionModel.Region);

                ImageProcessingHelper.disp_message(LabellingHWindow.HalconWindow, regionModel.LabelType, "image", regionModel.RegionLeftTopRow - 50 < 0 ? 0 : regionModel.RegionLeftTopRow - 50, regionModel.RegionLeftTopColumn, color, "false");
            }

            if (ProjectHelper.Instance.RegionForMove == null || ProjectHelper.Instance.RegionForMove.Region == null || !ProjectHelper.Instance.RegionForMove.Region.IsInitialized())
                return;

            LabellingHWindow.HalconWindow.SetDraw("fill");
            LabellingHWindow.HalconWindow.SetColor($"{RegionColors.GetColor(ProjectHelper.Instance.LabelTypes.FindIndex(x => x.Equals(ProjectHelper.Instance.RegionForMove.LabelType)))}60");
            LabellingHWindow.HalconWindow.DispObj(ProjectHelper.Instance.RegionForMove.Region);
        }

        private void DisplayImage()
        {
            LabellingHWindow.HalconWindow.ClearWindow();

            if (ProjectHelper.Instance.SelectedImage == null || ProjectHelper.Instance.SelectedImage.Image == null || !ProjectHelper.Instance.SelectedImage.Image.IsInitialized())
                return;

            LabellingHWindow.HalconWindow.DispObj(ProjectHelper.Instance.SelectedImage.Image);
        }

        private void DisplaySingleRegion(HRegion region, string roiColor)
        {
            LabellingHWindow.HalconWindow.SetDraw("fill");
            LabellingHWindow.HalconWindow.SetColor(roiColor);

            if (region != null && region.IsInitialized())
                LabellingHWindow.HalconWindow.DispObj(region);
        }

        public HRegion MoveRegion(HRegion region, HSmartWindowControlWPF.HMouseEventArgsWPF e)
        {
            if ((Math.Abs(e.Row - currentPoint.X) < 1) && (Math.Abs(e.Column - currentPoint.Y) < 1))
                return null;

            var newHRegion = region.MoveRegion((int)(e.Row - currentPoint.X), (int)(e.Column - currentPoint.Y));

            currentPoint.X = e.Row;
            currentPoint.Y = e.Column;

            region.Dispose();
            return newHRegion;
        }

        private void GetMinMax(double firstValue, double secondValue, out double minValue, out double maxValue)
        {
            minValue = firstValue > secondValue ? secondValue : firstValue;
            maxValue = firstValue < secondValue ? secondValue : firstValue;
        }

        private RectangleDirection FindRectangleDirection(HSmartWindowControlWPF.HMouseEventArgsWPF e, Point startPoint, Point endPoint)
        {
            if (e.Row < startPoint.X + 10 && e.Column < startPoint.Y + 10)
                return RectangleDirection.LeftTop;

            if (e.Row > endPoint.X - 10 && e.Column > endPoint.Y - 10)
                return RectangleDirection.RightBottom;

            if (e.Row < startPoint.X - 10 && e.Column > endPoint.Y - 10)
                return RectangleDirection.RightTop;

            if (e.Row > endPoint.X + 10 && e.Column < startPoint.Y + 10)
                return RectangleDirection.LeftBottom;

            if (e.Row > startPoint.X + 10 && e.Column < startPoint.Y + 10)
                return RectangleDirection.Left;

            if (e.Row < endPoint.X - 10 && e.Column > endPoint.Y - 10)
                return RectangleDirection.Right;

            if (e.Row > endPoint.X - 10 && e.Column < endPoint.Y - 10)
                return RectangleDirection.Bottom;

            if (e.Row < startPoint.X + 10 && e.Column > startPoint.Y + 10)
                return RectangleDirection.Top;

            return RectangleDirection.RightBottom;
        }

        private void ChangeRectanglePoints(RectangleDirection rectangleDirection, HSmartWindowControlWPF.HMouseEventArgsWPF e, Point rectangleCurrentPoint, Point rectangleStartPoint, Point rectangleStopPoint, out Point newRectangleStartPoint, out Point newRectangleStopPoint)
        {
            newRectangleStartPoint = new Point(rectangleStartPoint.X, rectangleStartPoint.Y);
            newRectangleStopPoint = new Point(rectangleStopPoint.X, rectangleStopPoint.Y);

            switch (rectangleDirection)
            {
                case RectangleDirection.LeftTop:
                    newRectangleStartPoint = new Point(newRectangleStartPoint.X + (e.Row - rectangleCurrentPoint.X), newRectangleStartPoint.Y + (e.Column - rectangleCurrentPoint.Y));
                    break;
                case RectangleDirection.Left:
                    newRectangleStartPoint.Y += e.Column - rectangleCurrentPoint.Y;
                    break;
                case RectangleDirection.Top:
                    newRectangleStartPoint.X += e.Row - rectangleCurrentPoint.X;
                    break;
                case RectangleDirection.RightBottom:
                    newRectangleStopPoint = new Point(newRectangleStopPoint.X + (e.Row - rectangleCurrentPoint.X), newRectangleStopPoint.Y + (e.Column - rectangleCurrentPoint.Y));
                    break;
                case RectangleDirection.Right:
                    newRectangleStopPoint.Y += e.Column - rectangleCurrentPoint.Y;
                    break;
                case RectangleDirection.Bottom:
                    newRectangleStopPoint.X += e.Row - rectangleCurrentPoint.X;
                    break;
                case RectangleDirection.RightTop:
                    newRectangleStartPoint.X += e.Row - rectangleCurrentPoint.X;
                    newRectangleStopPoint.Y += e.Column - rectangleCurrentPoint.Y;
                    break;
                case RectangleDirection.LeftBottom:
                    newRectangleStartPoint.Y += e.Column - rectangleCurrentPoint.Y;
                    newRectangleStopPoint.X += e.Row - rectangleCurrentPoint.X;
                    break;
            }
        }

        private void DeleteRegion()
        {
            if (isDraw)
            {
                MessageBox.Show("Region çizimi devam ediyor. Lütfen çizimi bitiriniz.", "Region Çizim");
                return;
            }

            if (ProjectHelper.Instance.SelectedImage == null || ProjectHelper.Instance.SelectedImage.SelectedRegion == null)
            {
                MessageBox.Show("Lütfen silinecek region'ı seçiniz", "Region Seçilmedi");
                return;
            }

            var selectedRegion = ProjectHelper.Instance.SelectedImage.SelectedRegion;

            selectedRegion.Dispose();

            var regionPath = Path.Combine(Paths.ProjectPath, ProjectHelper.Instance.ProjectName, Paths.LabeledImages, ProjectHelper.Instance.SelectedImage.GUID, Paths.LabelsFolder, selectedRegion.LabelType, selectedRegion.GUID);

            if (Directory.Exists(regionPath))
                Directory.Delete(regionPath, true);

            ProjectHelper.Instance.SelectedImage.RegionModels.Remove(selectedRegion);

            DisplayImageAndRegions();
        }

        private void CreateRegion()
        {
            isDraw = false;
            isMouseDown = false;

            HObject Region = new HObject();

            if (drawType == DrawTypeEnum.FreeHand)
            {
                if (selectedPoints.Count == 0)
                {
                    selectedPoints = new List<Point>();
                    ClearValues();
                    return;
                }

                selectedPoints.Add(new Point(selectedPoints[0].X, selectedPoints[0].Y));

                var rows = new HTuple(selectedPoints.Select(x => x.X).ToArray());
                var cols = new HTuple(selectedPoints.Select(x => x.Y).ToArray());

                HOperatorSet.GenRegionPolygon(out var region, rows, cols);
                HOperatorSet.FillUp(region, out Region);
                region.Dispose();
            }
            else if (drawType == DrawTypeEnum.Rectangle)
            {
                if (rectangleStartPoint.X == 0 && rectangleStartPoint.Y == 0)
                {
                    ClearValues();
                    return;
                }

                GetMinMax(rectangleStartPoint.X, rectangleStopPoint.X, out var minRow, out var maxRow);
                GetMinMax(rectangleStartPoint.Y, rectangleStopPoint.Y, out var minColumn, out var maxColumn);

                HOperatorSet.GenRectangle1(out var region, minRow, minColumn, maxRow, maxColumn);
                HOperatorSet.FillUp(region, out Region);
                region.Dispose();
            }
            else if (drawType == DrawTypeEnum.Cicle)
            {
                if (circleRadius <= 0)
                {
                    ClearValues();
                    return;
                }

                HOperatorSet.GenCircle(out var region, circleCenterPoint.X, circleCenterPoint.Y, circleRadius);
                HOperatorSet.FillUp(region, out Region);
                region.Dispose(); ;
            }

            HOperatorSet.RegionFeatures(Region, "area", out var regionArea);

            if (regionArea <= 0)
                return;

            var labeledRegionModel = new LabeledRegionModel(new HRegion(Region));

            labeledRegionModel.LabelType = (string)LabelTypeComboBox.SelectedItem;

            ProjectHelper.Instance.SelectedImage.RegionModels.Add(labeledRegionModel);

            var regionModelPath = Path.Combine(Paths.ProjectPath, ProjectHelper.Instance.ProjectName, Paths.LabeledImages, ProjectHelper.Instance.SelectedImage.GUID,
                                                Paths.LabelsFolder, labeledRegionModel.LabelType, labeledRegionModel.GUID);

            if (!Directory.Exists(regionModelPath))
                Directory.CreateDirectory(regionModelPath);

            HOperatorSet.WriteObject(labeledRegionModel.Region, Path.Combine(regionModelPath, Paths.Region));

            DisplayImageAndRegions();
            Region.Dispose();

            ClearValues();
        }

        private void ClearValues()
        {
            oldPoint = new Point(-1, -1);
            rectangleStartPoint = new Point(-1, -1);
            circleRadius = 0;
            circleCenterPoint = new Point(-1, -1);
            selectedPoints = new List<Point>();
            undoStack = new List<Point>();
            isRectangleChange = false;
            rectangleStopPoint = new Point();
        }

        private void UndoFreeDrawing()
        {
            if (!isDraw || (drawType != DrawTypeEnum.FreeHand))
                return;

            if (selectedPoints.Count == 0)
                return;

            var numberElements = selectedPoints.Count;

            undoStack.Add(new Point(selectedPoints[numberElements - 1].X, selectedPoints[numberElements - 1].Y));
            selectedPoints.RemoveAt(numberElements - 1);

            DisplayImageAndRegions();

            if (selectedPoints.Count == 0)
            {
                oldPoint = new Point(-1, -1);
                return;
            }

            var rows = new HTuple(selectedPoints.Select(x => x.X).ToArray());
            var cols = new HTuple(selectedPoints.Select(x => x.Y).ToArray());

            HOperatorSet.GenRegionPolygon(out var region, rows, cols);
            LabellingHWindow.HalconWindow.DispObj(region);
            region.Dispose();

            oldPoint.X = selectedPoints[selectedPoints.Count - 1].X;
            oldPoint.Y = selectedPoints[selectedPoints.Count - 1].Y;
        }

        private void FastForwardFreeDrawing()
        {
            if (!isDraw || (drawType != DrawTypeEnum.FreeHand))
                return;

            if (undoStack.Count == 0)
                return;

            var lastUndo = undoStack[undoStack.Count - 1];
            undoStack.RemoveAt(undoStack.Count - 1);

            selectedPoints.Add(lastUndo);

            DisplayImageAndRegions();

            var rows = new HTuple(selectedPoints.Select(x => x.X).ToArray());
            var cols = new HTuple(selectedPoints.Select(x => x.Y).ToArray());

            HOperatorSet.GenRegionPolygon(out var region, rows, cols);
            LabellingHWindow.HalconWindow.DispObj(region);
            region.Dispose();

            oldPoint.X = selectedPoints[selectedPoints.Count - 1].X;
            oldPoint.Y = selectedPoints[selectedPoints.Count - 1].Y;
        }

        private TextBlock CreateTextBlock(string text)
        {
            var textBlock = new TextBlock();

            textBlock.Text = text;
            textBlock.FontFamily = new FontFamily("Comic Sans MS");
            textBlock.FontSize = 11;
            textBlock.FontWeight = FontWeights.DemiBold;

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = new MenuItem() { Header = "Sil" };
            menuItem.Click += MenuItem_Click;
            contextMenu.Items.Add(menuItem);
            textBlock.ContextMenu = contextMenu;

            return textBlock;
        }

        #endregion
    }
}
