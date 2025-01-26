using Microsoft.Win32;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Linq;

namespace X4SaveEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private Save save;

        public MainWindow()
        {
            InitializeComponent();
            save = new Save(this);
            DataContext = save;
        }


        private void MainTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is XmlNodeItem selectedNode)
            {
                save.SelectedNode = selectedNode;
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            btnLoad.IsEnabled = false;
            btnSave.IsEnabled = false;
            save.LoadingFlag = true;
            save.ExecuteSave(new Save.FinishEventHandler(Finshhandler));
        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            // Browse from Documents\Egosoft\X4 folder, extension .xml. 
            var dialog = new OpenFileDialog
            {
                InitialDirectory = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Egosoft", "X4"),
                Filter = "XML files (*.xml)|*.xml|Any file|*.*",
                FilterIndex = 1,
                RestoreDirectory = true
            };
            dialog.CheckPathExists = true;
            dialog.CheckFileExists = true;


            if (dialog.ShowDialog() == true)
            {
                btnLoad.IsEnabled = true;
                save.SaveLocation = dialog.FileName;
            }
        }

        private void Load_Click(object sender, RoutedEventArgs e)
        {
            save.LoadingFlag = true;
            btnLoad.IsEnabled = false;
            save.LoadSave(new Save.FinishEventHandler(Finshhandler));

        }
        private void Finshhandler(bool success)
        {
            save.LoadingFlag = false;
            btnLoad.IsEnabled = true;
            btnChangeRespawn.IsEnabled = true;
            if (success)
            {
                EnableSave();
            }
            else
            {
                DisableSave();
            }
        }

        public void EnableSave()
        {
            btnSave.IsEnabled = true;
        }
        public void DisableSave()
        {
            btnSave.IsEnabled = false;
        }

        private void btnChangeRespawn_Click(object sender, RoutedEventArgs e)
        {
            var ModifyMineralWindow = new WindowModifyMineral();
            ModifyMineralWindow.ShowDialog();
            if (ModifyMineralWindow.DialogResult == true)
            {
                save.ChangeAllMineralRespawnTimeAndQuantity(ModifyMineralWindow.RespawnTime, ModifyMineralWindow.RechargeValue);
                MessageBox.Show("All mineral respawn times and quantities have been changed.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }

    public class BoolToVisConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value is bool && (bool)value) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is Visibility && (Visibility)value == Visibility.Visible;
        }
    }

    public class Save : INotifyPropertyChanged
    {
        private XmlNodeItem rootItem;
        private XmlNodeItem selectedNode;
        private string saveLocation = "";
        private bool loadingFlag = false;
        MainWindow instance;

        public Save(MainWindow inMw)
        {
            instance = inMw;
        }

        public delegate void FinishEventHandler(bool success);

        private Thread t;
        private string loadingText = "";

        public void ChangeAllMineralRespawnTimeAndQuantity(int newTime, int newQuantity)
        {
            double ratio_time = newTime / 36000f;
            double ratio_quantity = newQuantity / 49500f;
            // XML Path: savegame\universe\component\connections\[connection]\component\connections\[connection]\component\resourceareas\[area] 

            // Search for all "connection (l1)" nodes.
            List<XmlNodeItem> connections = rootItem.Children
                .Find(node => node.Name == "universe")
                .Children.Find(node => node.Name == "component")
                .Children.Find(node => node.Name == "connections")
                .Children.FindAll(node => node.Name == "connection");

            // Search for all "connection (l2)" nodes.

            foreach (XmlNodeItem connection in connections)
            {
                var compNodeL1 = connection.Children.Find(node => node.Name == "component");
                if (compNodeL1 == null)
                    continue;
                var connections2Node = compNodeL1
                     .Children.Find(node => node.Name == "connections");
                if (connections2Node == null)
                    continue;
                List<XmlNodeItem> connections2 = connections2Node
                    .Children.FindAll(node => node.Name == "connection");

                foreach (XmlNodeItem connection2 in connections2)
                {
                    var compNodeL2 = connection2.Children.Find(node => node.Name == "component");
                    if (compNodeL2 == null)
                        continue;
                    var resourceAreasNode = compNodeL2
                        .Children.Find(node => node.Name == "resourceareas");
                    if (resourceAreasNode == null)
                        continue;
                    var resourceAreas = resourceAreasNode.Children.FindAll(node => node.Name == "area");

                    foreach (XmlNodeItem resourceArea in resourceAreas)
                    {
                        if (resourceArea.Children.Count == 0)
                            continue;
                        // Search for all "wares\ware\recharge" nodes.
                        var waresNode = resourceArea.Children.Find(node => node.Name == "wares");
                        if (waresNode == null)
                            continue;
                        var wareNode = waresNode.Children.FindAll(node => node.Name == "ware");
                        if (wareNode == null)
                            continue;
                        foreach (XmlNodeItem ware in wareNode)
                        {
                            var rechargeNode = ware.Children.Find(node => node.Name == "recharge");
                            if (rechargeNode == null)
                                continue;
                            var time_str = rechargeNode.Attributes.Find(attr => attr.Key == "time");
                            var max_str = rechargeNode.Attributes.Find(attr => attr.Key == "max");

                            if (time_str == null || max_str == null)
                            {
                                continue;
                            }

                            var time_old_value = int.Parse(time_str.Value);
                            var max_old_value = int.Parse(max_str.Value);

                            rechargeNode.Attributes.Find(attr => attr.Key == "time").Value = Math.Round(time_old_value * ratio_time, 0).ToString();
                            rechargeNode.Attributes.Find(attr => attr.Key == "max").Value = Math.Round(max_old_value * ratio_quantity, 0).ToString();
                        }

                    }
                }
            }

        }

        public void LoadSave(FinishEventHandler handler)
        {
            LoadingText = "Loading... Please wait.";
            t = new Thread(() =>
            {
                try
                {
                    using FileStream fileStream = File.Open(saveLocation, FileMode.Open, FileAccess.Read, FileShare.Read);

                    // Parse the data as XML format into memory
                    // Use XmlReader to read the XML data (too large)

                    using XmlReader reader = XmlReader.Create(fileStream);
                    // Create a new XmlDocument to store the XML data

                    XmlDocument doc = new XmlDocument();
                    LoadingText = "Reading file... Please wait.";
                    doc.Load(reader);
                    LoadingText = "Parsing the save data... Please wait.";
                    RootItem = ParseXmlNode(doc.DocumentElement);
                    instance.Dispatcher.BeginInvoke(handler, true);

                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "Error parsing the file.", MessageBoxButton.OK, MessageBoxImage.Error);
                    instance.Dispatcher.BeginInvoke(handler, false);
                }
            });
            t.Start();
        }

        public void ExecuteSave(FinishEventHandler handler)
        {
            // Save the XML data
            if (rootItem != null)
            {
                LoadingText = "Saving... Please wait.";
                Thread t = new Thread(() =>
                {
                    try
                    {
                        SaveToXML();
                        instance.Dispatcher.BeginInvoke(handler, true);
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message, "Error saving the file.", MessageBoxButton.OK, MessageBoxImage.Error);
                        instance.Dispatcher.BeginInvoke(handler, false);
                    }
                });
                t.Start();
            }
        }

        private void SaveToXML()
        {
            // Filename: original filename with _edited appended
            string savePath = saveLocation.Insert(saveLocation.Length - 4, "_edited");
            // Serialize the XMLNodeItem into an XmlDocument

            XmlWriterSettings settings = new XmlWriterSettings
            {
                Indent = true, // For readable output; set to false for compact output.
                Encoding = System.Text.Encoding.UTF8,
                CloseOutput = true, // Automatically closes the underlying stream.
                 
            };

            using (XmlWriter writer = XmlWriter.Create(savePath, settings))
            {
                WriteNode(rootItem, writer);
                writer.Flush();
            }
        }

        public XmlNodeItem RootItem
        {
            get => rootItem;
            set
            {
                if (rootItem != value)
                {
                    rootItem = value;
                    OnPropertyChanged(nameof(RootItem));
                }
            }
        }

        public string LoadingText
        {
            get => loadingText;
            set
            {
                if (loadingText != value)
                {
                    loadingText = value;
                    OnPropertyChanged(nameof(LoadingText));
                }
            }
        }

        public XmlNodeItem SelectedNode
        {
            get => selectedNode;
            set
            {
                if (selectedNode != value)
                {
                    selectedNode = value;
                    OnPropertyChanged(nameof(SelectedNode));
                    OnPropertyChanged(nameof(SelectedAttributes));
                }
            }
        }

        public string SaveLocation
        {
            get => saveLocation;
            set
            {
                if (saveLocation != value)
                {
                    saveLocation = value;
                    OnPropertyChanged(nameof(SaveLocation));
                }
            }
        }
        public bool LoadingFlag
        {
            get => loadingFlag;
            set
            {
                if (loadingFlag != value)
                {
                    loadingFlag = value;
                    OnPropertyChanged(nameof(LoadingFlag));
                }
            }
        }

        public List<AttributeItem> SelectedAttributes => SelectedNode?.Attributes;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private XmlNodeItem ParseXmlNode(XmlElement? documentElement)
        {
            // Create a new XmlNodeItem
            XmlNodeItem node = new XmlNodeItem();
            // Set the name of the node
            node.Name = documentElement.Name;
            // Initialize the children list
            node.Children = new List<XmlNodeItem>();
            // Initialize the attributes list
            node.Attributes = new();
            //total_nodes++;
            //if (total_nodes % 1000 == 0) Console.Write($"Processed {total_nodes:D12} nodes.        \r");
            // Parse the attributes of the node
            foreach (XmlAttribute attribute in documentElement.Attributes)
            {
                AttributeItem attributeDict = new(attribute.Name, attribute.Value);
                node.Attributes.Add(attributeDict);
            }
            // Parse the children of the node
            foreach (XmlNode child in documentElement.ChildNodes)
            {
                if (child is XmlElement element)
                {
                    XmlNodeItem childNode = ParseXmlNode(element);
                    node.Children.Add(childNode);
                }
            }
            return node;
        }

        private void WriteNode(XmlNodeItem node, XmlWriter writer)
        {
            // Start the element with its name.
            writer.WriteStartElement(node.Name);

            // Write attributes.
            if (node.HasAttributes)
            {
                foreach (var attribute in node.Attributes)
                {
                    writer.WriteAttributeString(attribute.Key, attribute.Value);
                }
            }

            // Write child elements recursively.
            if (node.HasChildren)
            {
                foreach (var child in node.Children)
                {
                    WriteNode(child, writer);
                }
            }

            // End the element.
            writer.WriteEndElement();
        }
    }

    public class AttributeItem : INotifyPropertyChanged
    {
        private string key;
        private string value;

        public AttributeItem(string key, string value)
        {
            Key = key;
            Value = value;
        }

        public string Key
        {
            get => key;
            set
            {
                if (key != value)
                {
                    key = value;
                    OnPropertyChanged(nameof(Key));
                }
            }
        }

        public string Value
        {
            get => value;
            set
            {
                if (this.value != value)
                {
                    this.value = value;
                    OnPropertyChanged(nameof(Value));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }


    public class XmlNodeItem : INotifyPropertyChanged
    {
        private string name;
        private List<XmlNodeItem> children;
        private List<AttributeItem> attributes;

        public string Name
        {
            get => name; set
            {
                if (name != value)
                {
                    name = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
                }
            }
        }
        public List<XmlNodeItem> Children
        {
            get => children; set
            {
                if (children != value)
                {
                    children = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Children)));
                }
            }
        }
        public List<AttributeItem> Attributes
        {
            get => attributes; set
            {
                if (attributes != value)
                {
                    attributes = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Attributes)));
                }
            }
        }
        public bool HasChildren => Children.Count > 0;
        public bool HasAttributes => Attributes.Count > 0;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }


}