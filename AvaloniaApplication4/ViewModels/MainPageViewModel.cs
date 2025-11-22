using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing.Imaging;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Notification;
using AvaloniaApplication4.Models;
using QRCoder;
using ReactiveUI;
using System.Text.Json;

namespace AvaloniaApplication4.ViewModels
{
    public class MainPageViewModel : PageViewModelBase, IDisposable
    {
        // Constants
        private const double AUTO_GENERATE_DELAY_SECONDS = 0.5;
        private const int QR_CODE_PIXEL_SIZE = 20;
        private const string NOTIFICATION_ACCENT_COLOR = "#0078D4";
        private const string NOTIFICATION_BACKGROUND_COLOR = "#ffffffff";
        private const string NOTIFICATION_FOREGROUND_COLOR = "#212121";
        private const string DEFAULT_FILE_EXTENSION = ".jpg";

        // Fields
        private string _convertTextVal = "default";
        private string _qrFileName = "default";
        private string _selectedFileExtension = DEFAULT_FILE_EXTENSION;
        private readonly string _applicationDirectory;
        private readonly string _historyFileName = "qrlist.json";
        private byte[]? _qrcodeByte = null;
        private Bitmap? _bitmapVal;
        private ObservableCollection<QrCode> _qrcodes;
        private QRCodeGenerator _qrGenerator;
        private bool _disposed = false;

        public INotificationMessageManager Manager { get; } = new NotificationMessageManager();

        public MainPageViewModel()
        {
            Title = "QR Generator";
            Icon = "🏠";

            _qrGenerator = new QRCodeGenerator();
            _applicationDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location)
                ?? Environment.CurrentDirectory;
            _qrcodes = new ObservableCollection<QrCode>();

            GenerateQrCodeStreamCommand = ReactiveCommand.Create<string, Unit>(GenerateQRCodeStream);
            this.WhenAnyValue(x => x.ConvertText)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Throttle(TimeSpan.FromSeconds(AUTO_GENERATE_DELAY_SECONDS))
                .DistinctUntilChanged()
                .InvokeCommand(GenerateQrCodeStreamCommand);

            // Load history asynchronously
            _ = LoadHistoryAsync();
        }

        private async Task LoadHistoryAsync()
        {
            var filePath = Path.Combine(_applicationDirectory, _historyFileName);

            if (!File.Exists(filePath))
                return;

            try
            {
                var text = await File.ReadAllTextAsync(filePath);
                var list = JsonSerializer.Deserialize<List<QrCode>>(text);

                if (list != null && list.Count > 0)
                {
                    foreach (var item in list)
                    {
                        _qrcodes.Add(item);
                    }
                }
            }
            catch (JsonException)
            {
                // Invalid JSON format - start with empty list
            }
            catch (IOException)
            {
                // File I/O error - start with empty list
            }
        }

        public ReactiveCommand<string, Unit> GenerateQrCodeStreamCommand { get; }

        public Bitmap? BitmapVal
        {
            get => _bitmapVal;
            set => this.RaiseAndSetIfChanged(ref _bitmapVal, value);
        }

        public string QRFileName
        {
            get => _qrFileName;
            set => this.RaiseAndSetIfChanged(ref _qrFileName, value);
        }

        public string ConvertText
        {
            get => _convertTextVal;
            set => this.RaiseAndSetIfChanged(ref _convertTextVal, value);
        }

        public string SelectedFileExtension
        {
            get => _selectedFileExtension;
            set => this.RaiseAndSetIfChanged(ref _selectedFileExtension, value);
        }

        public ObservableCollection<QrCode> QRCodes
        {
            get => _qrcodes;
            set => this.RaiseAndSetIfChanged(ref _qrcodes, value);
        }

        public void DeleteListItem(object sender)
        {
            var index = sender is int ? (int)sender : 0;
            if (index >= 0 && index < QRCodes.Count)
            {
                QRCodes.RemoveAt(index);
                _ = SaveListToJsonAsync();
            }
        }

