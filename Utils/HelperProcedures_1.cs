using HalconDotNet;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Visiomex.Projects.LabelingTool.Models;

namespace Visiomex.Projects.LabelingTool.Utils
{
    public static class HelperProcedures
    {
        public static TextBlock CreateTextBlock(string text)
        {
            var textBlock = new TextBlock();

            textBlock.Text = text;
            textBlock.FontFamily = new FontFamily("Comic Sans MS");
            textBlock.FontSize = 11;
            textBlock.FontWeight = FontWeights.DemiBold;

            return textBlock;
        }
        public static void DisplayImageRegion(HWindow hWindow)
        {
            hWindow.ClearWindow();

            if (ProjectHelper.Instance.SelectedImage == null)
                return;

            var image = ProjectHelper.Instance.SelectedImage.GetImage();

            if (image == null || !image.IsInitialized())
                return;

            hWindow.DispObj(image);
            hWindow.SetPart(0, 0, -2, -2);

            var regionModels = ProjectHelper.Instance.SelectedImage.GetRegion();

            hWindow.SetDraw("margin");

            foreach (var regionModel in regionModels)
            {
                if (regionModel.Region == null || !regionModel.Region.IsInitialized())
                    continue;

                hWindow.SetColor(RegionColors.GetColor(ProjectHelper.Instance.LabelTypes.FindIndex(x => x.Equals(regionModel.LabelType))));
                hWindow.DispObj(regionModel.Region);
            }
        }

        public static string RemoveDiacritics(string text)
        {
            Encoding srcEncoding = Encoding.UTF8;
            Encoding destEncoding = Encoding.GetEncoding(1252); // Latin alphabet

            text = destEncoding.GetString(Encoding.Convert(srcEncoding, destEncoding, srcEncoding.GetBytes(text)));

            string normalizedString = text.Normalize(NormalizationForm.FormD);
            StringBuilder result = new StringBuilder();

            for (int i = 0; i < normalizedString.Length; i++)
            {
                if (!CharUnicodeInfo.GetUnicodeCategory(normalizedString[i]).Equals(UnicodeCategory.NonSpacingMark))
                {
                    result.Append(normalizedString[i]);
                }
            }

            return result.ToString();
        }

        public static void DeleteModel(string projectName)
        {
            var modelPath = Path.Combine(Paths.ProjectPath, projectName);

            if (Directory.Exists(modelPath))
                Directory.Delete(modelPath, true);
        }

    }
}
