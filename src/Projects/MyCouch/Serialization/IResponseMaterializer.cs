using System.IO;

namespace MyCouch.Serialization
{
    public interface IResponseMaterializer
    {
        void PopulateFailedResponse<T>(T response, Stream data) where T : IResponse;
        void PopulateBulkResponse(BulkResponse response, Stream data);
        void PopulateDocumentHeaderResponse(DocumentHeaderResponse response, Stream data);
        void PopulateViewQueryResponse<T>(ViewQueryResponse<T> response, Stream data) where T : class;
    }
}