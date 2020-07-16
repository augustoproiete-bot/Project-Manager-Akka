using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using JetBrains.Annotations;
using Tauron;
using Tauron.Application;

namespace ServiceHost.Core.Database
{
    [PublicAPI]
    public sealed class DatabaseHelper : IEnumerable<DatabaseHelper.DatabaseEntry>
    {
        private static readonly char[] LineSplitter = { '\t' };
        private static readonly char[] MetaSplitter = { '=' };

        private DatabaseArray<DatabaseEntry> _entrys = new DatabaseArray<DatabaseEntry>();
        public int DatabasElements => _entrys.Count;
        private HashSet<string> _databaseNames;
        private int _isDirty;
        public bool IsDirty => _isDirty == 1;
        
        public DatabaseHelper(IEnumerable<string> lines)
        {
            _databaseNames = new HashSet<string>();
            foreach (var line in lines)
            {
                if (line == "End") return;
                var tempentry = new DatabaseEntry(this);
                var acsessor = new DatabaseEntry.DatabaseEntryAcessor(tempentry);
                string[] pairs = line.Split(LineSplitter, StringSplitOptions.RemoveEmptyEntries);
                tempentry.Name = DecodeIfRequied(pairs[0]);
                if (!_databaseNames.Add(tempentry.Name)) continue;

                _entrys.Add(tempentry);

                var data = acsessor.MetadataAcc;
                foreach (var pair in pairs.Skip(1))
                {
                    string[] pairValue = pair.Split(MetaSplitter, StringSplitOptions.RemoveEmptyEntries);

                    var meta = new Metadata(this) { Key = DecodeIfRequied(pairValue[0]) };
                    if (pairValue.Length == 2) meta.Value = DecodeIfRequied(pairValue[1]);

                    if (acsessor.MetaNames.Add(meta.Key)) data.Add(meta);
                }
            }
        }

        public IEnumerable<string> Names => new ReadOnlyEnumerator<string>(_databaseNames);

        public DatabaseEntry FindEntry(string name) 
            => FindEntryNonBlock(name);

        private DatabaseEntry FindEntryNonBlock(string name)
        {
            var index = _entrys.Search(name);
            return index >= 0 ? _entrys[index]! : new DatabaseEntry(this) { Name = "Empty" };
        }

