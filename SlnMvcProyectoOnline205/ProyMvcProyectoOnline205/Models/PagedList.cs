using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ProyMvcProyectoOnline205.Models
{
    public class PagedList<T> : IEnumerable<T>
    {
        public IReadOnlyList<T> Items { get; }
        public int PageNumber { get; }
        public int PageSize { get; }
        public int TotalItems { get; }
        public int TotalPages => PageSize <= 0 ? 0 : (int)System.Math.Ceiling((double)TotalItems / PageSize);
        public bool HasPrevious => PageNumber > 1;
        public bool HasNext => PageNumber < TotalPages;

        public PagedList(IEnumerable<T> items, int totalItems, int pageNumber, int pageSize)
        {
            Items = items?.ToList() ?? new List<T>();
            TotalItems = totalItems;
            PageNumber = pageNumber;
            PageSize = pageSize;
        }

        public static PagedList<T> Create(IEnumerable<T> source, int pageNumber, int pageSize)
        {
            if (source == null) source = Enumerable.Empty<T>();
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            var list = source as IList<T> ?? source.ToList();
            int total = list.Count;

            var page = list.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            return new PagedList<T>(page, total, pageNumber, pageSize);
        }

        public IEnumerator<T> GetEnumerator() => Items.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
