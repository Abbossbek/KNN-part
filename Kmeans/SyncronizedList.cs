using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kmeans
{

    public class SyncronisedList<T> : IList<T>, IEnumerable<T>, System.Collections.IList
    {
        private ReaderWriterLockSlim cacheLock = new ReaderWriterLockSlim();
        private IList<T> innerCache = new List<T>();

        private U ReadReturn<U>(Func<U> function)
        {
            cacheLock.EnterReadLock();
            try { return function(); }
            finally { cacheLock.ExitReadLock(); }
        }

        private void Read(Action action)
        {
            cacheLock.EnterReadLock();
            try { action(); }
            finally { cacheLock.ExitReadLock(); }
        }

        private U WriteReturn<U>(Func<U> function)
        {
            cacheLock.EnterWriteLock();
            try { return function(); }
            finally { cacheLock.ExitWriteLock(); }
        }

        private void Write(Action action)
        {
            cacheLock.EnterWriteLock();
            try { action(); }
            finally { cacheLock.ExitWriteLock(); }
        }

        public T this[int index]
        {
            get { return ReadReturn(() => innerCache[index]); }
            set {  Write(() => innerCache[index] = value); }
        }

        public int IndexOf(T item) { return ReadReturn(() => innerCache.IndexOf(item)); }
        public void Insert(int index, T item)
        {
            Write(() => innerCache.Insert(index, item));
        }
        public void RemoveAt(int index)
        {
            Write(() => innerCache.RemoveAt(index));
        }
        public void Add(T item)
        {
            Write(() => innerCache.Add(item));
        }
        public void Clear()
        {
            Write(() => innerCache.Clear());
        }
        public bool Contains(T item) { return ReadReturn(() => innerCache.Contains(item)); }
        public int Count { get { return ReadReturn(() => innerCache.Count); } }
        public bool IsReadOnly { get { return ReadReturn(() => innerCache.IsReadOnly); } }

        public bool IsFixedSize
        {
            get
            {
                return false;
            }
        }

        public object SyncRoot
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool IsSynchronized
        {
            get
            {
                return true;
            }
        }

        object IList.this[int index]
        {
            get { return ReadReturn(() => innerCache[index]); }
            set {  Write(() => innerCache[index] = (T)value); }
        }

        public void CopyTo(T[] array, int arrayIndex) { Read(() => innerCache.CopyTo(array, arrayIndex)); }
        public bool Remove(T item)
        {
            bool ok= WriteReturn(() => innerCache.Remove(item));
            return ok;
        }
        public IEnumerator<T> GetEnumerator() { return ReadReturn(() => innerCache.GetEnumerator()); }
        IEnumerator IEnumerable.GetEnumerator() { return ReadReturn(() => (innerCache as IEnumerable).GetEnumerator()); }

        public int Add(object value)
        {
            Add((T)value);
            return 0;
        }

        public bool Contains(object value)
        {
            return Contains((T)value);
        }

        public int IndexOf(object value)
        {
            return IndexOf((T)value);
        }

        public void Insert(int index, object value)
        {
            Insert(index, (T)value);
        }

        public void Remove(object value)
        {
            Remove((T)value);
        }

        public void CopyTo(Array array, int index)
        {
            CopyTo((T[])array, index);
        }
        public void AddRange(IEnumerable<T> array)
        {
            foreach(T t in array)
            {
                Add(t);
            }
        }
    }

}
