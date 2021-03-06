﻿using System.Net.Http;
using System.Threading.Tasks;
using EnsureThat;
using MyCouch.Commands;
using MyCouch.Extensions;
using MyCouch.Net;
using MyCouch.ResponseFactories;

namespace MyCouch
{
    public class Documents : IDocuments
    {
        protected readonly IConnection Connection;
        protected readonly DocumentResponseFactory DocumentReponseFactory;
        protected readonly DocumentHeaderResponseFactory DocumentHeaderReponseFactory;
        protected readonly BulkResponseFactory BulkReponseFactory;

        public Documents(IConnection connection, DocumentResponseFactory documentReponseFactory, DocumentHeaderResponseFactory documentHeaderResponseFactory, BulkResponseFactory bulkResponseFactory)
        {
            Ensure.That(connection, "connection").IsNotNull();
            Ensure.That(documentReponseFactory, "documentReponseFactory").IsNotNull();
            Ensure.That(documentHeaderResponseFactory, "documentHeaderResponseFactory").IsNotNull();
            Ensure.That(bulkResponseFactory, "bulkResponseFactory").IsNotNull();

            Connection = connection;
            DocumentReponseFactory = documentReponseFactory;
            DocumentHeaderReponseFactory = documentHeaderResponseFactory;
            BulkReponseFactory = bulkResponseFactory;
        }

        public virtual async Task<BulkResponse> BulkAsync(BulkCommand cmd)
        {
            Ensure.That(cmd, "cmd").IsNotNull();

            var req = CreateRequest(cmd);
            var res = SendAsync(req);

            return ProcessBulkResponse(await res.ForAwait());
        }

        public virtual Task<DocumentHeaderResponse> CopyAsync(string srcId, string newId)
        {
            return CopyAsync(new CopyDocumentCommand(srcId, newId));
        }

        public virtual Task<DocumentHeaderResponse> CopyAsync(string srcId, string srcRev, string newId)
        {
            return CopyAsync(new CopyDocumentCommand(srcId, srcRev, newId));
        }

        public virtual async Task<DocumentHeaderResponse> CopyAsync(CopyDocumentCommand cmd)
        {
            Ensure.That(cmd, "cmd").IsNotNull();

            var req = CreateRequest(cmd);
            var res = SendAsync(req);

            return ProcessDocumentHeaderResponse(await res.ForAwait());
        }

        public virtual Task<DocumentHeaderResponse> ReplaceAsync(string srcId, string trgId, string trgRev)
        {
            return ReplaceAsync(new ReplaceDocumentCommand(srcId, trgId, trgRev));
        }

        public virtual Task<DocumentHeaderResponse> ReplaceAsync(string srcId, string srcRev, string trgId, string trgRev)
        {
            return ReplaceAsync(new ReplaceDocumentCommand(srcId, srcRev, trgId, trgRev));
        }

        public virtual async Task<DocumentHeaderResponse> ReplaceAsync(ReplaceDocumentCommand cmd)
        {
            Ensure.That(cmd, "cmd").IsNotNull();

            var req = CreateRequest(cmd);
            var res = SendAsync(req);

            return ProcessDocumentHeaderResponse(await res.ForAwait());
        }

        public virtual Task<DocumentHeaderResponse> ExistsAsync(string id, string rev = null)
        {
            return ExistsAsync(new DocumentExistsCommand(id, rev));
        }

        public virtual async Task<DocumentHeaderResponse> ExistsAsync(DocumentExistsCommand cmd)
        {
            Ensure.That(cmd, "cmd").IsNotNull();

            var req = CreateRequest(cmd);
            var res = SendAsync(req);

            return ProcessDocumentHeaderResponse(await res.ForAwait());
        }

        public virtual Task<DocumentResponse> GetAsync(string id, string rev = null)
        {
            return GetAsync(new GetDocumentCommand(id, rev));
        }

        public virtual async Task<DocumentResponse> GetAsync(GetDocumentCommand cmd)
        {
            Ensure.That(cmd, "cmd").IsNotNull();

            var req = CreateRequest(cmd);
            var res = SendAsync(req);

            return ProcessDocumentResponse(await res.ForAwait());
        }

        public virtual Task<DocumentHeaderResponse> PostAsync(string doc)
        {
            return PostAsync(new PostDocumentCommand(doc));
        }

        public virtual async Task<DocumentHeaderResponse> PostAsync(PostDocumentCommand cmd)
        {
            Ensure.That(cmd, "cmd").IsNotNull();

            var req = CreateRequest(cmd);
            var res = SendAsync(req);

            return ProcessDocumentHeaderResponse(await res.ForAwait());
        }

