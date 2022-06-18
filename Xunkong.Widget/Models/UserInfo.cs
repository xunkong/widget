using Xunkong.Widget.Hoyolab.Account;
using Xunkong.Widget.Hoyolab.DailyNote;
using Xunkong.Widget.Hoyolab.TravelNotes;

namespace Xunkong.Widget.Models
{
    public class UserInfo
    {


        public HoyolabUserInfo HoyolabUserInfo { get; set; }


        public GenshinRoleInfo GenshinRoleInfo { get; set; }


        public DailyNoteInfo DailyNoteInfo { get; set; }


        public TravelNotesDayData TravelNotesDayData { get; set; }


        public bool Error { get; set; }


        public string ErrorMessage { get; set; }


        public UserInfo Self => this;


    }
}
