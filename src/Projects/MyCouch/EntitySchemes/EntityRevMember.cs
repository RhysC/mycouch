﻿using System;
using MyCouch.EntitySchemes.Reflections;

namespace MyCouch.EntitySchemes
{
    public class EntityRevMember : EntityMember 
    {
        public EntityRevMember(IDynamicPropertyFactory dynamicPropertyFactory) 
            : base(dynamicPropertyFactory)
        {
        }

        public override int? GetMemberRankingIndex(Type entityType, string membername)
        {
            if (membername.Equals("_rev", StringComparison.OrdinalIgnoreCase))
                return 0;

            if (membername.Equals(string.Concat(entityType.Name, "rev"), StringComparison.OrdinalIgnoreCase))
                return 1;

            if (membername.Equals("documentrev", StringComparison.OrdinalIgnoreCase))
                return 2;

            if (membername.Equals("entityrev", StringComparison.OrdinalIgnoreCase))
                return 3;

            if (membername.Equals("rev", StringComparison.OrdinalIgnoreCase))
                return 4;

            return null;
        }
    }
}