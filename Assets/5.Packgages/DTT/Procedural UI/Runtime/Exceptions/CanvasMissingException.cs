using System;

namespace DTT.UI.ProceduralUI.Exceptions
{
	/// <summary>
	/// Should be thrown when the relevant gameobject doesn't have a canvas as parent, 
	/// even though it should be rendered through it.
	/// </summary>
	public class CanvasMissingException : ProceduralUIException
	{
		/// <summary>
		/// <inheritdoc/>
		/// </summary>
		public override string ErrorMessage
			=> "This component can only be added as child of a canvas object.";
		
		/// <summary>
		/// The prefixed message in front of any
		/// <see cref="CanvasMissingException"/>
		/// </summary>
		private const string PREFIX = "- [An error occured in the reflection cache] - ";
		
		/// <summary>
		/// Create a <see cref="CanvasMissingException"/> without a message.
		/// </summary>
		public CanvasMissingException() : this(PREFIX) { }

		/// <summary>
		/// Create a <see cref="CanvasMissingException"/> with the given message
		/// to be preceded by the prefix.
		/// </summary>
		/// <param name="message">The message to show.</param>
		public CanvasMissingException(string message) : base(Format(message, PREFIX)) { }

		/// <summary>
		/// Create a <see cref="CanvasMissingException"/> with the given message
		/// to be preceded by the prefix and inner exception.
		/// </summary>
		/// <param name="message">The message to show.</param>
		/// <param name="innerException">The inner exception thrown.</param>
		public CanvasMissingException(string message, Exception innerException)
			: base(Format(message, PREFIX), innerException) { }
	}
}