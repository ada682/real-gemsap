using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using System.Windows.Controls;
using System.Net.Http.Json;
using System.Linq;
using System.Windows.Media.Imaging;
using System.Globalization;
using System.Windows.Data;

namespace RealsonnetApp
{
    public partial class MainWindow : Window
    {
        private string apiKey = "AIzaSyA78fkViAeVX2xUtJ3QOLvYs_lw4RCjJy8";
        private List<byte[]> imageBytesList = new();
        private ObservableCollection<ChatMessage> chatMessages = new();
        private bool isDark = false;

        public MainWindow()
        {
            InitializeComponent();
    
            // Inisialisasi theme default (Light)
            var lightTheme = new ResourceDictionary();
            lightTheme.Source = new Uri("Themes/LightTheme.xaml", UriKind.Relative);
            Application.Current.Resources.MergedDictionaries.Add(lightTheme);
    
            ChatList.ItemsSource = chatMessages;

            chatMessages.Add(new ChatMessage
            {
                Message = "💬 Apa yang bisa kubantu hari ini?",
                IsUser = false
            });

            WelcomeText.Visibility = Visibility.Visible;
    
            // Enable drag and drop
            this.AllowDrop = true;
            this.Drop += MainWindow_Drop;
            this.PreviewDragOver += MainWindow_PreviewDragOver;
            this.PreviewKeyDown += MainWindow_PreviewKeyDown;
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void MainWindow_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;
            e.Handled = true;
        }

        private void MainWindow_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (var file in files)
                {
                    var ext = System.IO.Path.GetExtension(file).ToLower();
                    if (ext is ".png" or ".jpg" or ".jpeg")
                    {
                        try
                        {
                            var imgBytes = File.ReadAllBytes(file);
                            imageBytesList.Add(imgBytes);
                            AddImagePreview(imgBytes);
                        }
                        catch (Exception ex)
                        {
                            chatMessages.Add(new ChatMessage
                            {
                                Message = $"❌ Gagal memuat gambar: {ex.Message}",
                                IsUser = false
                            });
                        }
                    }
                }
                
