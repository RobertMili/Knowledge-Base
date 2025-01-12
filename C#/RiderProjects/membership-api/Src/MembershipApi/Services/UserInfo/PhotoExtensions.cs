using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace MembershipAPI.Services.UserInfo
{
    using Models;

    public static class PhotoExtensions
    {
        public static Photo ResizeToJpeg(this Photo photo, PhotoSize size, byte jpegQuality = 80)
            => ResizeToJpeg(photo: photo.Bytes, width: size.Width, height: size.Height, jpegQuality: jpegQuality);

        public static Photo ResizeToJpeg(this Photo photo, int width, int height, byte jpegQuality = 80)
            => ResizeToJpeg(photo: photo.Bytes, width: width, height: height, jpegQuality: jpegQuality);

        public static Photo ResizeToJpeg(byte[] photo, PhotoSize size, byte jpegQuality = 80)
            => ResizeToJpeg(photo: photo, width: size.Width, height: size.Height, jpegQuality: jpegQuality);

        public static Photo ResizeToJpeg(byte[] photo, int width, int height, byte jpegQuality = 80)
        {
            using (Image resized = new Bitmap(width: width, height: height))
            {
                using (var stream = new MemoryStream(photo, false))
                using (var input = Image.FromStream(stream))
                using (Graphics g = Graphics.FromImage(resized))
                    g.DrawImage(input, 0, 0, width: width, height: height);
                using (var eps = new EncoderParameters(1))
                using (var ep = new EncoderParameter(Encoder.Quality, (long)Math.Min((byte)100, jpegQuality)))
                {
                    eps.Param[0] = ep;
                    using (var stream = new MemoryStream())
                    {
                        var ici = ImageCodecInfo.GetImageEncoders().First(ie => ie.FormatID == ImageFormat.Jpeg.Guid);
                        resized.Save(stream, ici, eps);
                        return new Photo(stream.ToArray(), width: width, height: height, ici.MimeType);
                    }
                }
            }
        }
    }
}
