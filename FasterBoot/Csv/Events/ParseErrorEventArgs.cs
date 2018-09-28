//	LumenWorks.Framework.IO.CSV.ParseErrorEventArgs
//	Copyright (c) 2006 Sébastien Lorion
//
//	MIT license (http://en.wikipedia.org/wiki/MIT_License)
//
//	Permission is hereby granted, free of charge, to any person obtaining a copy
//	of this software and associated documentation files (the "Software"), to deal
//	in the Software without restriction, including without limitation the rights 
//	to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
//	of the Software, and to permit persons to whom the Software is furnished to do so, 
//	subject to the following conditions:
//
//	The above copyright notice and this permission notice shall be included in all 
//	copies or substantial portions of the Software.
//
//	THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
//	INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
//	PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE 
//	FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
//	ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;

namespace LumenWorks.Framework.IO.Csv
{
	/// <summary>
	/// Provides data for the <see cref="M:CsvReader.ParseError"/> event.
	/// </summary>
	public class ParseErrorEventArgs
		: EventArgs
	{
		#region Fields

		/// <summary>
		/// Contains the error that occured.
		/// </summary>
		private MalformedCsvException _error;

		/// <summary>
		/// Contains the action to take.
		/// </summary>
		private ParseErrorAction _action;

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the ParseErrorEventArgs class.
		/// </summary>
		/// <param name="error">The error that occured.</param>
		/// <param name="defaultAction">The default action to take.</param>
		public ParseErrorEventArgs(MalformedCsvException error, ParseErrorAction defaultAction)
			: base()
		{
			_error = error;
			_action = defaultAction;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the error that occured.
		/// </summary>
		/// <value>The error that occured.</value>
		public MalformedCsvException Error
		{
			get { return _error; }
		}

		/// <summary>
		/// Gets or sets the action to take.
		/// </summary>
		/// <value>The action to take.</value>
		public ParseErrorAction Action
		{
			get { return _action; }
			set { _action = value; }
		}

		#endregion
	}
}