﻿using System;

namespace MyCouch
{
#if !NETFX_CORE
    [Serializable]
#endif
    public class EntityResponse<T> : DocumentHeaderResponse where T : class
    {
        public T Entity { get; set; }

        public bool IsEmpty
        {
            get { return Entity == null; }
        }

        public override string GenerateToStringDebugVersion()
        {
            return string.Format("{0}{1}{0}Model: {2}{0}IsEmpty: {3}", 
                Environment.NewLine, 
                base.GenerateToStringDebugVersion(), 
                typeof(T).Name,
                IsEmpty);
        }
    }
}