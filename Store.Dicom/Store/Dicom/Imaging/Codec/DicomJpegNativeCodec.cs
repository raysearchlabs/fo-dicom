﻿using Dicom.Imaging.Codec.Jpeg;

// ReSharper disable CheckNamespace
namespace Dicom.Imaging.Codec
// ReSharper restore CheckNamespace
{
	public abstract class DicomJpegNativeCodec : DicomJpegCodec
	{
		#region CONSTRUCTORS

		#endregion

		#region METHODS

		protected abstract IJpegNativeCodec GetCodec(int bits, DicomJpegParams jparams);

		public override void Encode(DicomPixelData oldPixelData, DicomPixelData newPixelData, DicomCodecParams parameters)
		{
			throw new System.NotImplementedException();
/*	if (oldPixelData->NumberOfFrames == 0)
		return;

	// IJG eats the extra padding bits. Is there a better way to test for this?
	if (oldPixelData->BitsAllocated == 16 && oldPixelData->BitsStored <= 8) {
		// check for embedded overlays?
		newPixelData->BitsAllocated = 8;
	}

	if (parameters == nullptr || parameters->GetType() != DicomJpegParams::typeid)
		parameters = GetDefaultParameters();

	DicomJpegParams^ jparams = (DicomJpegParams^)parameters;

	JpegNativeCodec^ codec = GetCodec(oldPixelData->BitsStored, jparams);

	for (int frame = 0; frame < oldPixelData->NumberOfFrames; frame++) {
		codec->Encode(oldPixelData, newPixelData, jparams, frame);
	}*/
		}

		public override void Decode(DicomPixelData oldPixelData, DicomPixelData newPixelData, DicomCodecParams parameters)
		{
			if (oldPixelData.NumberOfFrames == 0)
				return;

			// IJG eats the extra padding bits. Is there a better way to test for this?
			if (newPixelData.BitsAllocated == 16 && newPixelData.BitsStored <= 8)
			{
				// check for embedded overlays here or below?
				newPixelData.BitsAllocated = 8;
			}

			var jparams = parameters as DicomJpegParams ?? GetDefaultParameters() as DicomJpegParams;

			var oldNativeData = oldPixelData.ToNativePixelData();
			int precision;
			try
			{
				try
				{
					precision = JpegHelper.ScanJpegForBitDepth(oldPixelData);
				}
				catch
				{
					// if the internal scanner chokes on an image, try again using ijg
					precision = new Jpeg12Codec(JpegMode.Baseline, 0, 0).ScanHeaderForPrecision(oldNativeData);
				}
			}
			catch
			{
				// the old scanner choked on several valid images...
				// assume the correct encoder was used and let libijg handle the rest
				precision = oldPixelData.BitsStored;
			}

			if (newPixelData.BitsStored <= 8 && precision > 8)
				newPixelData.BitsAllocated = 16; // embedded overlay?

			var codec = GetCodec(precision, jparams);

			var newNativeData = newPixelData.ToNativePixelData();
			var jNativeParams = jparams.ToNativeJpegParameters();
			for (var frame = 0; frame < oldPixelData.NumberOfFrames; frame++)
			{
				codec.Decode(oldNativeData, newNativeData, jNativeParams, frame);
			}
		}

		#endregion
	}
}