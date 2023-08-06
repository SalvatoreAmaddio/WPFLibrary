using Designer;
using Designer.Custom;
using SARWPF;
using System;
using System.Management;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics.Metrics;
using System.Drawing.Printing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;
using System.Diagnostics;
using static System.Formats.Asn1.AsnWriter;

namespace SAR
{
    public static class Sys
    {
        #region PDF
        /// <summary>
        /// A class that helps to set a default File Name when printing a PDF.
        /// </summary>
        /// <remarks>
        /// Important!
        /// <para>
        /// ADD THE LINE OF CODE BELOW IN THE APP MANIFEST. YOU CAN ADD THE MANIFEST BY CLICKING ON ADD NEW FILE
        /// </para>
        /// <example>
        /// <code>
        /// &lt;requestedExecutionLevel level="requireAdministrator" uiAccess="false"/>
        /// </code>
        /// </example>
        /// </remarks>
        public class MicrosoftPDFManager
        {
           // string FileName = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\Invoice.pdf";
            ConnectToWin32Printers? Win32Printers;
            string? PrinterName { get; set; } = Sys.PrinterManager.AllPrinters().FirstOrDefault(s => s.ToLower().Contains("pdf"));

            public MicrosoftPDFManager()
            {
                if (PrinterName == null)
                {
                    MessageBox.Show("No PDF Printer is installed in this computer", "Something is missing");
                    return;
                }
            }

            public void SetFileName(string fileName)=>
            Win32Printers = new(PrinterName, fileName);

            public void ChangePort() => Win32Printers?.SetNewPort();
            public void ResetPort() =>  Win32Printers?.ResetPort();
            public void DeletePort() => Win32Printers?.DeletePort();

        }
        /// <summary>
        /// A class that connects with Windows' Printers
        /// </summary>
        /// <remarks>
        /// Important!
        /// <para>
        /// ADD THE LINE OF CODE BELOW IN THE APP MANIFEST. YOU CAN ADD THE MANIFEST BY CLICKING ON ADD NEW FILE
        /// </para>
        /// <example>
        /// <code>
        /// &lt;requestedExecutionLevel level="requireAdministrator" uiAccess="false"/>
        /// </code>
        /// </example>
        /// </remarks>
        public class ConnectToWin32Printers
        {
            //SET THIS TO FALSE IN THE APP MANIFEST. YOU CAN ADD THE MANIFEST BY CLICKING ON ADD NEW FILE
            //<requestedExecutionLevel  level="requireAdministrator" uiAccess="false"/>

            static readonly string c_App = @"Custom\Microsoft.VC110.CRT\PDFDriverHelper.exe";
            ManagementObjectCollection Collection;
            string? OriginalPort;
            string FileName=string.Empty;
            public ActionOnPort ActionOnPort { get; set; } = ActionOnPort.AddPort;
            ProcessStartInfo StartInfo = new ();
            Process Process = new();
            ConnectionOptions Options => new()
            {
                Impersonation = ImpersonationLevel.Impersonate,
                Authentication = AuthenticationLevel.PacketPrivacy,
                EnablePrivileges = true
            };
            ManagementScope scope;
            string PrinterName = string.Empty;
            string SysQuery 
            {
                get => @"SELECT * FROM Win32_Printer WHERE Name = '" + PrinterName.Replace("\\", "\\\\") + "'";
            }

            public ConnectToWin32Printers(string printername, string filename) 
            {
                PrinterName = printername;
                FileName=filename;
                scope = new ManagementScope(ManagementPath.DefaultPath, Options);
                scope.Connect();
                ManagementObjectSearcher oObjectSearcher = new(scope, new(SysQuery));
                Collection = oObjectSearcher.Get();
            }
                       
            void GetOriginalPort()
            {
                foreach (ManagementObject oItem in Collection)
                    OriginalPort = oItem.Properties["PortName"].Value.ToString();
            }

            void Connect()
            {
                scope = new ManagementScope(ManagementPath.DefaultPath, Options);
                scope.Connect();
                ManagementObjectSearcher oObjectSearcher = new(scope, new(SysQuery));
                Collection = oObjectSearcher.Get();
            }

            void SetPort(string? port)
            {
                Connect();
                if (port == null) throw new Exception("Port is Null");
                foreach (ManagementObject oItem in Collection)
                {
                    oItem.Properties["PortName"].Value = port;
                    oItem.Put();
                }
            }

