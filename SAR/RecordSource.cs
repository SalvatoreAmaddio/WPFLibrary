using MvvmHelpers;
using Recordsource.Model;
using SARWPF;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Xml.Linq;

namespace SAR
{
    public class RecordSource<M> : ObservableRangeCollection<M>, IRecordSource where M : class, IAbstractModel,new()
    {
        #region Properties
        M model= new();
        public object CurrentRecord => model;
        public int RecordCount { get=>this.Count; }
        public int CurrentRecordPosition { get=>RecordIndex+1; }
        int RecordIndex { get => IndexOf(model); }
        public List<IRecordSource> Children { get; set; } = new();
        public string SourceName { get; set; } = string.Empty;
        public bool AllowNewRecord { get; set; } = true;
        public bool NoRecords => RecordCount==0;
        public bool IsEOF => CurrentRecordPosition >= RecordCount;
        public bool IsBOF => CurrentRecordPosition <= 1;
        public bool IsNewRecord => model.IsNewRecord;
        public event EventHandler<RecordMovedArgs>? OnRecordMoved;
        IRecordSource? OriginalSource;
        public object? FilterContext { get; set; }
        SourcePosition _sourcePosition;
        public SourcePosition SourcePosition => DetermineSourcePosition();

        public string Records
        {
            get
            {
                DetermineSourcePosition();
                if (SourcePosition.Equals(SourcePosition.NEW_RECORD)) return "NEW RECORD";
                if (SourcePosition.Equals(SourcePosition.NO_RECORDS)) return "NO RECORDS";
                return $"Record {CurrentRecordPosition} of {RecordCount}";
            }
        }

        public T? GetFilterContextAs<T>() => (T?)FilterContext;

        public AbstractListRestructurer? AbstractListRestructurer
        {
            get;
            set;
        }

        string _datasetbasedon=string.Empty;
        public string DataSetBasedOn
        {
            get => $"DataSet Based on: {_datasetbasedon}";
            set=>_datasetbasedon = value;
        }

        string _origin = string.Empty;
        public string Origin
        {
            get => $"Origin: {_origin}";
            set=>_origin = value;
        }
        #endregion

        #region Constructors
        public RecordSource()=>
        SourceName = $"RecordSource<{ModelType.Name}>()";

        public RecordSource(IEnumerable<M> source) : base(source)=>
        SourceName = $"RecordSource<{ModelType.Name}>()";

        public RecordSource(IRecordSource source, bool ischild = false, string origin="") : base(source.Cast<M>())
        {
            SourceName = $"RecordSource<{ModelType.Name}>()";
            Origin = origin;
            if (ischild)
            {
                SourceName = $"Child of {source.SourceName} - RecordSource<{ModelType.Name}>()";
                source.AddChild(this);
            }
        }
        #endregion

        public void SetMainSource(IRecordSource mainsource) =>OriginalSource = mainsource;
        public IRecordSource CopyMe(bool IsChildSource = false) => new RecordSource<M>(this, IsChildSource);

        public void AddChild(IRecordSource child) 
        {
            child.SetMainSource(this);
            Children.Add(child);
        }

        public IRecordSource? GetChild(IRecordSource source)
        {
            foreach (var child in Children)
            {
                bool criteria = 
                    child.SourceName.Equals(source.SourceName)
                    && child.Origin.Equals(source.Origin)
                    && child.DataSetBasedOn.Equals(source.DataSetBasedOn);
                if (criteria) return child;
            }

            return null;
        }
        
        public bool Exists(object record) => this.Any(s => s.IsEqualTo(record));

        #region CRUD
        public void Empty() => this.Clear();

        public void AddRecord(object record)
        {
            var x = Origin + " " + DataSetBasedOn;
            if (this.Any(s => s.IsEqualTo(record))) return;
            model = (M)record;
            Add(model);
            AbstractListRestructurer?.SetDataContext(this.FilterContext);
            AbstractListRestructurer?.Run();
            AbstractListRestructurer?.Selector?.Semaforo.TriggerEvent();

            foreach (var child in Children.ToList())
            {
                child.AddRecord(record);
            }
        }

