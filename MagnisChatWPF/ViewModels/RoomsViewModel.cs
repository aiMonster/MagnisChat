using Common.DTO.Account;
using Common.DTO.Messages;
using Common.DTO.Rooms;
using Common.DTO.Sockets;
using Common.Enums;
using MagnisChatWPF.Interfaces;
using MagnisChatWPF.Managers;
using MagnisChatWPF.Models;
using MagnisChatWPF.Views;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;

namespace MagnisChatWPF.ViewModels
{
    public class RoomsViewModel : INotifyPropertyChanged
    {
        #region Variables
        [Import(typeof(IHttpManager))]
        private IHttpManager _httpManager;

        [Import(typeof(IFileManager))]
        private IFileManager _fileManager;

        private readonly TokenResponse _httpToken;          

        private ConcurrentDictionary<Guid, UserProfile> CashedUsers = new ConcurrentDictionary<Guid, UserProfile>();

        private string newRoomTitle;
        private string messageContent;
        private string downloads;
       
        private RoomModel _selectedMyRoom;
        private RoomModel _selectedAnotherRoom;        
        private MessageModel _selectedMessage;

        public ObservableCollection<RoomModel> OtherRooms { get; set; }
        public ObservableCollection<RoomModel> MyRooms { get; set; }
        public ObservableCollection<MessageModel> Messages { get; set; } = new ObservableCollection<MessageModel>();
        
        public ICommand AddRoomCommand { get; protected set; }
        public ICommand SendMessageCommand { get; protected set; }
        public ICommand SendFileCommand { get; protected set; }
        public ICommand DownloadFileCommand { get; protected set; }
        public ICommand ParticipateRoomCommand { get; protected set; }
        public ICommand LogOutCommand { get; protected set; }

        public event Action LogOut;
        #endregion

        public RoomsViewModel(TokenResponse tokenResponse)
        {
            _httpToken = tokenResponse; 
            BuildImports();
            _httpManager.Authorize(_httpToken.Token);
            _fileManager.Authorize(_httpToken.Token);

            AddRoomCommand = new Command(obj => AddRoom());
            SendMessageCommand = new Command(obj => SendMessage());
            SendFileCommand = new Command(async obj => await SendFile());  
            DownloadFileCommand = new Command(obj => DownloadFile());
            ParticipateRoomCommand = new Command(obj => Participate());
            LogOutCommand = new Command(obj => LogOut());

            LoadRooms();                  
        }

        private void BuildImports()
        {
            var catalog = new AssemblyCatalog(this.GetType().Assembly);  
            var container = new CompositionContainer(catalog);            
            container.ComposeParts(this);
        }

        #region Handlers
        public async void OnRoomCreated(RoomDTO room)
        {
            var userProfile = await GetUserAsync(room.AdminId);  
            if(userProfile == null)
            {
                return;
            }
            if(room.AdminId.ToString() == _httpToken.UserId)
            {
                ChangeData(delegate { MyRooms.Insert(0,new RoomModel(room, userProfile)); });
            }
            else
            {
                ChangeData(delegate { OtherRooms.Insert(0, new RoomModel(room, userProfile)); });
            }
        }

        public async void OnFileStatusChanged(FileStatusDTO fileStatus)
        {
            var file = await GetFileAsync(fileStatus.Id);
            if(file == null)
            {
                return;
            }
            ChangeData(delegate
            {
                var message = Messages.FirstOrDefault(m => m.Id == fileStatus.MessageId);
                if(message != null)
                {
                    message.Content = GenerateContent(file);
                }
            });            
        }

        public async void OnMessageReceived(MessageDTO message)
        {
            var userProfile = await GetUserAsync(message.SenderId);  
            if(userProfile == null)
            {
                return;
            }
            if (SelectedMyRoom?.Id != null && SelectedMyRoom.Id == message.RoomId)
            {
                if(message.Type == MessageTypes.Text)
                {
                    ChangeData(delegate { Messages.Add(new MessageModel(message, userProfile));});
                }
                else if(message.Type == MessageTypes.File)
                {
                    var file = await GetFileAsync(new Guid(message.Content));    
                    if(file == null)
                    {
                        return;
                    }
                    message.Content = GenerateContent(file);                    
                    ChangeData(delegate { Messages.Add(new MessageModel(message, userProfile) { FileId = file.Id });});                   
                }                
            }
        }

