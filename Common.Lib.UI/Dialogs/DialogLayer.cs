using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Common.Lib.UI.Dialogs
{
	public class DialogLayer : Canvas
	{
		//TODO: Experiment with stacked dialogs?

		public static readonly DependencyProperty ModalBackgroundProperty = DependencyProperty.Register("ModalBackground", typeof(Brush),
			typeof(DialogLayer), new PropertyMetadata(Brushes.Transparent));

		/// <summary>
		/// Get/Set this color of the background while displaying modal dialog(s).  
		/// This brush should contain a low alpha value so that underlying content remains visible
		/// </summary>
		public Brush ModalBackground
		{
			get => (Brush)GetValue(ModalBackgroundProperty);
			set => SetValue(ModalBackgroundProperty, value);
		}

		internal Stack<IDialogRunner> OpenDialogs { get; private init; } = new();

		public int OpenDialogCount => OpenDialogs.Count;

		internal Task<IDialogResult<T>> PushDialog<T>(DialogView view, IDialogModelEx<T> model)
		{
			TaskCompletionSource<IDialogResult<T>> sink = new TaskCompletionSource<IDialogResult<T>>();
			OpenDialogs.Push(new DialogRunner<T>(this, view, model, sink));
			return sink.Task;
		}

		internal void ProcessEscapePressed()
		{
			if (OpenDialogs.Count > 0) OpenDialogs.Peek().ProcessEscapeKey();
		}

		private Brush DefaultBackground { get; set; } = Brushes.Transparent;
		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnPropertyChanged(e);
			switch(e.Property.Name)
			{
				case nameof(IsHitTestVisible):
					if (IsHitTestVisible)
					{
						Visibility = Visibility.Visible;
						DefaultBackground = Background;
						Background = ModalBackground;
						break;
					}
					else
					{
						Background = DefaultBackground;
						Visibility = Visibility.Collapsed;
					}
					break;
			}
		}
	}
}
