using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Notification;
using Avalonia.Platform;
using AvaloniaApplication4.Models;
using QRCoder;
using ReactiveUI;
using System.Text.Json;
namespace AvaloniaApplication4.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private string _convertTextVal = "default";
        private string _qrFileName = "default";
        private string app_dir;
        private string list_file = "qrlist.json";
        private byte[]? _qrcodeByte = null;
        private Avalonia.Media.Imaging.Bitmap _bitmapVal;
        private ObservableCollection<QrCode> _qrcodes;
        private ObservableCollection<string> _testString = new ObservableCollection<string>();
        public INotificationMessageManager Manager { get; } = new NotificationMessageManager();

        public MainWindowViewModel()
        {
            
            var text = String.Empty;
            app_dir = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            _qrcodes = new ObservableCollection<QrCode>();

            try
            {
                text =
                    System.IO.File.ReadAllText(
                        @$"{app_dir}\{list_file}");
                var list = JsonSerializer.Deserialize<IList<QrCode>>(text);
                if (list.Count > 0)
                    foreach (var item in list)
                    {
                        _qrcodes.Add(item);
                    }
                Trace.WriteLine(@$"{app_dir}\{list_file}");


            }
            catch (Exception ex)
            {
                if (ex is System.IO.FileNotFoundException)
                {
                    File.Create(@$"{app_dir}\{list_file}");
                    Trace.WriteLine(@$"{app_dir}\{list_file}");
                }
            }





        }
        public Avalonia.Media.Imaging.Bitmap BitmapVal
        {
            get => _bitmapVal;
            set
            {
                _bitmapVal = value;
                this.RaisePropertyChanged("BitmapVal");
            }
        }

        public ObservableCollection<String> TestString
        {
            get => this._testString;
            set => this.RaiseAndSetIfChanged(ref _testString, value);
        }
        public string QRFileName
        {
            get  => _qrFileName;
            set => this.RaiseAndSetIfChanged(ref _qrFileName, value);
        }
        public string ConvertText
        {
            get => _convertTextVal;
            set => this.RaiseAndSetIfChanged(ref _convertTextVal, value);
        }

        public ObservableCollection<QrCode> QRCodes
        {
            get => this._qrcodes;
            set
            {
                this._qrcodes = value;
                this.RaisePropertyChanged("QRCodes");
            }
        }

        public void DeleteListItem(object sender)
        {
            var x = sender is int ? (int)sender : 0;
            QRCodes.RemoveAt(x);
            SaveListToJson();
        }
        public byte[] GenerateQrCode(string str = "")
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(str, QRCodeGenerator.ECCLevel.Q);
            BitmapByteQRCode qrCode = new BitmapByteQRCode(qrCodeData);
            var codeByte = qrCode.GetGraphic(20);
            return codeByte;
        }

        public void QrCodeListGen(object sender )
        {   
            var x = sender as QrCode;
            ConvertText = x.Link.Trim();
            var _qrcodeByteTest = GenerateQrCode(ConvertText);
            using (var ms = new MemoryStream(_qrcodeByteTest))
            {
                var img = new Avalonia.Media.Imaging.Bitmap(ms);
                BitmapVal = img;
            }

            QRFileName = x.FileName;



        }

        public void getQRCode()
        {
            if (_convertTextVal != String.Empty)
            {
                var newString = _convertTextVal.Trim();
                _qrcodeByte = GenerateQrCode(newString);
                using (var ms = new MemoryStream(_qrcodeByte))
                {
                    var img = new Avalonia.Media.Imaging.Bitmap(ms);
                    BitmapVal = img;
                    
                }

                _qrcodes.Add(new QrCode
                {
                    Link = _convertTextVal,
                    FileName = _qrFileName
                });
            }
            ConvertText= String.Empty;
            SaveListToJson();
        }

        public void SaveQRFile(object sender)
        {
            var filetype = sender as ComboBoxItem;
            //Trace.WriteLine(filetype.Content + "LOL");
            if (BitmapVal != null && QRFileName != String.Empty)
            {
                
                var savename = QRFileName + filetype.Content;
                var _folderName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), savename);
                if (filetype.Content.ToString() == ".png")
                {
                    SaveToPng(_folderName);
                }
                else if (filetype.Content.ToString() == ".jpg")
                {
                    SaveToJpeg(_folderName);
                }
                else
                {
                    BitmapVal.Save(_folderName);

                }
                this.Manager
                    .CreateMessage()
                    .Accent("#00877d")
                    .Animates(true)
                    .Background("#ffffffff")
                    .Foreground("#212121")
                    .HasBadge("Info")
                    .HasMessage($"{savename} is saved to Desktop")
                    .Dismiss().WithButton("OK", button => { })
                    .Dismiss().WithDelay(TimeSpan.FromSeconds(5))
                    .Queue();
                
            }
        }

        private async void SaveToPng(string filename )
        {
            await Task.Run(() =>
            {
                using (var ms = new MemoryStream(_qrcodeByte))
                {
                    
                    var img = new System.Drawing.Bitmap(ms);
                    img.Save(filename, ImageFormat.Png);

                }
            });
            
        }

        private async void SaveToJpeg(string filename)
        {
            await Task.Run(() =>
            {
                using (var ms = new MemoryStream(_qrcodeByte))
                {
                     
                    var img = new System.Drawing.Bitmap(ms);
                    img.Save(filename, ImageFormat.Jpeg);

                }
            });
            
        }

        private async void SaveListToJson()
        {
            await Task.Run(() =>
            {
                var str = JsonSerializer.Serialize(_qrcodes);
                File.WriteAllTextAsync($"{app_dir}/{list_file}",str);
            });
        } 

    }
}
