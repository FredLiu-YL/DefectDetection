using Cognex.VisionPro;
using Cognex.VisionPro.Blob;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuanliCore.ImageProcess.Blob
{
    public class BlobParam
    {

        public ICogRegion ROI { get; set; }
        public CogBlob RunParams { get; set; }

    }

}
