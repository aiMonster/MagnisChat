using Common.DTO.Account;
using Common.DTO.Messages;
using Common.DTO.Rooms;
using Common.DTO.Sockets;
using Common.Enums;
using MagnisChatWPF.Interfaces;
using MagnisChatWPF.Managers;
using MagnisChatWPF.Views;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;
using WebSocketSharp;

namespace MagnisChatWPF.ViewModels
{
    public class MainWindowViewModel
    {        
        [Import(typeof(ISocketManager))]
        private ISocketManager _socketManager;       

        private TokenResponse _httpToken;

        private AuthorizationUserControl _authorizationUserControl = new AuthorizationUserControl();
        private AuthorizationViewModel _authorizationViewModel;

        private RoomsUserControl _roomsUserContorl = new RoomsUserControl();
        private RoomsViewModel _roomsViewModel;

        private readonly ContentControl _contentControl;

        public MainWindowViewModel(ContentControl contentControl)
        {
            BuildImports();
            _contentControl = contentControl;   
            InitializeAuthorizationControl();     
        }

        private void BuildImports()
        {
            var catalog = new AssemblyCatalog(this.GetType().Assembly);
            var container = new CompositionContainer(catalog);
            container.ComposeParts(this);
        }
        
        private void InitializeSockets()
        {
            _socketManager.Connect();
            _socketManager.Authorized += () => OnAuthorized();
            _socketManager.RoomCreated += (sr) => _roomsViewModel.OnRoomCreated(sr.Model);
            _socketManager.NewMessageReceived += (sr) => _roomsViewModel.OnMessageReceived(sr.Model);            
            _socketManager.RoomParticipated += (sr) => _roomsViewModel.OnRoomParticipated(sr.Model);
            _socketManager.FileStatusChanged += (sr) => _roomsViewModel.OnFileStatusChanged(sr.Model);
        }

        //this method is called by webSockets
        private void OnAuthorized()
        {            
            System.Windows.Application.Current.Dispatcher.Invoke(
                        DispatcherPriority.Normal, (Action)delegate
                        {
                            _roomsViewModel = new RoomsViewModel(_httpToken);
                            _roomsUserContorl.DataContext = _roomsViewModel;
                            _roomsViewModel.LogOut += () => OnLogOut();
                            _contentControl.Content = _roomsUserContorl;
                        });
        }       

        private void OnLogOut()
        {            
            Properties.Settings.Default["Token"] = "";
            Properties.Settings.Default.Save();            
            _authorizationViewModel = null;
            _authorizationUserControl = new AuthorizationUserControl();
            _roomsViewModel = null;
            _roomsUserContorl = new RoomsUserControl();
            InitializeAuthorizationControl();
        } 

        private void InitializeAuthorizationControl()
        {
             _authorizationViewModel = new AuthorizationViewModel();  
             _authorizationViewModel.Authorized += (tr, st) => ConnectToWS(tr, st);
             _authorizationUserControl.DataContext = _authorizationViewModel;   
            
             System.Windows.Application.Current.Dispatcher.Invoke(
                        DispatcherPriority.Normal, (Action)delegate
                        {
                            _contentControl.Content = _authorizationUserControl;
                        });

            _authorizationViewModel.CheckIsAuthorized();
        }       

        private async void ConnectToWS(TokenResponse tokenResponse, Guid socketToken)
        {   
            InitializeSockets();
            _httpToken = tokenResponse; 
            _socketManager.Login(socketToken);                      
        }
    }
}
