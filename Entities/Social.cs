using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Toolbelt.ComponentModel.DataAnnotations.Schema.V5;

namespace InformationWaves.Entities
{
    internal class Social
    {
        [Key]
        public int Id { get; set; }
        [Required, MaxLength(100)]
        public string Name { get; set; }
        [Required, MaxLength(200)]
        public string Group { get; set; }
        [Required]
        public string Text { get; set; }
        [Required, MaxLength(50)]
        public string Date { get; set; }
        [Required]
        public string Link { get; set; }
        [Required]
        public string Author { get; set; }
        [Required]
        public string AuthorId { get; set; }
        [Required]
        public bool IsComment { get; set; }
        [MaxLength(100)]
        public string? Tematics { get; set; }
        public bool? Keynote { get; set; }
        [Required]
        public bool Review { get; set; } = false;
        [Column(TypeName = "date")]
        public DateTime? DateView { get; set; }
        [Required]
        public string Annotation { get; set; }
        [Required]
        public string Keywords { get; set; }
    }
}