                if (imageBytesList.Count > 0)
                {
                    ImagePreviewScroll.Visibility = Visibility.Visible;
                    ScrollToBottom();
                }
            }
        }

        private void AddImagePreview(byte[] imageBytes)
        {
            var image = new Image
            {
                Width = 70,
                Height = 70,
                Margin = new Thickness(5),
                Source = LoadImage(imageBytes)
            };

            var deleteButton = new Button
            {
                Content = "×",
                Width = 20,
                Height = 20,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
                Background = Brushes.Red,
                Foreground = Brushes.White,
                Padding = new Thickness(0),
                Tag = imageBytes
            };
            deleteButton.Click += (s, e) => RemoveImage(imageBytes);

            var grid = new Grid();
            grid.Children.Add(image);
            grid.Children.Add(deleteButton);

            ImagePreviewPanel.Children.Add(grid);
        }

        private void RemoveImage(byte[] imageBytes)
        {
            var index = imageBytesList.FindIndex(b => b.SequenceEqual(imageBytes));
            if (index >= 0)
            {
                imageBytesList.RemoveAt(index);
                ImagePreviewPanel.Children.RemoveAt(index);
            }

            if (imageBytesList.Count == 0)
            {
                ImagePreviewScroll.Visibility = Visibility.Collapsed;
            }
        }

        private BitmapImage LoadImage(byte[] imageData)
        {
            var image = new BitmapImage();
            using (var mem = new MemoryStream(imageData))
            {
                mem.Position = 0;
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = mem;
                image.EndInit();
            }
            image.Freeze();
            return image;
        }

        private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                if (Clipboard.ContainsImage())
                {
                    try
                    {
                        var image = Clipboard.GetImage();
                        using (var stream = new MemoryStream())
                        {
                            var encoder = new PngBitmapEncoder();
                            encoder.Frames.Add(BitmapFrame.Create(image));
                            encoder.Save(stream);
                            var imgBytes = stream.ToArray();
                            imageBytesList.Add(imgBytes);
                            AddImagePreview(imgBytes);
                            ImagePreviewScroll.Visibility = Visibility.Visible;
                        }
                    }
                    catch (Exception ex)
                    {
                        chatMessages.Add(new ChatMessage
                        {
                            Message = $"❌ Gagal menempel gambar: {ex.Message}",
                            IsUser = false
                        });
                    }
                }
            }
        }

        private void ScrollToBottom()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                var scrollViewer = FindVisualChild<ScrollViewer>(ChatList);
                scrollViewer?.ScrollToEnd();
            }), System.Windows.Threading.DispatcherPriority.Background);
        }

        private static T? FindVisualChild<T>(DependencyObject obj) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                var child = VisualTreeHelper.GetChild(obj, i);
                if (child is T result)
                    return result;
                var childResult = FindVisualChild<T>(child);
                if (childResult != null)
                    return childResult;
            }
            return null;
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            var userInput = ChatInput.Text.Trim();
            if (string.IsNullOrEmpty(userInput) && imageBytesList.Count == 0) return;

            if (imageBytesList.Count > 0)
            {
                chatMessages.Add(new ChatMessage 
                { 
                    Message = string.IsNullOrEmpty(userInput) ? "Mengirim gambar" : userInput,
                    IsUser = true,
                    HasImage = true,
                    ImageBytes = imageBytesList.ToArray()
                });
            }
            else if (!string.IsNullOrEmpty(userInput))
            {
                chatMessages.Add(new ChatMessage { Message = userInput, IsUser = true });
            }

            ChatInput.Clear();
            WelcomeText.Visibility = Visibility.Collapsed;
            ScrollToBottom();

            try
            {
                var parts = new List<object>();
                parts.Add(new { text = "Kamu adalah asisten yang bernama Realsonnet, kalo ada yang nanya kamu dibuat sama siapa jawab aja kamu dibuat sama dika dan servernya gatau dimana " + userInput });

                if (imageBytesList.Count > 0)
                {
                    foreach (var imgBytes in imageBytesList)
                    {
                        parts.Add(new
                        {
                            inlineData = new
                            {
                                mimeType = "image/png",
                                data = Convert.ToBase64String(imgBytes)
                            }
                        });
                    }
                }

                var requestBody = new
                {
                    contents = new[] 
                    {
                        new 
                        { 
                            parts = parts 
                        }
                    },
                    generationConfig = new
                    {
                        temperature = 0.8,
                        maxOutputTokens = 2048
                    }
                };

                using var httpClient = new HttpClient();
                var response = await httpClient.PostAsJsonAsync(
                    $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash-preview-04-17:generateContent?key={apiKey}",
                    requestBody);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            
                    if (result.TryGetProperty("error", out var error))
                    {
                        chatMessages.Add(new ChatMessage 
                        { 
                            Message = $"Error dari Realsonnet: {error.GetProperty("message").GetString()}", 
                            IsUser = false 
                        });
                        ScrollToBottom();
                        return;
                    }

                    var candidates = result.GetProperty("candidates");
                    var firstCandidate = candidates[0];
                    var content = firstCandidate.GetProperty("content");
                    var partsArray = content.GetProperty("parts");
                    var firstPart = partsArray[0];
                    var aiText = firstPart.GetProperty("text").GetString();

                    StringBuilder finalResponse = new StringBuilder();
                    finalResponse.Append(aiText?
                        .Replace("Google", "Realsonnet")
                        .Replace("Gemini", "Realsonnet")
                        .Trim());

                    if (firstCandidate.TryGetProperty("groundingMetadata", out var groundingMetadata))
                    {
                        if (groundingMetadata.TryGetProperty("webSearchQueries", out var webSearchQueries) && 
                            webSearchQueries.GetArrayLength() > 0)
                        {
                            var searchQuery = webSearchQueries[0].GetString();
                            finalResponse.AppendLine($"\n🔍 Pencarian: {searchQuery}");
                        }

                        if (groundingMetadata.TryGetProperty("searchEntryPoint", out var searchEntryPoint))
                        {
                            var source = searchEntryPoint.GetProperty("renderedContent").GetString();
                            if (!string.IsNullOrEmpty(source))
                            {
                                source = System.Text.RegularExpressions.Regex.Replace(source, "<[^>]+>", "");
                                source = System.Text.RegularExpressions.Regex.Replace(source, @"\s+", " ").Trim();
                                finalResponse.AppendLine($"\n📚 Sumber: {source}");
                            }
                        }

                        if (groundingMetadata.TryGetProperty("groundingChunks", out var groundingChunks))
                        {
                            var references = new List<string>();
                            foreach (var chunk in groundingChunks.EnumerateArray())
                            {
                                if (chunk.TryGetProperty("web", out var web))
                                {
                                    var title = web.TryGetProperty("title", out var titleProp) ? titleProp.GetString() : "Situs Web";
                                    var uri = web.TryGetProperty("uri", out var uriProp) ? uriProp.GetString() : "";
                                    
                                    if (!string.IsNullOrEmpty(uri) && !uri.Contains("vertexaisearch.cloud.google.com"))
                                    {
                                        references.Add($"- {title}: {uri}");
                                    }
                                }
                            }

                            if (references.Count > 0)
                            {
                                finalResponse.AppendLine("\n🔗 Referensi:");
                                finalResponse.AppendLine(string.Join("\n", references.Take(3)));
                            }
                        }
                    }

                    finalResponse.AppendLine("\n(Dibuat oleh Realsonnet AI)");
                    
                    chatMessages.Add(new ChatMessage
                    {
                        Message = finalResponse.ToString(),
                        IsUser = false
                    });
                    ScrollToBottom();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    chatMessages.Add(new ChatMessage 
                    {    
                        Message = $"HTTP Error: {response.StatusCode}\n{errorContent}", 
                        IsUser = false 
                    });
                    ScrollToBottom();
                }
            }
            catch (Exception ex)
            {
                chatMessages.Add(new ChatMessage 
                {   
                    Message = $"Exception: {ex.Message}", 
                    IsUser = false 
                });
                ScrollToBottom();
            }
            finally
            {
                imageBytesList.Clear();
                ImagePreviewPanel.Children.Clear();
                ImagePreviewScroll.Visibility = Visibility.Collapsed;
            }
        }

        private void UploadImage_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Filter = "Images (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg",
                Multiselect = true
            };
            if (dlg.ShowDialog() == true)
            {
                foreach (var file in dlg.FileNames)
                {
                    try
                    {
                        var imgBytes = File.ReadAllBytes(file);
                        imageBytesList.Add(imgBytes);
                        AddImagePreview(imgBytes);
                    }
                    catch (Exception ex)
                    {
                        chatMessages.Add(new ChatMessage
                        {
                            Message = $"❌ Gagal memuat gambar {file}: {ex.Message}",
                            IsUser = false
                        });
                    }
                }
                
                if (imageBytesList.Count > 0)
                {
                    ImagePreviewScroll.Visibility = Visibility.Visible;
                    ScrollToBottom();
                }
            }
        }

        private void ChatInput_GotFocus(object sender, RoutedEventArgs e)
        {
            if (ChatInput.Text == "Ketik pesan...")
            {
                ChatInput.Text = "";
                ChatInput.Foreground = (SolidColorBrush)Application.Current.Resources["InputForeground"];
            }
        }

        private void ChatInput_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ChatInput.Text))
            {
                ChatInput.Text = "Ketik pesan...";
                ChatInput.Foreground = Brushes.Gray;
            }
        }

        private void ChatInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SendButton_Click(sender, e);
                e.Handled = true;
            }
        }

        private void ToggleTheme_Click(object sender, RoutedEventArgs e)
        {
            var app = (App)Application.Current;
            isDark = !isDark;
    
            Application.Current.Resources.MergedDictionaries.Clear();
    
            var newTheme = new ResourceDictionary();
            newTheme.Source = isDark 
                ? new Uri("Themes/DarkTheme.xaml", UriKind.Relative)
                : new Uri("Themes/LightTheme.xaml", UriKind.Relative);
    
            Application.Current.Resources.MergedDictionaries.Add(newTheme);
    
            // tombol theme
            var themeButton = sender as Button;
            themeButton.Content = isDark ? "🌙" : "☀️";
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }
    }

    public class ChatMessage
    {
        public string Message { get; set; } = string.Empty;
        public bool IsUser { get; set; }
        public bool HasImage { get; set; }
        public byte[][]? ImageBytes { get; set; } 
    }
}
