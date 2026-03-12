using Common.Lib.UI.Dialogs;
using Org.BouncyCastle.Asn1.X509.Qualified;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Input;

namespace Chess.Lib.UI.Moves
{
	/// <summary>
	/// Interaction logic for AutoPlayDialog.xaml
	/// </summary>
	public partial class AutoPlayDialog : DialogView	
	{
		public AutoPlayDialog()
		{
			InitializeComponent();
		}
	}

	public class AutoPlayTriggerConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is AutoplayTrigger at && parameter is string op)
				return op[0] == char.ToLower(at.ToString().First());
			return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is bool b && parameter is string op)
			{
				switch(op)
				{
					case "a": if (b) return AutoplayTrigger.Auto; break;
					case "k": if (b) return AutoplayTrigger.KeyPress; break;
				}
			}
			return value;
		}
	}
}
