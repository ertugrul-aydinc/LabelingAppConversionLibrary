using CustomizedLabelingLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomizedLabelingLibrary.Helpers
{
    public sealed class ProjectHelper
    {
        private static readonly object lockObject = new object();

        private static LabellingProject partModel;

        public static LabellingProject Instance
        {
            get
            {
                if (partModel == null)
                {
                    lock (lockObject)
                    {
                        if (partModel == null)
                        {
                            partModel = new LabellingProject();
                        }
                    }
                }
                return partModel;
            }
        }
    }
}
