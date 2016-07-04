using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Web.Mvc;
using System.IO;

namespace RobynHandMadeSoap.Models
{

    //A class for taking Image Information, an Image sent as Bytes and generating a full copy to the server as well as a resized version for use on the website
    //and returning the relative path name required by the browser to find the image on the website.
    //
    //TODO
    //Make IMAGE RESIZING DYNAMIC BASED ON BROWSER REQUEST
    //ALLOW FOR MULTIPLE IMAGE RESIZES FOR MAIN/THUMB/ETC
    public class UploadedFile
    {

        public string FileName {get;set;}
        public string ContentType { get; set; }
        public string FileExt { get; set; }
        public int FileSize { get; set; }
        public byte[] Contents { get; set; }
        public string FileRelPath { get; set; }
        public Dictionary<string, int[]> FilesToSave {get;set;}

        //Method to return Request information in the form of an UploadedFile class
        public static UploadedFile RetrieveFileFromRequest(HttpRequestBase Request)
        {
            string filename = null;
            string fileType = null;
            byte[] fileContents = null;
            string fileRelPath = null;
            Dictionary<string, int[]> filesToSave = new Dictionary<string, int[]>(2);

            if (Request.Files.Count > 0)
            { //we are uploading the old way
                var file = Request.Files[0];
                fileContents = new byte[file.ContentLength];
                file.InputStream.Read(fileContents, 0, file.ContentLength);
                fileType = file.ContentType;
                filename = file.FileName;
            }
            else if (Request.ContentLength > 0)
            {
                // Using FileAPI the content is in Request.InputStream!!!!
                fileContents = new byte[Request.ContentLength];
                Request.InputStream.Read(fileContents, 0, Request.ContentLength);
                filename = Request.Headers["X-File-Name"];
                fileType = Request.Headers["X-File-Type"];
                fileRelPath = Request.Headers["X-File-Path"];
                //

                int[] MainXY = { Int16.Parse(Request.Headers["X-File-MainX"]),
                             Int16.Parse(Request.Headers["X-File-MainY"])
                           };
                filesToSave.Add("Main", MainXY);

                if (Int16.Parse(Request.Headers["X-File-SubX"]) > 0)
                {
                    int[] ThumbXY = { Int16.Parse(Request.Headers["X-File-SubX"]),
                                Int16.Parse(Request.Headers["X-File-SubY"])
                              };
                    filesToSave.Add("Thumb", ThumbXY);
                }

            }

            //Return the file info passed from browser as a UploadFile object.
            return new UploadedFile()
            {
                FileName = Path.GetFileNameWithoutExtension(filename),
                ContentType = fileType,
                FileExt = fileType.Replace("image/", "").Replace("jpeg", "jpg"),
                FileSize = fileContents != null ? fileContents.Length : 0,
                Contents = fileContents,
                FileRelPath = fileRelPath,
                FilesToSave = filesToSave
            };
        }

        public string filePathCombine(string root, string relative,string pathSep = "\\", string file = "")
        {
            char[] pathSepChar = pathSep.ToCharArray();
            String result = root.TrimEnd(pathSepChar);
            result += (pathSep + relative.Replace("/", pathSep).TrimStart(pathSepChar)).TrimEnd(pathSepChar) + pathSep + file;
            return result;
        }

        public string BackupFile(string backupPath){
            //Generate the FULL server filename i.e. C:\..\...\..img.jpg
            string backupFileName = this.FileName + "_original." + this.FileExt;
            string savePathFull = filePathCombine(backupPath,this.FileRelPath,"\\",backupFileName);
            //savePathFull.Replace("\\\\", "\\").Replace("/","\\");
            //Save a backup of the file so we can resize or otherwise manipulate the image later if needed.
            File.WriteAllBytes(savePathFull, this.Contents );
            return savePathFull;
        }

