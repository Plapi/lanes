using System;

namespace DTT.UI.ProceduralUI.Exceptions
{
	/// <summary>
	/// Any exception that can occur in the Rounded Image package.
	/// </summary>
	public abstract class ProceduralUIException : Exception
	{
		/// <summary>
		/// The message of the error. This is used to show additional information to the user.
		/// </summary>
		public abstract string ErrorMessage { get; }
		
		/// <summary>
		/// Exception message for unsupported fillmode.
		/// Use <see cref="string.Format(string, object)"/> to pass the unsupported fillmode.
		/// </summary>
		public const string UNSUPPORTED_FILLMODE_EXCEPTION_MESSAGE
			= "This mode is not supported {0}.";

		/// <summary>
		/// Exception message for unsupported fillmode.
		/// Use <see cref="string.Format(string, object)"/> to pass the unsupported fillmode.
		/// </summary>
		public const string UNSUPPORTED_ROUNDING_UNIT_EXCEPTION_MESSAGE
			= "This unit is not supported {0}.";
		
		/// <summary>
		/// The prefixed message in front of any
		/// <see cref="RoundedImageException"/>.
		/// </summary>
		private const string PREFIX = "[DTT] - [PackageNameException] ";
		
		/// <summary>
		/// Create a <see cref="RoundedImageException"/> without a message.
		/// </summary>
		public ProceduralUIException() : this(string.Empty) { }

		/// <summary>
		/// Create a <see cref="RoundedImageException"/> with the given message
		/// to be preceded by the prefix.
		/// <param name="message">The message to show.</param>
		public ProceduralUIException(string message) : base(Format(message, PREFIX)) { }

		/// <summary>
		/// Create a <see cref="RoundedImageException"/> with the given message
		/// to be preceded by the prefix and inner exception.
		/// </summary>
		/// <param name="message">The message to show.</param>
		/// <param name="innerException">The inner exception thrown.</param>
		public ProceduralUIException(string message, Exception innerException)
			: base(Format(message, PREFIX), innerException) { }
		
		/// <summary>
		/// Returns a formatted version of the given message using the <see cref="PREFIX"/>.
		/// </summary>
		/// <param name="message">The message to be formatted.</param>
		/// <returns>Message with the prefix inserted.</returns>
		protected static string Format(string message, string prefix) => message.Insert(0, prefix);
	}
}