        public void UpdateRecord(object record)
        {
            model = (M)record;
            AbstractListRestructurer?.SetDataContext(this.FilterContext);
            AbstractListRestructurer?.Run();
            AbstractListRestructurer?.Selector?.Semaforo.TriggerEvent();
            int index =this.IndexOf(model);
            if (index < 0) return;
            this[index] = model;

            foreach (var child in Children.ToList())
            {
               child.UpdateRecord(record);
            }
        }

        void RecalculateIndexBackward(int index)
        {
            RecordMovement move = RecordMovement.PREVIOUS;

            if (index == 0 && RecordCount == 0)
            {
                model = new();
                goto IS_NEW_RECORD;
            }

            index--;
            if (index < 0 && RecordCount > 0)
            {
                index = 0;
                move = RecordMovement.NEXT;
            }
            
            model = (index >= 0) ? this[index] : new();

            IS_NEW_RECORD:
            if (model.IsNewRecord)
                move = RecordMovement.NEW;

            OnPropertyChanged(new(nameof(Records)));
            OnRecordMoved?.Invoke(this, new() { Record = model, Movement = move });
        }

        void RecalculateIndexForward(int index)
        {
            RecordMovement move = RecordMovement.NEXT;

            if (index==0 && RecordCount==0)
            {
                model = new();
                goto IS_NEW_RECORD;
            } 

            index = (index==0) ? 0 : index++;
            var MaxIndex = RecordCount - 1;

            if (index > MaxIndex)
            {
                index=MaxIndex;
                move = RecordMovement.PREVIOUS;
            }

            model = (index >= 0) ? this[index] : new();
    
            IS_NEW_RECORD:
            if (model.IsNewRecord)
                move = RecordMovement.NEW;

            OnPropertyChanged(new(nameof(Records)));
            OnRecordMoved?.Invoke(this, new() { Record = model, Movement = move });
        }

        public void DeleteRecord(object record) {
            M m = (M)record;
            int index = IndexOf(m);
            Remove(m);
            RecalculateIndexForward(index);
            foreach (var child in Children.ToList())
            {
                child.DeleteRecord(record);
            }

        }

        public void ReplaceDataSource(IEnumerable? source)
        {
            if (source == null) return;
            Clear();
            ReplaceRange(source.Cast<M>());
            model = this.FirstOrDefault() ?? new();
        }
        #endregion

        public Type ModelType { get=>model.GetType(); }

        #region ToStringEqualsHashCode
        public override string ToString() => SourceName;
        public override bool Equals(object? obj)
        {
            return obj is RecordSource<M> source &&
                   SourcePosition == source.SourcePosition &&
                   DataSetBasedOn == source.DataSetBasedOn &&
                   Origin == source.Origin;
        }

        public override int GetHashCode()=>HashCode.Combine(SourcePosition, Origin,DataSetBasedOn);
        #endregion

        #region RecordMovement
        SourcePosition DetermineSourcePosition()
        {
            _sourcePosition = SourcePosition.SLIDING;
            if (IsEOF) _sourcePosition = SourcePosition.EOF;
            if (IsBOF) _sourcePosition = SourcePosition.BOF;
            if (IsNewRecord && !NoRecords) _sourcePosition = SourcePosition.NEW_RECORD;
            if (IsNewRecord && NoRecords) _sourcePosition = SourcePosition.NO_RECORDS;
            return _sourcePosition;
        }

        public string CurrentRecordInfo() =>$"Position:{CurrentRecordPosition} - Status:{SourcePosition}";

        public TSource CurrentRecordAs<TSource>() where TSource : class, IAbstractModel, new() => (TSource)CurrentRecord;

        public void SetCurrentRecordPosition(object? value)
        { 
            model = (value == null) ? new() : (M)value;
            OnPropertyChanged(new(nameof(Records)));
        }

        public string SourceStatus() =>$"Records: {RecordCount} - Children: {Children.Count} - CurrentRecord: {CurrentRecord} - {CurrentRecordInfo()}";

