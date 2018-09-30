//	LumenWorks.Framework.IO.CSV.CachedCsvReader.CsvPropertyDescriptor
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
using System.Collections.Generic;
using System.ComponentModel;

namespace LumenWorks.Framework.IO.Csv
{
	public partial class CachedCsvReader
		: CsvReader
	{
		/// <summary>
		/// Represents a CSV field property descriptor.
		/// </summary>
		private class CsvPropertyDescriptor
			: PropertyDescriptor
		{
			#region Fields

			/// <summary>
			/// Contains the field index.
			/// </summary>
			private int _index;

			#endregion

			#region Constructors

			/// <summary>
			/// Initializes a new instance of the CsvPropertyDescriptor class.
			/// </summary>
			/// <param name="fieldName">The field name.</param>
			/// <param name="index">The field index.</param>
			public CsvPropertyDescriptor(string fieldName, int index)
				: base(fieldName, null)
			{
				_index = index;
			}

			#endregion

			#region Properties

			/// <summary>
			/// Gets the field index.
			/// </summary>
			/// <value>The field index.</value>
			public int Index
			{
				get { return _index; }
			}

			#endregion

			#region Overrides

			public override bool CanResetValue(object component)
			{
				return false;
			}

			public override object GetValue(object component)
			{
				return ((string[]) component)[_index];
			}

			public override void ResetValue(object component)
			{
			}

			public override void SetValue(object component, object value)
			{
			}

			public override bool ShouldSerializeValue(object component)
			{
				return false;
			}

			public override Type ComponentType
			{
				get
				{
					return typeof(CachedCsvReader);
				}
			}

			public override bool IsReadOnly
			{
				get
				{
					return true;
				}
			}

			public override Type PropertyType
			{
				get
				{
					return typeof(string);
				}
			}

			#endregion
		}
	}
}
