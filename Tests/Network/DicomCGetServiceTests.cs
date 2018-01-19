// Copyright (c) 2012-2017 fo-dicom contributors.
// Licensed under the Microsoft Public License (MS-PL).

namespace Dicom.Network
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using Dicom.Log;

    using Xunit;

    [Collection("Network"), Trait("Category", "Network")]
    public class DicomCGetServiceTest
    {
        #region Unit tests

        [Fact(Skip = "Not working")]
        public void Send_SingleRequest_DataSufficientlyTransported()
        {
            int port = Ports.GetNext();
            using (new DicomServer<SimpleCGetProvider>(port))
            {
                DicomDataset command = null, dataset = null;
                var getRequest = new DicomCGetRequest(DicomUID.Generate().UID);
                var allDone = new ManualResetEventSlim();
                getRequest.OnResponseReceived = (req, res) =>
                    {
                        command = getRequest.Command;
                        if (res.Status != DicomStatus.Pending)
                        {
                            allDone.Set();
                        }
                    };

                var client = new DicomClient();
                var pcs = DicomPresentationContext.GetScpRolePresentationContextsFromStorageUids(
                    DicomStorageCategory.Image,
                    DicomTransferSyntax.ExplicitVRLittleEndian,
                    DicomTransferSyntax.ImplicitVRLittleEndian,
                    DicomTransferSyntax.ImplicitVRBigEndian);
                client.AdditionalPresentationContexts.AddRange(pcs);
                client.AddRequest(getRequest);
                client.OnCStoreRequest = (storeRequest) =>
                    {
                        return new DicomCStoreResponse(storeRequest, DicomStatus.Success);
                    };

                client.Send("127.0.0.1", port, false, "SCU", "ANY-SCP");
                allDone.Wait();
            }
        }

        [Fact(Skip = "Not working")]
        public async Task SendAsync_SingleRequest_DataSufficientlyTransported()
        {
            int port = Ports.GetNext();
            using (new DicomServer<SimpleCGetProvider>(port))
            {
                DicomDataset command = null, dataset = null;
                var getRequest = new DicomCGetRequest(DicomUID.Generate().UID);
                var allDone = new ManualResetEventSlim();
                getRequest.OnResponseReceived = (req, res) =>
                    {
                        command = getRequest.Command;
                        if (res.Status != DicomStatus.Pending)
                        {
                            allDone.Set();
                        }
                    };

                var client = new DicomClient();
                var pcs = DicomPresentationContext.GetScpRolePresentationContextsFromStorageUids(
                    DicomStorageCategory.Image,
                    DicomTransferSyntax.ExplicitVRLittleEndian,
                    DicomTransferSyntax.ImplicitVRLittleEndian,
                    DicomTransferSyntax.ImplicitVRBigEndian);
                client.AdditionalPresentationContexts.AddRange(pcs);
                client.AddRequest(getRequest);
                client.OnCStoreRequest = (storeRequest) =>
                    {
                        return new DicomCStoreResponse(storeRequest.Command);
                    };
                client.SendAsync("127.0.0.1", port, false, "SCU", "ANY-SCP");
                allDone.Wait();
            }
        }

        #endregion
    }

    #region Support classes

    internal class SimpleCGetProvider : DicomService, IDicomServiceProvider, IDicomCGetProvider
    {
        private static readonly DicomTransferSyntax[] AcceptedTransferSyntaxes =
            {
                    DicomTransferSyntax.ExplicitVRLittleEndian,
                    DicomTransferSyntax.ExplicitVRBigEndian,
                    DicomTransferSyntax.ImplicitVRLittleEndian
                };

        private static readonly DicomTransferSyntax[] AcceptedImageTransferSyntaxes =
            {
                    // Lossless
                    DicomTransferSyntax.JPEGLSLossless,
                    DicomTransferSyntax.JPEG2000Lossless,
                    DicomTransferSyntax.JPEGProcess14SV1,
                    DicomTransferSyntax.JPEGProcess14,
                    DicomTransferSyntax.RLELossless,

                    // Lossy
                    DicomTransferSyntax.JPEGLSNearLossless,
                    DicomTransferSyntax.JPEG2000Lossy,
                    DicomTransferSyntax.JPEGProcess1,
                    DicomTransferSyntax.JPEGProcess2_4,

                    // Uncompressed
                    DicomTransferSyntax.ExplicitVRLittleEndian,
                    DicomTransferSyntax.ExplicitVRBigEndian,
                    DicomTransferSyntax.ImplicitVRLittleEndian
                };

        public SimpleCGetProvider(INetworkStream stream, Encoding fallbackEncoding, Logger log)
            : base(stream, fallbackEncoding, log)
        {
        }

        public void OnReceiveAssociationRequest(DicomAssociation association)
        {
            foreach (var pc in association.PresentationContexts)
            {
                if (pc.AbstractSyntax == DicomUID.Verification) pc.AcceptTransferSyntaxes(AcceptedTransferSyntaxes);
                else if (pc.AbstractSyntax.StorageCategory != DicomStorageCategory.None) pc.AcceptTransferSyntaxes(AcceptedImageTransferSyntaxes);
                else if (pc.AbstractSyntax == DicomUID.PatientRootQueryRetrieveInformationModelGET || pc.AbstractSyntax
                         == DicomUID.StudyRootQueryRetrieveInformationModelGET)
                {
                    pc.AcceptTransferSyntaxes(AcceptedTransferSyntaxes);
                }
            }

            this.SendAssociationAccept(association);
        }

        public void OnReceiveAssociationReleaseRequest()
        {
            this.SendAssociationReleaseResponse();
        }

        public void OnReceiveAbort(DicomAbortSource source, DicomAbortReason reason)
        {
        }

        public void OnConnectionClosed(Exception exception)
        {
        }

        public void OnCStoreRequestException(string tempFileName, Exception e)
        {
        }

        public IEnumerable<DicomCGetResponse> OnCGetRequest(DicomCGetRequest request)
        {
            yield return new DicomCGetResponse(request, DicomStatus.Pending)
            {
                Completed = 0, Failures = 0, Warnings = 0, Remaining = 1
            };

            var responseEvent = new ManualResetEventSlim();
            var storeRequest = new DicomCStoreRequest(@".\Test Data\CT1_J2KI")
            {
                OnResponseReceived = (rq, rp) =>
                {
                    Console.WriteLine(rp.RequestMessageID);
                    responseEvent.Set();
                }
            };

            this.SendRequest(storeRequest);
            responseEvent.Wait();

            yield return new DicomCGetResponse(request, DicomStatus.Success)
            {
                Completed = 1, Failures = 0, Warnings = 0, Remaining = 0
            };
        }
    }

    #endregion
}
