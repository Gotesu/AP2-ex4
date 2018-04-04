using ImageService.Infrastructure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Media.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ImageService.Modal
{
	public class ImageServiceModal : IImageServiceModal
	{
		#region Members
		// The Output Folder
		private string m_OutputFolder = @"C:";
		public string OutputFolder() { return m_OutputFolder; }
		// The Size Of The Thumbnail Size
		private int m_thumbnailSize = 120;
		public int thumbnailSize() { return m_thumbnailSize; }

		#endregion
		public string DateTaken(string imagePath)
		{
			string date;
			// open the file
			using (FileStream fs = File.OpenRead(imagePath))
			{
				// get the bitmap
				BitmapSource img = BitmapFrame.Create(fs);
				BitmapMetadata md = (BitmapMetadata)img.Metadata;
				// get the date-taken info
				date = md.DateTaken;
			}
			return date;
		}

		public void CopyFile(string sourcePath, string destPath, bool over = false)
		{
			// copy the file
			File.Copy(sourcePath, destPath, over);
		}

		public void MoveFile(string sourcePath, string destPath)
		{
			// move the file
			File.Move(sourcePath, destPath);
		}

		public void MoveFolder(string sourcePath, string destPath)
		{
			// move the folder
			Directory.Move(sourcePath, destPath);
		}

		public void DeleteFile(string path)
		{
			// delete the file
			File.Delete(path);
		}

		public void DeleteFolder(string path)
		{
			// delete the folder
			Directory.Delete(path);
		}

		public void CreateThumbnail(string sourcePath, string destPath)
		{
			Image image = Image.FromFile(sourcePath);
			Image thumb = image.GetThumbnailImage(
				thumbnailSize(), thumbnailSize(), () => false, IntPtr.Zero);
			thumb.Save(destPath);
		}

		public void CreateFolder(string path, bool hidden = false)
		{
			// check if the folder exists
			if (!Directory.Exists(path))
			{
				// create the folder
				DirectoryInfo di = Directory.CreateDirectory(path);
				if (hidden)
				{
					// set the new folder to hidden
					di.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
				}
			}
		}

		public string AddFile(string path, out bool result)
		{
			// defult status, until all the stages are done
			result = false;
			try
			{
				// create OutputDir folder
				CreateFolder(OutputFolder() + @"\OutputDir", true);
				// get the image date-taken
				string date = DateTaken(path);
				string[] parts = date.Split(':', ' ');
				// copy the image
				CopyFile(path, OutputFolder() + @"\OutputDir\" + parts[0] + @"\" + parts[1]);
				// create a thumbnail
				CreateThumbnail(path, OutputFolder() + @"\OutputDir\Thumbnails\" + parts[0] + @"\" + parts[1]);
				// change result to true
				result = true;
				return null;
			}
			catch (Exception e)
			{
				// return the exception message
				return e.Message;
			}
		}
	}
}