            async void RunCProcess()
            {
                Process = new();
                StartInfo = new ProcessStartInfo();
                StartInfo.FileName = c_App;
                StartInfo.ArgumentList.Add(FileName);
                StartInfo.ArgumentList.Add(((int)ActionOnPort).ToString());
                StartInfo.UseShellExecute = true;
                StartInfo.CreateNoWindow = true;
                StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                Process.StartInfo = StartInfo;
                Process.Start();
                await Process.WaitForExitAsync();
                Process.Kill();
            }

            public void DeletePort()
            {
                ActionOnPort = ActionOnPort.RemovePort;
                RunCProcess();
            }

            public void ResetPort()
            {
                SetPort(OriginalPort);
                ActionOnPort = ActionOnPort.RemovePort;
                RunCProcess();
            }
            
            public void SetNewPort()
            {
                GetOriginalPort();
                ActionOnPort = ActionOnPort.AddPort;
                RunCProcess();
                SetPort(FileName);
            }
        }

        public enum ActionOnPort
        {
            AddPort=0,
            RemovePort=1,
        }
        #endregion
        public static AbstractUser? CurrentUser { get; set; }

        public static Color GetColor(string hex) => (Color)ColorConverter.ConvertFromString(hex);

        public static void Merge()=>App.Current.Resources.MergedDictionaries.Add(SarResources.Resources);

        public static class SarResources 
        {
            static ResourceDictionary? _resources;
            static List<Object>? _keys;
            static List<Object>? _values;

            public static List<Object> Keys
            {
                get
                {
                    if (_keys==null)
                    {
                        _keys=Resources.Keys.Cast<Object>().ToList();
                    }
                    return _keys;
                }
            }
            public static List<Object> Values
            {
                get
                {
                    if (_values == null)
                    {
                        _values = Resources.Values.Cast<Object>().ToList();
                    }
                    return _values;
                }
            }

            public static ResourceDictionary Resources 
            {
                get
                {
                    if (_resources==null)
                    {
                        _resources = new() { Source = new Uri($"{AppDomain.CurrentDomain.BaseDirectory.Split("bin")[0]}\\Custom\\SARResources.xaml") };
                    }
                    return _resources;
                }
            }             
            
            public static T Get<T>(string key)
            {
                var found=Keys.FirstOrDefault(s=>s.ToString().ToLower().Equals(key.ToLower()));
                if (found == null) throw new Exception("Resource not found");
                var index =Keys.IndexOf(found);
                return (T)Values[index];
            }
        }

        public static class PrinterManager
        {
            public static string DefaultPrinter { get; set; } = string.Empty;

            public static async Task SetPrinter()
            {
                JSONManager.FileName = "PrinterSetting";
                var task1 = JSONManager.RecreateObjectFormJSONAsync<string>();
                await task1;
                if (DefaultPrinter.Length==0) DefaultPrinter = task1.Result ?? AllPrinters().First();
            }

            public static IEnumerable<string> AllPrinters()
            {
                foreach (string printname in PrinterSettings.InstalledPrinters)
                    yield return printname;
            }

        }

        public static class CultureManager
        {
            public static Country? DefaultCountry { get; set; }

            public static async Task SetCulture()
            {
                JSONManager.FileName = "Setting";
                var task1 = JSONManager.RecreateObjectFormJSONAsync<Country>();
                var task2 = WorldMap.GetDataAsync();
                await Task.WhenAll(task1, task2);

                DefaultCountry = (task1.Result == null)
                ? WorldMap.Countries.First(s => s.EnglishName.Contains("United State"))
                : task1.Result;

                var culture = new CultureInfo(DefaultCountry.ID);
                FrameworkElement.LanguageProperty.OverrideMetadata(
                                                  typeof(FrameworkElement),
                                                   new FrameworkPropertyMetadata(

                XmlLanguage.GetLanguage(culture.IetfLanguageTag)));
            }
        }

        public static void ConsoleWriteLine(string str)
        {
            int threadId = Thread.CurrentThread.ManagedThreadId;
            Console.ForegroundColor = threadId == 1 ? ConsoleColor.White : ConsoleColor.Cyan;
            Console.WriteLine($"{str}{new string(' ', 26 - str.Length)}   Thread {threadId}");
        }

