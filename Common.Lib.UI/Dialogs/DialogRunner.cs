namespace Common.Lib.UI.Dialogs
{
	internal interface IDialogRunner
	{
		void ProcessEscapeKey();
	}

	/// <summary>
	/// Simplifiy dialog lifecycle by bringing all players together here.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	internal class DialogRunner<T> : IDialogRunner
	{
		internal DialogRunner(DialogLayer layer, DialogView view, IDialogModelEx<T> model, TaskCompletionSource<IDialogResult<T>> sink)
		{
			Layer = layer;
			View = view;
			Model = model;
			Model.Closing += Model_Closing;
			ResultSink = sink;
			Layer.IsHitTestVisible = true;
			View.DataContext = Model;
			foreach (DialogView v in Layer.Children) v.IsHitTestVisible = false;
			Layer.Children.Add(View);
		}

		private DialogLayer Layer { get; init; }
		private DialogView View { get; init; }

		public IDialogModelEx<T> Model { get; private init; }

		internal TaskCompletionSource<IDialogResult<T>> ResultSink { get; private init; }

		void IDialogRunner.ProcessEscapeKey() => Model.ProcessEscapeKey();

		private void Model_Closing(IDialogResult<T> result)
		{
			View.ApplyCloseIndicator();
			ResultSink.SetResult(result);
			void close()
			{
				Layer.Children.Remove(View);
				Layer.OpenDialogs.Pop();
				if (Layer.Children.Count > 0) Layer.Children[Layer.Children.Count - 1].IsHitTestVisible = true;
				if (Layer.OpenDialogCount == 0) Layer.IsHitTestVisible = false;
			}
			if (View.Animation == AnimationType.None) close();
			else
			{
				DialogAnimator.RunCloseAnimation(View, () => close());
			}
		}

	}
}
