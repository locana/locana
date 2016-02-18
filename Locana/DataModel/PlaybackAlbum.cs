using Locana.Utility;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using Windows.UI.Xaml.Media.Imaging;

namespace Locana.DataModel
{
    public class Album : List<Thumbnail>, INotifyPropertyChanged, INotifyCollectionChanged
    {
        public static string LOCANA_DIRECTORY = SystemUtil.GetStringResource("ApplicationTitle");

        public string Key { private set; get; }

        private Thumbnail Thumb;

        public SortOrder Order { private set; get; }

        public BitmapImage RandomThumbnail
        {
            get
            {
                if (Thumb == null)
                {
                    lock (this)
                    {
                        Thumb = this[new Random().Next(0, Count - 1)];
                    }
                    Thumb.PropertyChanged += Thumb_PropertyChanged;
                }
                return Thumb.LargeImage;
            }
        }

        void Thumb_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Thumbnail.LargeImage))
            {
                OnPropertyChanged(nameof(RandomThumbnail));
            }
        }

        public void Sort(SortOrder order)
        {

        }

        public enum SortOrder
        {
            AddToTail,
            NewOneFirst,
        }

        public SelectivityFactor SelectivityFactor
        {
            set
            {
                lock (this)
                {
                    foreach (var thumb in this)
                    {
                        thumb.SelectivityFactor = value;
                    }
                }
            }
        }

        public Album(string key, SortOrder sortOrder = SortOrder.AddToTail)
        {
            Key = key;
            Order = sortOrder;
        }

        new public void Add(Thumbnail content)
        {
            lock (this)
            {
                if (Contains(content))
                {
                    return; // Avoid duplication
                }

                var previous = Count;
                int inserted = 0;
                switch (Order)
                {
                    case SortOrder.AddToTail:
                        inserted = previous;
                        break;
                    case SortOrder.NewOneFirst:
                        inserted = InsersionIndexNewOneFirst(content, this);
                        break;
                }

                base.Insert(inserted, content);
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, content, inserted));
                if (previous == 0)
                {
                    Thumb = null;
                    OnPropertyChanged(nameof(RandomThumbnail));
                }
            }
        }

        private static int InsersionIndexNewOneFirst(Thumbnail content, List<Thumbnail> stored)
        {
            var insertFlag = false;
            for (int i = 0; i < stored.Count; i++)
            {
                var thumb = stored[i].CacheFile;
                if (0 < thumb.DateCreated.CompareTo(content.CacheFile.DateCreated))
                {
                    insertFlag = true;
                }
                else if (insertFlag)
                {
                    return i;
                }
            }
            return stored.Count;
        }

        new public bool Remove(Thumbnail content)
        {
            lock (this)
            {
                var index = IndexOf(content);
                var removed = base.Remove(content);
                if (removed)
                {
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, content, index));
                }
                return removed;
            }
        }

        new public void AddRange(IEnumerable<Thumbnail> contents)
        {
            lock (this)
            {
                var previous = Count;
                base.AddRange(contents);
                var list = new List<Thumbnail>(contents);
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, list, previous));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;
        private void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(Count));
            OnPropertyChanged("Item[]");
            try
            {
                CollectionChanged?.Invoke(this, e);
            }
            catch (NotSupportedException)
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }
    }
}
