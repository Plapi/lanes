using System;
using UnityEngine;

namespace DTT.UI.ProceduralUI.Exceptions
{
	/// <summary>
	/// Should be thrown when the relevant gameobject that's being rendered through the canvas doesn't 
	/// contain the additional shader channel <see cref="AdditionalCanvasShaderChannels.TexCoord3"/>
	/// even though it's required.
	/// </summary>
	public class TexCoord3MissingException : ProceduralUIException, IFixableCanvasException
	{
		/// <summary>
		/// <inheritdoc/>
		/// </summary>
		public override string ErrorMessage
			=> "The additional shader channel TexCoord3 is missing in your canvas settings.";
		
		/// <summary>
		/// The canvas that has the missing shader channel.
		/// </summary>
		private Canvas _canvas;

		/// <summary>
		/// The prefixed message in front of any
		/// <see cref="TexCoord3MissingException"/>
		/// </summary>
		private const string PREFIX = "- [Texcoord 3 missing in Canvas parent] - ";
		
		/// <summary>
		/// Create a <see cref="TexCoord3MissingException"/> without a message.
		/// </summary>
		public TexCoord3MissingException(Canvas canvas) : this(canvas, PREFIX) { }

		/// <summary>
		/// Create a <see cref="TexCoord3MissingException"/> with the given message
		/// to be preceded by the prefix.
		/// </summary>
		/// <param name="message">The message to show.</param>
		public TexCoord3MissingException(Canvas canvas, string message) 
			: base(Format(message, PREFIX)) => this._canvas = canvas;

		/// <summary>
		/// Create a <see cref="TexCoord3MissingException"/> with the given message
		/// to be preceded by the prefix and inner exception.
		/// </summary>
		/// <param name="message">The message to show.</param>
		/// <param name="innerException">The inner exception thrown.</param>
		public TexCoord3MissingException(Canvas canvas, string message, Exception innerException)
			: base(Format(message, PREFIX), innerException) => this._canvas = canvas;
		
		/// <summary>
		/// Applies the additional shader channel to the canvas.
		/// </summary>
		public void Fix() => _canvas.additionalShaderChannels |= AdditionalCanvasShaderChannels.TexCoord3;
	}
}