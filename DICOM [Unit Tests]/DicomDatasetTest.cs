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

        [Fact]
        public void GettingPrivateTagsChangesNothingWhenNotPresent()
        {
            var dataSet = new DicomDataset
            {
                {DicomTag.SOPInstanceUID, "2.999.1241"},
                {DicomTag.SOPClassUID, "2.999.1242"}
            };

            DicomPrivateCreator privateCreator = DicomDictionary.Default.GetPrivateCreator("TESTCREATOR");
            DicomDictionary privDict = DicomDictionary.Default[privateCreator];

            var privTag = new DicomDictionaryEntry(DicomMaskedTag.Parse("0011", "xx10"), "TestPrivTagName", "TestPrivTagKeyword", DicomVM.VM_1, false, DicomVR.DT);

            privDict.Add(privTag);

            var dataBefore = SerializeDicom_(dataSet);

            var val = dataSet.Get<string>(privTag.Tag);

            var dataAfter = SerializeDicom_(dataSet);

            Assert.Equal(dataBefore, dataAfter);
            Assert.Null(val);
        }

        [Fact]
        public void ContainsPrivateTagsChangesNothingWhenNotPresent()
        {
            var dataSet = new DicomDataset
            {
                {DicomTag.SOPInstanceUID, "2.999.1241"},
                {DicomTag.SOPClassUID, "2.999.1242"}
            };

            DicomPrivateCreator privateCreator = DicomDictionary.Default.GetPrivateCreator("TESTCREATOR");
            DicomDictionary privDict = DicomDictionary.Default[privateCreator];

            var privTag = new DicomDictionaryEntry(DicomMaskedTag.Parse("0011", "xx10"), "TestPrivTagName", "TestPrivTagKeyword", DicomVM.VM_1, false, DicomVR.DT);

            privDict.Add(privTag);

            var dataBefore = SerializeDicom_(dataSet);

            var val = dataSet.Contains(privTag.Tag);

            var dataAfter = SerializeDicom_(dataSet);

            Assert.Equal(dataBefore, dataAfter);
            Assert.False(val);
        }

        [Fact]
        public void ContainsPrivateTagsChangesNothingWhenPresent()
        {
            DicomPrivateCreator privateCreator = DicomDictionary.Default.GetPrivateCreator("TESTCREATOR");
            DicomDictionary privDict = DicomDictionary.Default[privateCreator];

            var privTag = new DicomDictionaryEntry(DicomMaskedTag.Parse("0011", "xx10"), "TestPrivTagName", "TestPrivTagKeyword", DicomVM.VM_1, false, DicomVR.DT);

            privDict.Add(privTag);

            var dataSet = new DicomDataset
            {
                {DicomTag.SOPInstanceUID, "2.999.1241"},
                {DicomTag.SOPClassUID, "2.999.1242"},
                {privTag.Tag, "19700101123456"}
            };

            var dataBefore = SerializeDicom_(dataSet);

            var val = dataSet.Contains(privTag.Tag);

            var dataAfter = SerializeDicom_(dataSet);

            Assert.Equal(dataBefore, dataAfter);
            Assert.True(val);
        }

        [Fact]
        public void GetPrivateTagsChangesNothingWhenPresent()
        {
            DicomPrivateCreator privateCreator = DicomDictionary.Default.GetPrivateCreator("TESTCREATOR");
            DicomDictionary privDict = DicomDictionary.Default[privateCreator];

            var privTag = new DicomDictionaryEntry(DicomMaskedTag.Parse("0011", "xx10"), "TestPrivTagName", "TestPrivTagKeyword", DicomVM.VM_1, false, DicomVR.DT);

            privDict.Add(privTag);

            var dataSet = new DicomDataset
            {
                {DicomTag.SOPInstanceUID, "2.999.1241"},
                {DicomTag.SOPClassUID, "2.999.1242"},
                {privTag.Tag, "19700101123456"}
            };

            var dataBefore = SerializeDicom_(dataSet);

            var val = dataSet.Get<string>(privTag.Tag);

            var dataAfter = SerializeDicom_(dataSet);

            Assert.Equal(dataBefore, dataAfter);
            Assert.Equal(val, "19700101123456");
        }
    }
}
