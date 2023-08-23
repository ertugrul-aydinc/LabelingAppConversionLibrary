using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using CustomizedLabelingLibrary.Helpers;
using CustomizedLabelingLibrary.Models;
using CustomizedLabelingLibrary.Models.Enums;
using CustomizedLabelingLibrary.Utils;
using HalconDotNet;

namespace CustomizedLabelingLibrary
{
    public class DrawShapeTransactions
    {
        public static List<ImageModel> ImageModels { get; set; } = new List<ImageModel>();

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

        private bool isEllipseMove { get; set; } = false;

        private bool isCtrlDown { get; set; } = false;


        private Point oldPoint = new Point(-1, -1);
        private Point currentPoint = new Point();

        private Point rectangleStartPoint = new Point(-1, -1);
        private Point rectangleStopPoint = new Point();
        private Point rectangleCurrentPoint = new Point();

        private Point circleCenterPoint = new Point(-1, -1);
        private double circleRadius = 0;
        private Point circleCurrentPoint = new Point();

        //TODO
        private Point ellipseCenterPoint = new Point(-1, -1);
        private double ellipseRadius = 0;
        private Point ellipseCurrentPoint = new Point();

        private List<Point> selectedPoints = new List<Point>();
        private List<Point> undoStack = new List<Point>();

        private HTuple imageWidthSize;
        private HTuple imageHeightSize;


        #endregion


        public static HSmartWindowControlWPF HSmartWindowControlWPF { get; set; }

        public static DrawShapeTransactions Instance { get; set; } = new DrawShapeTransactions();



        #region Ok
        public static void DisplayImage(HSmartWindowControlWPF hSmartWindowControlWPF, HImage image, object obj)
        {
            hSmartWindowControlWPF.HalconWindow.ClearWindow();

            if (obj == null || image == null || !image.IsInitialized())
                return;

            hSmartWindowControlWPF.HalconWindow.DispObj(image);
        }

        public static void DisplayImage(HSmartWindowControlWPF hSmartWindowControlWPF, HImage image)
        {
            hSmartWindowControlWPF.HalconWindow.ClearWindow();

            if (image == null || !image.IsInitialized())
                return;

            hSmartWindowControlWPF.HalconWindow.DispObj(image);
        }

        public static void DisplaySingleRegion(HSmartWindowControlWPF hSmartWindowControlWPF, HRegion region, string roiColor)
        {
            hSmartWindowControlWPF.HalconWindow.SetDraw("fill");
            hSmartWindowControlWPF.HalconWindow.SetColor(roiColor);

            if (region != null && region.IsInitialized())
                hSmartWindowControlWPF.HalconWindow.DispObj(region);
        }

        public void DeleteRegion(ImageModel imageModel, string path)
        {
            if (isDraw)
            {
                MessageBox.Show("Region çizimi devam ediyor. Lütfen çizimi bitiriniz.", "Region Çizim");
                return;
            }

            if (imageModel == null || imageModel.SelectedRegion == null)
            {
                MessageBox.Show("Lütfen silinecek region'ı seçiniz", "Region Seçilmedi");
                return;
            }

            var selectedRegion = imageModel.SelectedRegion;

            selectedRegion.Dispose();

            //var regionPath = Path.Combine(Paths.ProjectPath, ProjectHelper.Instance.ProjectName, Paths.LabeledImages, ProjectHelper.Instance.SelectedImage.GUID, Paths.LabelsFolder, selectedRegion.LabelType, selectedRegion.GUID);

            if (Directory.Exists(path))
                Directory.Delete(path, true);

            imageModel.RegionModels.Remove(selectedRegion);

            //DisplayImageAndRegions();
        }
        public HRegion MoveRegion(HRegion region, double rowValue, double columnValue)
        {
            if ((Math.Abs(rowValue - currentPoint.X) < 1) && (Math.Abs(columnValue - currentPoint.Y) < 1))
                return null;

            var newHRegion = region.MoveRegion((int)(rowValue - currentPoint.X), (int)(columnValue - currentPoint.Y));

            currentPoint.X = rowValue;
            currentPoint.Y = columnValue;

            region.Dispose();
            return newHRegion;
        }
        private void GetMinMax(double firstValue, double secondValue, out double minValue, out double maxValue)
        {
            minValue = firstValue > secondValue ? secondValue : firstValue;
            maxValue = firstValue < secondValue ? secondValue : firstValue;
        }


