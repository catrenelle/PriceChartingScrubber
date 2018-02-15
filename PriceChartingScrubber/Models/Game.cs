//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PriceChartingScrubber.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Game
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Game()
        {
            this.GenreLists = new HashSet<GenreList>();
        }
    
        public long id { get; set; }
        public Nullable<int> SystemId { get; set; }
        public string GameName { get; set; }
        public Nullable<System.DateTime> ReleaseDate { get; set; }
        public string ESRBRating { get; set; }
        public string UPC { get; set; }
        public string AmazonASIN { get; set; }
        public string EbayEPID { get; set; }
        public Nullable<long> PriceChartingID { get; set; }
        public Nullable<decimal> LoosePricing { get; set; }
        public Nullable<decimal> CompletePricing { get; set; }
        public Nullable<decimal> NewPrice { get; set; }
        public Nullable<decimal> GradedPrice { get; set; }
        public Nullable<decimal> BoxOnlyPrice { get; set; }
        public Nullable<decimal> ManualOnlyPrice { get; set; }
        public Nullable<decimal> PriceChartingLoosePrice { get; set; }
        public Nullable<decimal> GameStopLoosePrice { get; set; }
        public string Genre { get; set; }
        public Nullable<System.DateTime> LastModified { get; set; }
    
        public virtual SystemList SystemList { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<GenreList> GenreLists { get; set; }
    }
}
