using System;
using System.Collections.Generic;
using System.IO;
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

            Assert.Equal(DicomVR.CS, ds.Get<DicomVR>(dictEntry.Tag));
        }

        private static DicomDataset ParseDicom_(byte[] content)
        {
            var stream = new MemoryStream(content);
            var file = DicomFile.Open(stream);
            var dataset = file.Dataset;

            // TODO: Is "file" correctly disposed?
            return dataset;
        }

        private static byte[] SerializeDicom_(DicomDataset dataset)
        {
            var stream = new MemoryStream();
            var file = new DicomFile(dataset);
            file.Save(stream);
            return stream.ToArray();
        }


        [Fact]
        public void SerializeAndDeserializePrivateTags()
        {
            var privCreatorDictEntry = new DicomDictionaryEntry(new DicomTag(0x0011, 0x0010), "Private Creator", "PrivateCreator", DicomVM.VM_1, false, DicomVR.LO);
            DicomDictionary.Default.Add(privCreatorDictEntry);

            DicomPrivateCreator privateCreator1 = DicomDictionary.Default.GetPrivateCreator("TESTCREATOR1");
            DicomDictionary privDict1 = DicomDictionary.Default[privateCreator1];

            var dictEntry = new DicomDictionaryEntry(DicomMaskedTag.Parse("0011", "xx01"), "TestPrivTagName", "TestPrivTagKeyword", DicomVM.VM_1, false, DicomVR.CS);
            privDict1.Add(dictEntry);

            var ds = new DicomDataset();
            ds.Add(dictEntry.Tag, "VAL1");
            ds.Add(DicomTag.SOPClassUID, DicomUID.CTImageStorage);
            ds.Add(DicomTag.SOPInstanceUID, "2.25.123");
            Assert.Equal(DicomVR.CS, ds.Get<DicomVR>(dictEntry.Tag));

            var bytes = SerializeDicom_(ds);

            File.OpenWrite("C:\\Temp\\x.dcm").Write(bytes, 0, bytes.Length);

            var ds2 = ParseDicom_(bytes);

            Assert.Equal(DicomVR.CS, ds2.Get<DicomVR>(dictEntry.Tag));

        }
    }
}
