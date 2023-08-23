using Basler.Pylon;
using HalconDotNet;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Visiomex.Projects.LabelingTool.Utils
{
    public class CameraHelper
    {
        public static CameraHelper Instance { get; } = new CameraHelper();

        public Bitmap TakeOneFrame()
        {
            IGrabResult grabResult;

            using (Camera camera = new Camera())
            {
                camera.Open();

                grabResult = camera.StreamGrabber.GrabOne(5000, TimeoutHandling.ThrowException);
            }

            return GrabResultToHImage.GrabResultToBitmap(grabResult, System.Drawing.Imaging.PixelFormat.Format24bppRgb, PixelType.BGR8packed);
        }
    }
}
