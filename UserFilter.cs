using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot
{
    class UserFilter
    {
        public string UserId { get; set; }
        public uint Views { get; set; }
        public uint Likes { get; set; }
        public enum FilterType
        {
            Youtube, Instagram
        }
        public FilterType Filter { get; set; }
        public UserFilter(string userId, uint views, uint likes, FilterType filter)
        {
            UserId = userId;
            Views = views;
            Likes = likes;
            Filter = filter;
        }

        public override string ToString()
        {
            return $"[{UserId}]'s {Filter} filter contains ({Views}) views and ({Likes}) likes";
        }
    }
}
