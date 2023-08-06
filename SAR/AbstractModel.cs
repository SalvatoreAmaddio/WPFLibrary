using MySqlConnector;
using SARWPF;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SAR
{
    public abstract class AbstractUser : AbstractModel
    {
        int _userid;
        public int UserID { get=>_userid; set=>Set<int>(ref value,ref _userid,nameof(UserID)); }

        string _username=string.Empty;
        public string UserName { get => _username; set => Set<string>(ref value, ref _username,nameof(UserName)); }

        string _password=string.Empty;
        public string Password { get => _password; set => Set<string>(ref value, ref _password,nameof(Password)); }

        public override int ObjectHashCode => HashCode.Combine(UserID);

        public override bool IsEqualTo(object? obj)
        {
            if (obj == null) return false;
            AbstractUser user = (AbstractUser)obj;
            return user.UserID == UserID;
        }

        public override string ObjectName => UserName;
    }

    public abstract class AbstractModel : AbstractNotifier, IAbstractModel
    {
        bool _isdirty = false;
        public bool IsDirty { get=>_isdirty; set=>Set<bool>(ref value, ref _isdirty,nameof(IsDirty)); }

        public abstract bool IsNewRecord { get; }

        public override void Set<T>(ref T value, ref T _backprop, string propName)
        {
            base.Set(ref value,ref _backprop,propName);
            if (propName.Equals(nameof(IsDirty))) return;
            IsDirty = true;
        }
    
        public abstract string ObjectName { get; }
        public abstract int ObjectHashCode { get; }

        public abstract bool IsEqualTo(object? obj);

        public override string ToString() => ObjectName;
        public override int GetHashCode() => ObjectHashCode;
        public override bool Equals(object? obj) => IsEqualTo(obj);
        public abstract bool CanSave();
    }

    public class PropChangedEventArgs : EventArgs
    {
        public object? NewValue { get; set; }
        public object? OldValue { get; set; }
        public string PropName { get; set; } = string.Empty;
        public bool Cancel { get; set; } = false;

        public PropChangedEventArgs() { }

        public PropChangedEventArgs(object? newValue, object? oldValue, string propName)
        {
            NewValue = newValue;
            OldValue = oldValue;
            PropName = propName;
        }

        public override int GetHashCode()=>HashCode.Combine(PropName);
        public T? GetNewValue<T>() => (T?)NewValue;
        public T? GetOldValue<T>() => (T?)OldValue;
        public bool PropIs(string prop)=>PropName.Equals(prop);

        public override string ToString() => PropName;

        public override bool Equals(object? obj) =>
        obj is PropChangedEventArgs args &&
        PropName == args.PropName;
    }

    public abstract class AbstractNotifier : IAbstractNotifier
    {
        public event EventHandler<PropChangedEventArgs>? AfterUpdate;
        public event EventHandler<PropChangedEventArgs>? BeforeUpdate;
        public event PropertyChangedEventHandler? PropertyChanged;

        public ValuesList Values = new();

        public virtual void Set<T>(ref T value, ref T _backprop, string propName)
        {
            PropChangedEventArgs prop = new(value, _backprop, propName);
            BeforeUpdate?.Invoke(this, prop);
            if (prop.Cancel) return;
            _backprop = value;
            AfterUpdate?.Invoke(this, prop);
            NotifyView(propName);
        }

        public void NotifyView(string propname)=>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));

        public void InvokeBeforeUpdate(PropChangedEventArgs e) =>
        BeforeUpdate?.Invoke(this, e);

        public void InvokeAfterUpdate(PropChangedEventArgs e)=>
        AfterUpdate?.Invoke(this, e);
    }

    public class ValuesList : List<PropChangedEventArgs>
    {
        public void AddValue(PropChangedEventArgs value)
        {
            var exist = this.Any(s => s.Equals(value));
            if (exist) return;
            this.Add(value);
        }

        public PropChangedEventArgs Get(string propname) => this.First(s => s.PropIs(propname));
    }

    public interface IAbstractModel : IAbstractNotifier
    {
        public bool IsDirty { get; set; }
        public abstract bool IsNewRecord { get; }
        public abstract string ObjectName { get; }
        public abstract int ObjectHashCode { get; }
        public abstract bool IsEqualTo(object? obj);
        public abstract bool CanSave();
    }

    public interface IAbstractNotifier : INotifyPropertyChanged
    {
        public abstract void Set<T>(ref T value, ref T _backprop, string propName);
        public abstract void NotifyView(string propname);
        public event EventHandler<PropChangedEventArgs>? AfterUpdate;
        public event EventHandler<PropChangedEventArgs>? BeforeUpdate;
    }

    public interface IDB<M> where M : IAbstractModel, new()
    {
        public M GetRecord(DbDataReader reader);
        public string SQLStatements(SQLType _sqlType);
        public Task SetForeignKeys();
        public void Params(DbParameterCollection parameters);
        public void SetPrimaryKey(Int64 id);
    }

    public enum SQLType
    {
        SELECT,
        UPDATE,
        INSERT,
        DELETE,
    }

}
