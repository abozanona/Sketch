using SQLite;
using System.ComponentModel;

namespace Sketch.Models
{
    public abstract class BaseModel
    {
        public abstract BaseModel createEmpty();

        [Ignore]
        [DisplayName("_id")]
        public abstract int _id { get; set; }

        [Ignore]
        [DisplayName("_name")]
        public string _name { get { return ToString(); } }
    }
}
