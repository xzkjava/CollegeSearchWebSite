using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace CLickDemo.Models
{
    public class FavCollege
    {
     
        [Key, Column(Order = 1)]
        public int Id { get; set; }
       
        public string Name { get; set; }
      
        public string City { get; set; }
      
        public string State { get; set; }
       
        public string ZipCode { get; set; }
       
        public string Website { get; set; }
       
        public Single? AdmissionRate { get; set; }
     
        public int? Tuition_In_State { get; set; }
     
        public int? Tuition_Out_State { get; set; }
        
        public Single? AverageSAT { get; set; }

        [Key, Column(Order=2)]
        [ForeignKey("UserName")]
        public string UserName { get; set; }

    }
}
