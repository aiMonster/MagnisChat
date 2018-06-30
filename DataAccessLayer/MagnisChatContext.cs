using Common.DTO.Account;
using Common.DTO.Messages;
using Common.DTO.Rooms;
using Common.DTO.Sockets;
using Common.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer
{
    public class MagnisChatContext : DbContext
    {

        public MagnisChatContext(DbContextOptions<MagnisChatContext> options)
           : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RoomParticipants>()
                .HasKey(pc => new { pc.UserId, pc.RoomId });

            modelBuilder.Entity<RoomParticipants>()
                .HasOne(pc => pc.User)
                .WithMany(p => p.Rooms)
                .HasForeignKey(pc => pc.UserId);

            modelBuilder.Entity<RoomParticipants>()
                .HasOne(pc => pc.Room)
                .WithMany(c => c.Participants)
                .HasForeignKey(pc => pc.RoomId);
        }

        public DbSet<UserEntity> Users { get; set; }
        public DbSet<SocketTokenEntity> SocketTokens { get; set; }
        public DbSet<RoomEntity> Rooms { get; set; }
        public DbSet<MessageEntity> Messages { get; set; }
        public DbSet<FileEntity> Files { get; set; }
    }
}
