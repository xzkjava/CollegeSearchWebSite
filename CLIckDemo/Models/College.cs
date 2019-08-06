using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CLickDemo.Models
{
    public class College
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("school.name")]
        public string Name { get; set; }
        [JsonProperty("school.city")]
        public string City { get; set; }
        [JsonProperty("school.state")]
        public string State { get; set; }
        [JsonProperty("school.zip")]
        public string ZipCode { get; set; }
        [JsonProperty("school.school_url")]
        public string Website { get; set; }
        [JsonProperty("latest.admissions.admission_rate.overall")]
        public Single? AdmissionRate { get; set; }
        [JsonProperty("latest.cost.tuition.in_state")]
        public int? Tuition_In_State { get; set; }
        [JsonProperty("latest.cost.tuition.out_of_state")]
        public int? Tuition_Out_State { get; set; }
        [JsonProperty("latest.admissions.sat_scores.average.overall")]
        public Single? AverageSAT { get; set; }

    }
   
}
