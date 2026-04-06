using LibVLCSharp.Shared;
using System;
using System.IO;

namespace CommonApps.Lib
{
	public static class VLC
	{
		private static readonly LibVLC _libVlc;

		static VLC()
		{
			Core.Initialize(VLCFolder);
			_libVlc = new LibVLC();			
		}

		public const string VLCFolder = @"C:\Program Files\VideoLAN\VLC\";
		public const string VLCExePath = @"C:\Program Files\VideoLAN\VLC\vlc.exe";

		public static bool Exists => File.Exists(VLCExePath);
		public static DirectoryInfo VlcPlayerFolderInfo => new DirectoryInfo(VLCFolder);

		public static Media CreateMedia(Uri uri) => new Media(_libVlc, uri);
		public static LibVLCSharp.Shared.MediaPlayer CreatePlayer() => new LibVLCSharp.Shared.MediaPlayer(_libVlc);
		public static LibVLCSharp.Shared.MediaPlayer CreatePlayer(Media media) => new LibVLCSharp.Shared.MediaPlayer(media);
	}
}
