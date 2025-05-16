using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Markup;  // Add this using
using System.Windows.Media.Imaging;

namespace RealsonnetApp
{
    public class BytesToImageConverter : MarkupExtension, IValueConverter
    {
        private static BytesToImageConverter? _instance;

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return _instance ??= new BytesToImageConverter();
        }

        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is byte[] bytes && bytes.Length > 0)
            {
                var image = new BitmapImage();
                using (var mem = new MemoryStream(bytes))
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
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}