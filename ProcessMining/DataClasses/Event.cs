using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessMining.DataClasses
{
    public class Event
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string EventCode { get; set; }
        public string EventName { get; set; }
        public string EventDescription { get; set; }
    }

    public class EventPair
    {
        public List<Event> Left { get; set; }
        public List<Event> Right { get; set; }
    }

    public class EventGridItem
    {
        public string Activity { get; set; }
        public string Originators { get; set; }
        public int MinTime { get; set; }
        public int AvgTime { get; set; }
        public int MaxTime { get; set; }
    }
    public class TransitionGridItem
    {
        public string From { get; set; }
        public string To { get; set; }
        public int MinTime { get; set; }
        public int AvgTime { get; set; }
        public int MaxTime { get; set; }
    }

}
