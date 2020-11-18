using System;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Functional.Maybe;
using Serilog;

namespace Tauron.Application.Wpf.Implementation
{
    public class ImageHelper : IImageHelper
    {
        private readonly WeakReferenceCollection<KeyedImage> _cache = new WeakReferenceCollection<KeyedImage>();

        private readonly IPackUriHelper _packUriHelper;

        public ImageHelper(IPackUriHelper packUriHelper) 
            => _packUriHelper = packUriHelper;

        public Maybe<ImageSource> Convert(Uri target, string assembly)
        {
            var source = _cache.FirstOrDefault(img => img.Key == target);
            var temp = source?.GetImage();
            if (temp != null) return temp.ToMaybe();

            var flag = target.IsAbsoluteUri && target.Scheme == Uri.UriSchemeFile && target.OriginalString.ExisFile();
            if (!flag) flag = target.IsAbsoluteUri;

            if (!flag) flag = target.OriginalString.ExisFile();

            if (flag)
            {
                ImageSource imgSource = BitmapFrame.Create(target);
                _cache.Add(new KeyedImage(target, imgSource));
                return imgSource.ToMaybe();
            }

            try
            {
                return BitmapFrame.Create(_packUriHelper.LoadStream(target.OriginalString, assembly)).ToMaybe<ImageSource>();
            }
            catch (Exception e)
            {
                Log.ForContext<ImageHelper>().Warning(e, "Faild To CreateResult image");

                return Maybe<ImageSource>.Nothing;
            }
        }

        public Maybe<ImageSource> Convert(string uri, string assembly)
        {
            return Uri.TryCreate(uri, UriKind.RelativeOrAbsolute, out var target) ? Convert(target, assembly) : Maybe<ImageSource>.Nothing;
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