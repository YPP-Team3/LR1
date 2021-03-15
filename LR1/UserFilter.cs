using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LanguagesEnum;

namespace TelegramBot
{
    class UserFilter
    {
        public long UserId { get; set; }
        public long ViewsYT { get; set; }
        public long LikesYT { get; set; }
        public long LikesInsta { get; set; }

        public string UserLanguage {get;set;}
        
        public UserFilter(long userId, long viewsYt, long likesYt)
        {
            UserId = userId;
            ViewsYT = viewsYt;
            LikesYT = likesYt;
            LikesInsta = 0;
            UserLanguage = "Undefined";
        }
        public UserFilter(long userId, long likesInsta)
        {
            UserId = userId;

            LikesInsta = likesInsta;
            ViewsYT = LikesYT = 0;

            UserLanguage = "Undefined";
        }

        public UserFilter()
        {
            UserLanguage = "Undefined";
        }
        public UserFilter(long userId)
        {
            UserId = userId;
            ViewsYT = LikesYT = LikesInsta = 0;

            UserLanguage = "Undefined";
        }
        public override string ToString()
        {
            return $"[{UserId}]'s filter contains ({ViewsYT}) views and ({LikesYT}) likes," +
                   $" language = {UserLanguage}";
        }
    }
}
