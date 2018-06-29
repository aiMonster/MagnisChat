using Common.DTO.Account;
using Common.DTO.Messages;
using MagnisChatWPF.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagnisChatWPF.Managers
{
    [Export(typeof(IFileManager))]
    public class FileManager : IFileManager
    {
        [Import(typeof(IHttpManager))]
        private IHttpManager _httpManager;

        const int BufferSize = 4194304;  

        public event Action<int> PartDownloaded;
        public event Action<string> Failed;
       
        public FileManager()
        {           
            BuildImports();           
        }

        private void BuildImports()
        {
            var catalog = new AssemblyCatalog(this.GetType().Assembly);
            var container = new CompositionContainer(catalog);
            container.ComposeParts(this);
        }

        public void Authorize(string token)
        {
            _httpManager.Authorize(token);
        }

        public async void Upload(FileMessageRequest file, Guid roomId)
        {  
            file.Size = File.OpenRead(file.Path).Length;
            file.PartSize = BufferSize;
            file.Parts = (int)Math.Ceiling(file.Size / (double)BufferSize);

            var response = await _httpManager.PostAsync<FileDTO>($"api/Rooms/{roomId.ToString()}/FileMessages", file);
            if(response.Error != null)
            {
                Failed?.Invoke(response.Error.ErrorDescription);
                return;
            }
                       
            var position = 0;
            var part = 1; 

            while(position < file.Size)
            {
                byte[] buffer;
                if(position + BufferSize > file.Size)
                {
                    buffer = new byte[(int)file.Size - position];
                }
                else
                {
                    buffer = new byte[BufferSize];
                }

                using (BinaryReader reader = new BinaryReader(new FileStream(file.Path, FileMode.Open, FileAccess.Read)))
                {
                    reader.BaseStream.Seek(position, SeekOrigin.Begin);
                    if(position + BufferSize > file.Size)
                    {
                        reader.Read(buffer, 0, (int)file.Size - position);
                    }
                    else
                    {
                        reader.Read(buffer, 0, BufferSize);
                    }                   
                }

                var request = new FilePartDTO
                {
                    Content = buffer,
                    PartNumber = part
                };  

                var resp = await _httpManager.PostAsync<bool>($"api/Files/{response.Data.Id}", request);
                if (resp.Error != null)
                {
                    Failed?.Invoke(resp.Error.ErrorDescription);
                    return;
                }
                
                position += BufferSize;
            }            
        }

        public async void Download(FileDTO file)
        { 
            var directory = "Downloads";
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            var destination = Path.Combine(directory, $"{file.Id.ToString()}{Path.GetExtension(file.Name)}");
            for (int part = 1; part <= file.Parts; part++)
            {                
                var filePartResponse = await _httpManager.GetAsync<FilePartDTO>($"api/Files/{file.Id}/DownloadPart/{part}");
                if (filePartResponse.Error != null)
                {
                    Failed?.Invoke(filePartResponse.Error.ErrorDescription);
                    return;
                }

                using (var fs = new FileStream(destination, FileMode.Append, FileAccess.Write, FileShare.Write))
                {

                    fs.Write(filePartResponse.Data.Content, 0, filePartResponse.Data.Content.Length);
                    //fs.BeginWrite(
                    //    filePartResponse.Data.Content, 
                    //    0, 
                    //    filePartResponse.Data.Content.Length,
                    //    new AsyncCallback((a) => { PartDownloaded(part); }), 
                    //    null);                    
                    PartDownloaded(part);
                }
            }
            
        }
    }
}