        public bool GoFirst()
        {
            if (!CanMoveFirstLast()) return false;
            model = this.First();
            OnPropertyChanged(new(nameof(Records)));
            OnRecordMoved?.Invoke(this, new() { Record = model, Movement = RecordMovement.FIRST });
            return true;
        }

        public bool GoLast()
        {
            if (!CanMoveFirstLast()) return false;
            model = this.Last();
            OnPropertyChanged(new(nameof(Records)));
            OnRecordMoved?.Invoke(this, new() { Record = model, Movement = RecordMovement.LAST });
            return true;
        }

        public bool GoPrevious()
        {
            if (IsNewRecord) 
            {
                Remove(model);
                return GoLast();
            }
            if (!CanMovePrevious()) return false;
            int previousIndex = RecordIndex - 1;
            model = this[previousIndex];
            OnPropertyChanged(new(nameof(Records)));
            OnRecordMoved?.Invoke(this, new() { Record = model, Movement = RecordMovement.PREVIOUS });
            return true;
        }

        public bool GoNext()
        {
            if (IsNewRecord) return false;
            if (!CanMoveNext() && !AllowNewRecord) return false;
            int nextIndex = RecordIndex + 1;
            if (nextIndex > IndexOf(this.Last())) return GoNewRecord();
            model = this[nextIndex];
            OnRecordMoved?.Invoke(this, new() { Record = model, Movement = RecordMovement.NEXT });
            OnPropertyChanged(new(nameof(Records)));
            return true;
        }

        bool CanMoveNext()
        {
            if (IsBOF && !IsEOF) return true;
            if (!IsBOF && IsEOF) return false;
            if (IsBOF && IsEOF) return false;
            if (!IsBOF && !IsEOF) return true;
            return false;
        }

        bool CanMovePrevious()
        {
            if (IsBOF && !IsEOF) return false;
            if (!IsBOF && IsEOF) return true;
            if (IsBOF && IsEOF) return false;
            if (!IsBOF && !IsEOF) return true;
            return false;
        }

        bool CanMoveFirstLast()
        {
            if (IsBOF && !IsEOF) return true;
            if (!IsBOF && IsEOF) return true;
            if (IsBOF && IsEOF) return false;
            if (!IsBOF && !IsEOF) return true;
            return false;
        }

        public bool GoNewRecord()
        {
            if (!AllowNewRecord) return false;
            model = new();
            Add(model);
            OnPropertyChanged(new(nameof(Records)));
            OnRecordMoved?.Invoke(this, new() { Record = model, Movement = RecordMovement.NEW });
            return true;
        }
        #endregion

        public void Print(int limit = -1)
        {
            foreach (var record in this)
            {
                Console.WriteLine(record);
            }
        }

        IEnumerator IRecordSource.GetEnumerator() => this.GetEnumerator();

        public IRecordSource TakeRows(int rows)=> this.Take(rows).AsIRecordSource();
        public IRecordSource FilterSource<TSource>(Func<TSource, bool> predicate) where TSource : class, IAbstractModel, new()
        {
            Func<M, bool> predicate2= (Func<M, bool>)predicate;
            var source = this.Where(predicate2).AsIRecordSource();
            source.SourceName = this.SourceName;
            source.Origin= this.Origin;
            source.DataSetBasedOn = this.DataSetBasedOn;
            return source;
        }
        public IEnumerable OrderSource<TSource,TKey>(Func<TSource, TKey> keyselector, SourceOrderWay orderway = SourceOrderWay.ASC) where TSource : class, IAbstractModel, new()
        {
            Func<M, TKey> keyselector2 = (Func<M, TKey>)keyselector;
            if (orderway.Equals(SourceOrderWay.ASC))
            {
                return this.OrderBy(keyselector2);
            }
            return this.OrderByDescending(keyselector2);
        }
        public void RemoveFilters() => ReplaceDataSource(OriginalSource);
    }

    public abstract class AbstractListRestructurer
    {
        public ISelector? Selector;
        object DataContext=new();
        public abstract IRecordSource? MainSource { get; }

        public IRecordSource? NewFilteredSource { get; set; }
        protected IRecordSource? FilteredSource;

