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

namespace ImageModal
{
	public class ImageModal : IImageModal
	{
		#region Members
		// The Output Folder
		private string m_OutputFolder;
		public string OutputFolder() { return m_OutputFolder; }
		// The Size Of The Thumbnail Size
		private int m_thumbnailSize;
		public int thumbnailSize() { return m_thumbnailSize; }
		#endregion

		public ImageModal(string OutputFolder, int thumbnailSize)
		{
			this.m_OutputFolder = OutputFolder;
			this.m_thumbnailSize = thumbnailSize;
		}

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
			// if needed, creates the destination folder
			CreateFolder(destPath);
			// copy the file
			File.Copy(sourcePath, destPath + @"\" + Path.GetFileName(sourcePath), over);
		}

		public void MoveFile(string sourcePath, string destPath)
		{
			// if needed, creates the destination folder
			CreateFolder(destPath);
			// move the file
			File.Move(sourcePath, destPath + @"\" + Path.GetFileName(sourcePath));
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
			// if needed, creates the destination folder
			CreateFolder(destPath);
			// get the source image
			Image image = Image.FromFile(sourcePath);
			// create the thumbnail
			Image thumb = image.GetThumbnailImage(
				thumbnailSize(), thumbnailSize(), () => false, IntPtr.Zero);
			thumb.Save(destPath + @"\" + Path.GetFileName(sourcePath));
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
				string[] parts = date.Split(':', ' ', '/');
				// build the destination path
				string destPath = parts[2] + @"\" + parts[1];

				// copy the image
				CopyFile(path, OutputFolder() + @"\OutputDir\" + destPath);
				// create a thumbnail
				CreateThumbnail(path, OutputFolder() + @"\OutputDir\Thumbnails\" + destPath);

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