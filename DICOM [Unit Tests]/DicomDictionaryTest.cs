using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dicom;
using Xunit;

namespace DICOM__Unit_Tests_
{
  public class DicomDictionaryTest
  {
    [Fact]
    public void AddTagAndReadOut()
    {
      var dict = new DicomDictionary();

      var tag = new DicomTag(0x0010, 0x0020);
      var dictEntry = new DicomDictionaryEntry(tag, "TestTagName", "TestTagKeyword", DicomVM.VM_1, false, DicomVR.DT);

      dict.Add(dictEntry);

      Assert.Equal(dictEntry, dict[tag]);
    }

    [Fact]
    public void AddPrivateTagAndReadOut()
    {
      var dict = new DicomDictionary
      {
        new DicomDictionaryEntry(new DicomTag(0x0011, 0x0010), "Private Creator", "PrivateCreator", DicomVM.VM_1, false, DicomVR.LO)
      };

      DicomPrivateCreator privateCreator = dict.GetPrivateCreator("TESTCREATOR");
      DicomDictionary privDict = dict[privateCreator];

      var dictEntry = new DicomDictionaryEntry(new DicomTag(0x0011, 0x1010), "TestPrivTagName", "TestPrivTagKeyword", DicomVM.VM_1, false, DicomVR.DT);

      privDict.Add(dictEntry);

      Assert.True(dictEntry.Equals(dict[dictEntry.Tag.PrivateCreator][dictEntry.Tag]));
    }

    [Fact]
    public void EnumerateBothPublicAndPrivateEntries()
    {
      var dict = new DicomDictionary();

      var tag1 = new DicomTag(0x0010, 0x0020);
      var dictEntry1 = new DicomDictionaryEntry(tag1, "TestPublicTagName", "TestPublicTagKeyword", DicomVM.VM_1, false, DicomVR.DT);
      var privCreatorDictEntry = new DicomDictionaryEntry(new DicomTag(0x0011, 0x0010), "Private Creator", "PrivateCreator", DicomVM.VM_1, false, DicomVR.LO);
      dict.Add(privCreatorDictEntry);

      DicomPrivateCreator privateCreator = dict.GetPrivateCreator("TESTCREATOR");
      DicomDictionary privDict = dict[privateCreator];

      var dictEntry2 = new DicomDictionaryEntry(DicomMaskedTag.Parse("0011", "xx10"), "TestPrivTagName", "TestPrivTagKeyword", DicomVM.VM_1, false, DicomVR.DT);

      privDict.Add(dictEntry2);
      dict.Add(dictEntry1);

      Assert.True(dict.Contains(dictEntry1));
      Assert.True(dict.Contains(privCreatorDictEntry));
      Assert.True(dict[dictEntry2.Tag.PrivateCreator].Contains(dictEntry2));
      Assert.True(dict.PrivateCreators.Any(pc => dict[pc].Contains(dictEntry2)));
    }
  }
}
