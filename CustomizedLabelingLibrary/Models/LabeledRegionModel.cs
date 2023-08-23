using CustomizedLabelingLibrary.Models.Enums;
using HalconDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomizedLabelingLibrary.Models
{
    public class LabeledRegionModel
    {
        public string GUID { get; set; } = Guid.NewGuid().ToString();
        public string LabelType = "";

        public double RegionCenterRow = 100;
        public double RegionCenterColumn = 100;

        public int RegionLeftTopRow = 100;
        public int RegionLeftTopColumn = 100;
        public int RegionRightBottomRow = 100;
        public int RegionRightBottomColumn = 100;

        public HRegion Region = new HRegion();

        public DrawTypeEnum DrawType = DrawTypeEnum.FreeHand;

        public LabeledRegionModel(HRegion Region)
        {
            this.Region = Region;

            Region.SmallestRectangle1(out RegionLeftTopRow, out RegionLeftTopColumn, out RegionRightBottomRow, out RegionRightBottomColumn);

            RegionCenterRow = (double)(RegionLeftTopRow + RegionRightBottomRow) / 2;
            RegionCenterRow = (double)(RegionLeftTopColumn + RegionRightBottomColumn) / 2;
        }

        public LabeledRegionModel(HRegion Region, string LabelType, string GUID)
        {
            this.Region = Region;
            this.LabelType = LabelType;
            this.GUID = GUID;

            Region.SmallestRectangle1(out RegionLeftTopRow, out RegionLeftTopColumn, out RegionRightBottomRow, out RegionRightBottomColumn);

            RegionCenterRow = (double)(RegionLeftTopRow + RegionRightBottomRow) / 2;
            RegionCenterRow = (double)(RegionLeftTopColumn + RegionRightBottomColumn) / 2;
        }

        public LabeledRegionModel() { }

        public void ChangeRegion(HRegion region)
        {
            if (region == null || !region.IsInitialized())
                return;

            if (Region != null && Region.IsInitialized())
                Region.Dispose();

            Region = region;

            Region.SmallestRectangle1(out RegionLeftTopRow, out RegionLeftTopColumn, out RegionRightBottomRow, out RegionRightBottomColumn);

            RegionCenterRow = (double)(RegionLeftTopRow + RegionRightBottomRow) / 2;
            RegionCenterRow = (double)(RegionLeftTopColumn + RegionRightBottomColumn) / 2;
        }

        public void Dispose()
        {
            if (Region != null && Region.IsInitialized())
                Region.Dispose();
        }
    }
}