        private string GenerateContent(FileDTO file)
        {
            var status = file.Parts == file.PartsUploaded ? "Loaded" : "Loading";
            return $"Sharing a file - {file.Name}" +
                $"\n{status}- {file.PartsUploaded * 100 / file.Parts}%" +
                $"\nSize - {file.Size} bytes";                
        }
                  
        public void OnRoomParticipated(RoomParticipatedDTO roomParticipatedDTO)
        {
            if(_httpToken.UserId == roomParticipatedDTO.UserId.ToString())
            {
                var room = OtherRooms.Where(r => r.Id == roomParticipatedDTO.RoomId).FirstOrDefault();
                ChangeData(delegate { MyRooms.Add(room); OtherRooms.Remove(room); });
            } 
        }
        #endregion       

        #region Commands
        private async void AddRoom()
        {
            if(string.IsNullOrEmpty(NewRoomTitle))
            {
                ShowNotification("Title cannot be empty");
                return;
            }
            var resp = await _httpManager.PostAsync<Guid>("api/Rooms", new RoomRequest() { Title = NewRoomTitle });
            if(resp.Error != null)
            {
                ShowNotification(resp.Error.ErrorDescription);
                return;
            }
            NewRoomTitle = string.Empty;
        }

        private async Task SendFile()
        {
            if (SelectedMyRoom == null)
            {
                ShowNotification("Room not selected");
                return;
            }
            var dialog = new OpenFileDialog();
            dialog.ShowDialog();

            if(string.IsNullOrEmpty(dialog.FileName))
            {
                return;
            }          
            var request = new FileMessageRequest { Name = dialog.SafeFileName, Path = dialog.FileName };
            var thread = new Thread(new ParameterizedThreadStart(param =>
            {                 
                _fileManager.Upload(request, SelectedMyRoom.Id);                
            }));
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }        

