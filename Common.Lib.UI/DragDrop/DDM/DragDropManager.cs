using Common.Lib.UI.Win32;
using System.Windows;
using System.Windows.Input;

namespace Common.Lib.UI.DragDrop.DDM
{
	/// <summary>
	/// A (relatively) simple and Xaml-friendly drag/drop interface
	/// </summary>
	public class DragDropManager
	{
		#region Dependency Property and RoutedEvent declarations

		/// <summary>
		/// Identifies the AllowDrag attached property
		/// </summary>
		public static readonly DependencyProperty AllowDragProperty = DependencyProperty.RegisterAttached("AllowDrag", typeof(bool),
			typeof(DragDropManager), new PropertyMetadata(false, HandleAllowDragChanged));

		/// <summary>
		/// Identifies the AllowDrop attached property
		/// </summary>
		public static readonly DependencyProperty AllowDropProperty = DependencyProperty.RegisterAttached("AllowDrop", typeof(bool),
			typeof(DragDropManager), new PropertyMetadata(false, HandleAllowDropChanged));

		/// <summary>
		/// Identifies the DragInfo attached routed event
		/// </summary>
		public static readonly RoutedEvent DragInfoEvent = EventManager.RegisterRoutedEvent("DragInfo", RoutingStrategy.Bubble,
			typeof(DragDropEventHandler), typeof(DragDropManager));

		/// <summary>
		/// Identifies the DragQuery attached routed event
		/// </summary>
		public static readonly RoutedEvent DragQueryEvent = EventManager.RegisterRoutedEvent("DragQuery", RoutingStrategy.Bubble,
			typeof(DragDropQueryEventHandler), typeof(DragDropManager));

		/// <summary>
		/// Identifies the DropInfo attached routed event
		/// </summary>
		public static readonly RoutedEvent DropInfoEvent = EventManager.RegisterRoutedEvent("DropInfo", RoutingStrategy.Bubble,
			typeof(DragDropEventHandler), typeof(DragDropManager));

		/// <summary>
		/// Identifies the DropQuery attached routed event
		/// </summary>
		public static readonly RoutedEvent DropQueryEvent = EventManager.RegisterRoutedEvent("DropQuery", RoutingStrategy.Bubble,
			typeof(DragDropQueryEventHandler), typeof(DragDropManager));

		#endregion

		#region Attached Property Getters / Setters

		/// <summary>
		/// Getter for the AllowDrag attached property
		/// </summary>
		/// <param name="dobj"></param>
		/// <returns></returns>
		public static bool GetAllowDrag(UIElement dobj)
		{
			object o = dobj.GetValue(AllowDragProperty);
			return (o == null) ? false : (bool)o;
		}

		/// <summary>
		/// Setter for the AllowDrag attached property
		/// </summary>
		/// <param name="dobj"></param>
		/// <param name="allowDrag"></param>
		public static void SetAllowDrag(UIElement dobj, bool allowDrag)
		{
			dobj.SetValue(AllowDragProperty, allowDrag);
		}

		/// <summary>
		/// Getter for the AllowDrop attached property
		/// </summary>
		/// <param name="dobj"></param>
		/// <returns></returns>
		public static bool GetAllowDrop(UIElement dobj)
		{
			object o = dobj.GetValue(AllowDropProperty);
			return (o == null) ? false : (bool)o;
		}

		/// <summary>
		/// Setter for the AllowDrop attached property
		/// </summary>
		/// <param name="dobj"></param>
		/// <param name="allowDrop"></param>
		public static void SetAllowDrop(UIElement dobj, bool allowDrop)
		{
			dobj.SetValue(AllowDropProperty, allowDrop);
		}

		#endregion

		#region Attached Event Add/Remove Methods

		/// <summary>
		/// Adder for the DragQuery attached routed event
		/// </summary>
		/// <param name="dobj">UIElement handling the event</param>
		/// <param name="handler">handler for the DragQuery event</param>
		/// <remarks>
		/// DragQuery events come with either of two DragStatus values:
		///		DragQuery:  queries whether a drag operation should be started
		///		DragContinue:	queries whether a drag operation should continue
		/// </remarks>
		public static void AddDragQueryHandler(UIElement dobj, DragDropQueryEventHandler handler)
		{
			dobj.AddHandler(DragQueryEvent, handler);
		}