        public virtual Task<DocumentHeaderResponse> PutAsync(string id, string doc)
        {
            return PutAsync(new PutDocumentCommand(id, doc));
        }

        public virtual Task<DocumentHeaderResponse> PutAsync(string id, string rev, string doc)
        {
            return PutAsync(new PutDocumentCommand(id, rev, doc));
        }

        public virtual async Task<DocumentHeaderResponse> PutAsync(PutDocumentCommand cmd)
        {
            Ensure.That(cmd, "cmd").IsNotNull();

            var req = CreateRequest(cmd);
            var res = SendAsync(req);

            return ProcessDocumentHeaderResponse(await res.ForAwait());
        }

        public virtual Task<DocumentHeaderResponse> DeleteAsync(string id, string rev)
        {
            return DeleteAsync(new DeleteDocumentCommand(id, rev));
        }

        public virtual async Task<DocumentHeaderResponse> DeleteAsync(DeleteDocumentCommand cmd)
        {
            Ensure.That(cmd, "cmd").IsNotNull();

            var req = CreateRequest(cmd);
            var res = SendAsync(req);

            return ProcessDocumentHeaderResponse(await res.ForAwait());
        }

        protected virtual Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            return Connection.SendAsync(request);
        }

        protected virtual HttpRequestMessage CreateRequest(BulkCommand cmd)
        {
            var req = new HttpRequest(HttpMethod.Post, GenerateRequestUrl(cmd));

            req.SetContent(cmd.ToJson());

            return req;
        }

        protected virtual HttpRequestMessage CreateRequest(CopyDocumentCommand cmd)
        {
            var req = new HttpRequest(new HttpMethod("COPY"), GenerateRequestUrl(cmd.SrcId, cmd.SrcRev));

            req.Headers.Add("Destination", cmd.NewId);

            return req;
        }

        protected virtual HttpRequestMessage CreateRequest(ReplaceDocumentCommand cmd)
        {
            var req = new HttpRequest(new HttpMethod("COPY"), GenerateRequestUrl(cmd.SrcId, cmd.SrcRev));

            req.Headers.Add("Destination", string.Concat(cmd.TrgId, "?rev=", cmd.TrgRev));

            return req;
        }

        protected virtual HttpRequestMessage CreateRequest(DocumentExistsCommand cmd)
        {
            var req = new HttpRequest(HttpMethod.Head, GenerateRequestUrl(cmd.Id, cmd.Rev));

            req.SetIfMatch(cmd.Rev);

            return req;
        }

        protected virtual HttpRequestMessage CreateRequest(GetDocumentCommand cmd)
        {
            var req = new HttpRequest(HttpMethod.Get, GenerateRequestUrl(cmd.Id, cmd.Rev));

            req.SetIfMatch(cmd.Rev);

            return req;
        }

        protected virtual HttpRequestMessage CreateRequest(DeleteDocumentCommand cmd)
        {
            var req = new HttpRequest(HttpMethod.Delete, GenerateRequestUrl(cmd.Id, cmd.Rev));

            req.SetIfMatch(cmd.Rev);

            return req;
        }

        protected virtual HttpRequestMessage CreateRequest(PutDocumentCommand cmd)
        {
            var req = new HttpRequest(HttpMethod.Put, GenerateRequestUrl(cmd.Id, cmd.Rev));

            req.SetIfMatch(cmd.Rev);
            req.SetContent(cmd.Content);

            return req;
        }

        protected virtual HttpRequestMessage CreateRequest(PostDocumentCommand cmd)
        {
            var req = new HttpRequest(HttpMethod.Post, GenerateRequestUrl());

            req.SetContent(cmd.Content);

            return req;
        }

        protected virtual string GenerateRequestUrl(BulkCommand cmd)
        {
            return string.Format("{0}/_bulk_docs", Connection.Address);
        }

        protected virtual string GenerateRequestUrl(string id = null, string rev = null)
        {
            return string.Format("{0}/{1}{2}",
                Connection.Address,
                id ?? string.Empty,
                rev == null ? string.Empty : string.Concat("?rev=", rev));
        }

        protected virtual BulkResponse ProcessBulkResponse(HttpResponseMessage response)
        {
            return BulkReponseFactory.Create(response);
        }

        protected virtual DocumentHeaderResponse ProcessDocumentHeaderResponse(HttpResponseMessage response)
        {
            return DocumentHeaderReponseFactory.Create(response);
        }

        protected virtual DocumentResponse ProcessDocumentResponse(HttpResponseMessage response)
        {
            return DocumentReponseFactory.Create(response);
        }
    }
}