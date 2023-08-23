using CustomizedLabelingLibrary.Helpers;
using CustomizedLabelingLibrary.Models;
using HalconDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomizedLabelingLibrary.Utils
{
    public static class HelperProcedure
    {
        public static void DisplayImageRegion(HWindow hWindow, string path, string imageName, string labelFolderName)
        {
            hWindow.ClearWindow();

            if (ProjectHelper.Instance.SelectedImage == null)
                return;

            var image = ProjectHelper.Instance.SelectedImage.GetImage(path, imageName);

            if (image == null || !image.IsInitialized())
                return;

            hWindow.DispObj(image);
            hWindow.SetPart(0, 0, -2, -2);

            var regionModels = ProjectHelper.Instance.SelectedImage.GetRegion(path, labelFolderName);

            hWindow.SetDraw("margin");

            foreach (var regionModel in regionModels)
            {
                if (regionModel.Region == null || !regionModel.Region.IsInitialized())
                    continue;

                hWindow.SetColor(RegionColors.GetColor(ProjectHelper.Instance.LabelTypes.FindIndex(x => x.Equals(regionModel.LabelType))));
                hWindow.DispObj(regionModel.Region);
            }
        }
    }
}