        /// <summary>
        /// Binder static class provides methods to easily perform binding operations.
        /// </summary>
        public static class Binder
        {
            /// <summary>
            /// Short hand method to generate a Binding object.
            ///<para>
            ///Usually used in a multibinding context.
            /// </para>
            /// </summary>
            /// <param name="sender">The object sending the input</param>
            /// <param name="senderProperty">The sender's property to bind from</param>
            /// <param name="mode"></param>
            /// <returns>Binding</returns>
            public static Binding QuickBindUp(object sender, string senderProperty, BindingMode mode = BindingMode.TwoWay) =>
            new QuickBinder(sender, senderProperty, mode);

            /// <summary>
            /// A short hand method to declare Multi Binding
            /// </summary>
            /// <param name="receiver">The object receiving the input</param>
            /// <param name="receiverProperty">The receiver's property to bind to</param>
            /// <param name="Converter"></param>
            /// <param name="Bindings">An array of bindings are the senders to bind from. Use QuickBindUp</param>
            public static void MultiBindUp(FrameworkElement receiver, DependencyProperty receiverProperty, IMultiValueConverter Converter, params Binding[] Bindings)
            {
                MultiBinding multiBinding = new MultiBinding();
                multiBinding.Converter = Converter;

                foreach (Binding bind in Bindings)
                    multiBinding.Bindings.Add(bind);

                receiver.SetBinding(receiverProperty, multiBinding);
            }

            /// <summary>
            /// A shorthand method to bind a object's property to another
            /// </summary>
            /// <param name="sender">The object sending the input</param>
            /// <param name="senderProperty">The object's property to bind from</param>
            /// <param name="receiver">The object receiving the input</param>
            /// <param name="receiverProperty">The object's property to bind to</param>
            /// <param name="mode"></param>
            /// <param name="converter"></param>
            public static void BindUp(object sender, string senderProperty, DependencyObject receiver, DependencyProperty receiverProperty, BindingMode mode = BindingMode.TwoWay, IValueConverter? converter = null) =>
            BindingOperations.SetBinding(receiver, receiverProperty, new QuickBinder(sender, senderProperty, mode, converter));

            /// <summary>
            ///This method registers an array dependency property.
                /// <example>
                ///<para>For Example:</para>
                    ///<code>
                    ///private static readonly DependencyPropertyKey namePropertyKey =
                    ///Sys.Binder.RegisterKey&lt;<typeparamref name = "T" />, <typeparamref name = "O" />>(...);
                    /// 
                    ///public static readonly DependencyProperty NameProperty = namePropertyKey.DependencyProperty;
                    ///
                    ///public Type PropertyNameGetAccessor => (Type)GetValue(NameProperty);
                    ///
                    ///Finally in class' constructor:
                    ///SetValue(namePropertyKey, new Type());
                    /// </code>
                /// </example>
            /// </summary>
            /// <remarks>
            /// <c>Author: Salvatore Amaddio R.</c>
            /// </remarks>
            /// <returns>
            /// DependencyPropertyKey
            /// </returns>
            public static DependencyPropertyKey RegisterKey<T,O>(string propName, bool twoway, T? defaultValue, PropertyChangedCallback? callback = null)
            {
                return DependencyProperty.RegisterReadOnly(
                        propName,
                        typeof(T),
                        typeof(O),
                        new FrameworkPropertyMetadata()
                                     {
                                      AffectsArrange = true,
                                      AffectsMeasure = true,
                                      DefaultValue = defaultValue,
                                      AffectsRender = true,
                                      AffectsParentArrange = true,
                                      AffectsParentMeasure = true,
                                      BindsTwoWayByDefault = twoway,
                                      PropertyChangedCallback = callback
                                      }
                );
            }

