using System;
using LanguagesEnum;

namespace TextConstPack
{

    public class TextPack
    {
        public string Name { get; set; }
        public TextPack() { }
        public string CommandStart = "/start";
        public string CommandHelp = "/help";
        public string PromptChoosingLanguage = "Please choose a language";
        public string PromptGreetings { get; set; }
        public string BtnBackToStart { get; set; }
        public string BtnChoosingPlatform_YT { get; set; }
        public string BtnChoosingPlatform_Insta { get; set; }
        public string PromptChoosingPlatform { get; set; }
        public string PromptChoosingYTFunction { get; set; }
        public string BtnChoosingYTFunction_Filters { get; set; }
        public string BtnChoosingYTFunction_Search { get; set; }
        public string BtnChoosingYTFunction_ChannelSearch { get; set; }

        public string PromptChoosingYTFilters { get; set; }
        public string PromptCurrentFiltersTemplate;
        public string PromptCurrentFilters(long views, long likes)
        {
            return string.Format(PromptCurrentFiltersTemplate,views, likes);
        }

        public string BtnSettingYTViews { get; set; }
        public string PromptSettingYTViews { get; set; }
        public string PromptSettingYTViews_Done { get; set; }
        public string PromptSettingYTViews_Disabled { get; set; }

        public string BtnSettingYTLikes { get; set; }
        public string PromptSettingYTLikes { get; set; }
        public string PromptSettingYTLikes_Done { get; set; }
        public string PromptSettingYTLikes_Disabled { get; set; }
        public string PromptSettingYTFilter_Error { get; set; }

        public string PromptSearchingYTVideo { get; set; }
        public string BtnSearchingYTVideo_Previous5 { get; set; }
        public string BtnSearchingYTVideo_Next5 { get; set; }
        public string PromptSearchingYTVideo_Done { get; set; }

        public string PromptSearchingYTChannel { get; set; }
        public string PromptInvalidCommandResponse { get; set; }
        public string PromptHelp { get; set; }
    }

