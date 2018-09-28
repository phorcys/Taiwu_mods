//	LumenWorks.Framework.IO.CSV.CachedCsvReader
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;

using LumenWorks.Framework.IO.Csv.Resources;

namespace LumenWorks.Framework.IO.Csv
{
	/// <summary>
	/// Represents a reader that provides fast, cached, dynamic access to CSV data.
	/// </summary>
	/// <remarks>The number of records is limited to <see cref="System.Int32.MaxValue"/> - 1.</remarks>
	public partial class CachedCsvReader
		: CsvReader, IListSource
	{
		#region Fields

		/// <summary>
		/// Contains the cached records.
		/// </summary>
		private List<string[]> _records;

		/// <summary>
		/// Contains the current record index (inside the cached records array).
		/// </summary>
		private long _currentRecordIndex;

		/// <summary>
		/// Indicates if a new record is being read from the CSV stream.
		/// </summary>
		private bool _readingStream;

		/// <summary>
		/// Contains the binding list linked to this reader.
		/// </summary>
		private CsvBindingList _bindingList;

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the CsvReader class.
		/// </summary>
		/// <param name="reader">A <see cref="T:TextReader"/> pointing to the CSV file.</param>
		/// <param name="hasHeaders"><see langword="true"/> if field names are located on the first non commented line, otherwise, <see langword="false"/>.</param>
		/// <exception cref="T:ArgumentNullException">
		///		<paramref name="reader"/> is a <see langword="null"/>.
		/// </exception>
		/// <exception cref="T:ArgumentException">
		///		Cannot read from <paramref name="reader"/>.
		/// </exception>
		public CachedCsvReader(TextReader reader, bool hasHeaders)
			: this(reader, hasHeaders, DefaultBufferSize)
		{
		}

		/// <summary>
		/// Initializes a new instance of the CsvReader class.
		/// </summary>
		/// <param name="reader">A <see cref="T:TextReader"/> pointing to the CSV file.</param>
		/// <param name="hasHeaders"><see langword="true"/> if field names are located on the first non commented line, otherwise, <see langword="false"/>.</param>
		/// <param name="bufferSize">The buffer size in bytes.</param>
		/// <exception cref="T:ArgumentNullException">
		///		<paramref name="reader"/> is a <see langword="null"/>.
		/// </exception>
		/// <exception cref="T:ArgumentException">
		///		Cannot read from <paramref name="reader"/>.
		/// </exception>
		public CachedCsvReader(TextReader reader, bool hasHeaders, int bufferSize)
			: this(reader, hasHeaders, DefaultDelimiter, DefaultQuote, DefaultEscape, DefaultComment, ValueTrimmingOptions.UnquotedOnly, bufferSize)
		{
		}

		/// <summary>
		/// Initializes a new instance of the CsvReader class.
		/// </summary>
		/// <param name="reader">A <see cref="T:TextReader"/> pointing to the CSV file.</param>
		/// <param name="hasHeaders"><see langword="true"/> if field names are located on the first non commented line, otherwise, <see langword="false"/>.</param>
		/// <param name="delimiter">The delimiter character separating each field (default is ',').</param>
		/// <exception cref="T:ArgumentNullException">
		///		<paramref name="reader"/> is a <see langword="null"/>.
		/// </exception>
		/// <exception cref="T:ArgumentException">
		///		Cannot read from <paramref name="reader"/>.
		/// </exception>
		public CachedCsvReader(TextReader reader, bool hasHeaders, char delimiter)
			: this(reader, hasHeaders, delimiter, DefaultQuote, DefaultEscape, DefaultComment, ValueTrimmingOptions.UnquotedOnly, DefaultBufferSize)
		{
		}

		/// <summary>
		/// Initializes a new instance of the CsvReader class.
		/// </summary>
		/// <param name="reader">A <see cref="T:TextReader"/> pointing to the CSV file.</param>
		/// <param name="hasHeaders"><see langword="true"/> if field names are located on the first non commented line, otherwise, <see langword="false"/>.</param>
		/// <param name="delimiter">The delimiter character separating each field (default is ',').</param>
		/// <param name="bufferSize">The buffer size in bytes.</param>
		/// <exception cref="T:ArgumentNullException">
		///		<paramref name="reader"/> is a <see langword="null"/>.
		/// </exception>
		/// <exception cref="T:ArgumentException">
		///		Cannot read from <paramref name="reader"/>.
		/// </exception>
		public CachedCsvReader(TextReader reader, bool hasHeaders, char delimiter, int bufferSize)
			: this(reader, hasHeaders, delimiter, DefaultQuote, DefaultEscape, DefaultComment, ValueTrimmingOptions.UnquotedOnly, bufferSize)
		{
		}

		/// <summary>
		/// Initializes a new instance of the CsvReader class.
		/// </summary>
		/// <param name="reader">A <see cref="T:TextReader"/> pointing to the CSV file.</param>
		/// <param name="hasHeaders"><see langword="true"/> if field names are located on the first non commented line, otherwise, <see langword="false"/>.</param>
		/// <param name="delimiter">The delimiter character separating each field (default is ',').</param>
		/// <param name="quote">The quotation character wrapping every field (default is ''').</param>
		/// <param name="escape">
		/// The escape character letting insert quotation characters inside a quoted field (default is '\').
		/// If no escape character, set to '\0' to gain some performance.
		/// </param>
		/// <param name="comment">The comment character indicating that a line is commented out (default is '#').</param>
		/// <param name="trimmingOptions">Determines how values should be trimmed.</param>
		/// <exception cref="T:ArgumentNullException">
		///		<paramref name="reader"/> is a <see langword="null"/>.
		/// </exception>
		/// <exception cref="T:ArgumentException">
		///		Cannot read from <paramref name="reader"/>.
		/// </exception>
		public CachedCsvReader(TextReader reader, bool hasHeaders, char delimiter, char quote, char escape, char comment, ValueTrimmingOptions trimmingOptions)
			: this(reader, hasHeaders, delimiter, quote, escape, comment, trimmingOptions, DefaultBufferSize)
		{
		}

		/// <summary>
		/// Initializes a new instance of the CsvReader class.
		/// </summary>
		/// <param name="reader">A <see cref="T:TextReader"/> pointing to the CSV file.</param>
		/// <param name="hasHeaders"><see langword="true"/> if field names are located on the first non commented line, otherwise, <see langword="false"/>.</param>
		/// <param name="delimiter">The delimiter character separating each field (default is ',').</param>
		/// <param name="quote">The quotation character wrapping every field (default is ''').</param>
		/// <param name="escape">
		/// The escape character letting insert quotation characters inside a quoted field (default is '\').
		/// If no escape character, set to '\0' to gain some performance.
		/// </param>
		/// <param name="comment">The comment character indicating that a line is commented out (default is '#').</param>
		/// <param name="trimSpaces"><see langword="true"/> if spaces at the start and end of a field are trimmed, otherwise, <see langword="false"/>. Default is <see langword="true"/>.</param>
		/// <param name="bufferSize">The buffer size in bytes.</param>
		/// <exception cref="T:ArgumentNullException">
		///		<paramref name="reader"/> is a <see langword="null"/>.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		///		<paramref name="bufferSize"/> must be 1 or more.
		/// </exception>
		public CachedCsvReader(TextReader reader, bool hasHeaders, char delimiter, char quote, char escape, char comment, ValueTrimmingOptions trimmingOptions, int bufferSize)
			: base(reader, hasHeaders, delimiter, quote, escape, comment, trimmingOptions, bufferSize)
		{
			_records = new List<string[]>();
			_currentRecordIndex = -1;
		}

		#endregion

		#region Properties

		#region State

		/// <summary>
		/// Gets the current record index in the CSV file.
		/// </summary>
		/// <value>The current record index in the CSV file.</value>
		public override long CurrentRecordIndex
		{
			get
			{
				return _currentRecordIndex;
			}
		}

		/// <summary>
		/// Gets a value that indicates whether the current stream position is at the end of the stream.
		/// </summary>
		/// <value><see langword="true"/> if the current stream position is at the end of the stream; otherwise <see langword="false"/>.</value>
		public override bool EndOfStream
		{
			get
			{
				if (_currentRecordIndex < base.CurrentRecordIndex)
					return false;
				else
					return base.EndOfStream;
			}
		}

		#endregion

		#endregion

		#region Indexers

		/// <summary>
		/// Gets the field at the specified index.
		/// </summary>
		/// <value>The field at the specified index.</value>
		/// <exception cref="T:ArgumentOutOfRangeException">
		///		<paramref name="field"/> must be included in [0, <see cref="M:FieldCount"/>[.
		/// </exception>
		/// <exception cref="T:InvalidOperationException">
		///		No record read yet. Call ReadLine() first.
		/// </exception>
		/// <exception cref="MissingFieldCsvException">
		///		The CSV data appears to be missing a field.
		/// </exception>
		/// <exception cref="T:MalformedCsvException">
		///		The CSV appears to be corrupt at the current position.
		/// </exception>
		/// <exception cref="T:System.ComponentModel.ObjectDisposedException">
		///		The instance has been disposed of.
		/// </exception>
		public override String this[int field]
		{
			get
			{
				if (_readingStream)
					return base[field];
				else if (_currentRecordIndex > -1)
				{
					if (field > -1 && field < this.FieldCount)
						return _records[(int) _currentRecordIndex][field];
					else
						throw new ArgumentOutOfRangeException("field", field, string.Format(CultureInfo.InvariantCulture, ExceptionMessage.FieldIndexOutOfRange, field));
				}
				else
					throw new InvalidOperationException(ExceptionMessage.NoCurrentRecord);
			}
		}

		#endregion

		#region Methods

		#region Read

		/// <summary>
		/// Reads the CSV stream from the current position to the end of the stream.
		/// </summary>
		/// <exception cref="T:System.ComponentModel.ObjectDisposedException">
		///	The instance has been disposed of.
		/// </exception>
		public virtual void ReadToEnd()
		{
			_currentRecordIndex = base.CurrentRecordIndex;

			while (ReadNextRecord()) ;
		}

		/// <summary>
		/// Reads the next record.
		/// </summary>
		/// <param name="onlyReadHeaders">
		/// Indicates if the reader will proceed to the next record after having read headers.
		/// <see langword="true"/> if it stops after having read headers; otherwise, <see langword="false"/>.
		/// </param>
		/// <param name="skipToNextLine">
		/// Indicates if the reader will skip directly to the next line without parsing the current one. 
		/// To be used when an error occurs.
		/// </param>
		/// <returns><see langword="true"/> if a record has been successfully reads; otherwise, <see langword="false"/>.</returns>
		/// <exception cref="T:System.ComponentModel.ObjectDisposedException">
		///	The instance has been disposed of.
		/// </exception>
		protected override bool ReadNextRecord(bool onlyReadHeaders, bool skipToNextLine)
		{
			if (_currentRecordIndex < base.CurrentRecordIndex)
			{
				_currentRecordIndex++;
				return true;
			}
			else
			{
				_readingStream = true;

				try
				{
					bool canRead = base.ReadNextRecord(onlyReadHeaders, skipToNextLine);

					if (canRead)
					{
						string[] record = new string[this.FieldCount];

						if (base.CurrentRecordIndex > -1)
						{
							CopyCurrentRecordTo(record);
							_records.Add(record);
						}
						else
						{
							if (MoveTo(0))
								CopyCurrentRecordTo(record);

							MoveTo(-1);
						}

						if (!onlyReadHeaders)
							_currentRecordIndex++;
					}
					else
					{
						// No more records to read, so set array size to only what is needed
						_records.Capacity = _records.Count;
					}

					return canRead;
				}
				finally
				{
					_readingStream = false;
				}
			}
		}

		#endregion

		#region Move

		/// <summary>
		/// Moves before the first record.
		/// </summary>
		public void MoveToStart()
		{
			_currentRecordIndex = -1;
		}

		/// <summary>
		/// Moves to the last record read so far.
		/// </summary>
		public void MoveToLastCachedRecord()
		{
			_currentRecordIndex = base.CurrentRecordIndex;
		}

		/// <summary>
		/// Moves to the specified record index.
		/// </summary>
		/// <param name="record">The record index.</param>
		/// <returns><c>true</c> if the operation was successful; otherwise, <c>false</c>.</returns>
		/// <exception cref="T:System.ComponentModel.ObjectDisposedException">
		///		The instance has been disposed of.
		/// </exception>
		public override bool MoveTo(long record)
		{
			if (record < -1)
				record = -1;

			if (record <= base.CurrentRecordIndex)
			{
				_currentRecordIndex = record;
				return true;
			}
			else
			{
				_currentRecordIndex = base.CurrentRecordIndex;
				return base.MoveTo(record);
			}
		}

		#endregion

		#endregion

		#region IListSource Members

		bool IListSource.ContainsListCollection
		{
			get { return false; }
		}

		System.Collections.IList IListSource.GetList()
		{
			if (_bindingList == null)
				_bindingList = new CsvBindingList(this);

			return _bindingList;
		}

		#endregion
	}
}