        private async void DownloadFile()
        {
            if (SelectedMessage == null || SelectedMessage.Type != MessageTypes.File)
            {
                ShowNotification("It is not a file");
                return;
            }

            var file = await GetFileAsync(SelectedMessage.FileId);
            if (file == null || file.PartsUploaded != file.Parts)
            {
                ShowNotification("It is not loaded yet");
                return;
            }

            var thread = new Thread(new ParameterizedThreadStart(param =>
            {
               
                _fileManager.PartDownloaded += (part) => {
                    Downloads = $"{file.Name} - {file.PartsUploaded * 100 / file.Parts}%";                   
                };
                
                Downloads = $"{file.Name} - 0%";
                _fileManager.Download(file);
            }));
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        private async void SendMessage()
        {
            if (SelectedMyRoom == null)
            {
                ShowNotification("Room not selected");
                return;
            }
            if (string.IsNullOrEmpty(MessageContent))
            {
                ShowNotification("Message cannot be empty");
                return;
            }
            var resp = await _httpManager.PostAsync<bool>($"api/Rooms/{SelectedMyRoom.Id.ToString()}/TextMessages", new TextMessageRequest() { Content = MessageContent });
            if(resp.Error != null)
            {
                ShowNotification(resp.Error.ErrorDescription);
                return;
            }
            MessageContent = string.Empty;
        }

        private async void Participate()
        {
            if(SelectedAnotherRoom == null)
            {
                return;
            }

            var resp = await _httpManager.PutAsync<bool>($"api/Rooms/{SelectedAnotherRoom.Id}/Participate");
            if (resp.Error != null)
            {
                ShowNotification(resp.Error.ErrorDescription);
                return;
            }
        }
        #endregion

        #region Properties
       
        public RoomModel SelectedMyRoom
        {
            get { return _selectedMyRoom; }
            set
            {
                _selectedMyRoom = value;
                LoadMessagesAndFiles(value.Id);
                OnPropertyChanged("SelectedMyRoom");
            }
        }

        public string Downloads
        {
            get { return downloads; }
            set
            {
                downloads = value;
                OnPropertyChanged("Downloads");
            }
        }

        public string UserId
        {
            get { return _httpToken.UserId; }
        }

        public RoomModel SelectedAnotherRoom
        {
            get { return _selectedAnotherRoom; }
            set
            {
                _selectedAnotherRoom = value;                
                OnPropertyChanged("SelectedFromAllRoom");
            }
        }
               
        public MessageModel SelectedMessage
        {
            get { return _selectedMessage; }
            set
            {
                _selectedMessage = value;               
                OnPropertyChanged("SelectedMessage");
            }
        }

        public string NewRoomTitle
        {
            get { return newRoomTitle; }
            set
            {
                newRoomTitle = value;
                OnPropertyChanged("NewRoomTitle");
            }
        }

        public string MessageContent
        {
            get { return messageContent; }
            set
            {
                messageContent = value;
                OnPropertyChanged("MessageContent");
            }
        }

        #endregion

        #region Private methods
        private async void LoadRooms()
        {
            var resp = await _httpManager.GetAsync<IEnumerable<RoomDTO>>("api/Rooms/Other");
            if(resp.Error != null)
            {
                ShowNotification(resp.Error.ErrorDescription);
                return;
            }

            OtherRooms = new ObservableCollection<RoomModel>();
            foreach(var r in resp.Data)
            {
                OtherRooms.Insert(0,new RoomModel(r, await GetUserAsync(r.AdminId)));
            }
            OnPropertyChanged("OtherRooms");

            resp = await _httpManager.GetAsync<IEnumerable<RoomDTO>>("api/Rooms/My");
            if (resp.Error != null)
            {
                ShowNotification(resp.Error.ErrorDescription);
                return;
            }

            MyRooms = new ObservableCollection<RoomModel>();
            foreach (var r in resp.Data)
            {
                MyRooms.Insert(0,new RoomModel(r, await GetUserAsync(r.AdminId)));
            }
            OnPropertyChanged("MyRooms");           
        }        

        private async Task<UserProfile> GetUserAsync(Guid id)
        {
            if(CashedUsers.ContainsKey(id))
            {
                return CashedUsers[id];
            }

            var userResponse = await _httpManager.GetAsync<UserProfile>($"api/Account/Users/{id}");
            if (userResponse.Error != null)
            {
                ShowNotification(userResponse.Error.ErrorDescription);
                return null;
            }
            CashedUsers.TryAdd(userResponse.Data.Id, userResponse.Data);
            return userResponse.Data;
        }

        private async void LoadMessagesAndFiles(Guid roomId)
        {
            var resp = await _httpManager.GetAsync<IEnumerable<MessageDTO>>($"api/Rooms/{roomId.ToString()}/Messages");
            if (resp.Error != null)
            {
                ShowNotification(resp.Error.ErrorDescription);
                return;
            }

            Messages = new ObservableCollection<MessageModel>();
            foreach(var message in resp.Data)
            {
                var user = await GetUserAsync(message.SenderId);
                if(user == null)
                {
                    continue;
                }
                if(message.Type == MessageTypes.Text)
                {
                    Messages.Add(new MessageModel(message, user));
                }
                else if(message.Type == MessageTypes.File)
                {
                    var file = await GetFileAsync(new Guid(message.Content));  
                    if(file == null)
                    {
                        continue;
                    }
                    message.Content = GenerateContent(file);
                    Messages.Add(new MessageModel(message, user) { FileId = file.Id });
                }                
            }
            OnPropertyChanged("Messages");            
        }

        private async Task<FileDTO> GetFileAsync(Guid id)
        {
            var fileResponse = await _httpManager.GetAsync<FileDTO>($"api/Files/{id}");
            if (fileResponse.Error != null)
            {
                ShowNotification(fileResponse.Error.ErrorDescription);
                return null;
            }
            return fileResponse.Data;
        }
        #endregion

        private void ChangeData(Action action)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(
                    DispatcherPriority.Normal, action);
        }

        private void ShowNotification(string message)
        {
            var window = new NotificationWindow(message);
            window.ShowDialog();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
