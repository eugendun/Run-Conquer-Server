//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Run_Conquer_Server.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class GameInstance
    {
        public GameInstance()
        {
            this.Players = new HashSet<Player>();
        }
    
        public int Id { get; set; }
    
        public virtual ICollection<Player> Players { get; set; }
    }
}
