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

namespace ImageService.Model
{
	public class ImageModel : IImageModel
	{
		#region Members
		// The Output Folder
		private string m_OutputFolder;
		public string OutputFolder() { return m_OutputFolder; }
		// The Size Of The Thumbnail Size
		private int m_thumbnailSize;
		public int thumbnailSize() { return m_thumbnailSize; }
		#endregion

		/// <summary>
		/// Constructor for ImageModel
		/// </summary>
		/// <param name="OutputFolder">String path to the output floder</param>
		/// <param name="thumbnailSize">The thumbnail size (for CreateThumbnail function)</param>
		public ImageModel(string OutputFolder, int thumbnailSize)
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

		/// <summary>
		/// The function copy a file from sourcePath to destPath.
		/// </summary>
		/// <param name="sourcePath">The string for file's source path</param>
		/// <param name="destPath">The string for file's destination path</param>
		/// <param name="over">A boolean value, if to override an existing file in destination path</param>
		public void CopyFile(string sourcePath, string destPath, bool over = false)
		{
			// if needed, creates the destination folder
			CreateFolder(destPath);
			// copy the file
			File.Copy(sourcePath, destPath + @"\" + Path.GetFileName(sourcePath), over);
		}

		/// <summary>
		/// The function move a file from sourcePath to destPath.
		/// </summary>
		/// <param name="sourcePath">The string for file's source path</param>
		/// <param name="destPath">The string for file's destination path</param>
		public void MoveFile(string sourcePath, string destPath)
		{
			// if needed, creates the destination folder
			CreateFolder(destPath);
			// move the file
			File.Move(sourcePath, destPath + @"\" + Path.GetFileName(sourcePath));
		}

		/// <summary>
		/// The function delete a file.
		/// </summary>
		/// <param name="path">The string for file's path</param>
		public void DeleteFile(string path)
		{
			// delete the file
			File.Delete(path);
		}

		/// <summary>
		/// The function delete a folder.
		/// </summary>
		/// <param name="path">The string for folder's path</param>
		public void DeleteFolder(string path)
		{
			// delete the folder
			Directory.Delete(path);
		}

		/// <summary>
		/// The function creates a thumbnail from a source image, with size of thumbnailSize.
		/// </summary>
		/// <param name="sourcePath">The string for image's source path</param>
		/// <param name="destPath">The string for thumbnail's destination path</param>
		public void CreateThumbnail(string sourcePath, string destPath)
		{
			// if needed, creates the destination folder
			CreateFolder(destPath);
			// get the source image
			using (FileStream fs = File.OpenRead(sourcePath))
			{
				// get the image bitmap
				Bitmap bm = new Bitmap(sourcePath);
				// Check if the Orientation property exist in the image data
				if (bm.PropertyIdList.Contains(0x112))
				{
					// get the Orientation property
					PropertyItem pr = bm.GetPropertyItem(0x112);
					if ((pr.Type == 3) && (pr.Len == 2))
						// check the property value to know how to rotate the image
						switch (BitConverter.ToUInt16(pr.Value, 0))
						{
							case 8: // need to rotate 270
								bm.RotateFlip(RotateFlipType.Rotate270FlipNone);
								break;
							case 3: // need to rotate 180
								bm.RotateFlip(RotateFlipType.Rotate180FlipNone);
								break;
							case 6: // need to rotate 90
								bm.RotateFlip(RotateFlipType.Rotate90FlipNone);
								break;
							default:
								break;
						}
				}
				// create the thumbnail
				Image thumb = bm.GetThumbnailImage(
					thumbnailSize(), thumbnailSize(), () => false, IntPtr.Zero);
				// save the thumbnail to destPath
				string full = destPath + @"\" + Path.GetFileName(sourcePath);
				thumb.Save(full);
				// dispose the image
				bm.Dispose();
			}

		}

		/// <summary>
		/// The function get a string path to new folder, and if the
		/// directory doesn't exist - it's been created.
		/// </summary>
		/// <param name="path">The path for the new directory</param>
		/// <param name="hidden">A boolean value, if to create the directory as hidden directory</param>
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

		/// <summary>
		/// The function add an image to the database, by moving it from the given path
		/// to the output folder, and creating a thumbnail for it.
		/// </summary>
		/// <param name="path">The string for image's source path</param>
		/// <param name="result">If succeed - set to true, else - set to false</param>
		/// <returns>If succeed - the image's new path, else - an error message</returns>
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

				// create a thumbnail
				CreateThumbnail(path, OutputFolder() + @"\OutputDir\Thumbnails\" + destPath);
				// move the image
				MoveFile(path, OutputFolder() + @"\OutputDir\" + destPath);


				// change result to true
				result = true;
				return OutputFolder() + @"\OutputDir\" + destPath;
			}
			catch (Exception e)
			{
				// return the exception message
				return e.Message;
			}
		}
	}
}