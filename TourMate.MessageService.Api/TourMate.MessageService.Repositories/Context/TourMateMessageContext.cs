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
            entity
                .HasNoKey()
                .ToTable("Conversation");

            entity.Property(e => e.Account1Id).HasColumnName("account1Id");
            entity.Property(e => e.Account2Id).HasColumnName("account2Id");
            entity.Property(e => e.ConversationId)
                .ValueGeneratedOnAdd()
                .HasColumnName("conversationId");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
        });

        modelBuilder.Entity<FileStorage>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("FileStorage");

            entity.Property(e => e.DownloadUrl).HasColumnName("downloadUrl");
            entity.Property(e => e.FileName).HasColumnName("fileName");
            entity.Property(e => e.Id)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("id");
            entity.Property(e => e.UploadTime)
                .HasColumnType("datetime")
                .HasColumnName("uploadTime");
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("Message");

            entity.Property(e => e.ConversationId).HasColumnName("conversationId");
            entity.Property(e => e.FileId)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("fileId");
            entity.Property(e => e.IsDeleted).HasColumnName("isDeleted");
            entity.Property(e => e.IsEdited).HasColumnName("isEdited");
            entity.Property(e => e.IsRead).HasColumnName("isRead");
            entity.Property(e => e.MessageId)
                .ValueGeneratedOnAdd()
                .HasColumnName("messageId");
            entity.Property(e => e.MessageText).HasColumnName("messageText");
            entity.Property(e => e.MessageTypeId).HasColumnName("messageTypeId");
            entity.Property(e => e.SendAt)
                .HasColumnType("datetime")
                .HasColumnName("sendAt");
            entity.Property(e => e.SenderId).HasColumnName("senderId");
        });

        modelBuilder.Entity<MessageType>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("MessageType");

            entity.Property(e => e.MessageTypeId)
                .ValueGeneratedOnAdd()
                .HasColumnName("messageTypeId");
            entity.Property(e => e.TypeName)
                .HasMaxLength(50)
                .HasColumnName("typeName");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
