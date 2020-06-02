using System;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Serilog;

namespace Tauron.Application.Wpf.Implementation
{
    public class ImageHelper : IImageHelper
    {
        private readonly WeakReferenceCollection<KeyedImage> _cache = new WeakReferenceCollection<KeyedImage>();

        private readonly IPackUriHelper _packUriHelper;

        public ImageHelper(IPackUriHelper packUriHelper)
        {
            _packUriHelper = packUriHelper;
        }

        public ImageSource? Convert(Uri target, string assembly)
        {
            var source = _cache.FirstOrDefault(img => img.Key == target);
            var temp = source?.GetImage();
            if (temp != null) return temp;

            var flag = target.IsAbsoluteUri && target.Scheme == Uri.UriSchemeFile && target.OriginalString.ExisFile();
            if (!flag) flag = target.IsAbsoluteUri;

            if (!flag) flag = target.OriginalString.ExisFile();

            if (flag)
            {
                ImageSource imgSource = BitmapFrame.Create(target);
                _cache.Add(new KeyedImage(target, imgSource));
                return imgSource;
            }

            try
            {
                return BitmapFrame.Create(_packUriHelper.LoadStream(target.OriginalString, assembly));
            }
            catch (Exception e)
            {
                Log.ForContext<ImageHelper>().Warning(e, "Faild To CreateResult image");

                return null;
            }
        }

        public ImageSource? Convert(string uri, string assembly)
        {
            return Uri.TryCreate(uri, UriKind.RelativeOrAbsolute, out var target) ? Convert(target, assembly) : null;
        }

        private class KeyedImage : IWeakReference
        {
            private readonly WeakReference _source;

            public KeyedImage(Uri key, ImageSource source)
            {
                Key = key;
                _source = new WeakReference(source);
            }

            public Uri Key { get; }

            public bool IsAlive => _source.IsAlive;

            public ImageSource? GetImage()
            {
                return _source.Target as ImageSource;
            }
        }
    }
}