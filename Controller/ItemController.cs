using Newtonsoft.Json.Linq;
using Recordsource.Model;
using SAR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;

namespace Designer.Controller
{
    public class ItemController : AbstractDataController<Item>
    {

        Department _selectedDepartment=new(0,"All Departments");

        public Department SelectedDepartment 
        {
            get => _selectedDepartment;
            set => Set<Department>(ref value, ref _selectedDepartment,nameof(SelectedDepartment));
        }

        public RecordSource<Barcode> Barcodes { get; }
        public RecordSource<Offer> Offers { get; }
        public RecordSource<Department> Departments { get; }
        public RecordSource<Department> DepartmentsFilterList { get; }
        Filtro _filtro = new();

        public Filtro Filtro { get => _filtro; set => Set<Filtro>(ref value, ref _filtro, nameof(Filtro)); }

        Task? t1;

        bool _nobarcode = false;
        public bool NoBarcode { get => _nobarcode; set => Set<bool>(ref value, ref _nobarcode,nameof(NoBarcode)); }

        public ItemController()
        {
            Offers = new(Sys.DatabaseManager.GetDatabaseTable<Offer>().DataSource,true,ToString());
            Departments = new(Sys.DatabaseManager.GetDatabaseTable<Department>().DataSource, true, ToString());
            DepartmentsFilterList = new(Sys.DatabaseManager.GetDatabaseTable<Department>().DataSource, true, ToString());
            DepartmentsFilterList.AddRecord(_selectedDepartment);
            DepartmentsFilterList.OrderMe<Department,long>(s=>s.DepartmentID);
            Barcodes =new(Sys.DatabaseManager.GetDatabaseTable<Barcode>().DataSource,true, ToString());
            BeforeUpdate += ItemController_BeforeUpdate;
            AfterUpdate += ItemController_AfterUpdate;
        }

        private void ItemController_BeforeUpdate(object? sender, PropChangedEventArgs e)
        {
            if (t1 != null && !t1.IsCompleted)
            {
                e.Cancel = true;
                return;
            }
            e.Cancel = false;
        }

        private async void ItemController_AfterUpdate(object? sender, PropChangedEventArgs e)
        {
            IRecordSource range;

            if (e.PropIs(nameof(Search)) || e.PropIs(nameof(SelectedDepartment)))
            {
                range = MainSource.FilterSource<Item>(s => ItemSearch(s));
                DataSource.ReplaceDataSource(range);
                SelectedRecord = DataSource.FirstRecord<Item>();
                return;
            }

            if (e.PropIs(nameof(NoBarcode)))
            {
                if (e.GetNewValue<bool>())
                {
                    IsLoading = true;
                    t1 = Parallel.ForEachAsync(GetRecordSource(), async (item, x) => await RecalculateBarcodeCountTask(item));
                    await t1;
                    IsLoading = false;
                }

                range = MainSource.FilterSource<Item>(s => ItemSearch(s));
                DataSource.ReplaceDataSource(range);
                SelectedRecord = DataSource.FirstRecord<Item>();
                return;
            }
        }

        bool ItemSearch(Item item)
        {
            long depId = SelectedDepartment.DepartmentID;
            bool FirstCondition = item.ItemName.ToLower().Contains(Search.ToLower()) || item.SKU.ToLower().StartsWith(Search.ToLower());
            bool SecondCondition = (NoBarcode) ? item.BarcodeCount == 0 : true;
            bool ThirdCondition = (depId > 0) ? item.Department.DepartmentID == depId : true;
            return FirstCondition && SecondCondition && ThirdCondition;
        }

        Task RecalculateBarcodeCountTask(Item item)
        {
            Filtro filtro = new();
            filtro.SetDataContext(item);
            filtro.RestructureLogic();
            return Task.CompletedTask;
        }

    }

    public class Filtro : AbstractListRestructurer
    {
        public override IRecordSource? MainSource => Sys.DatabaseManager.GetDatabaseTable<Barcode>().DataSource;

        public override void RestructureLogic()
        {
            Item? item = DataContextAs<Item>();
            FilteredSource = MainSource?.FilterSource<Barcode>(s => s.Item.IsEqualTo(item));
            if (FilteredSource != null && item != null)
            {
                item.BarcodeCount = FilteredSource.RecordCount;
                item.IsDirty = false;
            }
        }

        public override string ToString() => "FILTRO";
    }
}