        public static RectangleDirection FindRectangleDirection(double rowValue, double columnValue, Point startPoint, Point endPoint)
        {
            if (rowValue < startPoint.X + 10 && columnValue < startPoint.Y + 10)
                return RectangleDirection.LeftTop;

            if (rowValue > endPoint.X - 10 && columnValue > endPoint.Y - 10)
                return RectangleDirection.RightBottom;

            if (rowValue < startPoint.X - 10 && columnValue > endPoint.Y - 10)
                return RectangleDirection.RightTop;

            if (rowValue > endPoint.X + 10 && columnValue < startPoint.Y + 10)
                return RectangleDirection.LeftBottom;

            if (rowValue > startPoint.X + 10 && columnValue < startPoint.Y + 10)
                return RectangleDirection.Left;

            if (rowValue < endPoint.X - 10 && columnValue > endPoint.Y - 10)
                return RectangleDirection.Right;

            if (rowValue > endPoint.X - 10 && columnValue < endPoint.Y - 10)
                return RectangleDirection.Bottom;

            if (rowValue < startPoint.X + 10 && columnValue > startPoint.Y + 10)
                return RectangleDirection.Top;

            return RectangleDirection.RightBottom;
        }

        public static void ChangeRectanglePoints(RectangleDirection rectangleDirection, HSmartWindowControlWPF.HMouseEventArgsWPF e, Point rectangleCurrentPoint, Point rectangleStartPoint, Point rectangleStopPoint, out Point newRectangleStartPoint, out Point newRectangleStopPoint)
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


        #endregion



        public void DisplayRegions(HSmartWindowControlWPF hSmartWindowControlWPF, List<LabeledRegionModel> labeledRegionModels, List<string> labelTypes)
        {
            foreach (var regionModel in labeledRegionModels)
            {
                if (regionModel.Region == null || !regionModel.Region.IsInitialized())
                    continue;

                var color = RegionColors.GetColor(labelTypes.FindIndex(x => x.Equals(regionModel.LabelType)));

                hSmartWindowControlWPF.HalconWindow.SetColor(color);
                hSmartWindowControlWPF.HalconWindow.DispObj(regionModel.Region);

                ImageProcessingHelper.disp_message(hSmartWindowControlWPF.HalconWindow, regionModel.LabelType, "image", regionModel.RegionLeftTopRow - 50 < 0 ? 0 : regionModel.RegionLeftTopRow - 50, regionModel.RegionLeftTopColumn, color, "false");

            }

            if (ProjectHelper.Instance.RegionForMove == null || ProjectHelper.Instance.RegionForMove.Region == null || !ProjectHelper.Instance.RegionForMove.Region.IsInitialized())
                return;

            hSmartWindowControlWPF.HalconWindow.SetDraw("fill");
            hSmartWindowControlWPF.HalconWindow.SetColor($"{RegionColors.GetColor(ProjectHelper.Instance.LabelTypes.FindIndex(x => x.Equals(ProjectHelper.Instance.RegionForMove.LabelType)))}60");
            hSmartWindowControlWPF.HalconWindow.DispObj(ProjectHelper.Instance.RegionForMove.Region);
        }

        public List<Point> DrawLine(HSmartWindowControlWPF hSmartWindowControlWPF, Point currentPoint, HTuple imageHeightSize, HTuple imageWidthSize)
        {


            if (currentPoint.X < 0 || currentPoint.Y < 0 || currentPoint.X >= imageHeightSize - 1 || currentPoint.Y >= imageWidthSize - 1)
                return null;

            if (oldPoint.X == -1 && oldPoint.Y == -1)
            {
                oldPoint.X = currentPoint.X;
                oldPoint.Y = currentPoint.Y;
            }

            HOperatorSet.GenRegionLine(out var regionLines, oldPoint.X, oldPoint.Y, currentPoint.X, currentPoint.Y);
            hSmartWindowControlWPF.HalconWindow.DispObj(regionLines);

            oldPoint.X = currentPoint.X;
            oldPoint.Y = currentPoint.Y;

            selectedPoints.Add(new Point(currentPoint.X, currentPoint.Y));

            return selectedPoints;
        }



