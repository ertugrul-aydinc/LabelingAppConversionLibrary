using Basler.Pylon;
using HalconDotNet;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Visiomex.Projects.LabelingTool.Utils
{
    public static class GrabResultToHImage
    {
        public static HImage ConvertMono(IGrabResult grabResult)
        {
            var bitmap = GrabResultToBitmap(grabResult, PixelFormat.Format8bppIndexed, PixelType.Mono8);
            return BitmapToHImage(bitmap, "byte");
        }

        public static Bitmap GrabResultToBitmap(IGrabResult grabResult, PixelFormat pixelFormat, PixelType pixelType)
        {
            Bitmap bitmap = new Bitmap(grabResult.Width, grabResult.Height, pixelFormat);
            BitmapData bmpData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, pixelFormat);
            IntPtr ptrBmp = bmpData.Scan0;
            PixelDataConverter Converter = new PixelDataConverter();
            Converter.OutputPixelFormat = pixelType;
            Converter.Convert(ptrBmp, bmpData.Stride * bitmap.Height, grabResult);
            bitmap.UnlockBits(bmpData);

            return bitmap;
        }

        public static HImage BitmapToHImage(Bitmap bitmap, HTuple type)
        {
            try
            {
                BitmapData bd = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);
                IntPtr pval = bd.Scan0;
                HObject image = null;

                image = new HImage(type, bitmap.Width, bitmap.Height, pval);

                bitmap.UnlockBits(bd);
                bitmap.Dispose();

                HImage hImage = new HImage(image);

                image.Dispose();

                return hImage;
            }
            catch (Exception)
            {
                //Logger.SubmitLog(LogLevel.Error, ex, $@"ImageConversionUtils_{MethodBase.GetCurrentMethod().Name}");
                return null;
            }
        }

        public static HImage BitmapToHImage(Bitmap bitmap, HTuple type, HTuple colorType)
        {
            try
            {
                BitmapData bd = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                IntPtr pval = bd.Scan0;
                HObject image = null;

                HOperatorSet.GenImageInterleaved(out image, pval, colorType, bitmap.Width, bitmap.Height, 0, type, bitmap.Width, bitmap.Height, 0, 0, -1, 0);

                bitmap.UnlockBits(bd);
                bitmap.Dispose();

                HImage hImage = new HImage(image);

                image.Dispose();

                return hImage;
            }
            catch (Exception)
            {
                //Logger.SubmitLog(LogLevel.Error, ex, $@"ImageConversionUtils_{MethodBase.GetCurrentMethod().Name}");
                return null;
            }
        }

        public static byte[] BayerRG8ToRGB(byte[] byteArray, int width, int height)
        {
            var convertedByteArray = new byte[width * height * 3];

            Parallel.For(0, height / 2,
                   j =>
                   {
                       var redIndex = (j * 2) * width - 2;
                       var greenIndexFirst = redIndex + 1;
                       var greenIndexSecond = redIndex + width;
                       var blueIndex = greenIndexSecond + 1;

                       var threeR = redIndex * 3;
                       var threeG1 = greenIndexFirst * 3;
                       var threeG2 = greenIndexSecond * 3;
                       var threeB = blueIndex * 3;

                       for (int i = 0; i < width / 2; i++)
                       {
                           redIndex += 2;
                           greenIndexFirst += 2;
                           greenIndexSecond += 2;
                           blueIndex += 2;

                           threeR += 6;
                           threeG1 += 6;
                           threeG2 += 6;
                           threeB += 6;

                           var (red, green, blue) = (byteArray[redIndex], (byte)((byteArray[greenIndexFirst] + byteArray[greenIndexSecond]) / 2), byteArray[blueIndex]);

                           (convertedByteArray[threeR], convertedByteArray[threeR + 1], convertedByteArray[threeR + 2]) = (red, green, blue);
                           (convertedByteArray[threeG1], convertedByteArray[threeG1 + 1], convertedByteArray[threeG1 + 2]) = (red, green, blue);
                           (convertedByteArray[threeG2], convertedByteArray[threeG2 + 1], convertedByteArray[threeG2 + 2]) = (red, green, blue);
                           (convertedByteArray[threeB], convertedByteArray[threeB + 1], convertedByteArray[threeB + 2]) = (red, green, blue);
                       }
                   });

            return convertedByteArray;
        }

        public static Bitmap BytesToBitmap(byte[] byteArray, int width, int height)
        {
            var bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            var bitmap_data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            Marshal.Copy(byteArray, 0, bitmap_data.Scan0, byteArray.Length);
            bitmap.UnlockBits(bitmap_data);

            return bitmap;
        }

        public static HImage ConvertBayerRg8(IGrabResult grabResult)
        {
            var bitmap = BytesToBitmap(BayerRG8ToRGB((byte[])grabResult.PixelData, grabResult.Width, grabResult.Height), grabResult.Width, grabResult.Height);
            return BitmapToHImage(bitmap, "byte", "rgbx");
        }

        public static HImage ConvertDefault(IGrabResult grabResult)
        {
            var guid = Guid.NewGuid();
            ImagePersistence.Save(ImageFileFormat.Tiff, $"{guid}.tiff", grabResult);
            var image = new HImage($"{guid}.tiff");
            File.Delete($"{guid}.tiff");
            return image;
        }

        public static HImage GrabResultToHalconImage(IGrabResult grabResult)
        {
            switch (grabResult.PixelTypeValue)
            {
                case PixelType.Mono8:
                    return ConvertMono(grabResult);
                case PixelType.BayerRG8:
                    return ConvertBayerRg8(grabResult);
                default:
                    return ConvertDefault(grabResult);
            }
        }
    }
}
