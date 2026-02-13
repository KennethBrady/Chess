namespace Common.Lib.UI.DragDrop.DDM
{
	/// <summary>
	/// Enumeration of the stages of a drag / drop operation
	/// </summary>
	public enum DragStatus
	{
		/// <summary>
		/// Not used
		/// </summary>
		None = 0,

		/// <summary>
		/// Query the drag source whether a drag operation can begin. (DragQuery event)
		/// </summary>
		DragQuery = 1,

		/// <summary>
		/// Allow the drag source to update visual feedback (DragInfo event)
		/// </summary>
		DragUpdateFeedback = 2,

		/// <summary>
		/// Query the drag source whether to continue a drag operation. (DragQuery event)
		/// </summary>
		DragContinue = 3,

		/// <summary>
		/// Notify the drag source that the drag operation has completed. (DragInfo event)
		/// </summary>
		DragComplete = 4,

		/// <summary>
		/// Notify the drag source that the drag operation is cancelled. (DragInfo event)
		/// </summary>
		DragCancel = 5,

		/// <summary>
		/// Query a potential drop target as to whether a drop is possible (DropQuery event)
		/// </summary>
		DropDestinationQuery = 6,

		/// <summary>
		/// Notify a drop target that a drop is possible (DropInfo event)
		/// </summary>
		DropPossible = 7,

		/// <summary>
		/// Notify a drop target that a drop is not possible (DropInfo event)
		/// </summary>
		DropImpossible = 8,

		/// <summary>
		/// Notify a drop target that a drop has completed (DropInfo event)
		/// </summary>
		DropComplete = 9,

		/// <summary>
		/// Notify a drop target that a drag operation has been cancelled (DropInfo event)
		/// </summary>
		DropCancel = 10
	}
}