        private void ClearValues()
        {
            oldPoint = new Point(-1, -1);
            rectangleStartPoint = new Point(-1, -1);
            circleRadius = 0;
            circleCenterPoint = new Point(-1, -1);
            ellipseRadius = 0;
            ellipseCenterPoint = new Point(-1, -1);
            selectedPoints = new List<Point>();
            undoStack = new List<Point>();
            isRectangleChange = false;
            rectangleStopPoint = new Point();
        }

        #region InProgress
        public void CreateAndSaveRegion(DrawTypeEnum labelType, string modelPath)
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
                GetMinMax(rectangleStopPoint.Y, rectangleStopPoint.Y, out var minColumn, out var maxColumn);

                HOperatorSet.GenRectangle1(out var region, minRow, minColumn, maxRow, maxColumn);
                HOperatorSet.FillUp(region, out Region);
                region.Dispose();
            }
            else if (drawType == DrawTypeEnum.Circle)
            {
                if (circleRadius <= 0)
                {
                    ClearValues();
                    return;
                }

                HOperatorSet.GenCircle(out var region, circleCenterPoint.X, circleCenterPoint.Y, circleRadius);
                HOperatorSet.FillUp(region, out Region);
                region.Dispose();
            }


            HOperatorSet.RegionFeatures(Region, "area", out var regionArea);

            if (regionArea <= 0)
                return;

            var labeledRegionModel = new LabeledRegionModel(new HRegion(Region));
            labeledRegionModel.LabelType = labelType.ToString();

            ProjectHelper.Instance.SelectedImage.RegionModels.Add(labeledRegionModel);

            if (!Directory.Exists(modelPath))
                Directory.CreateDirectory(modelPath);

