﻿using System;

namespace MyCouch
{
#if !NETFX_CORE
    [Serializable]
#endif
    public class ViewQueryResponse<T> : Response where T : class 
    {
        public long TotalRows { get; set; }
        public long RowCount { get { return IsEmpty ? 0 : Rows.Length; } }
        public long UpdateSeq { get; set; }
        public long OffSet { get; set; }
        public bool IsEmpty
        {
            get { return Rows == null || Rows.Length == 0; }
        }
        public Row[] Rows { get; set; }

        public override string GenerateToStringDebugVersion()
        {
            return string.Format("{0}{1}{0}IsEmpty: {2}{0}TotalRows: {3}{0}RowCount:{4}{0}Offset: {5}",
                Environment.NewLine, 
                base.GenerateToStringDebugVersion(), 
                IsEmpty, 
                TotalRows, 
                RowCount, 
                OffSet);
        }

#if !NETFX_CORE
        [Serializable]
#endif
        public class Row
        {
            public string Id { get; set; }
            public string Key { get; set; }
            public T Value { get; set; }
            public T Doc { get; set; }
        }
    }
}