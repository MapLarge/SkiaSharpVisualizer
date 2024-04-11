using Microsoft.VisualStudio.DebuggerVisualizers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SkiaSharpVisualizer {

	public class SkiaSharpVisualizerDataSource {

		/// <summary>
		/// PNG image bytes encoded to Base64.
		/// </summary>
		public string pngBase64;
		/// <summary>
		/// Width of the image.
		/// </summary>
		public int width;
		/// <summary>
		/// Height of the image.
		/// </summary>
		public int height;

	}

	public class SkiaSharpVisualizerSource : VisualizerObjectSource {

		public override void GetData(object target, Stream outgoingData) {
			//Get raw bitmap bytes and serialize those.
			//If something terrible happens, return null.
			SkiaSharpVisualizerDataSource ds;
			switch (target) {
				case SkiaSharp.SKBitmap bitmap:
					ds = GetBitmapDataSource(bitmap);
					break;
				case SkiaSharp.SKImage image:
					ds = GetBitmapDataSource(image);
					break;
				case SkiaSharp.SKSurface surface:
					using (var snapshot = surface.Snapshot()) {
						ds = GetBitmapDataSource(snapshot);
					}
					break;
				default:
					throw new NotImplementedException(target.GetType().FullName);
			}
			SerializeAsJson(outgoingData, ds);
		}

		private SkiaSharpVisualizerDataSource GetBitmapDataSource(SkiaSharp.SKBitmap bitmap) {
			return new SkiaSharpVisualizerDataSource {
				pngBase64 = Convert.ToBase64String(SavePngBytes(bitmap)),
				width = bitmap.Width,
				height = bitmap.Height,
			};
		}
		private SkiaSharpVisualizerDataSource GetBitmapDataSource(SkiaSharp.SKImage image) {
			return new SkiaSharpVisualizerDataSource {
				pngBase64 = Convert.ToBase64String(SavePngBytes(image)),
				width = image.Width,
				height = image.Height,
			};
		}

		/// <summary>
		/// Encodes the provided Skia bitmap as a PNG and returns the bytes. Do not use while drawing with a canvas.
		/// </summary>
		/// <param name="bitmap"></param>
		/// <returns></returns>
		public static byte[] SavePngBytes(SkiaSharp.SKBitmap bitmap) {
			return SaveImageBytes(bitmap, SkiaSharp.SKEncodedImageFormat.Png, 100);
		}

		/// <summary>
		/// Encodes the provided Skia bitmap as a PNG and returns the bytes. Do not use while drawing with a canvas.
		/// </summary>
		/// <param name="bitmap"></param>
		/// <param name="imageFormat"></param>
		/// <param name="imageQuality"></param>
		/// <returns></returns>
		public static byte[] SaveImageBytes(SkiaSharp.SKBitmap bitmap, SkiaSharp.SKEncodedImageFormat imageFormat, int imageQuality) {
			if (bitmap == null)
				throw new ArgumentNullException(nameof(bitmap));

			using (var ms = new System.IO.MemoryStream()) {
				using (var skStream = new SkiaSharp.SKManagedWStream(ms, false)) {
					using (var pixmap = bitmap.PeekPixels()) {
						pixmap.Encode(skStream, imageFormat, imageQuality);
					}
					return ms.ToArray();
				}
			}
		}

		/// <summary>
		/// Encodes the provided Skia bitmap as a PNG and returns the bytes. Do not use while drawing with a canvas.
		/// </summary>
		/// <param name="image"></param>
		/// <returns></returns>
		public static byte[] SavePngBytes(SkiaSharp.SKImage image) {
			return SavePngBytes(image, SkiaSharp.SKEncodedImageFormat.Png, 100);
		}

		/// <summary>
		/// Encodes the provided Skia bitmap as a PNG and returns the bytes. Do not use while drawing with a canvas.
		/// </summary>
		/// <param name="image"></param>
		/// <param name="imageFormat"></param>
		/// <param name="imageQuality"></param>
		/// <returns></returns>
		public static byte[] SavePngBytes(SkiaSharp.SKImage image, SkiaSharp.SKEncodedImageFormat imageFormat, int imageQuality) {
			if (image == null)
				throw new ArgumentNullException(nameof(image));

			using (var ms = new System.IO.MemoryStream()) {
				SaveToStream(image, ms, imageFormat, imageQuality);
				return ms.ToArray();
			}
		}

		/// <summary>
		/// Write the Skia image into the provided stream.
		/// </summary>
		/// <param name="image"></param>
		/// <param name="stream"></param>
		public static void SaveToStream(SkiaSharp.SKImage image, System.IO.Stream stream) {
			SaveToStream(image, stream, SkiaSharp.SKEncodedImageFormat.Png, 100);
		}
		/// <summary>
		/// Write the Skia image into the provided stream.
		/// </summary>
		/// <param name="image"></param>
		/// <param name="stream"></param>
		/// <param name="imageFormat"></param>
		/// <param name="imageQuality"></param>
		public static void SaveToStream(SkiaSharp.SKImage image, System.IO.Stream stream, SkiaSharp.SKEncodedImageFormat imageFormat, int imageQuality) {
			if (image == null)
				throw new ArgumentNullException(nameof(image));
			if (stream == null)
				throw new ArgumentNullException(nameof(stream));
			if (!stream.CanWrite)
				throw new ArgumentException("Stream is not writable.", nameof(stream));

			switch (imageFormat) {
				case SkiaSharp.SKEncodedImageFormat.Webp:
					var opts = new SkiaSharp.SKWebpEncoderOptions(SkiaSharp.SKWebpEncoderCompression.Lossless, imageQuality);
					using (var bitmap = SkiaSharp.SKBitmap.FromImage(image))
					using (var skStream = new SkiaSharp.SKManagedWStream(stream, false))
					using (var pixmap = bitmap.PeekPixels()) {
						pixmap.Encode(skStream, opts);
					}
					break;
				default:
					using (var encoded = image.Encode(imageFormat, imageQuality)) {
						encoded.SaveTo(stream);
					}
					break;
			}
		}

	}
}