    public static class TextPackData
    {
        public static TextPack[] AvailablePacks = new[]
        {
            new TextPack()
            {
                Name = "English",
                PromptGreetings = "Hi, I can show you YouTube videos and Instagram posts " +
                "with desired number of views and likes.",
                BtnBackToStart = "Back to Start",
                PromptChoosingPlatform = "First, choose a platform you are interested in",
                BtnChoosingPlatform_YT = "YouTube",
                
                BtnChoosingPlatform_Insta = "Instagram",
                BtnChoosingYTFunction_Filters = "YT Filters",
                BtnChoosingYTFunction_Search = "YT Search",
                BtnChoosingYTFunction_ChannelSearch = "YT Channel Search",
                PromptChoosingYTFunction = $"\tTo search a YouTube video press the \"YT Search\"" +
                                           $" button\n\n" +
                                           $"\tTo filter your search results by likes or views press the " +
                                           $"\"YT Filters\" button\n\n" +
                                           $"\tTo view a specific channel videos press the " +
                                           $"\"YT Channel Search\" button",
                 PromptChoosingYTFilters = "\tTo set required filter press an appropriate button.\n",
                PromptCurrentFiltersTemplate = "\tCurrent filters: [{0} views & {1} likes]",
                 BtnSettingYTViews = "Set YT Views",
                 PromptSettingYTViews = "Enter the number of Views " +
                                        "to set the filter or 0 to disable it",
                 PromptSettingYTViews_Done = "Views filter has been set",
                 PromptSettingYTViews_Disabled = "Views filter has been disabled",

                 BtnSettingYTLikes = "Set YT Likes",
                 PromptSettingYTLikes = "Enter the number of Likes " +
                                        "to set the filter or 0 to disable it.",
                 PromptSettingYTLikes_Done = "Likes filter has been set",
                 PromptSettingYTLikes_Disabled = "Likes filter has been disabled",
                 PromptSettingYTFilter_Error = "Invalid input. Enter a positive " +
                                                                           "number or 0 to disable a filter.",
                 PromptSearchingYTVideo = "Enter your search request",
                 BtnSearchingYTVideo_Previous5 = "Previous 5",
                 BtnSearchingYTVideo_Next5 = "Next 5",
                 PromptSearchingYTVideo_Done = "Your search results:",
                 PromptSearchingYTChannel = "Enter a channel name",
                PromptInvalidCommandResponse = "Invalid command, use menu buttons",
                PromptHelp = " /start - Start over from language select\n" +
                             " /help - Request a help message"
    },
            new TextPack()
            {
                Name = "Русский",
                PromptGreetings = "Привет, я могу выводить видео с YouTube и посты из Instagram, " +
                "отфильтрованные по количеству просмотров и лайков.",
                BtnBackToStart = "В начало",
                BtnChoosingPlatform_YT = "YouTube",
                BtnChoosingPlatform_Insta = "Instagram",
                BtnChoosingYTFunction_Filters = "Фильтры YT",
                BtnChoosingYTFunction_Search = "Поиск YT",
                BtnChoosingYTFunction_ChannelSearch = "Поиск каналов YT",
                PromptChoosingPlatform = "Сначала, выберите интересующую вас платформу",
                PromptCurrentFiltersTemplate = "\tТекущие фильтры: [{0} просмотров & {1} лайков]",
                PromptChoosingYTFunction = $"\tДля поиска YouTube видео нажмите кнопку " +
                                           $"\"Поиск YT\"\n\n" +
                                           $"\tДля задания фильтров просмотров и лайков нажмите кнопку " +
                                           $"\"Фильтры YT\"\n\n" +
                                           $"\tДля просмотра видео с определённого канала нажмите кнопку " +
                                           $"\"Поиск каналов YT\"\n\n",

                 PromptChoosingYTFilters =
                    "\tДля задания требуемого фильтра нажмите соответствующую кнопку.\n",

                 BtnSettingYTViews = "Задать просмотры YT",
                 PromptSettingYTViews = "Введите число просмотров чтобы задать фильтр " +
                                                                    "или 0 чтобы его выключить",
                 PromptSettingYTViews_Done = "Фильтр просмотров задан",
                 PromptSettingYTViews_Disabled = "Фильтр просмотров отключен",

                 BtnSettingYTLikes = "Задать лайки YT",
                 PromptSettingYTLikes = "Введите число лайков чтобы задать фильтр " +
                                                                    "или 0 чтобы его выключить",
                 PromptSettingYTLikes_Done = "Фильтр лайков задан",
                 PromptSettingYTLikes_Disabled = "Фильтр лайков отключен",
                 PromptSettingYTFilter_Error = "Неверный ввод. Введите положительное число, " +
                                                                           "или 0 для сброса фильтра",

                 PromptSearchingYTVideo = "Введите поисковый запрос",
                 BtnSearchingYTVideo_Previous5 = "Предыдущие 5",
                 BtnSearchingYTVideo_Next5 = "Следующие 5",
                 PromptSearchingYTVideo_Done = "Видео по вашему запросу:",

                 PromptSearchingYTChannel = "Введите название канала",
                    PromptInvalidCommandResponse = "Такой команды нет, используйте кнопки меню",
                    PromptHelp = " /start - Начать с выбора языка\n" +
                         " /help - Запросить подсказку о командах"
            }
};
        
        
    }

    
    //public class TextPack/*English : TextPack*/
    //{
    //    public const string PromptGreetings = "Hi, I can show you YouTube videos and Instagram posts " +
    //                                                   "with desired number of views and likes.";

    //    public const string PromptChoosingPlatform = "First, choose a platform you are interested in";
    //    public const string BtnBackToStart = "Back to Start";
    //    public const string BtnChoosingPlatform_YT = "YouTube";
    //    public const string BtnChoosingPlatform_Insta = "Instagram";

    //    public string PromptChoosingYTFunction = $"To search YouTube video press the \"{BtnChoosingYTFunction_Search}\"" +
    //                                                            $" button\n" +
    //                                                            $"To filter your search results by likes or views press the " +
    //                                                            $"\"{BtnChoosingYTFunction_Filters}\" button\n" +
    //                                                            $"To view a specific channel videos press the " +
    //                                                            $"\"{BtnChoosingYTFunction_ChannelSearch}\" button";
    //    public const string BtnChoosingYTFunction_Filters = "YT Filters";
    //    public const string BtnChoosingYTFunction_Search = "YT Search";
    //    public const string BtnChoosingYTFunction_ChannelSearch = "YT Channel Search";

    //    public const string PromptChoosingYTFilters =
    //        "To set required filter press an appropriate button.";

    //    public string PromptCurrentFilters(long views, long likes)
    //    {
    //        return $"Current filters: [{views} views & {likes} likes]";
    //    }

