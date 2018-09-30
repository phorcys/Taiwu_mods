//	LumenWorks.Framework.IO.CSV.CachedCsvReader.CsvBindingList
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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace LumenWorks.Framework.IO.Csv
{
	public partial class CachedCsvReader
		: CsvReader
	{
		/// <summary>
		/// Represents a binding list wrapper for a CSV reader.
		/// </summary>
		private class CsvBindingList
			: IBindingList, ITypedList, IList<string[]>, IList
		{
			#region Fields

			/// <summary>
			/// Contains the linked CSV reader.
			/// </summary>
			private CachedCsvReader _csv;

			/// <summary>
			/// Contains the cached record count.
			/// </summary>
			private int _count;

			/// <summary>
			/// Contains the cached property descriptors.
			/// </summary>
			private PropertyDescriptorCollection _properties;

			/// <summary>
			/// Contains the current sort property.
			/// </summary>
			private CsvPropertyDescriptor _sort;

			/// <summary>
			/// Contains the current sort direction.
			/// </summary>
			private ListSortDirection _direction;

			#endregion

			#region Constructors

			/// <summary>
			/// Initializes a new instance of the CsvBindingList class.
			/// </summary>
			/// <param name="csv"></param>
			public CsvBindingList(CachedCsvReader csv)
			{
				_csv = csv;
				_count = -1;
				_direction = ListSortDirection.Ascending;
			}

			#endregion

			#region IBindingList members

			public void AddIndex(PropertyDescriptor property)
			{
			}

			public bool AllowNew
			{
				get
				{
					return false;
				}
			}

			public void ApplySort(PropertyDescriptor property, System.ComponentModel.ListSortDirection direction)
			{
				_sort = (CsvPropertyDescriptor) property;
				_direction = direction;

				_csv.ReadToEnd();

				_csv._records.Sort(new CsvRecordComparer(_sort.Index, _direction));
			}

			public PropertyDescriptor SortProperty
			{
				get
				{
					return _sort;
				}
			}

			public int Find(PropertyDescriptor property, object key)
			{
				int fieldIndex = ((CsvPropertyDescriptor) property).Index;
				string value = (string) key;

				int recordIndex = 0;
				int count = this.Count;

				while (recordIndex < count && _csv[recordIndex, fieldIndex] != value)
					recordIndex++;

				if (recordIndex == count)
					return -1;
				else
					return recordIndex;
			}

			public bool SupportsSorting
			{
				get
				{
					return true;
				}
			}

			public bool IsSorted
			{
				get
				{
					return _sort != null;
				}
			}

			public bool AllowRemove
			{
				get
				{
					return false;
				}
			}

			public bool SupportsSearching
			{
				get
				{
					return true;
				}
			}

			public System.ComponentModel.ListSortDirection SortDirection
			{
				get
				{
					return _direction;
				}
			}

			public event System.ComponentModel.ListChangedEventHandler ListChanged
			{
				add { }
				remove { }
			}

			public bool SupportsChangeNotification
			{
				get
				{
					return false;
				}
			}

			public void RemoveSort()
			{
				_sort = null;
				_direction = ListSortDirection.Ascending;
			}

			public object AddNew()
			{
				throw new NotSupportedException();
			}

			public bool AllowEdit
			{
				get
				{
					return false;
				}
			}

			public void RemoveIndex(PropertyDescriptor property)
			{
			}

			#endregion

			#region ITypedList Members

			public PropertyDescriptorCollection GetItemProperties(PropertyDescriptor[] listAccessors)
			{
				if (_properties == null)
				{
					PropertyDescriptor[] properties = new PropertyDescriptor[_csv.FieldCount];

					for (int i = 0; i < properties.Length; i++)
						properties[i] = new CsvPropertyDescriptor(((System.Data.IDataReader) _csv).GetName(i), i);

					_properties = new PropertyDescriptorCollection(properties);
				}

				return _properties;
			}

			public string GetListName(PropertyDescriptor[] listAccessors)
			{
				return string.Empty;
			}

			#endregion

			#region IList<string[]> Members

			public int IndexOf(string[] item)
			{
				throw new NotSupportedException();
			}

			public void Insert(int index, string[] item)
			{
				throw new NotSupportedException();
			}

			public void RemoveAt(int index)
			{
				throw new NotSupportedException();
			}

			public string[] this[int index]
			{
				get
				{
					_csv.MoveTo(index);
					return _csv._records[index];
				}
				set
				{
					throw new NotSupportedException();
				}
			}

			#endregion

			#region ICollection<string[]> Members

			public void Add(string[] item)
			{
				throw new NotSupportedException();
			}

			public void Clear()
			{
				throw new NotSupportedException();
			}

			public bool Contains(string[] item)
			{
				throw new NotSupportedException();
			}

			public void CopyTo(string[][] array, int arrayIndex)
			{
				_csv.MoveToStart();

				while (_csv.ReadNextRecord())
					_csv.CopyCurrentRecordTo(array[arrayIndex++]);
			}

			public int Count
			{
				get
				{
					if (_count < 0)
					{
						_csv.ReadToEnd();
						_count = (int) _csv.CurrentRecordIndex + 1;
					}

					return _count;
				}
			}

			public bool IsReadOnly
			{
				get { return true; }
			}

			public bool Remove(string[] item)
			{
				throw new NotSupportedException();
			}

			#endregion

			#region IEnumerable<string[]> Members

			public IEnumerator<string[]> GetEnumerator()
			{
				return _csv.GetEnumerator();
			}

			#endregion

			#region IList Members

			public int Add(object value)
			{
				throw new NotSupportedException();
			}

			public bool Contains(object value)
			{
				throw new NotSupportedException();
			}

			public int IndexOf(object value)
			{
				throw new NotSupportedException();
			}

			public void Insert(int index, object value)
			{
				throw new NotSupportedException();
			}

			public bool IsFixedSize
			{
				get { return true; }
			}

			public void Remove(object value)
			{
				throw new NotSupportedException();
			}

			object IList.this[int index]
			{
				get
				{
					return this[index];
				}
				set
				{
					throw new NotSupportedException();
				}
			}

			#endregion

			#region ICollection Members

			public void CopyTo(Array array, int index)
			{
				_csv.MoveToStart();

				while (_csv.ReadNextRecord())
					_csv.CopyCurrentRecordTo((string[]) array.GetValue(index++));
			}

			public bool IsSynchronized
			{
				get { return false; }
			}

			public object SyncRoot
			{
				get { return null; }
			}

			#endregion

			#region IEnumerable Members

			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
			{
				return this.GetEnumerator();
			}

			#endregion
		}
	}
}