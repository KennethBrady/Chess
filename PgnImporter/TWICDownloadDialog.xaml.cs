using Common.Lib.UI.Dialogs;
using Microsoft.Win32;
using PgnImporter.Models;

namespace PgnImporter
{
	/// <summary>
	/// Interaction logic for TWICDownloadDialog.xaml
	/// </summary>
	public partial class TWICDownloadDialog : DialogView
	{
		public TWICDownloadDialog()
		{
			InitializeComponent();
		}

		protected override void OnDataContextChanged(object? oldValue, object? newValue)
		{
			base.OnDataContextChanged(oldValue, newValue);
			if (newValue is TWICDownloadDialogModel m)
			{
				m.BrowseFolder = BrowseFolder;
				m.ScrollGrid = o => entries.ScrollIntoView(o);
			}
		}

		private async Task<string> BrowseFolder(string startFolder)
		{
			OpenFolderDialog dlg = new OpenFolderDialog();
			dlg.Title = "Select Download Folder";
			dlg.Multiselect = false;
			dlg.InitialDirectory = startFolder;
			bool? result = dlg.ShowDialog();
			return result == true ? dlg.FolderName : string.Empty;
		}
	}
}
