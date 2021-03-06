﻿using System.Net.Http;
using System.Threading.Tasks;
using EnsureThat;
using MyCouch.Commands;
using MyCouch.EntitySchemes;
using MyCouch.Extensions;
using MyCouch.Net;
using MyCouch.ResponseFactories;
using MyCouch.Serialization;

namespace MyCouch
{
    public class Entities : IEntities
    {
        protected readonly IConnection Connection;
        protected readonly EntityResponseFactory EntityResponseFactory;
        protected readonly IEntityReflector EntityReflector;

        public ISerializer Serializer { get; private set; }
        public IEntityReflector Reflector { get { return EntityReflector; } }

        public Entities(IConnection connection, EntityResponseFactory entityResponseFactory, ISerializer serializer, IEntityReflector entityReflector)
        {
            Ensure.That(connection, "connection").IsNotNull();
            Ensure.That(entityResponseFactory, "entityResponseFactory").IsNotNull();
            Ensure.That(serializer, "serializer").IsNotNull();
            Ensure.That(entityReflector, "entityReflector").IsNotNull();

            Connection = connection;
            EntityResponseFactory = entityResponseFactory;
            Serializer = serializer;
            EntityReflector = entityReflector;
        }

        public virtual Task<EntityResponse<T>> GetAsync<T>(string id, string rev = null) where T : class
        {
            return GetAsync<T>(new GetEntityCommand(id, rev));
        }

        public virtual async Task<EntityResponse<T>> GetAsync<T>(GetEntityCommand cmd) where T : class
        {
            Ensure.That(cmd, "cmd").IsNotNull();

            var req = CreateRequest(cmd);
            var res = SendAsync(req);

            return ProcessEntityResponse<T>(cmd, await res.ForAwait());
        }

        public virtual Task<EntityResponse<T>> PostAsync<T>(T entity) where T : class
        {
            return PostAsync(new PostEntityCommand<T>(entity));
        }

        public virtual async Task<EntityResponse<T>> PostAsync<T>(PostEntityCommand<T> cmd) where T : class
        {
            Ensure.That(cmd, "cmd").IsNotNull();

            var req = CreateRequest(cmd);
            var res = SendAsync(req);

            return ProcessEntityResponse(cmd, await res.ForAwait());
        }

        public virtual Task<EntityResponse<T>> PutAsync<T>(T entity) where T : class
        {
            return PutAsync(new PutEntityCommand<T>(entity));
        }

        public virtual async Task<EntityResponse<T>> PutAsync<T>(PutEntityCommand<T> cmd) where T : class
        {
            Ensure.That(cmd, "cmd").IsNotNull();

            var req = CreateRequest(cmd);
            var res = SendAsync(req);

            return ProcessEntityResponse(cmd, await res.ForAwait());
        }

        public virtual Task<EntityResponse<T>> DeleteAsync<T>(T entity) where T : class
        {
            return DeleteAsync(new DeleteEntityCommand<T>(entity));
        }

        public virtual async Task<EntityResponse<T>> DeleteAsync<T>(DeleteEntityCommand<T> cmd) where T : class
        {
            Ensure.That(cmd, "cmd").IsNotNull();

            var req = CreateRequest(cmd);
            var res = SendAsync(req);
            
            return ProcessEntityResponse(cmd, await res.ForAwait());
        }

        protected virtual Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            return Connection.SendAsync(request);
        }

        protected virtual string SerializeEntity<T>(T entity) where T : class
        {
            return Serializer.Serialize(entity);
        }

        protected virtual HttpRequestMessage CreateRequest(GetEntityCommand cmd)
        {
            var req = new HttpRequest(HttpMethod.Get, GenerateRequestUrl(cmd.Id, cmd.Rev));

            req.SetIfMatch(cmd.Rev);

            return req;
        }

        protected virtual HttpRequestMessage CreateRequest<T>(PostEntityCommand<T> cmd) where T : class
        {
            var req = new HttpRequest(HttpMethod.Post, GenerateRequestUrl());

            req.SetContent(SerializeEntity(cmd.Entity));

            return req;
        }

        protected virtual HttpRequestMessage CreateRequest<T>(PutEntityCommand<T> cmd) where T : class
        {
            var id = Reflector.IdMember.GetValueFrom(cmd.Entity);
            var rev = Reflector.RevMember.GetValueFrom(cmd.Entity);
            var req = new HttpRequest(HttpMethod.Put, GenerateRequestUrl(id, rev));

            req.SetIfMatch(rev);
            req.SetContent(SerializeEntity(cmd.Entity));

            return req;
        }

        protected virtual HttpRequestMessage CreateRequest<T>(DeleteEntityCommand<T> cmd) where T : class
        {
            var id = Reflector.IdMember.GetValueFrom(cmd.Entity);
            var rev = Reflector.RevMember.GetValueFrom(cmd.Entity);
            var req = new HttpRequest(HttpMethod.Delete, GenerateRequestUrl(id, rev));

            req.SetIfMatch(rev);

            return req;
        }

        protected virtual string GenerateRequestUrl(string id = null, string rev = null)
        {
            return string.Format("{0}/{1}{2}",
                Connection.Address,
                id ?? string.Empty,
                rev == null ? string.Empty : string.Concat("?rev=", rev));
        }

        protected virtual EntityResponse<T> ProcessEntityResponse<T>(GetEntityCommand cmd, HttpResponseMessage response) where T : class
        {
            return EntityResponseFactory.Create<T>(response);
        }

        protected virtual EntityResponse<T> ProcessEntityResponse<T>(PostEntityCommand<T> cmd, HttpResponseMessage response) where T : class
        {
            var entityResponse = EntityResponseFactory.Create<T>(response);
            entityResponse.Entity = cmd.Entity;

            if (entityResponse.IsSuccess)
            {
                Reflector.IdMember.SetValueTo(entityResponse.Entity, entityResponse.Id);
                Reflector.RevMember.SetValueTo(entityResponse.Entity, entityResponse.Rev);
            }

            return entityResponse;
        }

        protected virtual EntityResponse<T> ProcessEntityResponse<T>(PutEntityCommand<T> cmd, HttpResponseMessage response) where T : class
        {
            var entityResponse = EntityResponseFactory.Create<T>(response);
            entityResponse.Entity = cmd.Entity;

            if (entityResponse.IsSuccess)
                Reflector.RevMember.SetValueTo(entityResponse.Entity, entityResponse.Rev);

            return entityResponse;
        }

        protected virtual EntityResponse<T> ProcessEntityResponse<T>(DeleteEntityCommand<T> cmd, HttpResponseMessage response) where T : class
        {
            var entityResponse = EntityResponseFactory.Create<T>(response);
            entityResponse.Entity = cmd.Entity;

            if (entityResponse.IsSuccess)
                Reflector.RevMember.SetValueTo(entityResponse.Entity, entityResponse.Rev);

            return entityResponse;
        }
    }
}