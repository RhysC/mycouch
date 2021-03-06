﻿using System;

namespace MyCouch
{
#if !NETFX_CORE
    [Serializable]
#endif
    public class AttachmentResponse : DocumentHeaderResponse
    {
        public string Name { get; set; }
        public byte[] Content { get; set; }

        public bool IsEmpty
        {
            get { return Content == null || Content.Length < 1; }
        }

        public override string GenerateToStringDebugVersion()
        {
            return string.Format("{0}{1}{0}IsEmpty: {2}{0}Content: {3}",
                Environment.NewLine, 
                base.GenerateToStringDebugVersion(),
                IsEmpty,
                IsEmpty ? "<NULL>" : Convert.ToBase64String(Content));
        }
    }
}