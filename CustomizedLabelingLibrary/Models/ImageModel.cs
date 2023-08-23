using HalconDotNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CustomizedLabelingLibrary.Models
{
    public class ImageModel
    {
        public string ImagePath { get; set; } = "";
        public string GUID { get; set; } = Guid.NewGuid().ToString();

        public HImage Image { get; set; } = new HImage();

        public LabeledRegionModel SelectedRegion = null;
        public List<LabeledRegionModel> RegionModels = new List<LabeledRegionModel>();

        public Mutex ImageModelMutex = new Mutex();

        public ImageModel(string Path, string GUID)
        {
            this.ImagePath = Path;
            this.GUID = GUID;
        }

        public ImageModel() { }

        public void Dispose()
        {
            if (Image != null && Image.IsInitialized())
                Image.Dispose();

            foreach (var regionModel in RegionModels)
                regionModel.Dispose();

            RegionModels = new List<LabeledRegionModel>();
        }

        public void FindClickedRegion(double row, double column)
        {
            SelectedRegion = RegionModels.First(x => x.Region.TestRegionPoint(row, column) == 1);
        }

        public HImage GetImage(string path, string imageName)
        {
            if (Image != null && Image.IsInitialized())
                return Image;

            HObject image = new HObject();

            var imageFolderPath = Path.Combine(path, GUID);

            if (File.Exists(Path.Combine(imageFolderPath, imageName)))
                HOperatorSet.ReadImage(out image, Path.Combine(imageFolderPath, imageName));
            else
            {
                if (!Directory.Exists(imageFolderPath))
                    Directory.CreateDirectory(imageFolderPath);

                if (File.Exists(ImagePath))
                {
                    HOperatorSet.ReadImage(out image, ImagePath);
                    HOperatorSet.WriteImage(image, "tiff", 0, Path.Combine(imageFolderPath, imageName));
                }
            }

            if (image == null || !image.IsInitialized())
                return null;

            Image = new HImage(image);
            image.Dispose();

            return Image;
        }

        public List<LabeledRegionModel> GetRegion(string path, string labelFolderName)
        {
            if (RegionModels.Count > 0)
                return RegionModels;

            var regionModelPath = Path.Combine(path, GUID, labelFolderName);

            if (!Directory.Exists(regionModelPath))
                return RegionModels;

            var labelFolders = Directory.GetDirectories(regionModelPath);

            if (labelFolders.Count() == 0)
                return RegionModels;

            foreach (var labelFolder in labelFolders)
            {
                var regionFolderPaths = Directory.GetDirectories(labelFolder);

                foreach (var regionFolderPath in regionFolderPaths)
                {
                    if (!File.Exists(Path.Combine(regionFolderPath, "Region.hobj")))
                        continue;

                    HOperatorSet.ReadObject(out var region, Path.Combine(regionFolderPath, "Region.hobj"));

                    RegionModels.Add(new LabeledRegionModel(new HRegion(region), Path.GetFileName(labelFolder), Path.GetFileName(regionFolderPath)));
                }
            }

            return RegionModels;
        }
    }
}
