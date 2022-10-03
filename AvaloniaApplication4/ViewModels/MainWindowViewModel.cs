using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using QRCoder;
using ReactiveUI;

namespace AvaloniaApplication4.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private string _convertTextVal = "default";
        private string _qrFileName = "default";
        private Avalonia.Media.Imaging.Bitmap _bitmapVal;
        public string Greeting => "Welcome to Avalonia!";

        public MainWindowViewModel()
        {
            try
            {
                _bitmapVal = new Bitmap("test.png");

            }
            catch(Exception ex)
            {
                
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
        public void getQRCode()
        {
            if (!(_convertTextVal == null || _convertTextVal == String.Empty))
            {
                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(_convertTextVal, QRCodeGenerator.ECCLevel.Q);
                BitmapByteQRCode qrCode = new BitmapByteQRCode(qrCodeData);
                byte[] qrCodeAsBitmapByteArr = qrCode.GetGraphic(20);
                using (var ms = new MemoryStream(qrCodeAsBitmapByteArr))
                {
                    var img = new Avalonia.Media.Imaging.Bitmap(ms);
                    BitmapVal = img;
                    
                }
            }
            
            
        }

        async public void SaveQRFile(object sender)
        {
            var filetype = sender as ComboBoxItem;
            Trace.WriteLine(filetype.Content + "LOL");
            if (BitmapVal != null && QRFileName != String.Empty)
            {
                
                var savename = QRFileName + filetype.Content;
                var _folderName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), savename);

                BitmapVal.Save(_folderName);
             
            }
        }
    }
}
