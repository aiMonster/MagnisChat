﻿using Common.DTO.Communication;
using Common.DTO.Messages;
using Common.DTO.Sockets;
using Common.Enums;
using DataAccessLayer;
using Managers.Interfaces;
using Managers.WebSockets;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Managers
{
    public class FileManager : IFileManager
    {
        private readonly MagnisChatContext _context;
        private readonly ChatHandler _chatHandler;
        private readonly string _path;

        public FileManager(string path, MagnisChatContext context, ChatHandler chatHandler)
        {
            _path = path;
            _context = context;
            _chatHandler = chatHandler;            
        }

        public async Task<ResponseDTO<FileDTO>> GetFileAsync(Guid fileId)
        {
            var response = new ResponseDTO<FileDTO>();
            var file = await _context.Files.FirstOrDefaultAsync(f => f.Id == fileId);
            if (file == null)
            {
                response.Error = new Error(404, "File not found");
                return response;
            }
            response.Data = new FileDTO(file);
            return response;
        }

        public async Task<ResponseDTO<bool>> UploadPartFileAsync(FilePartDTO filePart, Guid fileId)
        {
            var response = new ResponseDTO<bool>();
            var file = await _context.Files.FirstOrDefaultAsync(f => f.Id == fileId);

            var path = Path.Combine(_path, fileId.ToString() + Path.GetExtension(file.Name));

            using (var fs = new FileStream(path, FileMode.Append))
            {
                fs.Write(filePart.Content, 0, filePart.Content.Length);
            }
            file.PartsUploaded++;
            await _context.SaveChangesAsync();

            var message = await _context.Messages.Include(m => m.Room).FirstOrDefaultAsync(m => m.Id == file.MessageId);           

            var socketMessage = new FileStatusDTO { Id = fileId, MessageId = file.MessageId, PartsUploaded = file.PartsUploaded };
            var socketDTO = new SocketResponseDTO<FileStatusDTO> { Type = SocketMessageTypes.FileStatusChanged, Model = socketMessage };

            foreach (var p in message.Room.Participants.Select(p => p.UserId))
            {
                await _chatHandler.SendMessageByUserId(p, socketDTO);
            }
            return response;
        }

        public async Task<ResponseDTO<FilePartDTO>> DownloadPartFileAsync(Guid fileId, int partNumber)
        {
            var response = new ResponseDTO<FilePartDTO>();
            var file = await _context.Files.FirstOrDefaultAsync(f => f.Id == fileId);

            byte[] buffer;
            var position = (partNumber - 1) * file.PartSize;
            if (position + file.PartSize > file.Size)
            {
                buffer = new byte[(int)file.Size - position];
            }
            else
            {
                buffer = new byte[file.PartSize];
            }

            string path = Path.Combine(_path, fileId.ToString() + Path.GetExtension(file.Name));

            using (BinaryReader reader = new BinaryReader(new FileStream(path, FileMode.Open, FileAccess.Read)))
            {
                reader.BaseStream.Seek(position, SeekOrigin.Begin);
                if (position + file.PartSize > file.Size)
                {
                    reader.Read(buffer, 0, (int)file.Size - position);
                }
                else
                {
                    reader.Read(buffer, 0, file.PartSize);
                }
            }

            response.Data = new FilePartDTO() { Content = buffer, PartNumber = partNumber };
            return response;
        }

    }
}
