//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CrawlerWeb.Data
{
    using System;
    using System.Collections.Generic;
    
    public partial class UserDetail
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public UserDetail()
        {
            this.SiteUsers = new HashSet<SiteUser>();
        }
    
        public int Id { get; set; }
        public int Obtained { get; set; }
        public string Usertype { get; set; }
        public string Academiclevel { get; set; }
        public string Field { get; set; }
        public string Phone { get; set; }
        public string Addr { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SiteUser> SiteUsers { get; set; }
    }
}