            /// <summary>
            ///This method registers a dependency property
            /// <example>
            ///<para>For Example:</para>
            /// <code>
            ///public static readonly DependencyProperty nameProperty = Sys.Binder.Register&lt;<typeparamref name = "T" />, <typeparamref name = "O" />>(...);
            /// 
            ///public Type PropertyName
            ///{
            ///     get => (Type)GetValue(nameProperty);
            ///     set => SetValue(nameProperty, value);
            ///}
            /// </code>
            /// </example>
            /// </summary>
            /// <remarks>
            /// Author: Salvatore Amaddio R.
            /// </remarks>
            /// <returns>
            /// DependencyProperty
            /// </returns>
            public static DependencyProperty Register<T, O>(string propName, bool twoway, T? defaultValue, PropertyChangedCallback? callback = null, bool affectsRender = false, bool affectsMeasure = false, bool affectsArrange = false)
            {
                return DependencyProperty.Register(
                propName,
                typeof(T),
                typeof(O),
                new FrameworkPropertyMetadata()
                {
                    BindsTwoWayByDefault = twoway,
                    DefaultValue = defaultValue,
                    PropertyChangedCallback = callback,
                    AffectsRender = affectsRender,
                    AffectsMeasure = affectsMeasure,
                    AffectsArrange = affectsArrange,
                }
            );
            }

            /// <summary>
            /// A short way to decleare a Binding
            /// </summary>
            internal class QuickBinder : Binding
            {
                public QuickBinder(object sender, string senderProperty, BindingMode mode = BindingMode.TwoWay, IValueConverter? converter = null) : base(senderProperty)
                {
                    Source = sender;
                    Mode = mode;
                    Converter = converter;
                }
            }
        }

        #region Exstentions

        public static void CloseAndOpen(this Window oldWindow, Window newWindow, bool dialog = false)
        {
            oldWindow.Hide();
            if (!dialog)
                newWindow.Show();
            else
                newWindow.ShowDialog();

            oldWindow.Close();
        }

        public static IRecordSource AsIRecordSource<M>(this IEnumerable<M> source) where M : class, IAbstractModel, new()=>
        new RecordSource<M>(source);

        public static IOrderedEnumerable<TSource> Save<TSource>(this IOrderedEnumerable<TSource> source, IRecordSource originalsource) where TSource : class, IAbstractModel, new()
        {
            RecordSource<TSource> rs = new(source);
            originalsource.ReplaceDataSource(rs);
            return source;
        }

        public static TSource? FirstRecord<TSource>(this IRecordSource source, Func<TSource, bool>? predicate=null) where TSource : class, IAbstractModel, new() 
        {
         return (predicate == null) ? ((RecordSource<TSource>)source).FirstOrDefault() : ((RecordSource<TSource>)source).FirstOrDefault(predicate);
        }


        /// <summary>The source records gets filtered.</summary>
        /// <param name="predicate">The condition to pass.</param>
        /// <returns><c>UpdateSource withouth the need to decleare another RecordSource</c></returns>
        public static IRecordSource Filter<TSource>(this IRecordSource source, Func<TSource, bool> predicate) where TSource : class, IAbstractModel, new()
        {
            var range = source.FilterSource<TSource>(predicate);
            source.Empty();
            source.ReplaceDataSource(range);
            return source;
        }

        public static IRecordSource TakeRows<TSource>(this IRecordSource source, int count) where TSource : class, IAbstractModel, new()
        {
            var range = source.TakeRows(count);
            source.Empty();
            source.ReplaceDataSource(range);
            return source;
        }

        public static IRecordSource TakeRowsBasedOn<TSource>(this IRecordSource source, IRecordSource MainSource, int count) where TSource : class, IAbstractModel, new()
        {
            var range=MainSource.TakeRows(count);
            source.Empty();
            source.ReplaceDataSource(range);
            return source;
        }

        public static IOrderedEnumerable<TSource> OrderMe<TSource, TKey>(this IRecordSource source, Func<TSource, TKey> keySelector, SourceOrderWay orderway = SourceOrderWay.ASC) where TSource : class, IAbstractModel, new()
        {
            var range=source.OrderSource(keySelector,orderway).Cast<TSource>();                
            source.ReplaceDataSource(new RecordSource<TSource>(range));
            return (IOrderedEnumerable<TSource>)range;
        }
        #endregion

        #region Database
        public static class DatabaseManager
        {
            public static string ConnectionString = string.Empty;

            static List<IDB> DBS { get; set; } = new();

            public static void AddDatabaseTable(params IDB[] dbs)
            {
                foreach (var x in dbs)
                {
                    DBS.Add(x);
                }
            }

            public static void AddDatabaseTable(IDB db) => DBS.Add(db);

            public static IDB GetDatabaseTable(string? name)
            {
                if (string.IsNullOrEmpty(name)) throw new Exception("Database name cannot be null.");
                name = name.ToLower();
                foreach (var db in DBS)
                {
                    if (db.ModelType.Name.ToLower().Equals(name))
                    {
                        return db;
                    }
                }
                throw new Exception("DATABASE NOT FOUND");
            }