        private byte[] GenerateQrCode(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return Array.Empty<byte>();

            QRCodeData qrCodeData = _qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
            BitmapByteQRCode qrCode = new BitmapByteQRCode(qrCodeData);
            return qrCode.GetGraphic(QR_CODE_PIXEL_SIZE);
        }

        private void UpdateQrCodePreview(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return;

            _qrcodeByte = GenerateQrCode(text.Trim());
            using (var ms = new MemoryStream(_qrcodeByte))
            {
                BitmapVal = new Bitmap(ms);
            }
        }

        public void RegenerateFromHistory(object sender)
        {
            if (sender is not QrCode qrCode)
                return;

            ConvertText = qrCode.Link;
            QRFileName = qrCode.FileName;
            UpdateQrCodePreview(ConvertText);
        }

        private Unit GenerateQRCodeStream(string parameter)
        {
            UpdateQrCodePreview(_convertTextVal);
            return Unit.Default;
        }

        public void GenerateAndSaveQRCode()
        {
            if (!ValidateInput(out string errorMessage))
            {
                ShowNotification("Validation Error", errorMessage);
                return;
            }

            if (!string.IsNullOrWhiteSpace(_convertTextVal))
            {
                UpdateQrCodePreview(_convertTextVal);

                _qrcodes.Add(new QrCode
                {
                    Link = _convertTextVal,
                    FileName = _qrFileName
                });
            }

            ConvertText = string.Empty;
            _ = SaveListToJsonAsync();
        }

        private bool ValidateInput(out string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(ConvertText))
            {
                errorMessage = "Please enter text to convert";
                return false;
            }

            if (string.IsNullOrWhiteSpace(QRFileName))
            {
                errorMessage = "Please enter a filename";
                return false;
            }

            var invalidChars = Path.GetInvalidFileNameChars();
            if (QRFileName.IndexOfAny(invalidChars) >= 0)
            {
                errorMessage = "Filename contains invalid characters";
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }

        public async void SaveQRFile()
        {
            if (BitmapVal == null || string.IsNullOrWhiteSpace(QRFileName) || _qrcodeByte == null)
                return;

            var saveName = QRFileName + _selectedFileExtension;
            var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var fullPath = Path.Combine(desktopPath, saveName);

            try
            {
                await SaveQRCodeImageAsync(fullPath, _selectedFileExtension);
                ShowNotification("Success", $"{saveName} is saved to Desktop");
            }
            catch (Exception ex)
            {
                ShowNotification("Error", $"Failed to save file: {ex.Message}");
            }
        }

        private async Task SaveQRCodeImageAsync(string filePath, string extension)
        {
            if (_qrcodeByte == null)
                return;

            await Task.Run(() =>
            {
                using var ms = new MemoryStream(_qrcodeByte);
                using var img = new System.Drawing.Bitmap(ms);

                var format = extension switch
                {
                    ".png" => ImageFormat.Png,
                    ".jpg" or ".jpeg" => ImageFormat.Jpeg,
                    ".bmp" => ImageFormat.Bmp,
                    _ => ImageFormat.Png
                };

                img.Save(filePath, format);
            });
        }

        private async Task SaveListToJsonAsync()
        {
            try
            {
                var filePath = Path.Combine(_applicationDirectory, _historyFileName);
                var json = JsonSerializer.Serialize(_qrcodes);
                await File.WriteAllTextAsync(filePath, json);
            }
            catch (IOException)
            {
                // Handle file save error silently or log
            }
        }

        private void ShowNotification(string badge, string message)
        {
            Manager
                .CreateMessage()
                .Accent(NOTIFICATION_ACCENT_COLOR)
                .Animates(true)
                .Background(NOTIFICATION_BACKGROUND_COLOR)
                .Foreground(NOTIFICATION_FOREGROUND_COLOR)
                .HasBadge(badge)
                .HasMessage(message)
                .Dismiss().WithButton("OK", button => { })
                .Dismiss().WithDelay(TimeSpan.FromSeconds(5))
                .Queue();
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _qrGenerator?.Dispose();
            _disposed = true;
        }
    }
}