    //    public const string BtnSettingYTViews = "Set YT Views";
    //    public const string PromptSettingYTViews = "Enter the number of Views " +
    //                                                        "to set the filter or 0 to disable it";
    //    public const string PromptSettingYTViews_Done = "Views filter has been set";
    //    public const string PromptSettingYTViews_Disabled = "Views filter has been disabled";

    //    public const string BtnSettingYTLikes = "Set YT Likes";
    //    public const string PromptSettingYTLikes = "Enter the number of Likes " +
    //                                                        "to set the filter or 0 to disable it.";
    //    public const string PromptSettingYTLikes_Done = "Likes filter has been set";
    //    public const string PromptSettingYTLikes_Disabled = "Likes filter has been disabled";
    //    public const string PromptSettingYTFilter_Error = "Invalid input. Enter a positive " +
    //                                                               "number or 0 to disable a filter.";

    //    public const string PromptSearchingYTVideo = "Enter your search request";
    //    public const string BtnSearchingYTVideo_Previous5 = "Previous 5";
    //    public const string BtnSearchingYTVideo_Next5 = "Next 5";
    //    public const string PromptSearchingYTVideo_Done = "Your search results:";

    //    public const string PromptSearchingYTChannel = "Enter a channel name";
    //}

    //public class TextPackRussian : TextPack
    //{
    //    public const string PromptGreetings = "Привет, я могу выводить видео с YouTube и посты из Instagram, " +
    //                                                   "отфильтрованные по количеству просмотров и лайков.";

    //    public const string PromptChoosingPlatform = "Сначала, выберите интересующую вас платформу";
    //    public const string BtnBackToStart = "В начало";
    //    public const string BtnChoosingPlatform_YT = "YouTube";
    //    public const string BtnChoosingPlatform_Insta = "Instagram";

    //    public string PromptChoosingYTFunction = $"Для поиска YouTube видео нажмите кнопку " +
    //                                                            $"\"{BtnChoosingYTFunction_Search}\"\n" +
    //                                                            $"Для задания фильтров просмотров и лайков нажмите кнопку " +
    //                                                            $"\"{BtnChoosingYTFunction_Filters}\"\n" +
    //                                                            $"Для просмотра видео с определённого канала нажмите кнопку " +
    //                                                            $"\"{BtnChoosingYTFunction_ChannelSearch}\"\n";
    //    public const string BtnChoosingYTFunction_Filters = "Фильтры YT";
    //    public const string BtnChoosingYTFunction_Search = "Поиск YT";
    //    public const string BtnChoosingYTFunction_ChannelSearch = "Поиск каналов YT";

    //    public const string PromptChoosingYTFilters =
    //        "Для задания требуемого фильтра нажмите соответствующую кнопку";

    //    public string PromptCurrentFilters(long views, long likes)
    //    {
    //        return $"Текущие фильтры: [{views} просмотров & {likes} лайков]";
    //    }

    //    public const string BtnSettingYTViews = "Задать просмотры YT";
    //    public const string PromptSettingYTViews = "Введите число просмотров чтобы задать фильтр " +
    //                                                        "или 0 чтобы его выключить";
    //    public const string PromptSettingYTViews_Done = "Фильтр просмотров задан";
    //    public const string PromptSettingYTViews_Disabled = "Фильтр просмотров отключен";

    //    public const string BtnSettingYTLikes = "Задать лайки YT";
    //    public const string PromptSettingYTLikes = "Введите число лайков чтобы задать фильтр " +
    //                                                        "или 0 чтобы его выключить";
    //    public const string PromptSettingYTLikes_Done = "Фильтр лайков задан";
    //    public const string PromptSettingYTLikes_Disabled = "Фильтр лайков отключен";
    //    public const string PromptSettingYTFilter_Error = "Неверный ввод. Введите положительное число, " +
    //                                                               "или 0 для сброса фильтра";

    //    public const string PromptSearchingYTVideo = "Введите поисковый запрос";
    //    public const string BtnSearchingYTVideo_Previous5 = "Предыдущие 5";
    //    public const string BtnSearchingYTVideo_Next5 = "Следующие 5";
    //    public const string PromptSearchingYTVideo_Done = "Видео по вашему запросу:";

    //    public const string PromptSearchingYTChannel = "Введите название канала";
    //}
}