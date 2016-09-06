using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dicom;
using Xunit;

namespace DICOM__Unit_Tests_
{
    public class DicomDatasetTest
    {
        [Fact]
        public void CheckAddedPrivateTagValueRepresentation()
        {
            var privCreatorDictEntry = new DicomDictionaryEntry(new DicomTag(0x0011, 0x0010), "Private Creator", "PrivateCreator", DicomVM.VM_1, false, DicomVR.LO);
            DicomDictionary.Default.Add(privCreatorDictEntry);

            DicomPrivateCreator privateCreator1 = DicomDictionary.Default.GetPrivateCreator("TESTCREATOR1");
            DicomDictionary privDict1 = DicomDictionary.Default[privateCreator1];

            var dictEntry = new DicomDictionaryEntry(DicomMaskedTag.Parse("0011", "xx10"), "TestPrivTagName", "TestPrivTagKeyword", DicomVM.VM_1, false, DicomVR.CS);
            privDict1.Add(dictEntry);

            var ds = new DicomDataset();
            ds.Add(dictEntry.Tag, "VAL1");

            Assert.Equal(DicomVR.CS, ds.Get<DicomVR>(ds.GetPrivateTag(dictEntry.Tag)));
        }
    }
}
