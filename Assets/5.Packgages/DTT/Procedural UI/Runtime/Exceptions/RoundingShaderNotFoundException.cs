using System;

namespace DTT.UI.ProceduralUI.Exceptions
{
	/// <summary>
	/// Should be thrown when the fill shader can't be found in the project.
	/// </summary>
	public class RoundingShaderNotFoundException : ProceduralUIException
	{
		/// <summary>
		/// <inheritdoc/>
		/// </summary>
		public override string ErrorMessage
			=> "The shader used for drawing the rounded effect can't be found in your project.";
		
		/// <summary>
		/// The prefixed message in front of any
		/// <see cref="RoundingShaderNotFoundException"/>
		/// </summary>
		private const string PREFIX = "- [The shader used by Procedural UI to round images can't be found] - ";
		
		/// <summary>
		/// Create a <see cref="RoundingShaderNotFoundException"/> without a message.
		/// </summary>
		public RoundingShaderNotFoundException() : this(PREFIX) { }

		/// <summary>
		/// Create a <see cref="RoundingShaderNotFoundException"/> with the given message
		/// to be preceded by the prefix.
		/// </summary>
		/// <param name="message">The message to show.</param>
		public RoundingShaderNotFoundException(string message) : base(Format(message, PREFIX)) { }

		/// <summary>
		/// Create a <see cref="RoundingShaderNotFoundException"/> with the given message
		/// to be preceded by the prefix and inner exception.
		/// </summary>
		/// <param name="message">The message to show.</param>
		/// <param name="innerException">The inner exception thrown.</param>
		public RoundingShaderNotFoundException(string message, Exception innerException)
			: base(Format(message, PREFIX), innerException) { }
	}
}