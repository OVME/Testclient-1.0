using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Client.Annotations;

using Client.Model;
using Client.ThreadPool;

using GalaSoft.MvvmLight.CommandWpf;
using Request = Client.ThreadPool.Request;
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
        private CsvExport<ThreadPool.Request> export; 
        public  delegate void DynamicTesting();

        private DynamicTesting _testing;
        #endregion

        #region properties

        public bool Linear { get; set; }
        public bool Exponential { get; set; }
        public bool Random { get; set; }

        public int UpperLimit { get; set; }
        public int DownerLimit { get; set; }

        public int SendPeriod { get; set; }

        public int PeriodCount { get; set; }

        public ObservableCollection<TestData> TestDataCollection { get; set; }

        public ObservableCollection<Task> Tasks { get; set; }
        public ObservableCollection<ThreadPool.Request> Results { get; set; }

        public ObservableCollection<Id> Ids { get; set; }
        public ThreadPool.ThreadPoolServiceSoapClient client { get; set; }
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

        public DispatcherTimer TestingTimer { get; set; }



        #endregion

        #region commands

        private RelayCommand _addTasksCommand;
        private RelayCommand _exportCommand;
        private RelayCommand _getStatusCommand;
        private RelayCommand _getResultCommand;
        private RelayCommand _generateTasksCommand;
        private RelayCommand _addNewTaskToListCommand;
        private RelayCommand _startTestingCommand;
        private RelayCommand _stopTestingCommand;
        private RelayCommand _exportTestingCommand;

        public RelayCommand ExportTestDataCommand
        {
            get { return _exportTestingCommand ?? (_exportTestingCommand = new RelayCommand(ExportTestingData)); }
        }

        

        public RelayCommand StopTestingCommand
        {
            get { return _stopTestingCommand ?? (_stopTestingCommand = new RelayCommand(StopTesting)); }
        }

       

        public RelayCommand StartTestingCommand
        {
            get { return _startTestingCommand ?? (_startTestingCommand = new RelayCommand(StartTesting)); }
        }

        

        public RelayCommand AddTasksCommand
        {
            get { return _addTasksCommand ?? (_addTasksCommand = new RelayCommand(AddTasks)); }
        }

        public RelayCommand ExportCommand
        {
            get { return _exportCommand ?? (_exportCommand = new RelayCommand(Export)); }
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
            client = new ThreadPoolServiceSoapClient("ThreadPoolServiceSoap");
            Ids = new ObservableCollection<Id>();
            Results = new ObservableCollection<ThreadPool.Request>();
            Tasks = new ObservableCollection<Task>();
            StartHandlingMessage = "";
            TestDataCollection = new ObservableCollection<TestData>();
            SendPeriod = 1;
            PeriodCount = 0;
            _testing = delegate
            {
                if (PeriodCount%SendPeriod == 0)
                {
                    AddTasks();
                }
                var data = client.GetProcTaskData();
                TestDataCollection.Add(new TestData() { ProcCount = data.Split(',')[0], TaskCount = data.Split(',')[1] });
                PeriodCount++;
            }; 
            
 
            
        }

        #region methods

        private void StopTesting()
        {
            if(TestingTimer!=null)
            TestingTimer.Stop();
            TestingTimer = null;
        }
        private void StartTesting()
        {
           TestingTimer = new DispatcherTimer(){Interval = new TimeSpan(0,0,5)};

           TestingTimer.Tick += delegate(object sender, EventArgs args)
           {
               if (PeriodCount % SendPeriod == 0)
               {
                   AddTasks();
               }
               var data = client.GetProcTaskData();
               TestDataCollection.Add(new TestData() { ProcCount = data.Split(',')[0], TaskCount = data.Split(',')[1] });
               PeriodCount++;
           };
            TestingTimer.Start();

        }

        private void ExportTestingData()
        {
            var exp = new CsvExport<TestData>(TestDataCollection.ToList());
            exp.ExportToFile("DynamicTest"+DateTime.Now.ToString().Replace(".", "").Replace(" ", "").Replace(":", "") + ".csv");
        }

        public void AddTasks()
        {
            Ids.Clear();
            
                foreach (var t in Tasks)
                    Ids.Add(new Id(){s = client.GetRId(t.s)});
            BeginId = Int32.Parse(Ids.First().s);
            StopId = Int32.Parse(Ids.Last().s);
        }

        public void GetStatus()
        {
            try
            {
                
                Status = client.Status(StatusId);
               
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
                   
                    Results.Add(client.GetFullRequest(i));
                    
                }
                catch (FaultException ex)
                {
                    Results.Add(new ThreadPool.Request(){ErrorData = ex.Message});
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
            //rnd.NextBytes(bytes);
            if(Random)
            for (int i = 0; i < Amount; i++)
            {
                Tasks.Add(new Task()
                {
                    s = rnd.Next(DownerLimit, UpperLimit).ToString()
                });
            }
            if(Linear)
                for (int i = 0; i < Amount; i++)
                {
                    Tasks.Add(new Task() { s = DownerLimit + i * (UpperLimit - DownerLimit) / Amount < UpperLimit ? (DownerLimit + i * (UpperLimit - DownerLimit) / Amount).ToString() : UpperLimit.ToString() });
                }
            if (Exponential)
            {
                int a=1,j=0;
                for(int i = 0; a < DownerLimit;i++)
                {
                    a = (int)(Math.Pow(2, i));
                    j = i;
                }
                for (int i = 0; i < Amount; i++)
                {
                    Tasks.Add(new Task() { s = Math.Pow(2, j + i) < UpperLimit ? Math.Pow(2, j + i).ToString() : UpperLimit.ToString() });
                }



            }
                

         }
        


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }


        public void Export()
        {
            export = new CsvExport<Request>(Results.ToList());
            try
            {
                export.ExportToFile(DateTime.Now.ToString().Replace(".","").Replace(" ","").Replace(":","") + ".csv");
            }
            catch (Exception e)
            {
                MessageBox.Show("Экспорт не удался");
            }
        }

        #endregion
    }
}
