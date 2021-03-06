﻿#if NETFX_CORE
using System.Reflection;
#endif
using System;
using System.Collections.Generic;
using EnsureThat;
using MyCouch.EntitySchemes;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace MyCouch.Serialization
{
    /// <summary>
    /// When deserializing and serializing with this contract resolver,
    /// Id and Rev members will be mapped according to conventions registrered
    /// in members of the <see cref="EntityReflector"/>.
    /// </summary>
    public class EntitySerializationContractResolver : SerializationContractResolver
    {
        protected readonly IEntityReflector EntityReflector;

        public EntitySerializationContractResolver(IEntityReflector entityReflector)
        {
            Ensure.That(entityReflector, "entityReflector").IsNotNull();

            EntityReflector = entityReflector;
        }

        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
#if !NETFX_CORE
            if (type == typeof(BulkResponse.Row) || (type.IsGenericType && typeof(ViewQueryResponse<>.Row) == type.GetGenericTypeDefinition()))
                return base.CreateProperties(type, memberSerialization);
#else
            //TODO: Ensure perf for GetTypeInfo etc in WinRT
            if (type == typeof(BulkResponse.Row) || (type.GetTypeInfo().IsGenericType && typeof(ViewQueryResponse<>.Row) == type.GetGenericTypeDefinition()))
                return base.CreateProperties(type, memberSerialization);
#endif
            var props = base.CreateProperties(type, memberSerialization);
            int? idRank = null, revRank = null;
            JsonProperty id = null, rev = null;

            foreach (var prop in props)
            {
                var tmpRank = EntityReflector.IdMember.GetMemberRankingIndex(type, prop.PropertyName);
                if (tmpRank != null)
                {
                    if (idRank == null || tmpRank < idRank)
                    {
                        idRank = tmpRank;
                        id = prop;
                    }

                    continue;
                }

                tmpRank = EntityReflector.RevMember.GetMemberRankingIndex(type, prop.PropertyName);
                if (tmpRank != null)
                {
                    if (revRank == null || tmpRank < revRank)
                    {
                        revRank = tmpRank;
                        rev = prop;
                    }
                }
            }

            if (id != null)
                id.PropertyName = "_id";

            if (rev != null)
                rev.PropertyName = "_rev";

            return props;
        }
    }
}