            HOperatorSet.WriteObject(labeledRegionModel.Region, System.IO.Path.Combine(modelPath, drawType.ToString(), labeledRegionModel.GUID, "Region.hobj"));
            Region.Dispose();
            ClearValues();

        }

        #endregion





        public void HWindow_HMouseDown(HSmartWindowControlWPF.HMouseEventArgsWPF e, HSmartWindowControlWPF hSmartWindowControlWPF, string modelPath)
        {

            currentPoint.X = e.Row;
            currentPoint.Y = e.Column;

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

                    CreateAndSaveRegion(drawType, modelPath);

                    hSmartWindowControlWPF.HMoveContent = true;
                }
                return;
            }
            isMouseDown = true;

            if (!isDraw && ProjectHelper.Instance.SelectedImage != null && e.Button == MouseButton.Left)
            {
                ProjectHelper.Instance.RegionForMove = ProjectHelper.Instance.SelectedImage.RegionModels.Find(x => x.Region.TestRegionPoint(e.Row, e.Column) == 1);

                if (ProjectHelper.Instance.RegionForMove != null)
                    hSmartWindowControlWPF.HMoveContent = false;

                DisplayImageAndRegions(hSmartWindowControlWPF);
            }

            isDraw = true;
            if (!isDraw)
                return;
            if (drawType == DrawTypeEnum.FreeHand)
            {
                selectedPoints = DrawLine(hSmartWindowControlWPF, currentPoint, imageHeightSize, imageWidthSize);

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
                    rectangleDirection = FindRectangleDirection(e.Row, e.Column, rectangleStartPoint, rectangleStopPoint);
                    rectangleCurrentPoint = new Point(e.Row, e.Column);
                }
            }
            else if (drawType == DrawTypeEnum.Circle)
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
            else if (drawType == DrawTypeEnum.Ellipse)
            {
                if (ellipseCenterPoint.X == -1 || ellipseCenterPoint.Y == -1)
                {
                    ellipseCenterPoint.X = e.Row;
                    ellipseCenterPoint.Y = e.Column;

                    ellipseRadius = 0;
                    ellipseCurrentPoint = new Point(e.Row, e.Column);

                    isEllipseMove = false;

                    return;
                }
                if (Math.Sqrt(Math.Pow(e.Row - ellipseCenterPoint.X, 2) + Math.Pow(e.Column - ellipseCenterPoint.Y, 2)) < circleRadius)
                {
                    isEllipseMove = true;
                    ellipseCenterPoint = new Point(e.Row, e.Column);
                }
                else
                {
                    isEllipseMove = false;
                    ellipseCurrentPoint = new Point(e.Row, e.Column);
                }

            }

        }

        public void HWindow_HMouseMove(object sender, HSmartWindowControlWPF.HMouseEventArgsWPF e, HSmartWindowControlWPF hSmartWindowControlWPF, LabeledRegionModel labeledRegionModel, DrawTypeEnum drawType)
        {
            if (!isDraw && isMouseDown)
            {
                if (labeledRegionModel != null && labeledRegionModel.Region != null && labeledRegionModel.Region.IsInitialized())
                    labeledRegionModel.ChangeRegion(MoveRegion(labeledRegionModel.Region, e.Row, e.Column));

                DisplayImageAndRegions(hSmartWindowControlWPF);

                return;
            }

            if (!isDraw || !isMouseDown)
                return;

            if (drawType == DrawTypeEnum.FreeHand)
            {
                currentPoint.X = e.Row;
                currentPoint.Y = e.Column;

                DrawLine(hSmartWindowControlWPF, currentPoint, imageHeightSize, imageWidthSize);
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

                DisplayImageAndRegions(hSmartWindowControlWPF);

                GetMinMax(rectangleStartPoint.X, rectangleStopPoint.X, out var minRow, out var maxRow);
                GetMinMax(rectangleStartPoint.Y, rectangleStopPoint.Y, out var minColumn, out var maxColumn);

                try
                {
                    HOperatorSet.GenRectangle1(out var rectangle, minRow, minColumn, maxRow, maxColumn);
                    hSmartWindowControlWPF.HalconWindow.DispObj(rectangle);
                    rectangle.Dispose();
                }
                catch { }
            }
            else if (drawType == DrawTypeEnum.Circle)
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

                DisplayImageAndRegions(hSmartWindowControlWPF);

                try
                {
                    HOperatorSet.GenCircle(out var circle, circleCenterPoint.X, circleCenterPoint.Y, circleRadius);
                    hSmartWindowControlWPF.HalconWindow.DispObj(circle);
                    circle.Dispose();
                }
                catch (Exception)
                { }
            }
            else if (drawType == DrawTypeEnum.Ellipse)
            {
                if (e.Row < 0 || e.Column < 0 || e.Row >= imageHeightSize - 1 || e.Column >= imageWidthSize - 1)
                    return;

                if (isEllipseMove)
                {
                    var differenceRow = e.Row - ellipseCurrentPoint.X;
                    var differenceColumn = e.Column - ellipseCurrentPoint.Y;

                    ellipseCenterPoint.X += differenceRow;
                    ellipseCenterPoint.Y += differenceColumn;

                    ellipseCurrentPoint = new Point(e.Row, e.Column);
                }
                else
                {
                    var oldDistance = Math.Sqrt(Math.Pow(ellipseCenterPoint.X - ellipseCurrentPoint.X, 2) + Math.Pow(ellipseCenterPoint.Y - ellipseCurrentPoint.Y, 2));
                    var newDistance = Math.Sqrt(Math.Pow((ellipseCenterPoint.X - e.Row), 2) + Math.Pow((ellipseCenterPoint.Y - e.Column), 2));

                    ellipseRadius += newDistance - oldDistance;

                    if (ellipseRadius < 0)
                        ellipseRadius = 0;

                    ellipseCurrentPoint = new Point(e.Row, e.Column);
                }

                DisplayImageAndRegions(hSmartWindowControlWPF);

                try
                {
                    HOperatorSet.GenCircle(out var ellipse, ellipseCenterPoint.X, ellipseCenterPoint.Y, ellipseRadius);
                    hSmartWindowControlWPF.HalconWindow.DispObj(ellipse);
                    ellipse.Dispose();
                }
                catch (Exception)
                { }
            }
        }
        //public void HWindow_HMouseUp(object sender, HSmartWindowControlWPF.HMouseEventArgsWPF e, HSmartWindowControlWPF hSmartWindowControlWPF)
        //{
        //    isMouseDown = false;

        //    if (!isDraw)
        //        hSmartWindowControlWPF.HMoveContent = true;

        //    if (ProjectHelper.Instance.SelectedImage != null && ProjectHelper.Instance.SelectedImage.Image != null && ProjectHelper.Instance.SelectedImage.Image.IsInitialized())
        //        ProjectHelper.Instance.SelectedImage.SelectedRegion = ProjectHelper.Instance.SelectedImage.RegionModels.Find(x => x.Region.TestRegionPoint(e.Row, e.Column) == 1);

        //    if (ProjectHelper.Instance.RegionForMove != null)
        //    {
        //        var regionPath = Path.Combine(Paths.ProjectPath, ProjectHelper.Instance.ProjectName, Paths.LabeledImages, ProjectHelper.Instance.SelectedImage.GUID, Paths.LabelsFolder, ProjectHelper.Instance.RegionForMove.LabelType, ProjectHelper.Instance.RegionForMove.GUID);

        //        if (File.Exists(Path.Combine(regionPath, Paths.Region)))
        //            File.Delete(Path.Combine(regionPath, Paths.Region));

        //        HOperatorSet.WriteObject(ProjectHelper.Instance.RegionForMove.Region, Path.Combine(regionPath, Paths.Region));

        //        ProjectHelper.Instance.RegionForMove = null;

        //        DisplayImageAndRegions();

        //        if (ProjectHelper.Instance.SelectedImage.SelectedRegion != null)
        //            DisplaySingleRegion(ProjectHelper.Instance.SelectedImage.SelectedRegion.Region, $"{RegionColors.GetColor(ProjectHelper.Instance.LabelTypes.FindIndex(x => x.Equals(ProjectHelper.Instance.SelectedImage.SelectedRegion.LabelType)))}60");
        //    }

        //    if (!isDraw)
        //        return;

        //    if (drawType == DrawTypeEnum.Rectangle)
        //    {
        //        GetMinMax(rectangleStartPoint.X, rectangleStopPoint.X, out var minRow, out var maxRow);
        //        GetMinMax(rectangleStartPoint.Y, rectangleStopPoint.Y, out var minColumn, out var maxColumn);

        //        rectangleStartPoint = new Point(minRow, minColumn);
        //        rectangleStopPoint = new Point(maxRow, maxColumn);
        //    }

        //    return;
        //}



        private void DisplayImageAndRegions(HSmartWindowControlWPF hSmartWindowControlWPF)
        {
            DisplayImage(hSmartWindowControlWPF, ProjectHelper.Instance.SelectedImage.Image, ProjectHelper.Instance.SelectedImage);
            //DisplayRegions(hSmartWindowControlWPF, ProjectHelper.Instance.SelectedImage.RegionModels, ProjectHelper.Instance.LabelTypes, ProjectHelper.Instance.RegionForMove);
        }

        public void SetSelectedImageForListView(SelectionChangedEventArgs e, ListView listView, HWindow hWindow, string path, string imageGuid)
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
                listView.SelectedItem = e.RemovedItems[0];

                return;
            }


            ProjectHelper.Instance.SelectedImage = ImageModels.FirstOrDefault(x => x.GUID == imageGuid);

            if (ProjectHelper.Instance.SelectedImage == null)
                return;

            HelperProcedure.DisplayImageRegion(hWindow, path, "Image.tif", "ETIKETLER");
            DisplayRegions(HSmartWindowControlWPF, ProjectHelper.Instance.SelectedImage.RegionModels, ProjectHelper.Instance.LabelTypes);

            if (ProjectHelper.Instance.SelectedImage.Image != null && ProjectHelper.Instance.SelectedImage.Image.IsInitialized())
                ProjectHelper.Instance.SelectedImage.Image.GetImageSize(out imageWidthSize, out imageHeightSize);
        }

        //public void LoadImageModels(object sender, RoutedEventArgs e, string projectPath, Window window)
        //{
        //    var projects = Directory.GetDirectories(projectPath);

        //    if (!projects.Any())
        //    {
        //        MessageBox.Show("Hiç kayıtlı model bulunamadı.", "Model Bulunamadı");
        //        return;
        //    }

        //    if (isDraw)
        //    {
        //        MessageBox.Show("Çizim devam ediyor, lütfen bitiriniz.", "Model Yüklenemedi");
        //        return;
        //    }

        //    projects = projects.Select(x => System.IO.Path.GetFileName(x)).ToArray();

        //    Window loadProjectWindow = new window(projects);
        //    loadProjectWindow.ShowDialog();

        //    if (loadProjectWindow.DeleteModel)
        //    {
        //        HelperProcedures.DeleteModel(loadProjectWindow.SelectedProjectForDelete);

        //        if (loadProjectWindow.SelectedProjectForDelete == ProjectHelper.Instance.ProjectName)
        //            ProjectHelper.Instance.ClearAll();

        //        MessageBox.Show("Proje başarılı bir şekilde silindi.", "Proje Sil");

        //        return;
        //    }

        //    if (string.IsNullOrEmpty(loadProjectWindow.SelectedProject))
        //    {
        //        MessageBox.Show("Lütfen model seçimi yapınız.", "Model Seçilmedi");
        //        return;
        //    }

        //    ClearProject();

        //    ProjectHelper.Instance.ProjectName = loadProjectWindow.SelectedProject;
        //    ProjectHelper.Instance.ProjectPath = Paths.ProjectPath;

        //    if (File.Exists(Path.Combine(Paths.ProjectPath, ProjectHelper.Instance.ProjectName, Paths.LabelsFolder, Paths.LabelsFile)))
        //    {
        //        var labelTypesText = File.ReadAllText(Path.Combine(Paths.ProjectPath, ProjectHelper.Instance.ProjectName, Paths.LabelsFolder, Paths.LabelsFile));

        //        ProjectHelper.Instance.LabelTypes = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(labelTypesText);

        //        foreach (var labelType in ProjectHelper.Instance.LabelTypes)
        //            LabelTypeComboBox.Items.Add(labelType);

        //        if (LabelTypeComboBox.Items.Count > 0)
        //            LabelTypeComboBox.SelectedIndex = 0;
        //    }



        //    if (Directory.Exists(Path.Combine(Paths.ProjectPath, ProjectHelper.Instance.ProjectName, Paths.LabeledImages)))
        //    {
        //        string xx = Path.Combine(Paths.ProjectPath, ProjectHelper.Instance.ProjectName, Paths.LabeledImages);
        //        var labeledImageFolders = Directory.GetDirectories(Path.Combine(Paths.ProjectPath, ProjectHelper.Instance.ProjectName, Paths.LabeledImages));

        //        foreach (var labeledImageFolder in labeledImageFolders)
        //        {
        //            var imagePath = "";

        //            var deneme = Path.Combine(labeledImageFolder, Paths.ImagePath);
        //            if (File.Exists(Path.Combine(labeledImageFolder, Paths.ImagePath)))
        //                imagePath = File.ReadAllText(Path.Combine(labeledImageFolder, Paths.ImagePath));

        //            ProjectHelper.Instance.ImageModels.Add(new ImageModel(imagePath, Path.GetFileName(labeledImageFolder)));
        //        }

        //        foreach (var imageModel in ProjectHelper.Instance.ImageModels)
        //            AllImagesListView.Items.Add(CreateTextBlock(imageModel.GUID));
        //    }
        //}

    }


}