		/// <summary>
		/// Adder for the DragInfo attached routed event
		/// </summary>
		/// <param name="dobj">UIElement handling the event</param>
		/// <param name="handler">handler for the DragInfo event</param>
		/// <remarks>
		/// DragInfo events come with any of 3 DragStatus values:
		///		DragUpdateFeedback:	allows the drag source to update the drag visual
		///		DragComplete:	notifies the drag source that the drag operation has completed
		///		DragCancel:	notifies the drag source that the drag operation has been cancelled
		/// </remarks>
		public static void AddDragInfoHandler(UIElement dobj, DragDropEventHandler handler)
		{
			dobj.AddHandler(DragInfoEvent, handler);
		}

		/// <summary>
		/// Adder for the DropQuery attached routed event
		/// </summary>
		/// <param name="dobj">UIElement handling the event</param>
		/// <param name="handler">handler for the DropQuery event</param>
		/// <remarks>
		/// DropQuery event uses DragStatus.DropDestinationQuery.
		/// </remarks>
		public static void AddDropQueryHandler(UIElement dobj, DragDropQueryEventHandler handler)
		{
			dobj.AddHandler(DropQueryEvent, handler);
		}

		/// <summary>
		/// Adder for the DropInfo attached routed event
		/// </summary>
		/// <param name="dobj">UIElement handling the event</param>
		/// <param name="handler">handler for the DropInfo event</param>
		/// <remarks>
		/// DropInfo uses any of 4 DragStatus values:
		///		DropPossible:	tells the drop source that a drop is possible
		///		DropImpossible:	tells a drop source that a drop is not possible
		///		DropComplete:	tells a drop source that the drop is completed
		///		DropCancel:	tells a drop source that the drop has been cancelled
		/// </remarks>
		public static void AddDropInfoHandler(UIElement dobj, DragDropEventHandler handler)
		{
			dobj.AddHandler(DropInfoEvent, handler);
		}

		/// <summary>
		/// Remover for the DragQuery attached routed event
		/// </summary>
		/// <param name="dobj"></param>
		/// <param name="handler"></param>
		public static void RemoveDragQueryHandler(UIElement dobj, DragDropQueryEventHandler handler)
		{
			dobj.RemoveHandler(DragQueryEvent, handler);
		}

		/// <summary>
		/// Remover for the DragInfo attached routed event
		/// </summary>
		/// <param name="dobj"></param>
		/// <param name="handler"></param>
		public static void RemoveDragInfoHandler(UIElement dobj, DragDropEventHandler handler)
		{
			dobj.RemoveHandler(DragInfoEvent, handler);
		}

		/// <summary>
		/// Remover for the DropQuery attached routed event
		/// </summary>
		/// <param name="dobj"></param>
		/// <param name="handler"></param>
		public static void RemoveDropQueryHandler(UIElement dobj, DragDropQueryEventHandler handler)
		{
			dobj.RemoveHandler(DropQueryEvent, handler);
		}

		/// <summary>
		/// Remover for the DropInfo attached routed event
		/// </summary>
		/// <param name="dobj"></param>
		/// <param name="handler"></param>
		public static void RemoveDropInfoHandler(UIElement dobj, DragDropEventHandler handler)
		{
			dobj.RemoveHandler(DropInfoEvent, handler);
		}

		#endregion

		/// <summary>
		/// Query whether a drag operation is in effect
		/// </summary>
		public static bool IsDragging { get; private set; }

		/// <summary>
		/// Get/Set whether diagnostic messages are written to System.Diagnostics.Debug.
		/// </summary>
		/// <remarks>
		/// Enable this if you need to see diagnostics for debugging.
		/// </remarks>
		public static bool ShowDiagnostics { get; set; }

		#region Private fields