        public DatabaseEntry AddEntry(string name, out bool added)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));

            if (_databaseNames.Add(name))
            {
                added = true;
                var entry = new DatabaseEntry(this) { Name = name };

                _entrys.Add(entry);

                Interlocked.Exchange(ref _isDirty, 1);

                return entry;
            }

            added = false;
            return FindEntryNonBlock(name);
        }

        public void RemoveEntry(string name)
        {
            var ent = _entrys.Remove(name);
            _databaseNames.Remove(name);
            ent?.Changed(ChangeType.Deleted, name, string.Empty);

            Interlocked.Exchange(ref _isDirty, 1);
        }

        IEnumerator IEnumerable.GetEnumerator() 
            => GetEnumerator();

        public void Save(TextWriter writer)
        {
            if (_isDirty == 0) return;
            Interlocked.Exchange(ref _isDirty, 0);

            foreach (DatabaseEntry entry in this)
            {
                writer.Write(EncodeIfRequied(entry.Name));

                foreach (Metadata data in entry) writer.Write("\t{0}={1}", EncodeIfRequied(data.Key), EncodeIfRequied(data.Value));
                writer.Write(writer.NewLine);
            }

            writer.Write("End");
        }

        public void Clear()
        {
            if (_entrys.Count == 0) return;

            _entrys = new DatabaseArray<DatabaseEntry>();
            _databaseNames.Clear();

            Interlocked.Exchange(ref _isDirty, 1);
        }

        private static string EncodeIfRequied(string? name)
        {
            if (name == null) return string.Empty;

            if (!name.Contains("\r") && !name.Contains("\t") && !name.Contains("\n") && !name.StartsWith(":")) return name;

            var temp = Coder.Encode(name);
            return ":" + temp;
        }

        private static string DecodeIfRequied(string name)
        {
            return !name.StartsWith(":") ? name : Coder.Decode(name.Substring(1));
        }

        public IEnumerator<DatabaseEntry> GetEnumerator() 
            => _entrys.Where(item => item != null).GetEnumerator()!;

        internal interface ICompareName
        {
            string CompareKey { get; }
        }

        internal sealed class DatabaseArray<TElement> : IEnumerable<TElement?>
            where TElement : class, ICompareName
        {
            private class IndexEntry : IComparable<IndexEntry>
            {
                public int IndexValue { get; set; }
                public int Count { get; set; }
                public char Char { get; set; }

                public int CompareTo(IndexEntry? other)
                {
                    if (other == null) return -1;
                    return Char - other.Char;
                }

                public override string ToString()
                    => Char.ToString(CultureInfo.InvariantCulture);
            }

            public int Count { get; private set; }

            private TElement?[] _elements = new TElement[0];

            private IndexEntry[] _index = new IndexEntry[0];

            private IndexEntry FindIndex(char targetChar)
            {
                if (_index.Length == 0)
                {
                    Array.Resize(ref _index, 1);
                    _index[0] = new IndexEntry { Char = targetChar, IndexValue = 0 };

                    return _index[0];
                }

                var entry = new IndexEntry { Char = targetChar };

                var pos = Array.BinarySearch(_index, entry);
                if (pos >= 0) entry = _index[pos];
                else
                {
                    var real = ~pos;
                    Array.Resize(ref _index, _index.Length + 1);
                    if (real != _index.Length) Array.Copy(_index, real, _index, real + 1, _index.Length - 1 - real);
                    _index[real] = entry;

                    pos = real;
                }

                var result = 0;
                for (var i = 0; i < pos; i++)
                    result += _index[i].Count;

                entry.IndexValue = result;
                return entry;
            }

            public int Search(string key)
            {
                var sourceIndex = FindIndex(key[0]).IndexValue;
                int i;

                for (i = sourceIndex; i < _elements.Length; i++)
                {
                    var ele = _elements[i];
                    var result = string.CompareOrdinal(ele?.CompareKey, key);
                    if (result == 0) return i;
                    if (result > 0) return ~i;
                }

                return ~i;
            }

            public void Add(TElement element)
            {
                string key = element.CompareKey;
                IndexEntry entry = FindIndex(key[0]);
                int i;
                for (i = entry.IndexValue; i < Count; i++)
                {
                    var num = string.CompareOrdinal(_elements[i]?.CompareKey, element.CompareKey);
                    if (num >= 0) break;
                }

                ResizeArray();

                if (i == Count)
                {
                    _elements[i] = element;
                    Count++;
                    entry.Count++;
                    return;
                }

                for (var currPos = Count; currPos >= i; currPos--)
                {
                    if (currPos == 0) break;
                    _elements[currPos] = _elements[currPos - 1];
                }

                _elements[i] = element;
                entry.Count++;
                Count++;
            }

            public TElement? Remove(string key)
            {
                IndexEntry entry = FindIndex(key[0]);
                int index;
                var result = 1;
                TElement? ele = null;

                for (index = entry.IndexValue; index < _elements.Length; index++)
                {
                    ele = _elements[index];
                    result = string.CompareOrdinal(ele?.CompareKey, key);
                    if (result == 0) break;
                    if (result > 0) return null;
                }
                if (index == 0 && result != 0)
                    return null;

                _elements[index] = default;

                entry.Count--;
                Count--;

                MoveArray(index);

                return ele;
            }

            public TElement? this[int index] => _elements[index];

            private void ResizeArray()
            {
                if (_elements.Length > Count) return;
                var num = (_elements.Length == 0) ? 4 : (_elements.Length * 2);
                if (num > 2146435071)
                    num = 2146435071;
                if (num < Count)
                    num = Count;

                if (_elements.Length < num)
                    Array.Resize(ref _elements, num);
            }
            private void MoveArray(int emptyIndex)
            {
                for (var i = emptyIndex; i < _elements.Length; i++)
                {
                    if (i + 1 == _elements.Length)
                        break;
                    _elements[i] = _elements[i + 1];
                }

                for (var i = _elements.Length - 1; i >= Count; i--)
                    _elements[i] = default;
            }

            public IEnumerator<TElement?> GetEnumerator()
            {
                for (var i = 0; i < Count; i++)
                {
                    yield return _elements[i];
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
                => GetEnumerator();
        }

        [PublicAPI]
        public abstract class ChangedHelper : ICompareName
        {
            protected ChangedHelper(DatabaseHelper database)
                => Database = database;

            public DatabaseHelper Database { get; }

            protected abstract string CompareName { get; }

            private WeakCollection<IChangedHandler>? _handlers;

            public void RegisterHandler(IChangedHandler handler)
            {
                lock (this)
                {
                    if (_handlers == null)
                    {
                        _handlers = new WeakCollection<IChangedHandler>();
                        _handlers.CleanedEvent += (sender, e) => { lock (this) if (_handlers.Count == 0) _handlers = null; };
                    }
                    _handlers.Add(handler);
                }
            }

            internal void Changed(ChangeType type, string oldContent, string content)
            {
                if (_handlers == null) return;

                foreach (var handler in _handlers) handler.Changed(type, CompareName, oldContent, content);
            }

            string ICompareName.CompareKey => CompareName;
        }
        public sealed class Metadata : ChangedHelper
        {
            public Metadata(DatabaseHelper database)
                : base(database)
            {
            }

            protected override string CompareName => _key;

            private string _key = string.Empty;

            public string Key
            {
                get => _key;
                set
                {
                    Changed(ChangeType.MetaKey, Interlocked.Exchange(ref _key, value), value);
                    Interlocked.Exchange(ref Database._isDirty, 1);
                }
            }

            private string _value = string.Empty;

            public string Value
            {
                get => _value;
                set
                {
                    Changed(ChangeType.MetaValue, Interlocked.Exchange(ref _value, value), value);
                    Interlocked.Exchange(ref Database._isDirty, 1);
                }
            }

            public override string ToString()
                => _key + ":" + _value;
        }
        [PublicAPI]
        public sealed class DatabaseEntry : ChangedHelper, IEnumerable<Metadata>
        {
            internal class DatabaseEntryAcessor
            {
                private readonly DatabaseEntry _entry;

                public DatabaseArray<Metadata> MetadataAcc => _entry._metadata;

                public HashSet<string> MetaNames => _entry._metaNames;

                public DatabaseEntryAcessor(DatabaseEntry entry)
                    => _entry = entry;
            }

            protected override string CompareName => _name;

            private string _name = string.Empty;

            public string Name
            {
                get => _name;
                set
                {
                    Changed(ChangeType.Name, Interlocked.Exchange(ref _name, value), value);
                    Interlocked.Exchange(ref Database._isDirty, 1);
                }
            }

            private readonly DatabaseArray<Metadata> _metadata = new DatabaseArray<Metadata>();
            private readonly HashSet<string> _metaNames = new HashSet<string>();


            public DatabaseEntry(DatabaseHelper database)
                : base(database)
            {
            }

            public IEnumerable<string> Keys => new ReadOnlyEnumerator<string>(_metaNames);

            public Metadata? FindMetadata(string key)
                => FindMeatadataNonBlock(key);

            private Metadata FindMeatadataNonBlock(string name)
            {
                var index = _metadata.Search(name);
                return index < 0 ? new Metadata(Database) { Key = "Empty", Value = "Empty" } : _metadata[index]!;
            }

            public Metadata AddMetadata(string key, out bool added)
            {
                if (string.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));

                if (_metaNames.Add(key))
                {
                    added = true;
                    var meta = new Metadata(Database) { Key = key };

                    _metadata.Add(meta);

                    Interlocked.Exchange(ref Database._isDirty, 1);

                    return meta;
                }

                added = false;
                return FindMeatadataNonBlock(key);
            }

            public void RemoveMetadata(string name)
            {
                var meta = _metadata.Remove(name);
                _metaNames.Remove(name);

                meta?.Changed(ChangeType.Deleted, name, string.Empty);

                Interlocked.Exchange(ref Database._isDirty, 1);
            }

            IEnumerator IEnumerable.GetEnumerator()
                => GetEnumerator();

            public IEnumerator<Metadata> GetEnumerator()
                => _metadata.Where(item => item != null).GetEnumerator()!;

            public override string ToString()
                => _name;
        }

        private static class Coder
        {
            private class EscapePart
            {
                public char Escaped { get; set; }
                public string Sequence { get; set; } = string.Empty;

                public override string ToString()
                    => Escaped + " " + Sequence;
            }
            private static readonly EscapePart[] Parts =
            {
                new EscapePart {Escaped = '\r', Sequence = "001"},
                new EscapePart {Escaped = '\t', Sequence = "002"},
                new EscapePart {Escaped = '\n', Sequence = "003"},
                new EscapePart {Escaped = ':', Sequence = "004"}
            };

            private const string EscapeString = @"\\";

            private static EscapePart? GetPartforChar(char @char)
            {
                char toSearch = @char switch
                {
                    '\r' => @char,
                    '\t' => @char,
                    '\n' => @char,
                    ':' => @char,
                    _ => 'a'
                };

                return toSearch == 'a' ? null : Parts.First(p => p.Escaped == toSearch);
            }

            private static EscapePart? GetPartforSequence(string @char)
            {
                string toSearch = @char switch
                {
                    "001" => @char,
                    "002" => @char,
                    "003" => @char,
                    "004" => @char,
                    _ => "a"
                };

                return toSearch == "a" ? null : Parts.First(p => p.Sequence == toSearch);
            }

            public static string Encode(IEnumerable<char> toEncode)
            {
                var builder = new StringBuilder();
                foreach (var @char in toEncode)
                {
                    var part = GetPartforChar(@char);
                    if (part == null) builder.Append(@char);
                    else builder.Append(EscapeString + part.Sequence);
                }

                return builder.ToString();
            }

            public static string Decode(IEnumerable<char> toDecode)
            {
                var builder = new StringBuilder();

                var flag = false;
                var flag2 = false;
                var pos = 0;
                var sequence = string.Empty;
                var temp = string.Empty;

                foreach (var @char in toDecode)
                {
                    if (flag2)
                    {
                        sequence += @char;
                        pos++;

                        if (pos != 3) continue;

                        var part = GetPartforSequence(sequence);
                        if (part == null) builder.Append(temp).Append(sequence);
                        else builder.Append(part.Escaped);

                        flag = false;
                        flag2 = false;
                        pos = 0;
                        sequence = string.Empty;
                        temp = string.Empty;
                    }
                    else if (flag)
                    {
                        flag2 = @char == '\\';
                        if (!flag2)
                        {
                            builder.Append("\\").Append(@char);
                            flag = false;
                        }
                        else temp += @char;
                    }
                    else
                    {
                        flag = @char == '\\';

                        if (!flag)
                        {
                            builder.Append(@char);
                        }
                        else temp += @char;
                    }
                }

                return builder.ToString();
            }
        }

    }
}