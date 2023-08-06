using Recordsource.Model;
using SAR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Designer.Controller
{
    public class BarcodeController : AbstractDataController<Barcode>
    {
        Item _product=new();
        public Item SelectedProduct { get=>_product; set=>Set(ref value, ref _product,nameof(SelectedProduct)); }

        BarcodeFiltro _filtro;
        public BarcodeFiltro Filtro { get => _filtro;set=>Set(ref value,ref _filtro,nameof(Filtro)); }

        public RecordSource<Item> Items { get; }
        
        public BarcodeController()
        {
            _filtro = new(this);
            Items=new(Sys.DatabaseManager.GetDatabaseTable<Item>().DataSource,true,ToString());
            SelectedProduct=Items.First();
        }
        
        public override bool Save(IAbstractModel? record)
        {
            Barcode? code = (Barcode?)record;            
            if (code==null) return false;
            code.Item = SelectedProduct;
            return base.Save(record);
        }

        public void Filtro2(Item item)
        {
            IRecordSource? range = MainSource?.FilterSource<Barcode>(s => s.Item.IsEqualTo(item));
            DataSource.ReplaceDataSource(range);
            SelectedRecord = DataSource.FirstRecord<Barcode>();
            _product = item;
        }
    }


    public class BarcodeFiltro : AbstractListRestructurer
    {
        IAbstractController Controller;

        public BarcodeFiltro(IAbstractController _controller) 
        {
            Controller=_controller;
        }

        public override IRecordSource? MainSource => Sys.DatabaseManager.GetDatabaseTable<Barcode>().DataSource;

        public override void RestructureLogic()
        {
            Item? item = DataContextAs<Item>();
            Controller?.DataSource.ReplaceDataSource(MainSource?.FilterSource<Barcode>(s => s.Item.IsEqualTo(item)));
            Controller?.SetSelectedRecord(Controller?.DataSource.FirstRecord<Barcode>());
        }
    }
}
