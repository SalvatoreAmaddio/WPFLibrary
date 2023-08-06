using Designer;
using MvvmHelpers.Commands;
using SARWPF;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SAR
{
    public abstract class AbstractDataController<M> : AbstractNotifier, IAbstractController where M : class, IDB<M>, IAbstractModel, new()
    {
        M? _selectedrecord;
        public M? SelectedRecord { get => _selectedrecord; set => Set<M?>(ref value, ref _selectedrecord, nameof(SelectedRecord)); }
        public IRecordSource DataSource { get; set; }
        public virtual IDB DB { get; }
        public IRecordSource MainSource => DB.DataSource;
        string _search = string.Empty;
        public string Search { get => _search; set => Set<string>(ref value, ref _search, nameof(Search)); }
        public ICommand SaveCMD { get; }
        public ICommand DeleteCMD { get; }
        bool _isloading=false;
        public bool IsLoading { get => _isloading; set => Set<bool>(ref value, ref _isloading,nameof(IsLoading)); }

        public Type ModelType
        {
            get
            {
                SelectedRecord ??= new();
                return SelectedRecord.GetType();
            }
        }

        public object? CurrentRecord { get => SelectedRecord; }

        public AbstractDataController()
        {
            DB =Sys.DatabaseManager.GetDatabaseTable<M>();
            DataSource = new RecordSource<M>(MainSource, true, ToString());
            DataSource.OnRecordMoved += OnRecordMoved;
            DataSource.GoFirst();
            SelectedRecord = DataSource.CurrentRecordAs<M>();
            SaveCMD = new Commando(Save);
            DeleteCMD = new Commando(Delete);
        }

        public RecordSource<M> GetRecordSource() => (RecordSource<M>)DataSource;

        public virtual void OnRecordMoved(object? sender, RecordMovedArgs e)=>            
        SelectedRecord = (M?)e.Record;

        public virtual bool Save(IAbstractModel? record)
        {
            if (record==null || !record.CanSave()) return false;
            SelectedRecord = null;
            DB.OpenConnection();
            if (record.IsNewRecord)
            DB.Insert(record);
            else
            DB.Update(record);            
            DB.CloseConnection();
            SelectedRecord = (M?)record;
            return true;
        }

        public virtual bool Delete(IAbstractModel? record)
        {
            if (record == null) return false;
            if (!DeleteActionConfirmed()) return false;
            SelectedRecord = null;
            DB.OpenConnection();
            DB.Delete(record);
            DB.CloseConnection();
            //SelectedRecord = (M?)DB.DataSource.CurrentRecord;
            return true;
        }
        
        public bool DeleteActionConfirmed()
        {
            DeleteDialog deleteMesssageDialog = new();
            deleteMesssageDialog.ShowDialog();
            return deleteMesssageDialog.Response.Equals(DialogResponse.YES);
        }

        public override void Set<T>(ref T value, ref T _backprop, string propName)
        {
            PropChangedEventArgs prop = new(value, _backprop, propName);

            if (propName.Equals(nameof(SelectedRecord)))
            {
                DataSource?.SetCurrentRecordPosition(value);
            }

            InvokeBeforeUpdate(prop);

            if (prop.Cancel) return;
            _backprop = value;

            InvokeAfterUpdate(prop);
            NotifyView(propName);
        }
        public void SetSelectedRecord(object? record) => SelectedRecord = (M?)record;
        public override string ToString() => $"DataController<{ModelType.Name}>";

    }

    public interface IAbstractController : IAbstractNotifier
    {
        public abstract bool Save(IAbstractModel? record);
        public abstract bool Delete(IAbstractModel? record);
        public Type ModelType { get; }
        public abstract IDB DB { get; }
        public IRecordSource DataSource { get; set; }
        public IRecordSource MainSource { get; }
        public object? CurrentRecord { get; }
        public void SetSelectedRecord(object? record);
    }

    public class Commando : ICommand
    {
        readonly Func<IAbstractModel?, bool> _execute;
        
        public Commando(Func<IAbstractModel?,bool> execute) =>_execute = execute;

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public async void Execute(object? parameter) 
        {
            var t= InternetConnection.IsAvailableTask();

            IAbstractModel? model=null;

            if (parameter is Button)
                model = (IAbstractModel)((Button)parameter).DataContext;

            if (parameter is IAbstractModel)
                model = (IAbstractModel?)parameter;
            
            if (parameter is IAbstractController)
                model = (IAbstractModel?)((IAbstractController?)parameter).CurrentRecord;

            await t;
            if (!t.Result)
            {
                
                return;
            }
            _execute(model);
        } 
    }
}