		private static MouseButtonEventHandler _mouseDownHandler = new MouseButtonEventHandler(HandleDragElementLeftMouseDown);
		private static MouseButtonEventHandler _mouseUpHandler = new MouseButtonEventHandler(HandleDragElementLeftMouseUp);
		private static MouseEventHandler _mouseDragMoveHandler = new MouseEventHandler(HandleDragElementMouseMove);
		private static GiveFeedbackEventHandler _dragElementGiveFeedbackHandler = new GiveFeedbackEventHandler(HandleDragElementGiveFeedback);
		private static QueryContinueDragEventHandler _dragQueryContinueDragHandler = new QueryContinueDragEventHandler(HandleDragElementQueryContinueDrag);
		private static DragEventHandler _dropElementDragEnterHandler = new DragEventHandler(HandleDropElementDragEnter);
		private static DragEventHandler _dropElementDragOverHandler = new DragEventHandler(HandleDropElementDragOver);
		private static DragEventHandler _dropElementDragLeaveHandler = new DragEventHandler(HandleDropElementDragLeave);
		private static DragEventHandler _dropElementDropHandler = new DragEventHandler(HandleDropElementDrop);
		private static Point? _downPoint = null;
		private static DragDropOptions? _options;
		private static bool _dropPossible;
		private static DragVisualWindow? _feedbackWindow;
		private static UIElement? _dragSource;
		private static UIElement? _dropTarget;
		private static DragStatus _lastSourceStatus, _lastDropStatus;

		#endregion

		#region Private implementaton

		private static void HandleAllowDragChanged(DependencyObject dobj, DependencyPropertyChangedEventArgs e)
		{
			if (dobj is UIElement uie)
			{
				bool allowDrag = (bool)e.NewValue;
				if (allowDrag)
				{
					uie.PreviewMouseLeftButtonDown += _mouseDownHandler;
					uie.PreviewMouseLeftButtonUp += _mouseUpHandler;
					uie.GiveFeedback += _dragElementGiveFeedbackHandler;
					uie.QueryContinueDrag += _dragQueryContinueDragHandler;
				}
				else
				{
					uie.PreviewMouseLeftButtonDown -= _mouseDownHandler;
					uie.PreviewMouseLeftButtonUp -= _mouseUpHandler;
					uie.GiveFeedback -= _dragElementGiveFeedbackHandler;
					uie.QueryContinueDrag -= _dragQueryContinueDragHandler;
				}
			}
		}

		private static void HandleAllowDropChanged(DependencyObject dobj, DependencyPropertyChangedEventArgs e)
		{
			if (dobj is UIElement uie && e.NewValue is bool allowDrop)
			{
				if (allowDrop)
				{
					uie.AllowDrop = true;
					uie.DragEnter += _dropElementDragEnterHandler;
					uie.DragLeave += _dropElementDragLeaveHandler;
					uie.DragOver += _dropElementDragOverHandler;
					uie.Drop += _dropElementDropHandler;
				}
				else
				{
					uie.AllowDrop = false;
					uie.DragEnter -= _dropElementDragEnterHandler;
					uie.DragLeave -= _dropElementDragLeaveHandler;
					uie.DragOver -= _dropElementDragOverHandler;
					uie.Drop -= _dropElementDropHandler;
				}
			}
		}

		private static void DoDragDrop(UIElement uie)
		{
			if (!_downPoint.HasValue) return;
			WriteDiagnostic("DoDragDrop");
			try
			{
				DragDropOptions options = new DragDropOptions { CurrentDragPosition = _downPoint.Value, Status = DragStatus.DragQuery };
				DragDropQueryEventArgs args = new DragDropQueryEventArgs(DragQueryEvent, uie, options);
				uie.RaiseEvent(args);
				if ((args.QueryResult == true) && (options.Payload != null))
				{
					_options = options;
					_feedbackWindow = new DragVisualWindow(options.SourceCue, _options.RectangleStyle);
					_feedbackWindow.Show();
					DataObject data = new DataObject(options.Payload);
					_dragSource = uie;
					IsDragging = true;
					DragDropEffects dropEffects = DragDropEffects.None;
					WriteDiagnostic("DragDrop Starting");
					try
					{
						dropEffects = System.Windows.DragDrop.DoDragDrop(uie, data, DragDropEffects.Copy | DragDropEffects.Move);
					}
					catch (Exception ex)
					{
						WriteDiagnostic("DragDrop Exception: {0}", ex.ToString());
					}
					WriteDiagnostic("DoDragDrop ended {0} {1} {2}", dropEffects, _lastSourceStatus, _lastDropStatus);
					if ((dropEffects == DragDropEffects.None) && (_lastSourceStatus == DragStatus.DragContinue))
					{
						// "Dropped" onto target where drop was not possible:
						_options.Status = DragStatus.DragCancel;
						DragDropEventArgs ddea = new DragDropEventArgs(DragInfoEvent, _dragSource, _options);
						_dragSource.RaiseEvent(ddea);
					}
				}
			}
			finally
			{
				IsDragging = false;
				if (_feedbackWindow != null) _feedbackWindow.Close();
				_feedbackWindow = null;
				_downPoint = null;
				_options = null;
				_dropPossible = false;
				_dropTarget = null;
				_dragSource = null;
			}
		}

