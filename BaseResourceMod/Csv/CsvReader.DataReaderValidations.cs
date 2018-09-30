//	LumenWorks.Framework.IO.CSV.CsvReader.DataReaderValidations
//	Copyright (c) 2005 Sébastien Lorion
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
	public partial class CsvReader
	{
		/// <summary>
		/// Defines the data reader validations.
		/// </summary>
		[Flags]
		private enum DataReaderValidations
		{
			/// <summary>
			/// No validation.
			/// </summary>
			None = 0,

			/// <summary>
			/// Validate that the data reader is initialized.
			/// </summary>
			IsInitialized = 1,

			/// <summary>
			/// Validate that the data reader is not closed.
			/// </summary>
			IsNotClosed = 2
		}
	}
}