        public T DataContextAs<T>() => (T)DataContext;
        public void SetDataContext(object datacontext) => DataContext=datacontext;
        public void SetSelector(ISelector? _selector)=>Selector = _selector;
        public abstract void RestructureLogic();

        public void Run()
        {
            RestructureLogic();
            SetDataSource();
        }

        void SetDataSource()
        {
            if (FilteredSource == null) return;

            FilteredSource.SourceName = $"Child of {MainSource} - RecordSource<{MainSource?.ModelType.Name}>()";
            FilteredSource.Origin = ToString();
            FilteredSource.DataSetBasedOn = ((IAbstractModel)DataContext).ObjectName;

            IRecordSource? AlreadyExistingChild = MainSource?.GetChild(FilteredSource);

            if (AlreadyExistingChild == null)
            {
                FilteredSource.AbstractListRestructurer = this;
                FilteredSource.FilterContext = DataContext;
                MainSource?.AddChild(FilteredSource);
                NewFilteredSource = MainSource?.GetChild(FilteredSource);
            }
            else
            {
                AlreadyExistingChild.ReplaceDataSource(FilteredSource.Cast<IAbstractModel>().ToList());
                NewFilteredSource = AlreadyExistingChild;
            }
           
            Selector?.BindUp(this, nameof(NewFilteredSource));
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }

    public interface IRecordSource : IEnumerable
    {
        public string DataSetBasedOn { get; set; }
        public string Origin { get; set; }
        public bool Exists(object record);
        public M? GetFilterContextAs<M>();
        public object? FilterContext { get; set; }
        public void AddChild(IRecordSource child);
        public IRecordSource? GetChild(IRecordSource source);
        public List<IRecordSource> Children { get; set; }
        public void AddRecord(object record);
        public void UpdateRecord(object record);
        public void DeleteRecord(object record);
        public void ReplaceDataSource(IEnumerable? source);
        public void Empty();
        public Type ModelType { get; }
        public string SourceName { get; set; }
        public int RecordCount { get; }
        public int CurrentRecordPosition { get; }
        public bool GoFirst();
        public bool GoLast();
        public bool GoNext();
        public bool GoPrevious();
        public bool GoNewRecord();
        public bool IsEOF { get; }
        public bool IsBOF { get; }
        public bool IsNewRecord { get; }
        public bool AllowNewRecord { get; set; }
        public string CurrentRecordInfo();
        public object CurrentRecord { get; }
        public string SourceStatus();
        public AbstractListRestructurer? AbstractListRestructurer { get; set; }
        public void Print(int limit = -1);
        new IEnumerator GetEnumerator();
        public IRecordSource TakeRows(int rows);
        public void SetMainSource(IRecordSource mainsource);
        public IRecordSource FilterSource<TSource>(Func<TSource, bool> predicate) where TSource : class, IAbstractModel, new();
        public IEnumerable OrderSource<TSource,TKey>(Func<TSource, TKey> keyselector, SourceOrderWay orderway= SourceOrderWay.ASC) where TSource : class, IAbstractModel, new();
        public void RemoveFilters();
        public IRecordSource CopyMe(bool IsChildSource=false);
        public SourcePosition SourcePosition { get; }
        public string Records { get; }
        public TSource CurrentRecordAs<TSource>() where TSource : class, IAbstractModel, new();
        public void SetCurrentRecordPosition(object? value);
        public event EventHandler<RecordMovedArgs>? OnRecordMoved;
    }

    public class NotifyChildrenArgs
    {
        public object? Record;
        public RecordAction RecordAction { get; set; } = RecordAction.ADD;
    }

    public class RecordMovedArgs : EventArgs
    {
        public object? Record;
        public RecordMovement? Movement;
    }

    public enum RecordAction
    {
        ADD,
        UPDATE,
        DELETE,
    }

    public enum RecordMovement
    {
        FIRST,
        PREVIOUS,
        NEXT,
        LAST,
        NEW
    }

    public enum SourcePosition
    {
        EOF,
        BOF,
        NEW_RECORD,
        NO_RECORDS,
        SLIDING
    }

    public enum SourceOrderWay
    {
        ASC,
        DESC,
    }
}
