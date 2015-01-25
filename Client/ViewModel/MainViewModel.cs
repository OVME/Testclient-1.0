using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Client.Annotations;
using Client.CombinedTP;
using Client.Model;
using Client.TP4;
using GalaSoft.MvvmLight.CommandWpf;
using Task = Client.Model.Task;


namespace Client.ViewModel 
{
    class MainViewModel : INotifyPropertyChanged
    {

        #region fields

        private string _status;
       
        private long _statusId;
        private long _stopId;
        private long _beginId;
        private int _amount;
        private int _taskVal;
        private string _startHandlingMessage;

        #endregion

        #region properties

        public bool IsCombTP { get; set; }
        public bool IsTP4 { get; set; }

        public ObservableCollection<Task> Tasks { get; set; }
        public ObservableCollection<Result> Results { get; set; }

        public ObservableCollection<Id> Ids { get; set; }
        public CombinedTP.CombinedServiceSoapClient client { get; set; }
        public ThreadPoolSeviceSoapClient tpclient { get; set; }
        public string StartHandlingMessage
        {
            get { return _startHandlingMessage; }
            set
            {
                _startHandlingMessage = value;
                OnPropertyChanged("StartHandlingMessage");
            }
        }
        

        public string Status
        {
            get { return _status; }
            set
            {
                _status = value;
                OnPropertyChanged("Status");
            }
        }

        public long StatusId
        {
            get { return _statusId; }
            set
            {
                _statusId = value;
                OnPropertyChanged("StatusId");
            }
        }



        public long StopId
        {
            get { return _stopId; }
            set
            {
                _stopId = value;
                OnPropertyChanged("StopId");
            }
        }

        public long BeginId
        {
            get { return _beginId; }
            set
            {
                _beginId = value;
                OnPropertyChanged("BeginId");
            }
        }

        public int Amount
        {
            get { return _amount; }
            set
            {
                _amount = value;
                OnPropertyChanged("Amount");
            }
        }


        public int TaskVal
        {
            get { return _taskVal; }
            set
            {
                _taskVal = value;
                OnPropertyChanged("TaskVal");
            }
        }



        #endregion

        #region commands

        private RelayCommand _addTasksCommand;
        private RelayCommand _getStatusCommand;
        private RelayCommand _getResultCommand;
        private RelayCommand _generateTasksCommand;
        private RelayCommand _addNewTaskToListCommand;

        public RelayCommand AddTasksCommand
        {
            get { return _addTasksCommand ?? (_addTasksCommand = new RelayCommand(AddTasks)); }
        }

        public RelayCommand GetStatusCommand
        {
            get { return _getStatusCommand ?? (_getStatusCommand = new RelayCommand(GetStatus)); }
        }

        public RelayCommand GetResultCommand
        {
            get { return _getResultCommand ?? (_getResultCommand = new RelayCommand(GetResults)); }
        }

 

        public RelayCommand GenerateTasksCommand
        {
            get { return _generateTasksCommand ?? (_generateTasksCommand = new RelayCommand(Generate)); }
        }

        public RelayCommand AddNewTaskToListCommand
        {
            get { return _addNewTaskToListCommand ?? (_addNewTaskToListCommand = new RelayCommand(AddNewTaskToList)); }
        }

        #endregion

        public MainViewModel()
        {
            client = new CombinedServiceSoapClient("CombinedServiceSoap");
            tpclient = new ThreadPoolSeviceSoapClient("ThreadPoolSeviceSoap");
            Ids = new ObservableCollection<Id>();
            Results = new ObservableCollection<Result>();
            Tasks = new ObservableCollection<Task>();
            StartHandlingMessage = "";
        }

        #region methods

        public void AddTasks()
        {
            Ids.Clear();
            if(IsCombTP)
                foreach (var t in Tasks)
                    Ids.Add(new Id(){s = client.GetRId(t.s)});
            if(IsTP4)
                foreach (var t in Tasks)
                    Ids.Add(new Id() { s = tpclient.GetRId(t.s) });
        }

        public void GetStatus()
        {
            try
            {
                if (IsCombTP)
                Status = client.Status(StatusId);
                if(IsTP4)
                    Status = tpclient.Status(StatusId);
            }
            catch (FaultException ex)
            {

                Status = ex.Message;
            }
            
        }


        public void GetResults()
        {
            Results.Clear();
            for (long i = BeginId; i < StopId+1; i++)
            {
                try
                {
                    if(IsCombTP)
                    Results.Add(new Result() {s = client.Result(i)});
                    if (IsTP4)
                    {
                        Results.Add(new Result() { s = tpclient.Result(i) });
                    }
                }
                catch (FaultException ex)
                {
                    Results.Add(new Result() { s = ex.Message });
                }
                
            }
            
        }

        public void AddNewTaskToList()
        {
            Tasks.Add(new Task() { s = TaskVal.ToString()});
        }

        

        public void Generate()
        {
            Tasks.Clear();
            byte[] bytes = new byte[Amount];
            var rnd = new Random();
            rnd.NextBytes(bytes);
            for (int i = 0; i < Amount; i++)
            {
                Tasks.Add(new Task()
                {
                    s = ((int) bytes[i]*100).ToString()
                });
            }
         }
        


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
