﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Entities
{
    public class FileEntity
    {
        public Guid Id { get; set; }
        public Guid MessageId { get; set; }

        public long Size { get; set; }
        public int PartSize { get; set; }

        public string Name { get; set; }
        public string Path { get; set; }

        public int Parts { get; set; }
        public int PartsUploaded { get; set; }

        public FileEntity()
        {

        }
    }
}