        //Method to save both a copy and a converted version of the file
        /*
         * serverImageRootPath: Full IIS path to webserver Images root.  i.e C:\myIIS\mywebsite\robyn\Images"
         * relativePath: A unix style relative path from the above root.  i.e /shop/
         *               which combine to tell use to transfer the file to C:\myIIS\mywebsite\robyn\Images\shop\
         * 
         * 
         */
        public static string SaveFile(UploadedFile FileUploaded,string serverImageRootPath,string relativePath)
        {
            //Create a backup filename to store the original image
            //File.ContentType
            //string rpoWindows = relativePath.Replace("/","\\");
            //string backupFileName = Path.GetFileNameWithoutExtension() + "_original." + File.FileExt;
            string savePathFull = FileUploaded.BackupFile(serverImageRootPath);

            //The combined image root with offset
            string resizeBasePath = Path.GetDirectoryName(savePathFull);

            string savePathServer = resizeBasePath;
            string resizeFileName = FileUploaded.FileName + "." + FileUploaded.FileExt;
            string mainFileName = "";

            MemoryStream imageStream = new MemoryStream(FileUploaded.Contents);
            //Read the saved file back into memory
            //Bitmap x = new Bitmap(File.Contents )
            Bitmap b = new Bitmap(imageStream);


            foreach (KeyValuePair<string, int[]> x in FileUploaded.FilesToSave)
            {
                if (FileUploaded.FilesToSave.ContainsKey("Thumb")){
                    savePathServer = Path.Combine(resizeBasePath, x.Key);
                    if (x.Key == "Main") { mainFileName = x.Key + "/" + resizeFileName; }
                }else
                {
                    mainFileName = resizeFileName;
                }
                //Read the saved file back in to an Image for resizing and saving as the actual image to be used
                
                Image i = resizeImage(b, new Size(x.Value[0], x.Value[1])); //Size(WIDTH, HEIGHT)

                //Save the resized image
                string tempFN = Path.Combine(savePathServer,resizeFileName);
                saveJpeg(tempFN, (Bitmap)i, 100);

                //If this is the main Image to save, then retun the saved filename for saving in the DB later
                

                //Cleanup some memory
                i.Dispose();
                
            }
            //Cleanup some memory
            b.Dispose();
            imageStream.Dispose();

            string result = "/Images/" + relativePath + mainFileName;
            result = result.Replace("//","/").Replace("\\","/");

            return result;
        }

        

        //Method of saving a Bitmap object as a JPG FILE to the server
        public static void saveJpeg(string path, Bitmap img, long quality)
        {
            // Encoder parameter for image quality
            EncoderParameter qualityParam = new EncoderParameter(Encoder.Quality, quality);

            // Jpeg image codec
            ImageCodecInfo jpegCodec = getEncoderInfo("image/jpeg");

            if (jpegCodec == null)
                return;

            EncoderParameters encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = qualityParam;

            img.Save(path, jpegCodec, encoderParams);
        }

        //Determine Image Codec information for transforming the image
        public static ImageCodecInfo getEncoderInfo(string mimeType)
        {
            // Get image codecs for all image formats
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();

            // Find the correct image codec
            for (int i = 0; i < codecs.Length; i++)
                if (codecs[i].MimeType == mimeType)
                    return codecs[i];
            return null;
        }

        //Resize the image as required
        public static Image resizeImage(Image imgToResize, Size size)
        {
            int sourceWidth = imgToResize.Width;
            int sourceHeight = imgToResize.Height;

            float nPercent = 0;
            float nPercentW = 0;
            float nPercentH = 0;

            nPercentW = ((float)size.Width / (float)sourceWidth);
            nPercentH = ((float)size.Height / (float)sourceHeight);

            if (nPercentH < nPercentW)
                nPercent = nPercentH;
            else
                nPercent = nPercentW;

            int destWidth = (int)(sourceWidth * nPercent);
            int destHeight = (int)(sourceHeight * nPercent);

            Bitmap b = new Bitmap(destWidth, destHeight);
            Graphics g = Graphics.FromImage((Image)b);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            g.DrawImage(imgToResize, 0, 0, destWidth, destHeight);
            g.Dispose();

            return (Image)b;
        }

    }
}