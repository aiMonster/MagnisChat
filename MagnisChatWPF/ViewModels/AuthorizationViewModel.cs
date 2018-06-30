using Common.DTO.Account;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using System.Windows.Controls;
using System.Threading;
using MagnisChatWPF.Managers;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition;
using MagnisChatWPF.Interfaces;
using Newtonsoft.Json;
using Common.DTO.Sockets;

namespace MagnisChatWPF.ViewModels
{
    public class AuthorizationViewModel : INotifyPropertyChanged
    {
        [Import(typeof(IHttpManager))]
        private IHttpManager _httpManager;

        private string login;
        private string errorLabel;
        private bool isActive;
        private bool rememberMe;   

        public event Action<TokenResponse, Guid> Authorized;

        public ICommand AuthorizeCommand { get; protected set; }   
        public ICommand RegisterCommand { get; protected set; }

        public AuthorizationViewModel()
        {
            BuildImports();
            AuthorizeCommand = new Command(obj => Authorize(obj));
            RegisterCommand = new Command(obj => Register(obj));
            IsActive = true;           
        }

        private void BuildImports()
        {
            var catalog = new AssemblyCatalog(this.GetType().Assembly);
            var container = new CompositionContainer(catalog);
            container.ComposeParts(this);
        }

        public async void CheckIsAuthorized()
        {     
            var token = Properties.Settings.Default["Token"].ToString();
            if (String.IsNullOrEmpty(token))
            {
                return;
            }

            //blocking btn Sign in
            IsActive = false;

            var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(token);
            Login = tokenResponse.UserLogin;
            ErrorLabel = "Logining ...";

            _httpManager.Authorize(tokenResponse.Token);
            var response = await _httpManager.PostAsync<SocketTokenDTO>("api/Account/SocketToken");
            if(response.Error != null)
            {
                IsActive = true;

                if (response.Error.ErrorCode == 401)
                {
                    ErrorLabel = "You have to enter you password again";
                    Properties.Settings.Default["Token"] = "";
                    Properties.Settings.Default.Save();
                    return;
                }
                else
                {
                    ErrorLabel = response.Error.ErrorDescription;
                    return;
                }
            }

            Authorized(tokenResponse, response.Data.Id);
            //unblocking btn Sign in
            IsActive = true;
        }       

        private async void Authorize(object parameter)
        {
            ErrorLabel = "";
            var passwordBox = parameter as PasswordBox;
            var password = passwordBox.Password;

            if (String.IsNullOrEmpty(login) || String.IsNullOrEmpty(password))
            {
                ErrorLabel = "Fill all fields!";
                return;
            }

            //blocking btn Sign in
            IsActive = false; 
            
            var response = await _httpManager.PostAsync<TokenResponse>("api/Account/Token",
                new LoginRequest() { Login = login, Password = password});

            if(response.Error != null)
            {
                ErrorLabel = response.Error.ErrorDescription;
                IsActive = true;
                return;
            }          
            
            if(RememberMe)
            {
                Properties.Settings.Default["Token"] = JsonConvert.SerializeObject(response.Data);
                Properties.Settings.Default.Save();
            }

            _httpManager.Authorize(response.Data.Token);
            var socketResponse = await _httpManager.PostAsync<SocketTokenDTO>("api/Account/SocketToken");
            if(socketResponse.Error != null)
            {
                ErrorLabel = socketResponse.Error.ErrorDescription;
                IsActive = true;
                return;
            }

            Authorized(response.Data, socketResponse.Data.Id);
            //unblocking btn Sign in
            IsActive = true;            
        }

        private async void Register(object parameter)
        {
            ErrorLabel = "";
            var passwordBox = parameter as PasswordBox;
            var password = passwordBox.Password;

            if (String.IsNullOrEmpty(login) || String.IsNullOrEmpty(password))
            {
                ErrorLabel = "Fill all fields!";
                return;
            }

            //blocking btn Sign in
            IsActive = false;

            var response = await _httpManager.PostAsync<bool>("api/Account/Register",
                new LoginRequest() { Login = login, Password = password });

            if (response.Error != null)
            {
                ErrorLabel = response.Error.ErrorDescription;
                IsActive = true;
                return;
            }

            if (response.Data != true)
            {
                ErrorLabel = "Unknown error, try again";
                IsActive = true;
                return;
            }

            ErrorLabel = "You've been successfully registered";
            passwordBox.Password = "";
            //unblocking btn Sign in
            IsActive = true;
        }
        #region Properties

        public bool RememberMe
        {
            get { return rememberMe; }
            set
            {
                if (rememberMe != value)
                {
                    rememberMe = value;
                    OnPropertyChanged("RememberMe");
                }
            }
        }

        public string Login
        {
            get { return login; }
            set
            {
                if (login != value)
                {
                    login = value;
                    OnPropertyChanged("Login");
                }
            }
        }

        public string ErrorLabel
        {
            get { return errorLabel; }
            set
            {
                if (errorLabel != value)
                {
                    errorLabel = value;
                    OnPropertyChanged("ErrorLabel");
                }
            }
        }

        public bool IsActive
        {
            get { return isActive; }
            set
            {
                if (isActive != value)
                {
                    isActive = value;
                    OnPropertyChanged("IsActive");
                }
            }
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
