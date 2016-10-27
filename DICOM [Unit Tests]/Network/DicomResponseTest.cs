using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dicom;
using Dicom.Network;
using Xunit;

namespace DICOM__Unit_Tests_.Network
{
    public class DicomResponseTest
    {
        [Fact]
        public void ErrorCommentIsNotThereForSuccess()
        {
            var rsp = new DicomResponse(new DicomDataset());

            rsp.Status = DicomStatus.Success;

            Assert.False(rsp.Command.Contains(DicomTag.ErrorComment));
        }

        [Fact]
        public void ErrorCommentIsThereForFailure()
        {
            var rsp = new DicomResponse(new DicomDataset());

            var failure = new DicomStatus("FF01", DicomState.Failure, "Failed", "Comment on the failure mode");

            rsp.Status = failure;

            Assert.True(rsp.Command.Contains(DicomTag.ErrorComment));
        }
    }
}
