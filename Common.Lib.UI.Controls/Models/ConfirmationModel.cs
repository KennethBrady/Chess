using Common.Lib.UI.Dialogs;

namespace Common.Lib.UI.Controls.Models
{
	public record struct ConfirmationResult();

	public class ConfirmationModel : DialogModel<ConfirmationResult>
	{
		private string _okButtonLabel = "Ok", _cancelButtonLabel = "Cancel", _title = string.Empty, _message = string.Empty;

		public string OkButtonLabel
		{
			get => _okButtonLabel;
			set
			{
				_okButtonLabel = value;
				Notify(nameof(OkButtonLabel));
			}
		}

		public string CancelButtonLabel
		{
			get => _cancelButtonLabel;
			set
			{
				_cancelButtonLabel = value;
				Notify(nameof(CancelButtonLabel));
			}
		}

		public string Title
		{
			get => _title;
			set
			{
				_title = value;
				Notify(nameof(Title));
			}
		}

		public string Message
		{
			get => _message;
			set
			{
				_message = value;
				Notify(nameof(Message));
			}
		}

		protected override void Execute(string? parameter)
		{
			switch (parameter)
			{
				case OKParameter: Accept(new ConfirmationResult()); break;
				case CancelParameter: Cancel(); break;
			}
		}
	}
}
