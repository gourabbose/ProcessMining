using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessMining.DataClasses
{
    public class Log
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey("Process")]
        public int ProcessId { get; set; }
        public Process Process { get; set; }

        [ForeignKey("Event")]
        public int EventId { get; set; }
        public Event Event { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        [ForeignKey("Originator")]
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; }

        public int InstanceId { get; set; }
    }
}
