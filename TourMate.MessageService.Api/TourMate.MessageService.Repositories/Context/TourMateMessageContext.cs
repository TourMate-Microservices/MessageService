using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using TourMate.MessageService.Repositories.Models;

namespace TourMate.MessageService.Repositories.Context;

public partial class TourMateMessageContext : DbContext
{
    public TourMateMessageContext()
    {
    }

    public TourMateMessageContext(DbContextOptions<TourMateMessageContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Conversation> Conversations { get; set; }

    public virtual DbSet<FileStorage> FileStorages { get; set; }

    public virtual DbSet<Message> Messages { get; set; }

    public virtual DbSet<MessageType> MessageTypes { get; set; }

    public static string GetConnectionString(string connectionStringName)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json")
            .Build();
        string connectionString = config.GetConnectionString(connectionStringName);
        return connectionString;
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
       => optionsBuilder.UseSqlServer(GetConnectionString("DefaultConnection"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Conversation>(entity =>
        {
            entity.HasKey(e => e.ConversationId).HasName("PK__Conversa__2860E54E1C7786AA");

            entity.ToTable("Conversation");

            entity.Property(e => e.ConversationId).HasColumnName("conversationId");
            entity.Property(e => e.Account1Id).HasColumnName("account1Id");
            entity.Property(e => e.Account2Id).HasColumnName("account2Id");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
        });

        modelBuilder.Entity<FileStorage>(entity =>
        {
            entity.HasKey(e => e.FileId).HasName("PK__FileStor__C2C6FFDC2EA93EE8");

            entity.ToTable("FileStorage");

            entity.Property(e => e.FileId).HasColumnName("fileId");
            entity.Property(e => e.DownloadUrl).HasColumnName("downloadUrl");
            entity.Property(e => e.FileName)
                .HasMaxLength(255)
                .HasColumnName("fileName");
            entity.Property(e => e.UploadTime)
                .HasColumnType("datetime")
                .HasColumnName("uploadTime");
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.MessageId).HasName("PK__Message__4808B99339A1E043");

            entity.ToTable("Message");

            entity.Property(e => e.MessageId).HasColumnName("messageId");
            entity.Property(e => e.ConversationId).HasColumnName("conversationId");
            entity.Property(e => e.FileId).HasColumnName("fileId");
            entity.Property(e => e.IsDeleted).HasColumnName("isDeleted");
            entity.Property(e => e.IsEdited).HasColumnName("isEdited");
            entity.Property(e => e.IsRead).HasColumnName("isRead");
            entity.Property(e => e.MessageText).HasColumnName("messageText");
            entity.Property(e => e.MessageTypeId).HasColumnName("messageTypeId");
            entity.Property(e => e.SendAt)
                .HasColumnType("datetime")
                .HasColumnName("sendAt");
            entity.Property(e => e.SenderId).HasColumnName("senderId");

            entity.HasOne(d => d.Conversation).WithMany(p => p.Messages)
                .HasForeignKey(d => d.ConversationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FKMessage862833");

            entity.HasOne(d => d.File).WithMany(p => p.Messages)
                .HasForeignKey(d => d.FileId)
                .HasConstraintName("FKMessage917868");

            entity.HasOne(d => d.MessageType).WithMany(p => p.Messages)
                .HasForeignKey(d => d.MessageTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FKMessage568142");
        });

        modelBuilder.Entity<MessageType>(entity =>
        {
            entity.HasKey(e => e.MessageTypeId).HasName("PK__MessageT__A95058F5D3BABB03");

            entity.ToTable("MessageType");

            entity.Property(e => e.MessageTypeId).HasColumnName("messageTypeId");
            entity.Property(e => e.TypeName)
                .HasMaxLength(255)
                .HasColumnName("typeName");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