		private static void CancelDrag()
		{
			WriteDiagnostic("CancelDrag");
			if (_options != null && _dragSource != null)
			{
				_options.Status = _lastSourceStatus = DragStatus.DragCancel;
				DragDropEventArgs args = new DragDropEventArgs(DragInfoEvent, _dragSource, _options);
				_dragSource.RaiseEvent(args);

				if (_dropTarget != null)
				{
					_options.Status = _lastDropStatus = DragStatus.DropCancel;
					args = new DragDropEventArgs(DropInfoEvent, _dropTarget, _options);
					_dropTarget.RaiseEvent(args);
				}
			}
		}

		private static void HandleDragElementLeftMouseDown(object sender, MouseButtonEventArgs e)
		{
			WriteDiagnostic("HandleDragElementLeftMouseDown");
			if (sender is UIElement uie)
			{
				uie.PreviewMouseMove += _mouseDragMoveHandler;
				_downPoint = NativeMethods.GetCursorPos();
			}
		}

		private static void HandleDragElementLeftMouseUp(object sender, MouseButtonEventArgs e)
		{
			WriteDiagnostic("HandleDragElementLeftMouseUp");
			if (sender is UIElement uie)
			{
				_downPoint = null;
				uie.PreviewMouseMove -= _mouseDragMoveHandler;
			}
		}

		private static void HandleDragElementMouseMove(object sender, MouseEventArgs e)
		{
			if (_downPoint.HasValue && Mouse.LeftButton == MouseButtonState.Pressed)
			{
				WriteDiagnostic("HandleDragElementMouseMove");
				Point here = NativeMethods.GetCursorPos();
				double dx = Math.Abs(here.X - _downPoint.Value.X);
				double dy = Math.Abs(here.Y - _downPoint.Value.Y);
				if ((dx > SystemParameters.MinimumHorizontalDragDistance) || (dy > SystemParameters.MinimumVerticalDragDistance))
				{
					DoDragDrop((UIElement)sender);
				}
			}
		}

		private static void HandleDragElementGiveFeedback(object sender, GiveFeedbackEventArgs e)
		{
			WriteDiagnostic("HandleDragElementGiveFeedback");
			if (sender is UIElement uie && _options != null)
			{
				_feedbackWindow?.UpdateLocation();
				_options.Status = _lastSourceStatus = DragStatus.DragUpdateFeedback;
				object? feedback = _options.SourceCue;
				DragDropEventArgs args = new DragDropEventArgs(DragInfoEvent, uie, _options);
				uie.RaiseEvent(args);
				if (!object.ReferenceEquals(feedback, _options.SourceCue))
				{
					_feedbackWindow?.SetContent(_options.SourceCue);
				}
				if (_dropTarget != null)
				{
					feedback = _options.DestinationCue;
					_options.Status = _lastDropStatus = _dropPossible ? DragStatus.DropPossible : DragStatus.DropImpossible;
					args = new DragDropEventArgs(DropInfoEvent, _dropTarget, _options);
					_dropTarget.RaiseEvent(args);
					if (!object.ReferenceEquals(feedback, _options.DestinationCue))
					{
						_feedbackWindow?.SetContent(_options.DestinationCue);
					}
				}
			}
		}

