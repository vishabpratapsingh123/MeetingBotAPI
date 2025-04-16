using Microsoft.EntityFrameworkCore;
using static Azure.Core.HttpHeader;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MeetingBotAPI.Models
{
    public class ActionItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ActionId { get; set; }

        public int? MeetingId { get; set; }  // Nullable foreign key

        public string? Description { get; set; }

        [StringLength(255)]
        public string? Responsible { get; set; }

        public DateTime? Deadline { get; set; }

        [StringLength(100)]
        public string? Status { get; set; }

        public string? Notes { get; set; }

        public DateTime? CreatedAt { get; set; }
    }
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ActionItem> ActionItems { get; set; }
    }

}
