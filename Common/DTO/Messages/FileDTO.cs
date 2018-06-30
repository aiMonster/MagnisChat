using Common.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTO.Messages
{
    public class FileDTO
    {
        public Guid Id { get; set; }
        public Guid MessageId { get; set; }

        public long Size { get; set; }
        public int PartSize { get; set; }

        public string Name { get; set; }
        public string Path { get; set; }

        public int Parts { get; set; }
        public int PartsUploaded { get; set; }   
        
        public FileDTO() { }

        public FileDTO(FileEntity entity)
        {
            Id = entity.Id;
            MessageId = entity.MessageId;

            Size = entity.Size;
            PartSize = entity.PartSize;

            Name = entity.Name;
            Path = entity.Path;

            Parts = entity.Parts;
            PartsUploaded = entity.PartsUploaded;
        }
    }
}