		private static void HandleDragElementQueryContinueDrag(object sender, QueryContinueDragEventArgs e)
		{
			WriteDiagnostic("HandleDragElementQueryContinueDrag {0} {1}", e.EscapePressed, e.Action);
			if (e.Action ==	DragAction.Continue && !e.EscapePressed && sender is UIElement uie && _options != null)
			{
				_options.Status = _lastSourceStatus = DragStatus.DragContinue;
				DragDropQueryEventArgs args = new DragDropQueryEventArgs(DragQueryEvent, sender, _options);
				args.QueryResult = true;
				object? content = _options.SourceCue;
				uie.RaiseEvent(args);
				if (args.QueryResult != true)
				{
					e.Action = DragAction.Cancel;
					e.Handled = true;
					CancelDrag();
				}
				else
				{
					if (!ReferenceEquals(content, _options.SourceCue)) _feedbackWindow?.SetContent(_options.SourceCue);
				}
			}
			if (e.EscapePressed)
			{
				CancelDrag();
			}
		}

		private static void HandleDropElementDragEnter(object sender, DragEventArgs e)
		{
			WriteDiagnostic("HandleDropElementDragEnter");
			if(sender is UIElement uie && _options != null)
			{
				_options.DropTarget = _dropTarget = uie;
				_options.Status = _lastDropStatus = DragStatus.DropDestinationQuery;
				DragDropQueryEventArgs args = new DragDropQueryEventArgs(DropQueryEvent, sender, _options);
				((UIElement)sender).RaiseEvent(args);
				if (args.QueryResult == true)
				{
					e.Effects = DragDropEffects.Move;
					_dropPossible = _options.CanDrop = true;
				}
				else
				{
					e.Effects = DragDropEffects.None;
					_dropPossible = _options.CanDrop = false;
				}
				e.Handled = true;
			}
		}

		private static void HandleDropElementDragOver(object sender, DragEventArgs e)
		{
			WriteDiagnostic($"HandleDropElementDragOver: {e.Source.GetType().Name}");
			if (sender is UIElement uie && _options != null)
			{
				_options.Status = _lastDropStatus = DragStatus.DropDestinationQuery;
				DragDropQueryEventArgs args = new DragDropQueryEventArgs(DropQueryEvent, sender, _options);
				uie.RaiseEvent(args);	// Ask the dragged-over element if it can accept a drop
				if (args.QueryResult == true)
				{
					e.Effects = DragDropEffects.Move;
					_dropPossible = _options.CanDrop = true;
				}
				else
				{
					e.Effects = DragDropEffects.None;
					_dropPossible = _options.CanDrop = false;
				}
				e.Handled = true;
			}
		}

		private static void HandleDropElementDragLeave(object sender, DragEventArgs e)
		{
			WriteDiagnostic("HandleDropElementDragLeave");
			if (_options != null && _dropTarget!= null)
			{
				_options.Status = _lastDropStatus = DragStatus.DropImpossible;
				DragDropEventArgs args = new DragDropEventArgs(DropInfoEvent, _dropTarget, _options);
				_dropTarget.RaiseEvent(args);
				_options.DropTarget = _dropTarget = null;
				e.Handled = true;
			}
		}

		private static void HandleDropElementDrop(object sender, DragEventArgs e)
		{
			WriteDiagnostic("HandleDropElementDrop");
			if (sender is UIElement uie && _options != null && _dragSource != null)
			{
				_options.Status = _lastDropStatus = DragStatus.DropComplete;
				DragDropEventArgs args = new DragDropEventArgs(DropInfoEvent, sender, _options);
				uie.RaiseEvent(args);
				_options.Status = _lastSourceStatus = DragStatus.DragComplete;
				args = new DragDropEventArgs(DragInfoEvent, _dragSource, _options);
				_dragSource.RaiseEvent(args);
			}
		}

		private static void WriteDiagnostic(string fmt, params object[] args)
		{
			if (ShowDiagnostics) System.Diagnostics.Debug.WriteLine(string.Format(fmt, args));
		}

		#endregion
	}
}
