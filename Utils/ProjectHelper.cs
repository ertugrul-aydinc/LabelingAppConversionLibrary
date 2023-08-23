using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Visiomex.Projects.LabelingTool.Models;

namespace Visiomex.Projects.LabelingTool.Utils
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
