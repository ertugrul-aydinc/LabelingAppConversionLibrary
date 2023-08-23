using CustomizedLabelingLibrary.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomizedLabelingLibrary.Models
{
    public class LabellingProject
    {
        public string ProjectName = "";
        public string ProjectPath = "";

        public List<ImageModel> ImageModels = new List<ImageModel>();
        public ImageModel SelectedImage = null;
        public LabeledRegionModel RegionForMove = null;

        public List<string> LabelTypes = new List<string>();
        public DrawTypeEnum DrawType = DrawTypeEnum.FreeHand;

        public void ClearAll()
        {
            foreach (var imageModel in ImageModels)
                imageModel.Dispose();

            ImageModels = new List<ImageModel>();

            LabelTypes = new List<string>();
            ProjectName = "";
        }

    }
}
