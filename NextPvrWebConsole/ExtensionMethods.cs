using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Drawing;
using System.IO;

namespace NextPvrWebConsole
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// Replaces one or more format items in a specified string with the string representation of a specified object.
        /// </summary>
        /// <param name="Input">A composite format string.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <returns>A copy of format in which the format items have been replaced by the string representation of the corresponding objects in args.</returns>
        public static string FormatStr(this string Input, params object[] args)
        {
            return String.Format(Input, args);
        }
        /// <summary>
        /// Converts a long into a date time
        /// </summary>
        /// <param name="unixTime">the seconds since 1 jan 1970</param>
        /// <returns>a dateime</returns>
        public static DateTime FromUnixTime(this long unixTime)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddSeconds(unixTime);
        }
        /// <summary>
        /// Converts a date to the seconds since 1 jan 1970
        /// </summary>
        /// <param name="date">the date</param>
        /// <returns>the number of milliseconds</returns>
        public static long ToUnixTime(this DateTime date)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return Convert.ToInt64((date - epoch).TotalSeconds);
        }

        public static NextPvrWebConsole.Models.User GetUser(this System.Web.Http.ApiController Controller)
        {
            var user = NextPvrWebConsole.Models.User.GetByUsername(Controller.User.Identity.Name);
            if (user == null)
                throw new UnauthorizedAccessException();
            return user;
        }

        public static NextPvrWebConsole.Models.User GetUser(this System.Web.Mvc.Controller Controller)
        {
            var user = NextPvrWebConsole.Models.User.GetByUsername(Controller.User.Identity.Name);
            if (user == null)
                throw new UnauthorizedAccessException();
            return user;
        }

        public enum ResizeMode {
            Fill = 0,
            Shrink = 1
        }
        /// <summary>
        /// Converts an image to a base 64 string
        /// </summary>
        /// <param name="image">the image to convert</param>
        /// <returns>a base64 version of the image</returns>
        public static string ToBase64String(this Image image, int DesiredWidth = 64, int DesiredHeight = 64, ResizeMode Mode = ResizeMode.Fill)
        {
            Func<Image, int, int, int, int, int, int, Image> resize = delegate(Image input, int ImageWidth, int ImageHeight, int xPos, int yPos, int width, int height){
                Bitmap bmp = new Bitmap(ImageWidth, ImageHeight);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.DrawImage(input, xPos, yPos, width, height);
                }
                return bmp;
            };
            using (MemoryStream ms = new MemoryStream())
            {
                try
                {
                    // Convert Image to byte[]
                    if (DesiredWidth <= 0 && DesiredHeight <= 0)
                    {
                        // no resizing needed
                        image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    }
                    else
                    {
                        int w = image.Width, h = image.Height, xPos = 0, yPos = 0, targetWidth = image.Width, targetHeight = image.Height;
                        if(DesiredWidth <= 0)
                        {
                            // no width restrictions
                            float multiplier = image.Height / DesiredHeight;
                            h = DesiredHeight;
                            w = (int)(image.Width / multiplier);                        
                        }
                        else if (DesiredHeight <= 0)
                        {
                            // no height restrictions
                            float multiplier = image.Width / DesiredWidth;
                            w = DesiredWidth;
                            h = (int)(image.Height / multiplier);
                        }
                        else
                        {
                            // best fit
                            w = DesiredWidth;
                            h = (int)((((float)DesiredWidth) / image.Width) * image.Height);
                            if (h > DesiredHeight)
                            {
                                h = DesiredHeight;
                                w = (int)((((float)DesiredHeight) / image.Height) * image.Width);
                            }

                            if (Mode == ResizeMode.Fill)
                            {
                                yPos = (DesiredHeight - h) / 2;
                                xPos = (DesiredWidth - w) / 2;
                                targetWidth = DesiredWidth;
                                targetHeight = DesiredHeight;
                            }
                            else
                            {
                                targetHeight = h;
                                targetWidth = w;
                            }
                        }
                        using (Image temp = resize(image, targetWidth, targetHeight, xPos, yPos, w, h))
                        {
                            temp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                        }
                    }
                    byte[] imageBytes = ms.ToArray();

                    // Convert byte[] to Base64 String
                    string base64String = Convert.ToBase64String(imageBytes);
                    return base64String;
                }
                catch (Exception ex) { 
                    return null; 
                }
            }
        }
    }
}