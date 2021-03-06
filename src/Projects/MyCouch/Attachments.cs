﻿using System.Net.Http;
using System.Threading.Tasks;
using EnsureThat;
using MyCouch.Commands;
using MyCouch.Extensions;
using MyCouch.Net;
using MyCouch.ResponseFactories;

namespace MyCouch
{
    public class Attachments : IAttachments
    {
        protected readonly IConnection Connection;
        protected readonly AttachmentResponseFactory AttachmentResponseFactory;
        protected readonly DocumentHeaderResponseFactory DocumentHeaderResponseFactory;

        public Attachments(IConnection connection, AttachmentResponseFactory attachmentResponseFactory, DocumentHeaderResponseFactory documentHeaderResponseFactory)
        {
            Ensure.That(connection, "connection").IsNotNull();
            Ensure.That(attachmentResponseFactory, "attachmentResponseFactory").IsNotNull();
            Ensure.That(documentHeaderResponseFactory, "documentHeaderResponseFactory").IsNotNull();

            Connection = connection;
            AttachmentResponseFactory = attachmentResponseFactory;
            DocumentHeaderResponseFactory = documentHeaderResponseFactory;
        }

        public virtual Task<AttachmentResponse> GetAsync(string docId, string attachmentName)
        {
            return GetAsync(new GetAttachmentCommand(docId, attachmentName));
        }

        public virtual Task<AttachmentResponse> GetAsync(string docId, string docRev, string attachmentName)
        {
            return GetAsync(new GetAttachmentCommand(docId, docRev, attachmentName));
        }

        public virtual async Task<AttachmentResponse> GetAsync(GetAttachmentCommand cmd)
        {
            Ensure.That(cmd, "cmd").IsNotNull();

            var req = CreateRequest(cmd);
            var res = SendAsync(req);

            return ProcessAttachmentResponse(await res.ForAwait());
        }

        public virtual async Task<DocumentHeaderResponse> PutAsync(PutAttachmentCommand cmd)
        {
            Ensure.That(cmd, "cmd").IsNotNull();

            var req = CreateRequest(cmd);
            var res = SendAsync(req);

            return ProcessDocumentHeaderResponse(await res.ForAwait());
        }

        public virtual Task<DocumentHeaderResponse> DeleteAsync(string docId, string docRev, string attachmentName)
        {
            return DeleteAsync(new DeleteAttachmentCommand(docId, docRev, attachmentName));
        }

        public virtual async Task<DocumentHeaderResponse> DeleteAsync(DeleteAttachmentCommand cmd)
        {
            Ensure.That(cmd, "cmd").IsNotNull();

            var req = CreateRequest(cmd);
            var res = SendAsync(req);

            return ProcessDocumentHeaderResponse(await res.ForAwait());
        }

        protected virtual HttpRequestMessage CreateRequest(GetAttachmentCommand cmd)
        {
            var req = new HttpRequest(HttpMethod.Get, GenerateRequestUrl(cmd.DocId, cmd.DocRev, cmd.Name));

            req.SetIfMatch(cmd.DocRev);

            return req;
        }

        protected virtual HttpRequestMessage CreateRequest(PutAttachmentCommand cmd)
        {
            var req = new HttpRequest(HttpMethod.Put, GenerateRequestUrl(cmd.DocId, cmd.DocRev, cmd.Name));

            req.SetIfMatch(cmd.DocRev);
            req.SetContent(cmd.ContentType, cmd.Content);

            return req;
        }

        protected virtual HttpRequestMessage CreateRequest(DeleteAttachmentCommand cmd)
        {
            var req = new HttpRequest(HttpMethod.Delete, GenerateRequestUrl(cmd.DocId, cmd.DocRev, cmd.Name));

            req.SetIfMatch(cmd.DocRev);

            return req;
        }

        protected virtual string GenerateRequestUrl(string docId, string docRev, string attachmentName)
        {
            return string.Format("{0}/{1}/{2}{3}",
                Connection.Address,
                docId,
                attachmentName,
                docRev == null ? string.Empty : string.Concat("?rev=", docRev));
        }

        protected virtual Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            return Connection.SendAsync(request);
        }

        protected virtual AttachmentResponse ProcessAttachmentResponse(HttpResponseMessage response)
        {
            return AttachmentResponseFactory.Create(response);
        }

        protected virtual DocumentHeaderResponse ProcessDocumentHeaderResponse(HttpResponseMessage response)
        {
            return DocumentHeaderResponseFactory.Create(response);
        }
    }
}