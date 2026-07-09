using System;
using System.IO;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Windows.Shell;
using System.Threading.Tasks;
using System.Globalization;
using System.Collections.Generic;
using Microsoft.Win32;

namespace VideoTrimmer
{
    public class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            Logger.Log("Uygulama baslatildi. Argumanlar: " + string.Join(" ", args));
            Application app = new Application();
            if (args.Length > 0 && File.Exists(args[0]))
            {
                Logger.Log("Video kirpici modunda baslatiliyor. Dosya: " + args[0]);
                var trimmerWin = new TrimmerWindow(args[0]);
                app.Run(trimmerWin);
            }
            else
            {
                Logger.Log("Kurulum modunda baslatiliyor.");
                var setupWin = new SetupWindow();
                app.Run(setupWin);
            }
        }
    }

    public static class Logger
    {
        private static string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "debug.txt");

        public static void Log(string message)
        {
            try
            {
                string logLine = string.Format("[{0}] {1}\r\n", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), message);
                File.AppendAllText(logPath, logLine);
            }
            catch { }
        }
    }

    public static class UI
    {
        public static class Loc
        {
            public static readonly bool IsTurkish = false;

            public static string Get(string enText, string trText)
            {
                return enText;
            }
        }

        public static readonly Brush BgDark = new SolidColorBrush(Color.FromRgb(24, 24, 30)); // #18181E
        public static readonly Brush BgPanel = new SolidColorBrush(Color.FromRgb(34, 34, 42)); // #22222A
        public static readonly Brush BgHeader = new SolidColorBrush(Color.FromRgb(18, 18, 22)); // #121216
        public static readonly Brush AccentViolet = new SolidColorBrush(Color.FromRgb(131, 56, 236)); // #8338EC
        public static readonly Brush AccentVioletHover = new SolidColorBrush(Color.FromRgb(157, 78, 221)); // #9D4EDD
        public static readonly Brush AccentCyan = new SolidColorBrush(Color.FromRgb(6, 214, 160)); // #06D6A0
        public static readonly Brush AccentBlue = new SolidColorBrush(Color.FromRgb(58, 134, 255)); // #3A86FF
        public static readonly Brush AccentBlueHover = new SolidColorBrush(Color.FromRgb(0, 180, 216)); // #00B4D8
        public static readonly Brush BtnGray = new SolidColorBrush(Color.FromRgb(50, 50, 62)); // #32323E
        public static readonly Brush BtnGrayHover = new SolidColorBrush(Color.FromRgb(70, 70, 85)); // #464655
        public static readonly Brush Red = new SolidColorBrush(Color.FromRgb(232, 17, 35)); // #E81123
        public static readonly Brush TextWhite = Brushes.White;
        public static readonly Brush TextGray = new SolidColorBrush(Color.FromRgb(160, 160, 176)); // #A0A0B0

        public static System.Windows.Media.ImageSource GetLogoImageSource()
        {
            try
            {
                using (var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("app_logo.png"))
                {
                    if (stream != null)
                    {
                        var bitmap = new System.Windows.Media.Imaging.BitmapImage();
                        bitmap.BeginInit();
                        bitmap.StreamSource = stream;
                        bitmap.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                        return bitmap;
                    }
                }
            }
            catch { }

            try
            {
                string logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app_logo.png");
                if (File.Exists(logoPath))
                {
                    return new System.Windows.Media.Imaging.BitmapImage(new Uri(logoPath));
                }
            }
            catch { }

            return null;
        }

        public static Button CreateButton(string content, Brush background, Brush hoverBackground)
        {
            Button btn = new Button();
            btn.Content = content;
            btn.Background = background;
            btn.Foreground = TextWhite;
            btn.BorderThickness = new Thickness(0);
            btn.Cursor = Cursors.Hand;
            btn.Padding = new Thickness(15, 8, 15, 8);
            btn.FontFamily = new FontFamily("Segoe UI");
            btn.FontSize = 13;
            btn.FontWeight = FontWeights.SemiBold;

            ControlTemplate template = new ControlTemplate(typeof(Button));
            FrameworkElementFactory border = new FrameworkElementFactory(typeof(Border));
            border.SetValue(Border.BackgroundProperty, new TemplateBindingExtension(Button.BackgroundProperty));
            border.SetValue(Border.CornerRadiusProperty, new CornerRadius(6));

            FrameworkElementFactory presenter = new FrameworkElementFactory(typeof(ContentPresenter));
            presenter.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            presenter.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
            presenter.SetValue(ContentPresenter.MarginProperty, new Thickness(10, 6, 10, 6));
            border.AppendChild(presenter);
            template.VisualTree = border;
            btn.Template = template;

            btn.MouseEnter += (s, e) => { btn.Background = hoverBackground; };
            btn.MouseLeave += (s, e) => { btn.Background = background; };

            return btn;
        }

        public static Grid CreateTitleBar(Window window, string titleText)
        {
            Grid titleBar = new Grid();
            titleBar.Height = 32;
            titleBar.Background = BgHeader;

            ColumnDefinition colTitle = new ColumnDefinition();
            colTitle.Width = new GridLength(1, GridUnitType.Star);
            ColumnDefinition colButtons = new ColumnDefinition();
            colButtons.Width = GridLength.Auto;

            titleBar.ColumnDefinitions.Add(colTitle);
            titleBar.ColumnDefinitions.Add(colButtons);

            StackPanel titlePanel = new StackPanel();
            titlePanel.Orientation = Orientation.Horizontal;
            titlePanel.VerticalAlignment = VerticalAlignment.Center;
            titlePanel.Margin = new Thickness(12, 0, 0, 0);

            var logoSrc = GetLogoImageSource();
            if (logoSrc != null)
            {
                try
                {
                    Image img = new Image();
                    img.Source = logoSrc;
                    img.Width = 16;
                    img.Height = 16;
                    img.Margin = new Thickness(0, 0, 8, 0);
                    titlePanel.Children.Add(img);
                }
                catch { }
            }

            TextBlock textBlock = new TextBlock();
            textBlock.Text = titleText;
            textBlock.Foreground = TextWhite;
            textBlock.VerticalAlignment = VerticalAlignment.Center;
            textBlock.FontFamily = new FontFamily("Segoe UI");
            textBlock.FontWeight = FontWeights.Medium;
            textBlock.FontSize = 12;
            titlePanel.Children.Add(textBlock);

            Grid.SetColumn(titlePanel, 0);
            titleBar.Children.Add(titlePanel);

            StackPanel buttonsPanel = new StackPanel();
            buttonsPanel.Orientation = Orientation.Horizontal;
            Grid.SetColumn(buttonsPanel, 1);
            titleBar.Children.Add(buttonsPanel);

            Button btnMin = new Button();
            btnMin.Content = "—";
            btnMin.Width = 46;
            btnMin.Height = 32;
            btnMin.Background = Brushes.Transparent;
            btnMin.Foreground = TextWhite;
            btnMin.BorderThickness = new Thickness(0);
            btnMin.FontFamily = new FontFamily("Segoe UI");
            btnMin.FontSize = 10;
            btnMin.Click += (s, e) => { window.WindowState = WindowState.Minimized; };
            btnMin.MouseEnter += (s, e) => { btnMin.Background = new SolidColorBrush(Color.FromArgb(20, 255, 255, 255)); };
            btnMin.MouseLeave += (s, e) => { btnMin.Background = Brushes.Transparent; };
            buttonsPanel.Children.Add(btnMin);

            Button btnClose = new Button();
            btnClose.Content = "✕";
            btnClose.Width = 46;
            btnClose.Height = 32;
            btnClose.Background = Brushes.Transparent;
            btnClose.Foreground = TextWhite;
            btnClose.BorderThickness = new Thickness(0);
            btnClose.FontFamily = new FontFamily("Segoe UI");
            btnClose.FontSize = 10;
            btnClose.Click += (s, e) => { window.Close(); };
            btnClose.MouseEnter += (s, e) => { btnClose.Background = Red; };
            btnClose.MouseLeave += (s, e) => { btnClose.Background = Brushes.Transparent; };
            buttonsPanel.Children.Add(btnClose);

            WindowChrome.SetIsHitTestVisibleInChrome(btnMin, true);
            WindowChrome.SetIsHitTestVisibleInChrome(btnClose, true);

            return titleBar;
        }

        public static void SetupWindowChrome(Window window)
        {
            WindowChrome chrome = new WindowChrome();
            chrome.CaptionHeight = 32;
            chrome.ResizeBorderThickness = new Thickness(6);
            chrome.UseAeroCaptionButtons = false;
            WindowChrome.SetWindowChrome(window, chrome);

            var logoSrc = GetLogoImageSource();
            if (logoSrc != null)
            {
                try
                {
                    window.Icon = logoSrc;
                }
                catch { }
            }
        }

        public static string FindFFmpegPath()
        {
            // 1. Check application directory
            string localFfmpeg = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg.exe");
            if (File.Exists(localFfmpeg)) return localFfmpeg;

            // 2. Gather PATH directories from Process, User, and Machine targets
            var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            
            // Process PATH
            var processPath = Environment.GetEnvironmentVariable("PATH");
            if (!string.IsNullOrEmpty(processPath))
            {
                foreach (var p in processPath.Split(';')) paths.Add(p.Trim());
            }

            // User PATH (where WinGet often puts things)
            var userPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User);
            if (!string.IsNullOrEmpty(userPath))
            {
                foreach (var p in userPath.Split(';')) paths.Add(p.Trim());
            }

            // Machine PATH
            var machinePath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine);
            if (!string.IsNullOrEmpty(machinePath))
            {
                foreach (var p in machinePath.Split(';')) paths.Add(p.Trim());
            }

            // Common fallback folders
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            paths.Add(Path.Combine(localAppData, @"Microsoft\WinGet\Links"));
            paths.Add(@"C:\ffmpeg\bin");
            paths.Add(@"C:\Program Files\ffmpeg\bin");

            foreach (var dir in paths)
            {
                if (string.IsNullOrEmpty(dir)) continue;
                try
                {
                    string cleanDir = dir.Trim('"', ' ');
                    string fullPath = Path.Combine(cleanDir, "ffmpeg.exe");
                    if (File.Exists(fullPath))
                    {
                        Logger.Log("FFmpeg bulundu: " + fullPath);
                        return fullPath;
                    }
                }
                catch { }
            }

            Logger.Log("FFmpeg bulunamadi, varsayilan 'ffmpeg' kullanilacak.");
            return "ffmpeg";
        }
    }

    public class SetupWindow : Window
    {
        private TextBlock textStatus;
        private Button btnInstall;
        private Button btnUninstall;
        private Border ffmpegWarningBorder;
        private TextBlock footerText;
        private Grid overlayGrid;
        private TextBlock overlayText;
        private TextBlock overlaySubtext;
        private ProgressBar overlayProgress;

        public SetupWindow()
        {
            this.Title = UI.Loc.Get("Fast Video Trimmer Setup", "Hızlı Video Kırpıcı Kurulumu");
            this.Width = 480;
            this.Height = 380;
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            this.ResizeMode = ResizeMode.NoResize;
            this.Background = UI.BgDark;
            this.WindowStyle = WindowStyle.None;

            UI.SetupWindowChrome(this);

            Grid mainGrid = new Grid();
            mainGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto }); // Title bar
            mainGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) }); // Content
            mainGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto }); // Footer

            // Title Bar
            var titleBar = UI.CreateTitleBar(this, UI.Loc.Get("Fast Video Trimmer - Setup", "Hızlı Video Kırpıcı - Kurulum"));
            Grid.SetRow(titleBar, 0);
            mainGrid.Children.Add(titleBar);

            // Content Panel
            StackPanel contentPanel = new StackPanel();
            contentPanel.Margin = new Thickness(25);
            Grid.SetRow(contentPanel, 1);
            mainGrid.Children.Add(contentPanel);

            Grid headerGrid = new Grid() { Margin = new Thickness(0, 0, 0, 15) };
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });

            var logoSrcForSetup = UI.GetLogoImageSource();
            if (logoSrcForSetup != null)
            {
                try
                {
                    Image imgLogo = new Image();
                    imgLogo.Source = logoSrcForSetup;
                    imgLogo.Width = 48;
                    imgLogo.Height = 48;
                    imgLogo.Margin = new Thickness(0, 0, 15, 0);
                    imgLogo.VerticalAlignment = VerticalAlignment.Center;
                    Grid.SetColumn(imgLogo, 0);
                    headerGrid.Children.Add(imgLogo);
                }
                catch { }
            }

            StackPanel titleTextPanel = new StackPanel();
            titleTextPanel.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(titleTextPanel, 1);
            TextBlock titleLabel = new TextBlock();
            titleLabel.Text = UI.Loc.Get("Fast Video Trimmer", "Hızlı Video Kırpıcı");
            titleLabel.Foreground = UI.TextWhite;
            titleLabel.FontSize = 22;
            titleLabel.FontWeight = FontWeights.Bold;
            titleLabel.Margin = new Thickness(0, 0, 0, 2);
            titleTextPanel.Children.Add(titleLabel);

            TextBlock descLabel = new TextBlock();
            descLabel.Text = UI.Loc.Get("Integrates into the Windows right-click menu to let you trim videos losslessly in seconds.", "Windows sağ tık menüsüne eklenerek videoları saniyeler içinde kayıpsız kesmenizi sağlar.");
            descLabel.Foreground = UI.TextGray;
            descLabel.FontSize = 12;
            descLabel.TextWrapping = TextWrapping.Wrap;
            titleTextPanel.Children.Add(descLabel);

            headerGrid.Children.Add(titleTextPanel);
            contentPanel.Children.Add(headerGrid);

            bool ffmpegFound = IsFFmpegAvailable();

            // FFmpeg Missing Warning & Auto-Install Card
            ffmpegWarningBorder = new Border();
            ffmpegWarningBorder.Background = new SolidColorBrush(Color.FromRgb(48, 30, 36));
            ffmpegWarningBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(180, 50, 50));
            ffmpegWarningBorder.BorderThickness = new Thickness(1);
            ffmpegWarningBorder.CornerRadius = new CornerRadius(6);
            ffmpegWarningBorder.Padding = new Thickness(12, 8, 12, 8);
            ffmpegWarningBorder.Margin = new Thickness(0, 0, 0, 15);
            ffmpegWarningBorder.Visibility = ffmpegFound ? Visibility.Collapsed : Visibility.Visible;

            Grid ffmpegWarningGrid = new Grid();
            ffmpegWarningGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            ffmpegWarningGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            TextBlock warningText = new TextBlock();
            warningText.Text = UI.Loc.Get("⚠️ FFmpeg not found on your PC. It is required for lossless trimming and external HEVC preview.", "⚠️ Bilgisayarınızda FFmpeg bulunamadı. Kayıpsız kesim ve harici HEVC önizleme için gereklidir.");
            warningText.Foreground = new SolidColorBrush(Color.FromRgb(255, 180, 180));
            warningText.FontSize = 11;
            warningText.TextWrapping = TextWrapping.Wrap;
            warningText.VerticalAlignment = VerticalAlignment.Center;
            warningText.Margin = new Thickness(0, 0, 10, 0);
            Grid.SetColumn(warningText, 0);
            ffmpegWarningGrid.Children.Add(warningText);

            Button btnDownloadFFmpeg = UI.CreateButton(UI.Loc.Get("Auto Install", "Otomatik Yükle"), new SolidColorBrush(Color.FromRgb(180, 50, 50)), new SolidColorBrush(Color.FromRgb(210, 70, 70)));
            btnDownloadFFmpeg.FontSize = 11;
            btnDownloadFFmpeg.Padding = new Thickness(10, 5, 10, 5);
            btnDownloadFFmpeg.Click += DownloadFFmpeg_Click;
            Grid.SetColumn(btnDownloadFFmpeg, 1);
            ffmpegWarningGrid.Children.Add(btnDownloadFFmpeg);

            ffmpegWarningBorder.Child = ffmpegWarningGrid;
            contentPanel.Children.Add(ffmpegWarningBorder);

            // Status indicator
            Border statusBorder = new Border();
            statusBorder.Background = UI.BgPanel;
            statusBorder.CornerRadius = new CornerRadius(6);
            statusBorder.Padding = new Thickness(12, 8, 12, 8);
            statusBorder.Margin = new Thickness(0, 0, 0, 15);

            textStatus = new TextBlock();
            textStatus.Foreground = UI.TextWhite;
            textStatus.FontSize = 12;
            textStatus.FontWeight = FontWeights.Medium;
            statusBorder.Child = textStatus;
            contentPanel.Children.Add(statusBorder);

            // Action Buttons Row
            Grid buttonsGrid = new Grid();
            buttonsGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            buttonsGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            buttonsGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });

            btnInstall = UI.CreateButton(UI.Loc.Get("Add to Right-Click", "Sağ Tıka Ekle"), UI.AccentViolet, UI.AccentVioletHover);
            btnInstall.Click += Install_Click;
            btnInstall.Margin = new Thickness(0, 0, 5, 0);
            Grid.SetColumn(btnInstall, 0);
            buttonsGrid.Children.Add(btnInstall);

            btnUninstall = UI.CreateButton(UI.Loc.Get("Remove Right-Click", "Sağ Tık Kaldır"), UI.BtnGray, UI.BtnGrayHover);
            btnUninstall.Click += Uninstall_Click;
            btnUninstall.Margin = new Thickness(5, 0, 5, 0);
            Grid.SetColumn(btnUninstall, 1);
            buttonsGrid.Children.Add(btnUninstall);

            Button btnOpenVideo = UI.CreateButton(UI.Loc.Get("Open Video", "Video Aç"), UI.AccentBlue, UI.AccentBlueHover);
            btnOpenVideo.Click += OpenVideo_Click;
            btnOpenVideo.Margin = new Thickness(5, 0, 0, 0);
            Grid.SetColumn(btnOpenVideo, 2);
            buttonsGrid.Children.Add(btnOpenVideo);

            contentPanel.Children.Add(buttonsGrid);

            // Footer / FFmpeg status
            Border footerBorder = new Border();
            footerBorder.Background = UI.BgHeader;
            footerBorder.Padding = new Thickness(15, 6, 15, 6);
            Grid.SetRow(footerBorder, 2);

            footerText = new TextBlock();
            footerText.FontSize = 11;

            if (ffmpegFound)
            {
                footerText.Text = UI.Loc.Get("✓ FFmpeg Detected - Your operations will run at light speed.", "✓ FFmpeg Algılandı - İşlemleriniz ışık hızında gerçekleşecek.");
                footerText.Foreground = UI.AccentCyan;
            }
            else
            {
                footerText.Text = UI.Loc.Get("⚠️ FFmpeg NOT FOUND! Fast trimming may not work. Please install automatically.", "⚠️ FFmpeg BULUNAMADI! Hızlı kesim çalışmayabilir. Lütfen otomatik yükleyin.");
                footerText.Foreground = new SolidColorBrush(Color.FromRgb(255, 107, 107));
            }

            footerBorder.Child = footerText;
            mainGrid.Children.Add(footerBorder);

            // Overlay Grid (for Download / Install Progress)
            overlayGrid = new Grid();
            overlayGrid.Background = new SolidColorBrush(Color.FromArgb(220, 15, 15, 20));
            overlayGrid.Visibility = Visibility.Collapsed;
            Grid.SetRowSpan(overlayGrid, 3);

            StackPanel overlayContent = new StackPanel();
            overlayContent.VerticalAlignment = VerticalAlignment.Center;
            overlayContent.HorizontalAlignment = HorizontalAlignment.Center;

            overlayText = new TextBlock();
            overlayText.Text = UI.Loc.Get("Downloading FFmpeg...", "FFmpeg İndiriliyor...");
            overlayText.Foreground = UI.TextWhite;
            overlayText.FontSize = 18;
            overlayText.FontWeight = FontWeights.Bold;
            overlayText.HorizontalAlignment = HorizontalAlignment.Center;
            overlayText.Margin = new Thickness(0, 0, 0, 8);
            overlayContent.Children.Add(overlayText);

            overlaySubtext = new TextBlock();
            overlaySubtext.Text = UI.Loc.Get("Please wait...", "Lütfen bekleyin...");
            overlaySubtext.Foreground = UI.TextGray;
            overlaySubtext.FontSize = 12;
            overlaySubtext.HorizontalAlignment = HorizontalAlignment.Center;
            overlaySubtext.Margin = new Thickness(0, 0, 0, 15);
            overlayContent.Children.Add(overlaySubtext);

            overlayProgress = new ProgressBar();
            overlayProgress.Width = 300;
            overlayProgress.Height = 6;
            overlayProgress.Foreground = UI.AccentViolet;
            overlayProgress.Background = new SolidColorBrush(Color.FromRgb(42, 42, 53));

            Border progBorder = new Border();
            progBorder.CornerRadius = new CornerRadius(3);
            progBorder.ClipToBounds = true;
            progBorder.Child = overlayProgress;
            overlayContent.Children.Add(progBorder);

            overlayGrid.Children.Add(overlayContent);
            mainGrid.Children.Add(overlayGrid);

            this.Content = mainGrid;

            UpdateStatus();
        }

        private void DownloadFFmpeg_Click(object sender, RoutedEventArgs e)
        {
            overlayGrid.Visibility = Visibility.Visible;
            overlayText.Text = UI.Loc.Get("Downloading FFmpeg...", "FFmpeg İndiriliyor...");
            overlaySubtext.Text = UI.Loc.Get("Establishing connection...", "Bağlantı kuruluyor...");
            overlayProgress.IsIndeterminate = false;
            overlayProgress.Value = 0;

            try
            {
                // Force TLS 1.2 for modern HTTPS connections
                System.Net.ServicePointManager.SecurityProtocol = (System.Net.SecurityProtocolType)3072;

                string zipUrl = "https://www.gyan.dev/ffmpeg/builds/ffmpeg-release-essentials.zip";
                string localZip = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg.zip");

                using (var webClient = new System.Net.WebClient())
                {
                    webClient.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");

                    webClient.DownloadProgressChanged += (s, ev) =>
                    {
                        double downloadedMb = (double)ev.BytesReceived / 1024 / 1024;
                        double totalMb = (double)ev.TotalBytesToReceive / 1024 / 1024;
                        overlayProgress.Value = ev.ProgressPercentage;
                        overlaySubtext.Text = string.Format(UI.Loc.Get("Downloading: {0:F1} MB / {1:F1} MB ({2}%)", "İndiriliyor: {0:F1} MB / {1:F1} MB ({2}%)"), downloadedMb, totalMb, ev.ProgressPercentage);
                    };

                    webClient.DownloadFileCompleted += (s, ev) =>
                    {
                        if (ev.Error != null)
                        {
                            overlayGrid.Visibility = Visibility.Collapsed;
                            MessageBox.Show(UI.Loc.Get("An error occurred during download: ", "İndirme sırasında hata oluştu: ") + ev.Error.Message, UI.Loc.Get("Error", "Hata"), MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        // Validate file size to ensure we didn't just download a tiny error HTML page or get a corrupted zip
                        try
                        {
                            FileInfo fi = new FileInfo(localZip);
                            if (!fi.Exists || fi.Length < 10000000) // FFmpeg zip is typically >50MB
                            {
                                overlayGrid.Visibility = Visibility.Collapsed;
                                MessageBox.Show(UI.Loc.Get("Downloaded file appears to be corrupted or incomplete. Please try again later.", "İndirilen dosya bozuk veya eksik görünüyor. Lütfen daha sonra tekrar deneyin."), UI.Loc.Get("Download Error", "İndirme Hatası"), MessageBoxButton.OK, MessageBoxImage.Error);
                                try { File.Delete(localZip); } catch { }
                                return;
                            }
                        }
                        catch (Exception ex)
                        {
                            overlayGrid.Visibility = Visibility.Collapsed;
                            MessageBox.Show(UI.Loc.Get("Error verifying downloaded file: ", "İndirilen dosya doğrulanırken hata: ") + ex.Message, UI.Loc.Get("Error", "Hata"), MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        ExtractAndInstallFFmpeg(localZip);
                    };

                    webClient.DownloadFileAsync(new Uri(zipUrl), localZip);
                }
            }
            catch (Exception ex)
            {
                overlayGrid.Visibility = Visibility.Collapsed;
                MessageBox.Show(UI.Loc.Get("Failed to start download: ", "İndirme başlatılamadı: ") + ex.Message, UI.Loc.Get("Error", "Hata"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExtractAndInstallFFmpeg(string zipPath)
        {
            overlayText.Text = UI.Loc.Get("Extracting Archive...", "Arşivden Çıkarılıyor...");
            overlaySubtext.Text = UI.Loc.Get("Extracting files, please wait (this might take a while)...", "Dosyalar çıkarılıyor, lütfen bekleyin (biraz zaman alabilir)...");
            overlayProgress.IsIndeterminate = true;

            Task.Factory.StartNew(() =>
            {
                string tempDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg_temp");
                try
                {
                    if (Directory.Exists(tempDir))
                    {
                        Directory.Delete(tempDir, true);
                    }
                    Directory.CreateDirectory(tempDir);

                    // Call PowerShell to extract zip archive silently
                    string psCommand = string.Format("Expand-Archive -Path '{0}' -DestinationPath '{1}' -Force", zipPath, tempDir);
                    ProcessStartInfo psi = new ProcessStartInfo("powershell", "-Command \"" + psCommand + "\"");
                    psi.UseShellExecute = false;
                    psi.CreateNoWindow = true;
                    using (var p = Process.Start(psi))
                    {
                        if (p != null) p.WaitForExit();
                    }

                    // Traverse the temp folder recursively to find ffmpeg.exe and ffplay.exe
                    string[] foundFfmpeg = Directory.GetFiles(tempDir, "ffmpeg.exe", SearchOption.AllDirectories);
                    string[] foundFfplay = Directory.GetFiles(tempDir, "ffplay.exe", SearchOption.AllDirectories);

                    string destDir = AppDomain.CurrentDomain.BaseDirectory;
                    if (foundFfmpeg.Length > 0)
                    {
                        File.Copy(foundFfmpeg[0], Path.Combine(destDir, "ffmpeg.exe"), true);
                    }
                    if (foundFfplay.Length > 0)
                    {
                        File.Copy(foundFfplay[0], Path.Combine(destDir, "ffplay.exe"), true);
                    }

                    // Cleanup zip and temp folders
                    try { File.Delete(zipPath); } catch { }
                    try { Directory.Delete(tempDir, true); } catch { }

                    // Refresh UI state on main UI thread
                    Dispatcher.Invoke(new Action(() =>
                    {
                        overlayGrid.Visibility = Visibility.Collapsed;
                        bool ffmpegFound = IsFFmpegAvailable();
                        if (ffmpegFound)
                        {
                            ffmpegWarningBorder.Visibility = Visibility.Collapsed;
                            footerText.Text = UI.Loc.Get("✓ FFmpeg Detected - Your operations will run at light speed.", "✓ FFmpeg Algılandı - İşlemleriniz ışık hızında gerçekleşecek.");
                            footerText.Foreground = UI.AccentCyan;
                            MessageBox.Show(UI.Loc.Get("FFmpeg and FFplay successfully installed! You can now trim and preview videos without issues.", "FFmpeg ve FFplay başarıyla kuruldu! Artık videoları sorunsuzca kesip önizleyebilirsiniz."), UI.Loc.Get("Success", "Başarılı"), MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show(UI.Loc.Get("Extraction completed, but FFmpeg could not be executed. Please restart the application.", "Çıkartma işlemi bitti ancak FFmpeg çalıştırılamadı. Lütfen uygulamayı yeniden başlatın."), UI.Loc.Get("Warning", "Uyarı"), MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }));
                }
                catch (Exception ex)
                {
                    // Cleanup on error
                    try { File.Delete(zipPath); } catch { }
                    try { Directory.Delete(tempDir, true); } catch { }

                    Dispatcher.Invoke(new Action(() =>
                    {
                        overlayGrid.Visibility = Visibility.Collapsed;
                        MessageBox.Show(UI.Loc.Get("FFmpeg installation error: ", "FFmpeg yükleme hatası: ") + ex.Message, UI.Loc.Get("Error", "Hata"), MessageBoxButton.OK, MessageBoxImage.Error);
                    }));
                }
            });
        }

        private void UpdateStatus()
        {
            bool isInstalled = IsContextMenuRegistered();
            if (isInstalled)
            {
                textStatus.Text = UI.Loc.Get("Status: Active in Context Menu (Right-click videos and choose 'Trim Video')", "Durum: Sağ Tık Menüsünde Aktif (Videolara sağ tıklayarak 'Videoyu Kes' seçeneğini kullanabilirsiniz)");
                textStatus.Foreground = UI.AccentCyan;
                btnInstall.IsEnabled = false;
                btnUninstall.IsEnabled = true;
            }
            else
            {
                textStatus.Text = UI.Loc.Get("Status: Not registered. Click 'Add to Right-Click' to register it.", "Durum: Sağ Tık Menüsüne Eklenmemiş. Kurulum yapmak için 'Sağ Tıka Ekle' butonuna basın.");
                textStatus.Foreground = UI.TextGray;
                btnInstall.IsEnabled = true;
                btnUninstall.IsEnabled = false;
            }
        }

        private void Install_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RegisterContextMenu();
                UpdateStatus();
                MessageBox.Show(UI.Loc.Get("Successfully added to right-click menu! You can now right-click any video file and select 'Trim Video (Fast)'.", "Başarıyla sağ tık menüsüne eklendi! Artık herhangi bir video dosyasına sağ tıklayıp 'Videoyu Kes (Hızlı)' seçeneğini kullanabilirsiniz."), UI.Loc.Get("Success", "Başarılı"), MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(UI.Loc.Get("Error occurred: ", "Hata oluştu: ") + ex.Message, UI.Loc.Get("Error", "Hata"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Uninstall_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                UnregisterContextMenu();
                UpdateStatus();
                MessageBox.Show(UI.Loc.Get("Removed from right-click menu.", "Sağ tık menüsünden kaldırıldı."), UI.Loc.Get("Success", "Başarılı"), MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(UI.Loc.Get("Error occurred: ", "Hata oluştu: ") + ex.Message, UI.Loc.Get("Error", "Hata"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenVideo_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = UI.Loc.Get("Video Files|*.mp4;*.mkv;*.mov;*.avi;*.wmv;*.webm;*.flv;*.3gp|All Files|*.*", "Video Dosyaları|*.mp4;*.mkv;*.mov;*.avi;*.wmv;*.webm;*.flv;*.3gp|Tüm Dosyalar|*.*");
            if (ofd.ShowDialog() == true)
            {
                var trimmerWin = new TrimmerWindow(ofd.FileName);
                trimmerWin.Show();
                this.Close();
            }
        }

        private bool IsContextMenuRegistered()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Classes\SystemFileAssociations\video\shell\TrimVideo"))
                {
                    return key != null;
                }
            }
            catch
            {
                return false;
            }
        }

        private void RegisterContextMenu()
        {
            string appPath = Process.GetCurrentProcess().MainModule.FileName;
            if (appPath.EndsWith(".dll"))
            {
                appPath = Path.ChangeExtension(appPath, ".exe");
            }

            string[] extensions = { "video", ".mp4", ".mkv", ".mov", ".avi", ".wmv", ".webm", ".flv", ".3gp" };

            foreach (var ext in extensions)
            {
                string keyPath = string.Format(@"Software\Classes\SystemFileAssociations\{0}\shell\TrimVideo", ext);
                using (var key = Registry.CurrentUser.CreateSubKey(keyPath))
                {
                    if (key != null)
                    {
                        key.SetValue("", UI.Loc.Get("Trim Video (Fast)", "Videoyu Kes (Hızlı)"));
                        key.SetValue("Icon", appPath);
                        using (var cmdKey = key.CreateSubKey("command"))
                        {
                            if (cmdKey != null)
                            {
                                cmdKey.SetValue("", string.Format("\"{0}\" \"%1\"", appPath));
                            }
                        }
                    }
                }
            }
            Logger.Log("Sag tik entegrasyonu kuruldu.");
        }

        private void UnregisterContextMenu()
        {
            string[] extensions = { "video", ".mp4", ".mkv", ".mov", ".avi", ".wmv", ".webm", ".flv", ".3gp" };
            foreach (var ext in extensions)
            {
                string keyPath = string.Format(@"Software\Classes\SystemFileAssociations\{0}\shell\TrimVideo", ext);
                try
                {
                    Registry.CurrentUser.DeleteSubKeyTree(keyPath, false);
                }
                catch { }
            }
            Logger.Log("Sag tik entegrasyonu kaldirildi.");
        }

        private bool IsFFmpegAvailable()
        {
            try
            {
                string ffmpegPath = UI.FindFFmpegPath();
                ProcessStartInfo psi = new ProcessStartInfo(ffmpegPath, "-version");
                psi.UseShellExecute = false;
                psi.CreateNoWindow = true;
                using (var p = Process.Start(psi))
                {
                    if (p != null) p.WaitForExit();
                    return p != null && p.ExitCode == 0;
                }
            }
            catch
            {
                return false;
            }
        }
    }

    public class TrimmerWindow : Window
    {
        private string inputFilePath;
        private string finalOutputPath;
        private MediaElement mediaElement;
        private Slider seekBar;
        private TextBlock textCurrentTime;
        private TextBox textBoxStart;
        private TextBox textBoxEnd;
        private TextBlock textDuration;
        private TextBlock textOutputPath;
        private CheckBox chkFixResolution;

        private Grid rangeGrid;
        private ColumnDefinition colStart;
        private ColumnDefinition colLeftHandle;
        private ColumnDefinition colActive;
        private ColumnDefinition colRightHandle;
        private ColumnDefinition colEnd;
        private Border rangeTrack;
        private Border leftHandle;
        private Border rightHandle;
        private Border playhead;

        private Button btnPlayPause;
        private Grid overlayGrid;
        private TextBlock overlayText;
        private TextBlock overlaySubtext;
        private ProgressBar overlayProgress;
        private StackPanel overlayButtonsPanel;
        private Button btnPlayOutput;
        private Button btnOpenOutputFolder;

        private Grid videoContainer;
        private StackPanel fallbackPanel;
        private TextBlock textPreviewFallback;
        private Button btnFfplayPreview;
        private Button btnInstallCodec;

        private TimeSpan startTime;
        private TimeSpan endTime;
        private TimeSpan totalDuration;
        private DispatcherTimer timer;
        private bool isBuffering = false;
        private bool isPlaying = false;
        private bool isMediaLoaded = false;
        private string tempPreviewFile = null;
        private int videoWidth = 0;
        private int videoHeight = 0;
        private bool wasPlayingBeforeDrag = false;
        private bool isDraggingSeekBar = false;
        private DateTime lastSeekTime = DateTime.MinValue;
        private bool isDraggingLeftHandle = false;
        private bool isDraggingRightHandle = false;
        private System.Diagnostics.Stopwatch playbackStopwatch = new System.Diagnostics.Stopwatch();
        private TimeSpan stopwatchBasePosition = TimeSpan.Zero;

        public TrimmerWindow(string videoPath)
        {
            this.inputFilePath = videoPath;
            this.Title = UI.Loc.Get("Video Trimmer - ", "Video Kırpıcı - ") + Path.GetFileName(videoPath);
            this.Width = 850;
            this.Height = 620;
            this.MinWidth = 700;
            this.MinHeight = 500;
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            this.Background = UI.BgDark;
            this.WindowStyle = WindowStyle.None;

            UI.SetupWindowChrome(this);
            InitializeUI();

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(15);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void InitializeUI()
        {
            Grid rootGrid = new Grid();
            rootGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto }); // Title bar
            rootGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) }); // Video (star)
            rootGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto }); // Seekbar
            rootGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto }); // Trim parameters
            rootGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto }); // Action bar

            // 1. Title bar
            var titleBar = UI.CreateTitleBar(this, UI.Loc.Get("Video Trimmer - ", "Video Kırpıcı - ") + Path.GetFileName(inputFilePath));
            Grid.SetRow(titleBar, 0);
            rootGrid.Children.Add(titleBar);

            // 2. Video Player area
            Border videoBorder = new Border();
            videoBorder.Background = Brushes.Black;
            videoBorder.Margin = new Thickness(12, 10, 12, 5);
            videoBorder.CornerRadius = new CornerRadius(8);
            videoBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(45, 45, 55));
            videoBorder.BorderThickness = new Thickness(1);
            videoBorder.ClipToBounds = true;
            Grid.SetRow(videoBorder, 1);

            videoContainer = new Grid();
            videoContainer.Background = Brushes.Black;

            mediaElement = new MediaElement();
            mediaElement.LoadedBehavior = MediaState.Manual;
            mediaElement.UnloadedBehavior = MediaState.Manual;
            mediaElement.Stretch = Stretch.Uniform;
            mediaElement.ScrubbingEnabled = true; // Frame-accurate seek preview while paused
            mediaElement.MediaOpened += MediaOpened;
            mediaElement.MediaFailed += MediaFailed;
            mediaElement.MouseDown += (s, e) => { TogglePlay(); };
            videoContainer.Children.Add(mediaElement);

            fallbackPanel = new StackPanel();
            fallbackPanel.VerticalAlignment = VerticalAlignment.Center;
            fallbackPanel.HorizontalAlignment = HorizontalAlignment.Center;
            fallbackPanel.Margin = new Thickness(25);

            textPreviewFallback = new TextBlock();
            textPreviewFallback.Text = UI.Loc.Get("Loading Video Preview...", "Video Önizleme Yükleniyor...");
            textPreviewFallback.Foreground = UI.TextGray;
            textPreviewFallback.FontSize = 14;
            textPreviewFallback.HorizontalAlignment = HorizontalAlignment.Center;
            textPreviewFallback.TextAlignment = TextAlignment.Center;
            textPreviewFallback.TextWrapping = TextWrapping.Wrap;
            textPreviewFallback.Margin = new Thickness(0, 0, 0, 15);
            fallbackPanel.Children.Add(textPreviewFallback);

            btnFfplayPreview = UI.CreateButton(UI.Loc.Get("► Start Preview with ffplay (External)", "► Önizlemeyi ffplay ile Başlat (Harici)"), UI.AccentCyan, new SolidColorBrush(Color.FromRgb(10, 186, 140)));
            btnFfplayPreview.HorizontalAlignment = HorizontalAlignment.Center;
            btnFfplayPreview.Padding = new Thickness(20, 10, 20, 10);
            btnFfplayPreview.FontSize = 13;
            btnFfplayPreview.FontWeight = FontWeights.Bold;
            btnFfplayPreview.Visibility = Visibility.Collapsed;
            btnFfplayPreview.Click += (s, e) => { PreviewWithFFplay(); };
            fallbackPanel.Children.Add(btnFfplayPreview);

            btnInstallCodec = UI.CreateButton(UI.Loc.Get("📦 Permanent Fix: Install Video Codec Pack (Free)", "📦 Kalıcı Çözüm: Video Kodek Paketini Yükle (Ücretsiz)"), UI.AccentBlue, Brushes.White);
            btnInstallCodec.HorizontalAlignment = HorizontalAlignment.Center;
            btnInstallCodec.Padding = new Thickness(20, 10, 20, 10);
            btnInstallCodec.FontSize = 13;
            btnInstallCodec.FontWeight = FontWeights.Bold;
            btnInstallCodec.Margin = new Thickness(0, 10, 0, 0);
            btnInstallCodec.Visibility = Visibility.Collapsed;
            btnInstallCodec.Click += (s, e) => { InstallCodecPack(); };
            fallbackPanel.Children.Add(btnInstallCodec);

            videoContainer.Children.Add(fallbackPanel);

            videoBorder.Child = videoContainer;
            rootGrid.Children.Add(videoBorder);

            // 3. Seek Panel
            StackPanel seekPanel = new StackPanel();
            seekPanel.Margin = new Thickness(12, 5, 12, 5);
            Grid.SetRow(seekPanel, 2);

            seekBar = new Slider();
            seekBar.Minimum = 0;
            seekBar.Maximum = 100;
            seekBar.Value = 0;
            seekBar.Height = 22;
            seekBar.Cursor = Cursors.Hand;
            seekBar.Margin = new Thickness(5, 0, 5, 0);
            seekBar.Visibility = Visibility.Collapsed; // Hide the separate seekbar
            seekBar.ValueChanged += SeekBar_ValueChanged;
            seekBar.PreviewMouseLeftButtonDown += SeekBar_PreviewMouseLeftButtonDown;
            seekBar.PreviewMouseLeftButtonUp += SeekBar_PreviewMouseLeftButtonUp;
            seekPanel.Children.Add(seekBar);

            // Range indicator track bar (iOS Style Double Slider)
            rangeTrack = new Border();
            rangeTrack.Background = new SolidColorBrush(Color.FromRgb(30, 30, 38)); // Dark track background
            rangeTrack.Height = 24;
            rangeTrack.CornerRadius = new CornerRadius(6);
            rangeTrack.BorderBrush = new SolidColorBrush(Color.FromRgb(45, 45, 58));
            rangeTrack.BorderThickness = new Thickness(1);
            rangeTrack.Margin = new Thickness(10, 8, 10, 12);
            rangeTrack.ClipToBounds = true;
            rangeTrack.Cursor = Cursors.Hand;
            rangeTrack.MouseLeftButtonDown += RangeTrack_MouseLeftButtonDown;
            rangeTrack.MouseMove += RangeTrack_MouseMove;
            rangeTrack.MouseLeftButtonUp += RangeTrack_MouseLeftButtonUp;
            rangeTrack.SizeChanged += (s, e) => { UpdateRangeBar(); };

            rangeGrid = new Grid();
            colStart = new ColumnDefinition() { Width = new GridLength(0, GridUnitType.Star) };
            colLeftHandle = new ColumnDefinition() { Width = GridLength.Auto };
            colActive = new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) };
            colRightHandle = new ColumnDefinition() { Width = GridLength.Auto };
            colEnd = new ColumnDefinition() { Width = new GridLength(0, GridUnitType.Star) };
            
            rangeGrid.ColumnDefinitions.Add(colStart);
            rangeGrid.ColumnDefinitions.Add(colLeftHandle);
            rangeGrid.ColumnDefinitions.Add(colActive);
            rangeGrid.ColumnDefinitions.Add(colRightHandle);
            rangeGrid.ColumnDefinitions.Add(colEnd);

            // iOS Accent Golden Yellow (#FBBF24)
            var iOSYellow = new SolidColorBrush(Color.FromRgb(251, 191, 36));
            var iOSYellowAlpha = new SolidColorBrush(Color.FromArgb(48, 251, 191, 36));

            Border activeBar = new Border();
            activeBar.Background = iOSYellowAlpha;
            activeBar.BorderThickness = new Thickness(0, 2, 0, 2);
            activeBar.BorderBrush = iOSYellow;
            Grid.SetColumn(activeBar, 2);
            rangeGrid.Children.Add(activeBar);

            // Left Handle (iOS Bracket Style)
            leftHandle = new Border();
            leftHandle.Width = 12;
            leftHandle.Height = 24;
            leftHandle.Background = iOSYellow;
            leftHandle.CornerRadius = new CornerRadius(4, 0, 0, 4);
            leftHandle.Cursor = Cursors.SizeWE;
            
            Grid leftInside = new Grid();
            Border leftLine = new Border();
            leftLine.Width = 2;
            leftLine.Height = 10;
            leftLine.Background = Brushes.White;
            leftLine.CornerRadius = new CornerRadius(1);
            leftInside.Children.Add(leftLine);
            leftHandle.Child = leftInside;
            
            leftHandle.MouseLeftButtonDown += LeftHandle_MouseLeftButtonDown;
            leftHandle.MouseMove += LeftHandle_MouseMove;
            leftHandle.MouseLeftButtonUp += LeftHandle_MouseLeftButtonUp;
            Grid.SetColumn(leftHandle, 1);
            rangeGrid.Children.Add(leftHandle);

            // Right Handle (iOS Bracket Style)
            rightHandle = new Border();
            rightHandle.Width = 12;
            rightHandle.Height = 24;
            rightHandle.Background = iOSYellow;
            rightHandle.CornerRadius = new CornerRadius(0, 4, 4, 0);
            rightHandle.Cursor = Cursors.SizeWE;
            
            Grid rightInside = new Grid();
            Border rightLine = new Border();
            rightLine.Width = 2;
            rightLine.Height = 10;
            rightLine.Background = Brushes.White;
            rightLine.CornerRadius = new CornerRadius(1);
            rightInside.Children.Add(rightLine);
            rightHandle.Child = rightInside;
            
            rightHandle.MouseLeftButtonDown += RightHandle_MouseLeftButtonDown;
            rightHandle.MouseMove += RightHandle_MouseMove;
            rightHandle.MouseLeftButtonUp += RightHandle_MouseLeftButtonUp;
            Grid.SetColumn(rightHandle, 3);
            rangeGrid.Children.Add(rightHandle);

            // Overlay container for rangeGrid and playhead
            Grid rangeContainer = new Grid();
            rangeContainer.Children.Add(rangeGrid);

            Canvas playheadCanvas = new Canvas();
            playheadCanvas.IsHitTestVisible = false;

            playhead = new Border();
            playhead.Width = 4;
            playhead.Background = Brushes.White;
            playhead.CornerRadius = new CornerRadius(2);
            playhead.Height = 28;
            playhead.Margin = new Thickness(0, -2, 0, -2);
            playhead.Visibility = Visibility.Collapsed;

            System.Windows.Media.Effects.DropShadowEffect shadow = new System.Windows.Media.Effects.DropShadowEffect();
            shadow.Color = Colors.Black;
            shadow.BlurRadius = 3;
            shadow.ShadowDepth = 1;
            shadow.Opacity = 0.5;
            playhead.Effect = shadow;

            playheadCanvas.Children.Add(playhead);
            rangeContainer.Children.Add(playheadCanvas);

            rangeTrack.Child = rangeContainer;
            seekPanel.Children.Add(rangeTrack);

            rootGrid.Children.Add(seekPanel);

            // 4. Trim Controls Panel
            Grid trimPanel = new Grid();
            trimPanel.Margin = new Thickness(15, 5, 15, 5);
            trimPanel.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            trimPanel.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            trimPanel.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            Grid.SetRow(trimPanel, 3);

            // Play controls and Time label
            StackPanel playControls = new StackPanel();
            playControls.Orientation = Orientation.Horizontal;
            playControls.VerticalAlignment = VerticalAlignment.Center;

            btnPlayPause = UI.CreateButton("", Brushes.Transparent, Brushes.Transparent);
            btnPlayPause.Width = 40;
            btnPlayPause.Padding = new Thickness(0);
            btnPlayPause.BorderThickness = new Thickness(0);
            btnPlayPause.FocusVisualStyle = null; // Remove focus rectangle
            UpdatePlayPauseIcon(false);
            btnPlayPause.Click += (s, e) => { TogglePlay(); };
            playControls.Children.Add(btnPlayPause);

            textCurrentTime = new TextBlock();
            textCurrentTime.Text = "00:00:00.000 / 00:00:00.000";
            textCurrentTime.Foreground = UI.TextWhite;
            textCurrentTime.FontSize = 12;
            textCurrentTime.VerticalAlignment = VerticalAlignment.Center;
            textCurrentTime.Margin = new Thickness(10, 0, 0, 0);
            textCurrentTime.FontFamily = new FontFamily("Consolas");
            playControls.Children.Add(textCurrentTime);

            Grid.SetColumn(playControls, 0);
            trimPanel.Children.Add(playControls);

            // Middle Panel: Duration details
            StackPanel durPanel = new StackPanel();
            durPanel.VerticalAlignment = VerticalAlignment.Center;
            durPanel.HorizontalAlignment = HorizontalAlignment.Center;
            durPanel.Margin = new Thickness(20, 0, 20, 0);

            TextBlock lblDur = new TextBlock();
            lblDur.Text = UI.Loc.Get("TRIMMED DURATION", "KIRPILAN SÜRE");
            lblDur.Foreground = UI.TextGray;
            lblDur.FontSize = 10;
            lblDur.FontWeight = FontWeights.Bold;
            lblDur.HorizontalAlignment = HorizontalAlignment.Center;
            durPanel.Children.Add(lblDur);

            textDuration = new TextBlock();
            textDuration.Text = "00:00:00.000";
            textDuration.Foreground = UI.AccentCyan;
            textDuration.FontSize = 18;
            textDuration.FontWeight = FontWeights.Bold;
            textDuration.HorizontalAlignment = HorizontalAlignment.Center;
            textDuration.FontFamily = new FontFamily("Consolas");
            durPanel.Children.Add(textDuration);

            Grid.SetColumn(durPanel, 1);
            trimPanel.Children.Add(durPanel);

            // Right Panel: Trim Set buttons and TextBoxes
            StackPanel trimActions = new StackPanel();
            trimActions.Orientation = Orientation.Horizontal;
            trimActions.HorizontalAlignment = HorizontalAlignment.Right;
            trimActions.VerticalAlignment = VerticalAlignment.Center;

            // Start Input
            StackPanel startInputGroup = new StackPanel();
            startInputGroup.Margin = new Thickness(0, 0, 10, 0);

            TextBlock lblStart = new TextBlock();
            lblStart.Text = UI.Loc.Get("Start (I)", "Başlangıç (I)");
            lblStart.Foreground = UI.TextGray;
            lblStart.FontSize = 11;
            lblStart.Margin = new Thickness(0, 0, 0, 2);
            startInputGroup.Children.Add(lblStart);

            StackPanel startHGroup = new StackPanel();
            startHGroup.Orientation = Orientation.Horizontal;

            var startTxtWrap = new Border();
            startTxtWrap.Background = new SolidColorBrush(Color.FromRgb(46, 46, 56));
            startTxtWrap.CornerRadius = new CornerRadius(4);
            startTxtWrap.BorderBrush = new SolidColorBrush(Color.FromRgb(70, 70, 85));
            startTxtWrap.BorderThickness = new Thickness(1);
            startTxtWrap.Padding = new Thickness(5, 3, 5, 3);

            textBoxStart = new TextBox();
            textBoxStart.Background = Brushes.Transparent;
            textBoxStart.BorderThickness = new Thickness(0);
            textBoxStart.Foreground = UI.TextWhite;
            textBoxStart.CaretBrush = UI.TextWhite;
            textBoxStart.Width = 90;
            textBoxStart.VerticalContentAlignment = VerticalAlignment.Center;
            textBoxStart.FontFamily = new FontFamily("Consolas");
            textBoxStart.LostFocus += TextBoxStart_LostFocus;
            textBoxStart.KeyDown += TextBoxStart_KeyDown;
            startTxtWrap.Child = textBoxStart;
            startHGroup.Children.Add(startTxtWrap);

            Button btnSetStart = UI.CreateButton("[", UI.BtnGray, UI.BtnGrayHover);
            btnSetStart.ToolTip = UI.Loc.Get("Set start to current position (Shortcut: I)", "Başlangıcı buraya ayarla (Klavye kısayolu: I)");
            btnSetStart.Width = 30;
            btnSetStart.Margin = new Thickness(4, 0, 0, 0);
            btnSetStart.Click += (s, e) => { SetStartToCurrent(); };
            startHGroup.Children.Add(btnSetStart);

            startInputGroup.Children.Add(startHGroup);
            trimActions.Children.Add(startInputGroup);

            // End Input
            StackPanel endInputGroup = new StackPanel();

            TextBlock lblEnd = new TextBlock();
            lblEnd.Text = UI.Loc.Get("End (O)", "Bitiş (O)");
            lblEnd.Foreground = UI.TextGray;
            lblEnd.FontSize = 11;
            lblEnd.Margin = new Thickness(0, 0, 0, 2);
            endInputGroup.Children.Add(lblEnd);

            StackPanel endHGroup = new StackPanel();
            endHGroup.Orientation = Orientation.Horizontal;

            var endTxtWrap = new Border();
            endTxtWrap.Background = new SolidColorBrush(Color.FromRgb(46, 46, 56));
            endTxtWrap.CornerRadius = new CornerRadius(4);
            endTxtWrap.BorderBrush = new SolidColorBrush(Color.FromRgb(70, 70, 85));
            endTxtWrap.BorderThickness = new Thickness(1);
            endTxtWrap.Padding = new Thickness(5, 3, 5, 3);

            textBoxEnd = new TextBox();
            textBoxEnd.Background = Brushes.Transparent;
            textBoxEnd.BorderThickness = new Thickness(0);
            textBoxEnd.Foreground = UI.TextWhite;
            textBoxEnd.CaretBrush = UI.TextWhite;
            textBoxEnd.Width = 90;
            textBoxEnd.VerticalContentAlignment = VerticalAlignment.Center;
            textBoxEnd.FontFamily = new FontFamily("Consolas");
            textBoxEnd.LostFocus += TextBoxEnd_LostFocus;
            textBoxEnd.KeyDown += TextBoxEnd_KeyDown;
            endTxtWrap.Child = textBoxEnd;
            endHGroup.Children.Add(endTxtWrap);

            Button btnSetEnd = UI.CreateButton("]", UI.BtnGray, UI.BtnGrayHover);
            btnSetEnd.ToolTip = UI.Loc.Get("Set end to current position (Shortcut: O)", "Bitişi buraya ayarla (Klavye kısayolu: O)");
            btnSetEnd.Width = 30;
            btnSetEnd.Margin = new Thickness(4, 0, 0, 0);
            btnSetEnd.Click += (s, e) => { SetEndToCurrent(); };
            endHGroup.Children.Add(btnSetEnd);

            endInputGroup.Children.Add(endHGroup);
            trimActions.Children.Add(endInputGroup);

            Grid.SetColumn(trimActions, 2);
            trimPanel.Children.Add(trimActions);

            rootGrid.Children.Add(trimPanel);

            // 5. Action Bar (Footer)
            Border actionBarBorder = new Border();
            actionBarBorder.Background = UI.BgPanel;
            actionBarBorder.Padding = new Thickness(15, 8, 15, 8);
            actionBarBorder.Margin = new Thickness(0, 10, 0, 0);
            Grid.SetRow(actionBarBorder, 4);

            Grid actionGrid = new Grid();
            actionGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            actionGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            StackPanel pathPanel = new StackPanel();
            pathPanel.VerticalAlignment = VerticalAlignment.Center;

            textOutputPath = new TextBlock();
            string defaultOut = GetDefaultOutputPath(inputFilePath);
            textOutputPath.Text = UI.Loc.Get("Output File: ", "Kaydedilecek Dosya: ") + Path.GetFileName(defaultOut);
            textOutputPath.Foreground = UI.TextWhite;
            textOutputPath.FontSize = 12;
            textOutputPath.FontWeight = FontWeights.Medium;
            pathPanel.Children.Add(textOutputPath);

            TextBlock textPathFolder = new TextBlock();
            textPathFolder.Text = UI.Loc.Get("Folder: ", "Klasör: ") + Path.GetDirectoryName(inputFilePath);
            textPathFolder.Foreground = UI.TextGray;
            textPathFolder.FontSize = 11;
            textPathFolder.TextTrimming = TextTrimming.CharacterEllipsis;
            textPathFolder.Margin = new Thickness(0, 2, 0, 0);
            pathPanel.Children.Add(textPathFolder);

            chkFixResolution = new CheckBox();
            chkFixResolution.Content = UI.Loc.Get("Fix Resolution Mismatch", "Çözünürlük Uyumsuzluğunu Düzelt");
            chkFixResolution.Foreground = UI.TextGray;
            chkFixResolution.FontSize = 11;
            chkFixResolution.Margin = new Thickness(0, 4, 0, 0);
            chkFixResolution.VerticalAlignment = VerticalAlignment.Center;
            chkFixResolution.Visibility = Visibility.Collapsed;
            pathPanel.Children.Add(chkFixResolution);

            Grid.SetColumn(pathPanel, 0);
            actionGrid.Children.Add(pathPanel);

            Button btnTrim = UI.CreateButton(UI.Loc.Get("TRIM AND SAVE", "KES VE KAYDET"), UI.AccentViolet, UI.AccentVioletHover);
            btnTrim.FontSize = 14;
            btnTrim.FontWeight = FontWeights.Bold;
            btnTrim.Padding = new Thickness(25, 10, 25, 10);
            btnTrim.Click += Trim_Click;
            Grid.SetColumn(btnTrim, 1);
            actionGrid.Children.Add(btnTrim);

            actionBarBorder.Child = actionGrid;
            rootGrid.Children.Add(actionBarBorder);

            // 6. Overlay Loading Screen
            overlayGrid = new Grid();
            overlayGrid.Background = new SolidColorBrush(Color.FromArgb(200, 18, 18, 24));
            overlayGrid.Visibility = Visibility.Collapsed;
            Grid.SetRowSpan(overlayGrid, 5); // Cover everything

            StackPanel overlayContent = new StackPanel();
            overlayContent.VerticalAlignment = VerticalAlignment.Center;
            overlayContent.HorizontalAlignment = HorizontalAlignment.Center;

            overlayText = new TextBlock();
            overlayText.Text = UI.Loc.Get("Starting Trim Process...", "Kırpma İşlemi Başlatılıyor...");
            overlayText.Foreground = UI.TextWhite;
            overlayText.FontSize = 20;
            overlayText.FontWeight = FontWeights.Bold;
            overlayText.HorizontalAlignment = HorizontalAlignment.Center;
            overlayText.Margin = new Thickness(0, 0, 0, 10);
            overlayContent.Children.Add(overlayText);

            overlaySubtext = new TextBlock();
            overlaySubtext.Text = UI.Loc.Get("Please wait...", "Lütfen bekleyin...");
            overlaySubtext.Foreground = UI.TextGray;
            overlaySubtext.FontSize = 13;
            overlaySubtext.HorizontalAlignment = HorizontalAlignment.Center;
            overlaySubtext.Margin = new Thickness(0, 0, 0, 20);
            overlayContent.Children.Add(overlaySubtext);

            overlayProgress = new ProgressBar();
            overlayProgress.Width = 320;
            overlayProgress.Height = 6;
            overlayProgress.IsIndeterminate = true;
            overlayProgress.Foreground = UI.AccentViolet;
            overlayProgress.Background = new SolidColorBrush(Color.FromRgb(42, 42, 53));

            Border progBorder = new Border();
            progBorder.CornerRadius = new CornerRadius(3);
            progBorder.ClipToBounds = true;
            progBorder.Child = overlayProgress;
            overlayContent.Children.Add(progBorder);

            // Success / Failure Buttons Panel
            overlayButtonsPanel = new StackPanel();
            overlayButtonsPanel.Orientation = Orientation.Horizontal;
            overlayButtonsPanel.HorizontalAlignment = HorizontalAlignment.Center;
            overlayButtonsPanel.Margin = new Thickness(0, 25, 0, 0);
            overlayButtonsPanel.Visibility = Visibility.Collapsed;

            btnPlayOutput = UI.CreateButton(UI.Loc.Get("Play Video", "Videoyu Oynat"), UI.AccentCyan, new SolidColorBrush(Color.FromRgb(10, 186, 140)));
            btnPlayOutput.Click += PlayOutput_Click;
            btnPlayOutput.Margin = new Thickness(0, 0, 10, 0);
            overlayButtonsPanel.Children.Add(btnPlayOutput);

            btnOpenOutputFolder = UI.CreateButton(UI.Loc.Get("Open Folder", "Klasörü Aç"), UI.AccentBlue, UI.AccentBlueHover);
            btnOpenOutputFolder.Click += OpenOutputFolder_Click;
            btnOpenOutputFolder.Margin = new Thickness(0, 0, 10, 0);
            overlayButtonsPanel.Children.Add(btnOpenOutputFolder);

            Button btnCloseOverlay = UI.CreateButton(UI.Loc.Get("Close", "Kapat"), UI.BtnGray, UI.BtnGrayHover);
            btnCloseOverlay.Click += CloseOverlay_Click;
            overlayButtonsPanel.Children.Add(btnCloseOverlay);

            overlayContent.Children.Add(overlayButtonsPanel);
            overlayGrid.Children.Add(overlayContent);

            rootGrid.Children.Add(overlayGrid);

            this.Content = rootGrid;

            // Set media source on Window Loaded event to ensure visual tree and handle are fully initialized
            this.Loaded += (s, e) => {
                try
                {
                    string absolutePath = Path.GetFullPath(inputFilePath);
                    Logger.Log("Window Loaded. Video aciliyor: " + absolutePath);
                    mediaElement.Source = new Uri(absolutePath);
                    mediaElement.Play(); // Force MediaElement to load and pre-roll

                    // 2-second timeout for native media engine loading
                    var timer = new System.Windows.Threading.DispatcherTimer();
                    timer.Interval = TimeSpan.FromSeconds(2.0);
                    timer.Tick += (st, te) => {
                        timer.Stop();
                        if (!isMediaLoaded)
                        {
                            Logger.Log("Media loading timed out after 2s. Falling back to FFmpeg duration detection.");
                            try { mediaElement.Close(); } catch {}
                            TriggerFFmpegFallback();
                        }
                    };
                    timer.Start();
                }
                catch (Exception ex)
                {
                    Logger.Log("Video yuklenirken hata: " + ex.Message);
                }
            };
        }

        private void MediaOpened(object sender, RoutedEventArgs e)
        {
            Logger.Log("MediaOpened: Video başarıyla yüklendi.");
            isMediaLoaded = true;
            fallbackPanel.Visibility = Visibility.Collapsed; // Hide the fallback panel completely

            totalDuration = mediaElement.NaturalDuration.HasTimeSpan ? 
                mediaElement.NaturalDuration.TimeSpan : TimeSpan.Zero;

            videoWidth = mediaElement.NaturalVideoWidth;
            videoHeight = mediaElement.NaturalVideoHeight;
            Logger.Log(string.Format("MediaOpened: cozunurluk tespiti. Genislik: {0}, Yukseklik: {1}", videoWidth, videoHeight));
            CheckResolutionCompliance();

            startTime = TimeSpan.Zero;
            endTime = totalDuration;

            seekBar.Minimum = 0;
            seekBar.Maximum = totalDuration.TotalSeconds;
            seekBar.Value = 0;
            seekBar.IsEnabled = true;

            textBoxStart.Text = FormatTime(startTime);
            textBoxEnd.Text = FormatTime(endTime);

            textCurrentTime.Text = FormatTime(TimeSpan.Zero) + " / " + FormatTime(totalDuration);

            UpdateRangeBar();
            UpdateDurationDisplay();

            // Pre-decode the first frame by pausing at 1ms, then start playing after a brief delay.
            // This prevents the common WPF MediaElement initial freeze caused by decoder warm-up.
            mediaElement.Pause();
            mediaElement.Position = TimeSpan.FromMilliseconds(1);

            var startDelay = new DispatcherTimer();
            startDelay.Interval = TimeSpan.FromMilliseconds(150);
            startDelay.Tick += (st, te) => {
                startDelay.Stop();
                mediaElement.Play();
                isPlaying = true;
                UpdatePlayPauseIcon(true);
                stopwatchBasePosition = TimeSpan.Zero;
                playbackStopwatch.Restart();
            };
            startDelay.Start();
        }

        private void TriggerFFmpegFallback()
        {
            if (isMediaLoaded) return; // Already loaded successfully

            isMediaLoaded = false;
            textPreviewFallback.Text = UI.Loc.Get("⚠️ Preview failed to load natively. Preparing preview in background, please wait...\n(This will take only a few seconds)", "⚠️ Önizleme yüklenemedi. Video önizlemesi arka planda hazırlanıyor, lütfen bekleyin...\n(Bu işlem sadece birkaç saniye sürecektir)");
            btnFfplayPreview.Visibility = Visibility.Visible;
            btnInstallCodec.Visibility = Visibility.Visible;

            string videoPath = inputFilePath;
            Task.Run(() => {
                // First get the duration and resolution using FFmpeg so we can populate the UI immediately
                TimeSpan duration;
                int ffWidth = 0;
                int ffHeight = 0;
                DetectVideoInfoWithFFmpeg(videoPath, out duration, out ffWidth, out ffHeight);
                
                this.Dispatcher.Invoke(() => {
                    videoWidth = ffWidth;
                    videoHeight = ffHeight;
                    CheckResolutionCompliance();

                    if (duration > TimeSpan.Zero)
                    {
                        totalDuration = duration;
                        startTime = TimeSpan.Zero;
                        endTime = totalDuration;

                        seekBar.Minimum = 0;
                        seekBar.Maximum = totalDuration.TotalSeconds;
                        seekBar.Value = 0;
                        seekBar.IsEnabled = true; // Enable seekbar for H.264 preview!

                        textBoxStart.Text = FormatTime(startTime);
                        textBoxEnd.Text = FormatTime(endTime);
                        textCurrentTime.Text = "00:00:00.000 / " + FormatTime(totalDuration);

                        UpdateRangeBar();
                        UpdateDurationDisplay();
                    }
                });

                // Start background transcoding to standard H.264
                string ffmpegPath = UI.FindFFmpegPath();
                if (string.IsNullOrEmpty(ffmpegPath) || !File.Exists(ffmpegPath))
                {
                    this.Dispatcher.Invoke(() => {
                        textPreviewFallback.Text = UI.Loc.Get("❌ FFmpeg not found, cannot generate preview.", "❌ FFmpeg bulunamadı, önizleme oluşturulamıyor.");
                    });
                    return;
                }

                tempPreviewFile = Path.Combine(Path.GetTempPath(), "trim_preview_" + Guid.NewGuid().ToString("N") + ".mp4");
                
                bool useNvenc = ProbeNvencSupport(ffmpegPath);
                string arguments;
                if (useNvenc)
                {
                    Logger.Log("NVIDIA NVENC donanim hizlandirma algilandi. H.264 onizleme dosyasi olusturuluyor: " + tempPreviewFile);
                    this.Dispatcher.Invoke(() => {
                        textPreviewFallback.Text = UI.Loc.Get("⚡ Creating super fast GPU (NVIDIA NVENC) preview, please wait...", "⚡ Ekran kartı (NVIDIA NVENC) ile süper hızlı önizleme oluşturuluyor, lütfen bekleyin...");
                    });
                    // qp 22 for excellent high quality, -g 1 (All-Intra) for instant lag-free seeking, scale to 720p height and crop to multiples of 16
                    arguments = string.Format("-y -i \"{0}\" -c:v h264_nvenc -preset p1 -rc constqp -qp 22 -g 1 -vf \"scale=-2:720,crop=w='iw-mod(iw,16)':h=720\" -c:a aac -b:a 96k -map 0:v:0 -map 0:a:0? \"{1}\"", videoPath, tempPreviewFile);
                }
                else
                {
                    Logger.Log("CPU ile H.264 onizleme dosyasi olusturuluyor: " + tempPreviewFile);
                    this.Dispatcher.Invoke(() => {
                        textPreviewFallback.Text = UI.Loc.Get("⏳ Creating CPU preview, please wait...\n(For faster and high-quality preview, you can install the Codec Pack below)", "⏳ İşlemci (CPU) ile önizleme oluşturuluyor, lütfen bekleyin...\n(Daha hızlı ve kaliteli önizleme için alttaki Kodek Paketini yükleyebilirsiniz)");
                    });
                    // crf 22 for excellent high quality, -g 1 (All-Intra) for instant lag-free seeking, scale to 720p height and crop to multiples of 16
                    arguments = string.Format("-y -i \"{0}\" -c:v libx264 -preset ultrafast -crf 22 -g 1 -vf \"scale=-2:720,crop=w='iw-mod(iw,16)':h=720\" -c:a aac -b:a 96k -map 0:v:0 -map 0:a:0? \"{1}\"", videoPath, tempPreviewFile);
                }

                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = ffmpegPath,
                    Arguments = arguments,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardError = true
                };

                try
                {
                    using (var process = Process.Start(psi))
                    {
                        string err = process.StandardError.ReadToEnd();
                        process.WaitForExit();
                        if (process.ExitCode == 0 && File.Exists(tempPreviewFile))
                        {
                            Logger.Log("H.264 onizleme dosyasi basariyla olusturuldu.");
                            this.Dispatcher.Invoke(() => {
                                // Load the transcoded H.264 file
                                isMediaLoaded = true;
                                fallbackPanel.Visibility = Visibility.Collapsed;
                                mediaElement.Source = new Uri(tempPreviewFile);
                                mediaElement.Play();
                                isPlaying = true;
                                UpdatePlayPauseIcon(true);
                                stopwatchBasePosition = TimeSpan.Zero;
                                playbackStopwatch.Restart();
                            });
                        }
                        else
                        {
                            Logger.Log("H.264 onizleme olusturulamadi. FFmpeg Hatası: " + err);
                            this.Dispatcher.Invoke(() => {
                                textPreviewFallback.Text = UI.Loc.Get("❌ Failed to create video preview.\nYou can still trim by entering times manually.", "❌ Video önizlemesi oluşturulamadı.\nSüreleri el ile girerek kesebilirsiniz.");
                                btnFfplayPreview.Visibility = Visibility.Visible;
                                btnInstallCodec.Visibility = Visibility.Visible;
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log("Transcode hatası: " + ex.Message);
                    this.Dispatcher.Invoke(() => {
                        textPreviewFallback.Text = UI.Loc.Get("❌ Preview error: ", "❌ Önizleme hatası: ") + ex.Message;
                        btnFfplayPreview.Visibility = Visibility.Visible;
                        btnInstallCodec.Visibility = Visibility.Visible;
                    });
                }
            });
        }

        private bool ProbeNvencSupport(string ffmpegPath)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = ffmpegPath,
                    Arguments = "-f lavfi -i color=c=black:s=256x256 -c:v h264_nvenc -t 0.01 -f null -",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardError = true
                };
                using (var process = Process.Start(psi))
                {
                    process.WaitForExit(1000); // 1 second timeout
                    return process.ExitCode == 0;
                }
            }
            catch
            {
                return false;
            }
        }

        private void InstallCodecPack()
        {
            btnInstallCodec.IsEnabled = false;
            textPreviewFallback.Text = UI.Loc.Get("⏳ Codec pack is being installed in the background...\nPlease approve the Windows Administrator Permission (UAC) prompt when it appears.", "⏳ Kodek paketi arka planda yükleniyor...\nLütfen açılacak olan Windows Yönetici İzni (UAC) penceresini onaylayın.");
            
            Task.Run(() => {
                try
                {
                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        FileName = "powershell.exe",
                        Arguments = "-NoProfile -ExecutionPolicy Bypass -Command \"winget install --id KLiteCodecPack.Basic --silent --accept-source-agreements --accept-package-agreements\"",
                        UseShellExecute = true,
                        Verb = "runas", // Request Administrator privileges (UAC prompt)
                        WindowStyle = ProcessWindowStyle.Hidden
                    };
                    
                    using (var process = Process.Start(psi))
                    {
                        process.WaitForExit();
                        if (process.ExitCode == 0)
                        {
                            this.Dispatcher.Invoke(() => {
                                textPreviewFallback.Text = UI.Loc.Get("✅ Codec installed successfully! Restarting application...", "✅ Kodek başarıyla yüklendi! Uygulama yeniden başlatılıyor...");
                                MessageBox.Show(UI.Loc.Get("Codec pack installed successfully. The application will restart to enable video preview.", "Kodek paketi başarıyla yüklendi. Video önizlemesini etkinleştirmek için uygulama yeniden başlatılacak."), UI.Loc.Get("Success", "Başarılı"), MessageBoxButton.OK, MessageBoxImage.Information);
                                
                                Process.Start(Process.GetCurrentProcess().MainModule.FileName, string.Format("\"{0}\"", inputFilePath));
                                Application.Current.Shutdown();
                            });
                        }
                        else
                        {
                            this.Dispatcher.Invoke(() => {
                                btnInstallCodec.IsEnabled = true;
                                textPreviewFallback.Text = UI.Loc.Get("❌ Codec installation failed or cancelled.\nYou can try manual installation or preview using ffplay.", "❌ Kodek yüklenemedi veya iptal edildi.\nManuel yüklemeyi deneyebilirsiniz veya ffplay ile önizleme yapabilirsiniz.");
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.Dispatcher.Invoke(() => {
                        btnInstallCodec.IsEnabled = true;
                        textPreviewFallback.Text = UI.Loc.Get("❌ Codec installation error: ", "❌ Kodek yükleme hatası: ") + ex.Message;
                    });
                }
            });
        }

        private void MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            Logger.Log("MediaFailed: Video önizleme yüklenemedi. Detay: " + e.ErrorException.Message);
            
            MessageBoxResult result = MessageBox.Show(
                UI.Loc.Get(
                    "This video file (HEVC/H.265 or special format) cannot be played directly by Windows.\n\n" +
                    "Instead of waiting for conversion every time, would you like to install the free Codec Pack to solve this issue PERMANENTLY on your computer?\n\n" +
                    "(The installation takes about 15-20 seconds, installs safe codecs to Windows, and allows all videos to open instantly.)",
                    "Bu video dosyası (HEVC/H.265 veya özel format) Windows tarafından doğrudan oynatılamıyor.\n\n" +
                    "Her seferinde dönüştürme beklemek yerine, bu sorunu bilgisayarınızda KALICI olarak çözmek için ücretsiz Kodek Paketini yüklemek ister misiniz?\n\n" +
                    "(Yükleme işlemi yaklaşık 15-20 saniye sürer, Windows'a güvenli kodekleri yükler ve tüm videoların anında açılmasını sağlar.)"
                ),
                UI.Loc.Get("Permanent Video Preview Solution", "Kalıcı Video Önizleme Çözümü"),
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                fallbackPanel.Visibility = Visibility.Visible;
                btnFfplayPreview.Visibility = Visibility.Collapsed;
                btnInstallCodec.Visibility = Visibility.Visible;
                InstallCodecPack();
            }
            else
            {
                TriggerFFmpegFallback();
            }
        }

        private void CheckResolutionCompliance()
        {
            if (!this.Dispatcher.CheckAccess())
            {
                this.Dispatcher.Invoke(new Action(CheckResolutionCompliance));
                return;
            }

            if (videoWidth <= 0 || videoHeight <= 0) return;

            bool isCompliant = (videoWidth % 8 == 0) && (videoHeight % 8 == 0);
            if (!isCompliant)
            {
                int cropW = videoWidth - (videoWidth % 8);
                int cropH = videoHeight - (videoHeight % 8);
                chkFixResolution.Content = string.Format(UI.Loc.Get("Fix Resolution Mismatch ({0}x{1} -> {2}x{3})", "Çözünürlük Uyumsuzluğunu Düzelt ({0}x{1} -> {2}x{3})"), 
                    videoWidth, videoHeight, cropW, cropH);
                chkFixResolution.ToolTip = UI.Loc.Get("Because this video has a non-standard resolution, players might experience green/white line issues. Trimming will automatically prevent this error (requires re-encoding).", "Bu video standart dışı çözünürlüğe sahip olduğu için oynatıcılarda yeşil/beyaz çizgi hatası oluşebilir. Kırpma işlemi ile bu hata otomatik önlenecektir (Yeniden kodlama gerektirir).");
                chkFixResolution.Visibility = Visibility.Visible;
                chkFixResolution.IsChecked = true;
            }
            else
            {
                chkFixResolution.Visibility = Visibility.Collapsed;
                chkFixResolution.IsChecked = false;
            }
        }

        private TimeSpan GetVideoDurationWithFFmpeg(string videoPath)
        {
            TimeSpan duration;
            int w, h;
            DetectVideoInfoWithFFmpeg(videoPath, out duration, out w, out h);
            return duration;
        }

        private void DetectVideoInfoWithFFmpeg(string videoPath, out TimeSpan duration, out int width, out int height)
        {
            duration = TimeSpan.Zero;
            width = 0;
            height = 0;
            try
            {
                string ffmpegPath = UI.FindFFmpegPath();
                Logger.Log("FFmpeg ile video bilgisi tespiti baslatiliyor. ffmpeg: " + ffmpegPath);
                ProcessStartInfo psi = new ProcessStartInfo(ffmpegPath, string.Format("-i \"{0}\"", videoPath));
                psi.UseShellExecute = false;
                psi.CreateNoWindow = true;
                psi.RedirectStandardError = true;

                using (var process = Process.Start(psi))
                {
                    string output = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    // Look for "Duration: 00:00:00.00,"
                    int index = output.IndexOf("Duration:");
                    if (index >= 0)
                    {
                        string durationStr = output.Substring(index + 9, 12).Trim();
                        TimeSpan.TryParse(durationStr, out duration);
                    }

                    // Look for video resolution
                    int videoStreamIndex = output.IndexOf("Video:");
                    if (videoStreamIndex >= 0)
                    {
                        string videoStreamInfo = output.Substring(videoStreamIndex);
                        var match = System.Text.RegularExpressions.Regex.Match(videoStreamInfo, @"\b(\d{3,5})x(\d{3,5})\b");
                        if (match.Success)
                        {
                            int.TryParse(match.Groups[1].Value, out width);
                            int.TryParse(match.Groups[2].Value, out height);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log("FFmpeg ile video bilgisi okunurken hata: " + ex.Message);
            }
        }

        private void PreviewWithFFplay()
        {
            try
            {
                string ffmpegPath = UI.FindFFmpegPath();
                if (string.IsNullOrEmpty(ffmpegPath))
                {
                    MessageBox.Show(UI.Loc.Get("FFmpeg/FFplay not found!", "FFmpeg/FFplay bulunamadı!"), UI.Loc.Get("Error", "Hata"), MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                
                string ffplayPath = ffmpegPath.Replace("ffmpeg.exe", "ffplay.exe");
                if (!File.Exists(ffplayPath))
                {
                    MessageBox.Show(UI.Loc.Get("ffplay.exe not found!\nIt must be in the same folder as ffmpeg.exe.", "ffplay.exe bulunamadı!\nffmpeg.exe ile aynı klasörde olmalıdır."), UI.Loc.Get("Error", "Hata"), MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                Logger.Log("ffplay baslatiliyor: " + ffplayPath + " Dosya: " + inputFilePath);
                
                // Start ffplay process
                string title = UI.Loc.Get("Preview (Press ESC to close)", "Önizleme (Kapatmak için ESC tuşuna basın)");
                ProcessStartInfo psi = new ProcessStartInfo(ffplayPath, string.Format("-window_title \"{0}\" \"{1}\"", title, inputFilePath));
                psi.UseShellExecute = true;
                psi.CreateNoWindow = false;
                
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                Logger.Log("ffplay baslatilamadı: " + ex.Message);
                MessageBox.Show(UI.Loc.Get("Failed to start preview player: ", "Önizleme oynatıcısı başlatılamadı: ") + ex.Message, UI.Loc.Get("Error", "Hata"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (isMediaLoaded && isPlaying && !seekBar.IsMouseCaptureWithin && seekBar.IsEnabled)
            {
                TimeSpan currentPos;
                if (playbackStopwatch.IsRunning)
                {
                    currentPos = stopwatchBasePosition + playbackStopwatch.Elapsed;
                    
                    // Periodic resync (if drift is > 0.2s) to ensure it stays in sync with actual playback
                    var drift = Math.Abs((currentPos - mediaElement.Position).TotalSeconds);
                    if (drift > 0.4)
                    {
                        // Give the video up to 1.5 seconds to start playing when at the very beginning
                        // This prevents the playhead from stuttering/jumping back to 0 repeatedly
                        if (mediaElement.Position.TotalSeconds > 0.1 || currentPos.TotalSeconds > 1.5)
                        {
                            stopwatchBasePosition = mediaElement.Position;
                            playbackStopwatch.Restart();
                            currentPos = stopwatchBasePosition;
                        }
                    }
                }
                else
                {
                    currentPos = mediaElement.Position;
                }

                if (currentPos > totalDuration) currentPos = totalDuration;

                seekBar.Value = currentPos.TotalSeconds;
                textCurrentTime.Text = FormatTime(currentPos) + " / " + FormatTime(totalDuration);
                UpdatePlayheadPosition(currentPos.TotalSeconds);

                // Loop back to start if past end - use Stop()+Play() to avoid WPF re-buffering freeze
                if (currentPos >= endTime)
                {
                    mediaElement.Stop();
                    mediaElement.Play();
                    if (startTime > TimeSpan.Zero)
                    {
                        mediaElement.Position = startTime;
                    }
                    stopwatchBasePosition = startTime;
                    playbackStopwatch.Restart();
                }
            }
        }

        private void SeekBar_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (isMediaLoaded)
            {
                isDraggingSeekBar = true;
                wasPlayingBeforeDrag = isPlaying;
                if (isPlaying)
                {
                    mediaElement.Pause();
                    isPlaying = false;
                    UpdatePlayPauseIcon(false);
                    playbackStopwatch.Stop();
                }
            }
        }

        private void SeekBar_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isMediaLoaded && isDraggingSeekBar)
            {
                isDraggingSeekBar = false;
                var seekTarget = TimeSpan.FromSeconds(seekBar.Value);
                if (wasPlayingBeforeDrag)
                {
                    SmartResume(seekTarget);
                }
                else
                {
                    mediaElement.Pause();
                    mediaElement.Position = seekTarget;
                    playbackStopwatch.Reset();
                    stopwatchBasePosition = seekTarget;
                }
            }
        }

        private void SeekBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (isMediaLoaded && seekBar.IsEnabled)
            {
                bool isUserAction = isDraggingSeekBar || seekBar.IsMouseCaptureWithin || seekBar.IsFocused;
                if (isUserAction)
                {
                    // Update time display text instantly so it feels responsive
                    textCurrentTime.Text = FormatTime(TimeSpan.FromSeconds(seekBar.Value)) + " / " + FormatTime(totalDuration);

                    // Throttle seeking to avoid WPF MediaElement lag/stutter (at most once every 60ms)
                    var now = DateTime.UtcNow;
                    if ((now - lastSeekTime).TotalMilliseconds >= 60 || !isDraggingSeekBar)
                    {
                        mediaElement.Position = TimeSpan.FromSeconds(seekBar.Value);
                        lastSeekTime = now;
                    }
                }
            }
        }

        private void TextBoxStart_LostFocus(object sender, RoutedEventArgs e)
        {
            var parsed = ParseTime(textBoxStart.Text, startTime);
            if (parsed >= TimeSpan.Zero && parsed < endTime)
            {
                startTime = parsed;
            }
            textBoxStart.Text = FormatTime(startTime);
            
            // Snap playhead
            if (isMediaLoaded)
            {
                mediaElement.Position = startTime;
                seekBar.Value = startTime.TotalSeconds;
                textCurrentTime.Text = FormatTime(startTime) + " / " + FormatTime(totalDuration);
                if (!isPlaying) { stopwatchBasePosition = startTime; playbackStopwatch.Reset(); }
            }
            
            UpdateRangeBar();
            UpdateDurationDisplay();
        }

        private void TextBoxStart_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Keyboard.ClearFocus();
            }
        }

        private void TextBoxEnd_LostFocus(object sender, RoutedEventArgs e)
        {
            var parsed = ParseTime(textBoxEnd.Text, endTime);
            if (parsed > startTime && parsed <= totalDuration)
            {
                endTime = parsed;
            }
            textBoxEnd.Text = FormatTime(endTime);
            
            // Snap playhead
            if (isMediaLoaded)
            {
                mediaElement.Position = endTime;
                seekBar.Value = endTime.TotalSeconds;
                textCurrentTime.Text = FormatTime(endTime) + " / " + FormatTime(totalDuration);
                if (!isPlaying) { stopwatchBasePosition = endTime; playbackStopwatch.Reset(); }
            }
            
            UpdateRangeBar();
            UpdateDurationDisplay();
        }

        private void TextBoxEnd_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Keyboard.ClearFocus();
            }
        }

        private void Trim_Click(object sender, RoutedEventArgs e)
        {
            if (!isMediaLoaded)
            {
                MessageBox.Show(UI.Loc.Get("Cannot trim because video or duration information has not been loaded.", "Video veya süre bilgisi yüklenmediği için kırpma yapılamaz."), UI.Loc.Get("Invalid Operation", "İşlem Geçersiz"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (isPlaying)
            {
                TogglePlay();
            }

            string defaultOut = GetDefaultOutputPath(inputFilePath);

            this.IsEnabled = false;

            overlayText.Text = UI.Loc.Get("Trimming Video...", "Video Kırpılıyor...");
            overlayText.Foreground = UI.TextWhite;
            overlaySubtext.Text = UI.Loc.Get("Performing FFmpeg lossless cut process...", "FFmpeg kayıpsız (lossless) kesim işlemi yapılıyor...");
            overlayProgress.IsIndeterminate = true;
            overlayProgress.Foreground = UI.AccentViolet;
            overlayProgress.Visibility = Visibility.Visible;
            overlayButtonsPanel.Visibility = Visibility.Collapsed;
            overlayGrid.Visibility = Visibility.Visible;

            double startSec = startTime.TotalSeconds;
            double durationSec = (endTime - startTime).TotalSeconds;

            if (durationSec <= 0)
            {
                ShowErrorOverlay(UI.Loc.Get("Error: Trimmed duration cannot be zero or negative!", "Hata: Kırpılan süre sıfır veya negatif olamaz!"));
                this.IsEnabled = true;
                return;
            }

            Logger.Log(string.Format("Kirpma tetiklendi. Baslangic: {0}s, Sure: {1}s, Cikti: {2}", startSec, durationSec, defaultOut));
            Task.Run(() => {
                PerformTrim(startSec, durationSec, defaultOut);
            });
        }

        private void PerformTrim(double startSec, double durationSec, string outputFilePath)
        {
            bool success = false;
            string errOutput = "";

            double fastSeek = 0;
            double slowSeek = startSec;
            if (startSec > 10)
            {
                fastSeek = startSec - 10;
                slowSeek = 10;
            }

            string fastSeekStr = fastSeek.ToString("F3", CultureInfo.InvariantCulture);
            string slowSeekStr = slowSeek.ToString("F3", CultureInfo.InvariantCulture);
            string startStr = startSec.ToString("F3", CultureInfo.InvariantCulture);
            string durStr = durationSec.ToString("F3", CultureInfo.InvariantCulture);

            string ffmpegPath = UI.FindFFmpegPath();

            bool shouldCrop = false;
            int cropW = 0;
            int cropH = 0;

            this.Dispatcher.Invoke(() => {
                if (chkFixResolution != null && chkFixResolution.Visibility == Visibility.Visible && chkFixResolution.IsChecked == true)
                {
                    shouldCrop = true;
                    cropW = videoWidth - (videoWidth % 8);
                    cropH = videoHeight - (videoHeight % 8);
                }
            });

            if (shouldCrop)
            {
                try
                {
                    bool useNvenc = ProbeNvencSupport(ffmpegPath);
                    string args;
                    if (useNvenc)
                    {
                        Logger.Log(string.Format("Re-encoding with NVENC hardware acceleration to fix resolution. Crop: {0}x{1}", cropW, cropH));
                        args = string.Format("-y -ss {0} -i \"{1}\" -t {2} -c:v h264_nvenc -preset p1 -rc constqp -qp 20 -vf \"crop={3}:{4}\" -c:a copy \"{5}\"",
                            startStr, inputFilePath, durStr, cropW, cropH, outputFilePath);
                    }
                    else
                    {
                        Logger.Log(string.Format("Re-encoding with CPU to fix resolution. Crop: {0}x{1}", cropW, cropH));
                        args = string.Format("-y -ss {0} -i \"{1}\" -t {2} -c:v libx264 -preset superfast -crf 20 -vf \"crop={3}:{4}\" -c:a copy \"{5}\"",
                            startStr, inputFilePath, durStr, cropW, cropH, outputFilePath);
                    }

                    Logger.Log("FFmpeg Kırpma/Yeniden Kodlama Başlatılıyor: " + ffmpegPath + " " + args);
                    ProcessStartInfo psi = new ProcessStartInfo(ffmpegPath, args);
                    psi.UseShellExecute = false;
                    psi.CreateNoWindow = true;
                    psi.RedirectStandardError = true;

                    using (Process process = Process.Start(psi))
                    {
                        errOutput = process.StandardError.ReadToEnd();
                        process.WaitForExit();
                        success = (process.ExitCode == 0);
                        Logger.Log("FFmpeg Kırpma/Yeniden Kodlama Sonucu: " + process.ExitCode);
                    }
                }
                catch (Exception ex)
                {
                    errOutput = ex.Message;
                    Logger.Log("FFmpeg Kırpma/Yeniden Kodlama Hatası: " + ex.Message);
                }
            }

            if (!success)
            {
                // Attempt 1: Smart lossless cut - ss placed before -i for keyframe-aligned fast seek.
                // avoid_negative_ts make_zero resets timestamps so players start cleanly at 0.
                try
                {
                    string args = string.Format("-y -ss {0} -i \"{1}\" -t {2} -c copy -map 0 -avoid_negative_ts make_zero \"{3}\"",
                        startStr, inputFilePath, durStr, outputFilePath);

                    Logger.Log("FFmpeg Attempt 1 (Lossless key-frame seek): " + ffmpegPath + " " + args);
                    ProcessStartInfo psi = new ProcessStartInfo(ffmpegPath, args);
                    psi.UseShellExecute = false;
                    psi.CreateNoWindow = true;
                    psi.RedirectStandardError = true;

                    using (Process process = Process.Start(psi))
                    {
                        errOutput = process.StandardError.ReadToEnd();
                        process.WaitForExit();
                        success = (process.ExitCode == 0);
                        Logger.Log("FFmpeg Attempt 1 Result: " + process.ExitCode + " StdErr length: " + (errOutput ?? "").Length);
                    }
                }
                catch (Exception ex)
                {
                    errOutput = ex.Message;
                    Logger.Log("FFmpeg Attempt 1 Error: " + ex.Message);
                }
            }

            // Attempt 2: Smart hybrid approach - re-encode a small segment at the cut point
            // to force a clean I-frame, then stream-copy the rest.  Zero freeze.
            if (!success)
            {
                try
                {
                    // Re-encode just this clip as libx264 ultrafast - quality loss minimal for short clips
                    string args = string.Format("-y -ss {0} -i \"{1}\" -t {2} -c:v libx264 -preset ultrafast -crf 18 -c:a aac -avoid_negative_ts make_zero \"{3}\"",
                        startStr, inputFilePath, durStr, outputFilePath);

                    Logger.Log("FFmpeg Attempt 2 (Re-encode ultrafast, no freeze): " + ffmpegPath + " " + args);
                    ProcessStartInfo psi = new ProcessStartInfo(ffmpegPath, args);
                    psi.UseShellExecute = false;
                    psi.CreateNoWindow = true;
                    psi.RedirectStandardError = true;

                    using (Process process = Process.Start(psi))
                    {
                        errOutput = process.StandardError.ReadToEnd();
                        process.WaitForExit();
                        success = (process.ExitCode == 0);
                        Logger.Log("FFmpeg Attempt 2 Result: " + process.ExitCode);
                    }
                }
                catch (Exception ex)
                {
                    errOutput += UI.Loc.Get("\n\nFallback error: ", "\n\nGeri dönüş hatası: ") + ex.Message;
                    Logger.Log("FFmpeg Attempt 2 Error: " + ex.Message);
                }
            }

            this.Dispatcher.Invoke(() => {
                this.IsEnabled = true;
                if (success)
                {
                    Logger.Log("Kırpma basariyla tamamlandi.");
                    ShowSuccessOverlay(outputFilePath);
                }
                else
                {
                    Logger.Log("Kırpma basarisiz oldu. Hata: " + errOutput);
                    ShowErrorOverlay(UI.Loc.Get("FFmpeg failed to perform the trim operation. The file format might be incompatible.\n\nDetails:\n", "FFmpeg kırpma işlemini gerçekleştiremedi. Dosya biçimi uyumsuz olabilir.\n\nDetay:\n") + errOutput);
                }
            });
        }

        private void ShowSuccessOverlay(string outPath)
        {
            finalOutputPath = outPath;
            overlayText.Text = UI.Loc.Get("Trim Completed Successfully!", "Kırpma Başarıyla Tamamlandı!");
            overlayText.Foreground = UI.AccentCyan;
            overlaySubtext.Text = UI.Loc.Get("Output File:\n", "Çıktı Dosyası:\n") + Path.GetFileName(outPath);
            overlayProgress.IsIndeterminate = false;
            overlayProgress.Value = 100;
            overlayProgress.Foreground = UI.AccentCyan;
            overlayButtonsPanel.Visibility = Visibility.Visible;
            btnPlayOutput.Visibility = Visibility.Visible;
            btnOpenOutputFolder.Visibility = Visibility.Visible;
        }

        private void ShowErrorOverlay(string errorDetails)
        {
            overlayText.Text = UI.Loc.Get("Trim Failed!", "Kırpma Başarısız Oldu!");
            overlayText.Foreground = new SolidColorBrush(Color.FromRgb(255, 107, 107));
            overlaySubtext.Text = errorDetails;
            overlayProgress.Visibility = Visibility.Collapsed;
            overlayButtonsPanel.Visibility = Visibility.Visible;
            btnPlayOutput.Visibility = Visibility.Collapsed;
            btnOpenOutputFolder.Visibility = Visibility.Collapsed;
        }

        private void PlayOutput_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(finalOutputPath) && File.Exists(finalOutputPath))
                {
                    Process.Start(finalOutputPath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(UI.Loc.Get("Failed to play file: ", "Dosya oynatılamadı: ") + ex.Message, UI.Loc.Get("Error", "Hata"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenOutputFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(finalOutputPath) && File.Exists(finalOutputPath))
                {
                    Process.Start("explorer.exe", string.Format("/select,\"{0}\"", finalOutputPath));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(UI.Loc.Get("Failed to open folder: ", "Klasör açılamadı: ") + ex.Message, UI.Loc.Get("Error", "Hata"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseOverlay_Click(object sender, RoutedEventArgs e)
        {
            overlayGrid.Visibility = Visibility.Collapsed;
            if (overlayText.Text.Contains("Başarıyla") || overlayText.Text.Contains("Successfully"))
            {
                string defaultOut = GetDefaultOutputPath(inputFilePath);
                textOutputPath.Text = UI.Loc.Get("Output File: ", "Kaydedilecek Dosya: ") + Path.GetFileName(defaultOut);
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.OriginalSource is TextBox) return;

            if (e.Key == Key.Space)
            {
                TogglePlay();
                e.Handled = true;
            }
            else if (e.Key == Key.Left)
            {
                SeekRelative(-2);
                e.Handled = true;
            }
            else if (e.Key == Key.Right)
            {
                SeekRelative(2);
                e.Handled = true;
            }
            else if (e.Key == Key.I)
            {
                SetStartToCurrent();
                e.Handled = true;
            }
            else if (e.Key == Key.O)
            {
                SetEndToCurrent();
                e.Handled = true;
            }
        }

        private void UpdatePlayPauseIcon(bool playing)
        {
            var path = new System.Windows.Shapes.Path();
            path.Fill = UI.TextWhite;
            path.Stretch = Stretch.Uniform;
            path.Width = 14;
            path.Height = 16;
            path.HorizontalAlignment = HorizontalAlignment.Center;
            path.VerticalAlignment = VerticalAlignment.Center;

            if (playing)
            {
                // Pause icon (two vertical bars)
                path.Data = Geometry.Parse("M0,0 H4 V16 H0 Z M10,0 H14 V16 H10 Z");
            }
            else
            {
                // Play icon (triangle)
                path.Data = Geometry.Parse("M0,0 L14,8 L0,16 Z");
                path.Margin = new Thickness(2, 0, 0, 0); // Visually center it
            }

            btnPlayPause.Content = path;
        }

        private void TogglePlay()
        {
            if (!isMediaLoaded || !seekBar.IsEnabled) return;
            if (isPlaying)
            {
                mediaElement.Pause();
                isPlaying = false;
                UpdatePlayPauseIcon(false);
                playbackStopwatch.Stop();
            }
            else
            {
                var resumePos = mediaElement.Position;
                if (resumePos >= endTime || resumePos >= totalDuration)
                    resumePos = startTime;
                SmartResume(resumePos);
            }
        }

        private void SeekRelative(double seconds)
        {
            if (!isMediaLoaded || !seekBar.IsEnabled) return;
            var newPos = mediaElement.Position + TimeSpan.FromSeconds(seconds);
            if (newPos < TimeSpan.Zero) newPos = TimeSpan.Zero;
            if (newPos > totalDuration) newPos = totalDuration;
            mediaElement.Position = newPos;
            seekBar.Value = newPos.TotalSeconds;
            textCurrentTime.Text = FormatTime(newPos) + " / " + FormatTime(totalDuration);
            
            // Sync stopwatch
            stopwatchBasePosition = newPos;
            if (isPlaying)
            {
                playbackStopwatch.Restart();
            }
            else
            {
                playbackStopwatch.Reset();
            }
        }

        // SmartResume: sets position, then waits a short delay before calling Play()
        // to let the WPF decoder pre-roll the first frame and avoid the initial freeze/stutter.
        private DispatcherTimer smartResumeTimer;
        private void SmartResume(TimeSpan targetPos)
        {
            if (smartResumeTimer != null)
            {
                smartResumeTimer.Stop();
                smartResumeTimer = null;
            }

            mediaElement.Pause();
            mediaElement.Position = targetPos;
            isPlaying = false;

            smartResumeTimer = new DispatcherTimer();
            smartResumeTimer.Interval = TimeSpan.FromMilliseconds(80);
            smartResumeTimer.Tick += (st, te) => {
                smartResumeTimer.Stop();
                smartResumeTimer = null;
                mediaElement.Play();
                isPlaying = true;
                UpdatePlayPauseIcon(true);
                stopwatchBasePosition = targetPos;
                playbackStopwatch.Restart();
            };
            smartResumeTimer.Start();
        }

        private void SetStartToCurrent()
        {
            if (!isMediaLoaded) return;
            var pos = seekBar.IsEnabled ? mediaElement.Position : TimeSpan.FromSeconds(seekBar.Value);
            if (pos < endTime)
            {
                startTime = pos;
                textBoxStart.Text = FormatTime(startTime);
                UpdateRangeBar();
                UpdateDurationDisplay();
            }
        }

        private void SetEndToCurrent()
        {
            if (!isMediaLoaded) return;
            var pos = seekBar.IsEnabled ? mediaElement.Position : TimeSpan.FromSeconds(seekBar.Value);
            if (pos > startTime)
            {
                endTime = pos;
                textBoxEnd.Text = FormatTime(endTime);
                UpdateRangeBar();
                UpdateDurationDisplay();
            }
        }

        private string FormatTime(TimeSpan time)
        {
            return string.Format("{0:D2}:{1:D2}:{2:D2}.{3:D3}", 
                time.Hours, time.Minutes, time.Seconds, time.Milliseconds);
        }

        private TimeSpan ParseTime(string text, TimeSpan defaultValue)
        {
            TimeSpan result;
            if (TimeSpan.TryParse(text, out result))
            {
                return result;
            }

            double seconds;
            if (double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out seconds))
            {
                return TimeSpan.FromSeconds(seconds);
            }

            string[] parts = text.Split(':');
            if (parts.Length == 2)
            {
                double m, s;
                if (double.TryParse(parts[0], out m) && double.TryParse(parts[1], out s))
                {
                    return TimeSpan.FromMinutes(m) + TimeSpan.FromSeconds(s);
                }
            }
            else if (parts.Length == 3)
            {
                double h, m, s;
                if (double.TryParse(parts[0], out h) && double.TryParse(parts[1], out m) && double.TryParse(parts[2], out s))
                {
                    return TimeSpan.FromHours(h) + TimeSpan.FromMinutes(m) + TimeSpan.FromSeconds(s);
                }
            }

            return defaultValue;
        }

        private void UpdateRangeBar()
        {
            if (totalDuration.TotalSeconds <= 0) return;

            double startSec = startTime.TotalSeconds;
            double endSec = endTime.TotalSeconds;
            double totalSec = totalDuration.TotalSeconds;

            if (startSec < 0) startSec = 0;
            if (endSec > totalSec) endSec = totalSec;
            if (startSec > endSec) startSec = endSec;

            double p0 = startSec / totalSec;
            double p1 = (endSec - startSec) / totalSec;
            double p2 = (totalSec - endSec) / totalSec;

            p0 = Math.Max(0.0001, p0);
            p1 = Math.Max(0.0001, p1);
            p2 = Math.Max(0.0001, p2);

            colStart.Width = new GridLength(p0, GridUnitType.Star);
            colActive.Width = new GridLength(p1, GridUnitType.Star);
            colEnd.Width = new GridLength(p2, GridUnitType.Star);

            UpdatePlayheadPosition(seekBar.Value);
        }

        private void UpdateDurationDisplay()
        {
            var dur = endTime - startTime;
            textDuration.Text = FormatTime(dur);
        }

        private void RangeTrack_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!isMediaLoaded) return;

            // Check if clicking handles first
            if (e.OriginalSource == leftHandle || e.OriginalSource == rightHandle)
                return;

            isDraggingSeekBar = true;
            wasPlayingBeforeDrag = isPlaying;
            if (isPlaying)
            {
                mediaElement.Pause();
                isPlaying = false;
                UpdatePlayPauseIcon(false);
                playbackStopwatch.Stop();
            }

            rangeTrack.CaptureMouse();
            UpdateScrubPosition(e.GetPosition(rangeTrack).X);
            e.Handled = true;
        }

        private void RangeTrack_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDraggingSeekBar && rangeTrack.IsMouseCaptured)
            {
                UpdateScrubPosition(e.GetPosition(rangeTrack).X);
                e.Handled = true;
            }
        }

        private void RangeTrack_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isDraggingSeekBar)
            {
                isDraggingSeekBar = false;
                rangeTrack.ReleaseMouseCapture();

                // Final position set
                double ratio = e.GetPosition(rangeTrack).X / rangeTrack.ActualWidth;
                ratio = Math.Max(0.0, Math.Min(1.0, ratio));
                TimeSpan targetPos = TimeSpan.FromSeconds(ratio * totalDuration.TotalSeconds);
                seekBar.Value = targetPos.TotalSeconds;

                if (wasPlayingBeforeDrag)
                {
                    SmartResume(targetPos);
                }
                else
                {
                    mediaElement.Pause();
                    mediaElement.Position = targetPos;
                    playbackStopwatch.Reset();
                    stopwatchBasePosition = targetPos;
                }

                e.Handled = true;
            }
        }

        private void UpdateScrubPosition(double mouseX)
        {
            double ratio = mouseX / rangeTrack.ActualWidth;
            ratio = Math.Max(0.0, Math.Min(1.0, ratio));
            double targetSeconds = ratio * totalDuration.TotalSeconds;

            // Update textCurrentTime and playhead instantly
            TimeSpan targetPos = TimeSpan.FromSeconds(targetSeconds);
            seekBar.Value = targetSeconds;
            textCurrentTime.Text = FormatTime(targetPos) + " / " + FormatTime(totalDuration);
            UpdatePlayheadPosition(targetSeconds);

            // Throttle seeking to avoid WPF MediaElement lag/stutter
            var now = DateTime.UtcNow;
            if ((now - lastSeekTime).TotalMilliseconds >= 150)
            {
                var safePos = targetPos.TotalSeconds < 0.01 ? TimeSpan.FromMilliseconds(10) : targetPos;
                mediaElement.Position = safePos;
                lastSeekTime = now;
            }
        }

        private void UpdatePlayheadPosition(double currentSeconds)
        {
            if (playhead == null || totalDuration.TotalSeconds <= 0) return;

            if (playhead.Visibility != Visibility.Visible)
            {
                playhead.Visibility = Visibility.Visible;
            }

            double ratio = currentSeconds / totalDuration.TotalSeconds;
            ratio = Math.Max(0.0, Math.Min(1.0, ratio));

            double trackWidth = rangeTrack.ActualWidth;
            if (trackWidth > 0)
            {
                double leftPos = ratio * trackWidth - (playhead.Width / 2.0);
                Canvas.SetLeft(playhead, leftPos);
            }
        }

        private void LeftHandle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (isMediaLoaded)
            {
                isDraggingLeftHandle = true;
                wasPlayingBeforeDrag = isPlaying;
                if (isPlaying)
                {
                    mediaElement.Pause();
                    isPlaying = false;
                    UpdatePlayPauseIcon(false);
                    playbackStopwatch.Stop();
                }
                
                // Snap playhead to start time instantly on click
                mediaElement.Position = startTime;
                seekBar.Value = startTime.TotalSeconds;
                textCurrentTime.Text = FormatTime(startTime) + " / " + FormatTime(totalDuration);
                UpdatePlayheadPosition(seekBar.Value);
                if (!wasPlayingBeforeDrag) { stopwatchBasePosition = startTime; playbackStopwatch.Reset(); }

                leftHandle.CaptureMouse();
                e.Handled = true;
            }
        }

        private void LeftHandle_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDraggingLeftHandle && totalDuration.TotalSeconds > 0)
            {
                Point p = e.GetPosition(rangeTrack);
                double ratio = p.X / rangeTrack.ActualWidth;
                ratio = Math.Max(0.0, Math.Min(ratio, 1.0));
                
                TimeSpan newStart = TimeSpan.FromSeconds(ratio * totalDuration.TotalSeconds);
                if (newStart < endTime - TimeSpan.FromMilliseconds(500))
                {
                    startTime = newStart;
                    textBoxStart.Text = FormatTime(startTime);
                    
                    // Show preview of the dragged handle
                    var now = DateTime.UtcNow;
                    if ((now - lastSeekTime).TotalMilliseconds >= 150)
                    {
                        var safePos = startTime.TotalSeconds < 0.01 ? TimeSpan.FromMilliseconds(10) : startTime;
                        mediaElement.Position = safePos;
                        lastSeekTime = now;
                    }
                    seekBar.Value = startTime.TotalSeconds;
                    textCurrentTime.Text = FormatTime(startTime) + " / " + FormatTime(totalDuration);
                    
                    UpdateRangeBar();
                    UpdateDurationDisplay();
                }
                e.Handled = true;
            }
        }

        private void LeftHandle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isDraggingLeftHandle)
            {
                isDraggingLeftHandle = false;
                leftHandle.ReleaseMouseCapture();
                
                if (wasPlayingBeforeDrag)
                {
                    SmartResume(startTime);
                }
                else
                {
                    mediaElement.Pause();
                    mediaElement.Position = startTime;
                    playbackStopwatch.Reset();
                    stopwatchBasePosition = startTime;
                }
                e.Handled = true;
            }
        }

        private void RightHandle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (isMediaLoaded)
            {
                isDraggingRightHandle = true;
                wasPlayingBeforeDrag = isPlaying;
                if (isPlaying)
                {
                    mediaElement.Pause();
                    isPlaying = false;
                    UpdatePlayPauseIcon(false);
                    playbackStopwatch.Stop();
                }
                
                // Snap playhead to end time instantly on click
                mediaElement.Position = endTime;
                seekBar.Value = endTime.TotalSeconds;
                textCurrentTime.Text = FormatTime(endTime) + " / " + FormatTime(totalDuration);
                UpdatePlayheadPosition(seekBar.Value);
                if (!wasPlayingBeforeDrag) { stopwatchBasePosition = endTime; playbackStopwatch.Reset(); }

                rightHandle.CaptureMouse();
                e.Handled = true;
            }
        }

        private void RightHandle_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDraggingRightHandle && totalDuration.TotalSeconds > 0)
            {
                Point p = e.GetPosition(rangeTrack);
                double ratio = p.X / rangeTrack.ActualWidth;
                ratio = Math.Max(0.0, Math.Min(ratio, 1.0));
                
                TimeSpan newEnd = TimeSpan.FromSeconds(ratio * totalDuration.TotalSeconds);
                if (newEnd > startTime + TimeSpan.FromMilliseconds(500))
                {
                    endTime = newEnd;
                    textBoxEnd.Text = FormatTime(endTime);
                    
                    // Show preview of the dragged handle
                    var now = DateTime.UtcNow;
                    if ((now - lastSeekTime).TotalMilliseconds >= 150)
                    {
                        var safePos = endTime.TotalSeconds < 0.01 ? TimeSpan.FromMilliseconds(10) : endTime;
                        mediaElement.Position = safePos;
                        lastSeekTime = now;
                    }
                    seekBar.Value = endTime.TotalSeconds;
                    textCurrentTime.Text = FormatTime(endTime) + " / " + FormatTime(totalDuration);
                    
                    UpdateRangeBar();
                    UpdateDurationDisplay();
                }
                e.Handled = true;
            }
        }

        private void RightHandle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isDraggingRightHandle)
            {
                isDraggingRightHandle = false;
                rightHandle.ReleaseMouseCapture();
                
                if (wasPlayingBeforeDrag)
                {
                    SmartResume(endTime);
                }
                else
                {
                    mediaElement.Pause();
                    mediaElement.Position = endTime;
                    playbackStopwatch.Reset();
                    stopwatchBasePosition = endTime;
                }
                e.Handled = true;
            }
        }

        private string GetDefaultOutputPath(string inputPath)
        {
            string dir = Path.GetDirectoryName(inputPath);
            string name = Path.GetFileNameWithoutExtension(inputPath);
            string ext = Path.GetExtension(inputPath);

            string outPath = Path.Combine(dir, name + "_trimmed" + ext);
            int index = 1;
            while (File.Exists(outPath))
            {
                outPath = Path.Combine(dir, string.Format("{0}_trimmed_{1}{2}", name, index++, ext));
            }
            return outPath;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            
            // Clean up temporary preview file
            if (!string.IsNullOrEmpty(tempPreviewFile) && File.Exists(tempPreviewFile))
            {
                try
                {
                    File.Delete(tempPreviewFile);
                    Logger.Log("Gecici onizleme dosyasi silindi: " + tempPreviewFile);
                }
                catch (Exception ex)
                {
                    Logger.Log("Gecici onizleme dosyasi silinirken hata: " + ex.Message);
                }
            }
        }
    }
}