            public static MySQLDatabaseTable<M> GetDatabaseTable<M>() where M : class, IDB<M>, IAbstractModel, new()
            {
                M _temp = new();
                foreach (var db in DBS)
                {
                    if (db.ModelType.Name.Equals(_temp.GetType().Name))
                    {
                        return (MySQLDatabaseTable<M>)db;
                    }
                }
                throw new Exception("DATABASE NOT FOUND");
            }

            public static async Task FecthDatabaseTablesData()
            {
                await Parallel.ForEachAsync(DBS, async (db, token) => await Task.WhenAll(db.GetTable()));
                await Parallel.ForEachAsync(DBS, async (db, token) => await Task.WhenAll(db.SetForeignKeys()));
            }
        }
        #endregion

        #region JSON
        public static class JSONManager
        {

            public static string FileName { get; set; } = string.Empty;

            static string Path()=> $"{AppDomain.CurrentDomain.BaseDirectory.Split("bin")[0]}SAR\\{FileName}.json";
            private static byte[]? jsonUtf8Bytes;

            static JsonSerializerOptions options = new()
            {
                Converters =
            {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
            },
                PropertyNameCaseInsensitive = true,
                IncludeFields = true,
                WriteIndented = true,
                ReferenceHandler = ReferenceHandler.IgnoreCycles
            };

            public static void WriteAsJSON<T>(T obj, bool save = true)
            {
                jsonUtf8Bytes = JsonSerializer.SerializeToUtf8Bytes(obj, options);
                if (save) SaveJSON();
            }

            public static void SaveJSON()
            {
                if (jsonUtf8Bytes == null) return;
                if (File.Exists(Path()))
                    File.Delete(Path());
                File.WriteAllBytes(Path(), jsonUtf8Bytes);
            }

            public static async Task<T?> RecreateObjectFormJSONAsync<T>()
            {
                try
                {
                    using FileStream openStream = File.OpenRead(Path());
                    return await JsonSerializer.DeserializeAsync<T>(openStream, options);
                }
                catch
                {
                    return default(T);
                }
            }

            public static T? RecreateObjectFormJSON<T>()
            {
                try
                {
                    jsonUtf8Bytes = File.ReadAllBytes(Path());
                    Utf8JsonReader utf8Reader = new Utf8JsonReader(jsonUtf8Bytes);
                    return JsonSerializer.Deserialize<T?>(ref utf8Reader, options);
                }
                catch
                {
                    return default(T);
                }
            }
        }
        #endregion
    }

    #region ConnectionCheckerClass
    public static class InternetConnection 
    {
        public static event EventHandler<ConnectionCheckerArgs>? StatusEvent;
        public static void Call(bool value)=>StatusEvent?.Invoke(new(), new() { IsConnected = value });

        public static async Task TaskTryUntilConnect(bool shouldCheck = true)
        {
            if (!shouldCheck) return;
            await Task.Run(() =>
            {
                App.Current?.Dispatcher?.Invoke(new Action(() => Call(InternetConnection.IsAvailable())));
                while (!IsAvailable())
                {
                }
            });
        }
        public static async Task CheckingInternetConnection(bool shouldCheck = true)
        {
            if (!shouldCheck) return;
            bool lastCheck = IsAvailable();
            while (true)
            {
                await Task.Run(() =>
                {
                    bool nextCheck = IsAvailable();
                    if (lastCheck != nextCheck)
                    {
                        lastCheck = nextCheck;
                        App.Current?.Dispatcher?.Invoke(new Action(() => InternetConnection.Call(nextCheck)));
                    }
                });
            }
        }
        public static Task<bool> IsAvailableTask()
        {
            bool result = IsAvailable();
            Call(result);
            return Task.FromResult(result);
        }

        [DllImport("wininet.dll")]
        private extern static bool InternetGetConnectedState(out int description, int reservedValue);

        static bool IsAvailable()
        {
            int description;
            return InternetGetConnectedState(out description, 0);
        }

        public class ConnectionCheckerArgs : EventArgs
        {
            public bool IsConnected { get; set; }
            public string Message { get; set; } = "NO INTERNET, I'll keep trying...";
        }
    }

    #endregion

}
