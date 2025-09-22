using Art.OnShift.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Security.Claims;

namespace Art.OnShift.Scheduler.Data
{
    public class AppDbContext : DbContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AppDbContext(DbContextOptions<AppDbContext> options,
            IHttpContextAccessor httpContextAccessor)
            : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public DbSet<UserModel> Users { get; set; }
        public DbSet<EventModel?> Events { get; set; }
        public DbSet<EventAuditModel> EventsAudits { get; set; }
        public DbSet<EventAcknowledgeModel> EventAcknowledges { get; set; }

        private string? GetCurrentUserId()
        {
            return _httpContextAccessor.HttpContext?.User?.Identity.Name ??
                   _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserModel>().ToTable("Users");
            modelBuilder.Entity<EventModel>().ToTable("Events");

            // Configure relationships if necessary
            modelBuilder.Entity<EventModel>()
                .HasOne(e => e.Level1)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EventModel>()
                .HasOne(e => e.Level2)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EventModel>()
                .HasOne(e => e.Level3)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);
        }

        public override int SaveChanges()
        {
            // Captura informações de auditoria ANTES de salvar
            var auditEntries = PrepareAuditEntries();

            // Salva as mudanças principais (isso gera os IDs)
            var result = base.SaveChanges();

            // Finaliza e salva as auditorias com os IDs corretos
            SaveAuditEntries(auditEntries);

            return result;
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Captura informações de auditoria ANTES de salvar
            var auditEntries = PrepareAuditEntries();

            // Salva as mudanças principais (isso gera os IDs)
            var result = await base.SaveChangesAsync(cancellationToken);

            // Finaliza e salva as auditorias com os IDs corretos
            await SaveAuditEntriesAsync(auditEntries, cancellationToken);

            return result;
        }

        private List<AuditEntry> PrepareAuditEntries()
        {
            var userId = GetCurrentUserId();
            var auditEntries = new List<AuditEntry>();

            var entries = ChangeTracker.Entries<EventModel>()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted)
                .ToList();

            foreach (var entry in entries)
            {
                var auditEntry = new AuditEntry
                {
                    EntityEntry = entry,
                    UserId = userId,
                    Operation = entry.State switch
                    {
                        EntityState.Added => "CREATE",
                        EntityState.Modified => "UPDATE",
                        EntityState.Deleted => "DELETE",
                        _ => "UNKNOWN"
                    },
                    EventDate = DateTime.UtcNow
                };

                // Captura valores baseado no tipo de operação
                switch (entry.State)
                {
                    case EntityState.Added: // CREATE - apenas valores atuais
                        auditEntry.Title = entry.Entity.Title;
                        auditEntry.Start = entry.Entity.Start;
                        auditEntry.End = entry.Entity.End;
                        auditEntry.Level1Id = entry.Entity.Level1Id;
                        auditEntry.Level2Id = entry.Entity.Level2Id;
                        auditEntry.Level3Id = entry.Entity.Level3Id;
                        break;

                    case EntityState.Modified: // UPDATE - valores anteriores e atuais
                                               // Valores anteriores
                        auditEntry.PreviousTitle = entry.OriginalValues.GetValue<string?>("Title");
                        auditEntry.PreviousStart = entry.OriginalValues.GetValue<DateTime?>("Start");
                        auditEntry.PreviousEnd = entry.OriginalValues.GetValue<DateTime?>("End");
                        auditEntry.PreviousLevel1Id = entry.OriginalValues.GetValue<string>("Level1Id");
                        auditEntry.PreviousLevel2Id = entry.OriginalValues.GetValue<string>("Level2Id");
                        auditEntry.PreviousLevel3Id = entry.OriginalValues.GetValue<string>("Level3Id");

                        // Valores atuais
                        auditEntry.Title = entry.Entity.Title;
                        auditEntry.Start = entry.Entity.Start;
                        auditEntry.End = entry.Entity.End;
                        auditEntry.Level1Id = entry.Entity.Level1Id;
                        auditEntry.Level2Id = entry.Entity.Level2Id;
                        auditEntry.Level3Id = entry.Entity.Level3Id;
                        break;

                    case EntityState.Deleted: // DELETE - apenas valores originais
                        auditEntry.Title = entry.OriginalValues.GetValue<string?>("Title");
                        auditEntry.Start = entry.OriginalValues.GetValue<DateTime?>("Start");
                        auditEntry.End = entry.OriginalValues.GetValue<DateTime?>("End");
                        auditEntry.Level1Id = entry.OriginalValues.GetValue<string>("Level1Id");
                        auditEntry.Level2Id = entry.OriginalValues.GetValue<string>("Level2Id");
                        auditEntry.Level3Id = entry.OriginalValues.GetValue<string>("Level3Id");
                        break;
                }

                auditEntries.Add(auditEntry);
            }

            return auditEntries;
        }

        private void SaveAuditEntries(List<AuditEntry> auditEntries)
        {
            foreach (var auditEntry in auditEntries)
            {
                var audit = new EventAuditModel
                {
                    EventId = auditEntry.EntityEntry.Entity.Id, // Agora o ID já foi gerado
                    Operation = auditEntry.Operation,
                    EventDate = auditEntry.EventDate,
                    UserId = auditEntry.UserId,
                    Title = auditEntry.Title,
                    Start = auditEntry.Start,
                    End = auditEntry.End,
                    Level1Id = auditEntry.Level1Id,
                    Level2Id = auditEntry.Level2Id,
                    Level3Id = auditEntry.Level3Id,
                    PreviousTitle = auditEntry.PreviousTitle,
                    PreviousStart = auditEntry.PreviousStart,
                    PreviousEnd = auditEntry.PreviousEnd,
                    PreviousLevel1Id = auditEntry.PreviousLevel1Id,
                    PreviousLevel2Id = auditEntry.PreviousLevel2Id,
                    PreviousLevel3Id = auditEntry.PreviousLevel3Id
                };

                EventsAudits.Add(audit);
            }

            // Salva apenas as auditorias
            base.SaveChanges();
        }

        private async Task SaveAuditEntriesAsync(List<AuditEntry> auditEntries, CancellationToken cancellationToken)
        {
            foreach (var auditEntry in auditEntries)
            {
                var audit = new EventAuditModel
                {
                    EventId = auditEntry.EntityEntry.Entity.Id, // Agora o ID já foi gerado
                    Operation = auditEntry.Operation,
                    EventDate = auditEntry.EventDate,
                    UserId = auditEntry.UserId,
                    Title = auditEntry.Title,
                    Start = auditEntry.Start,
                    End = auditEntry.End,
                    Level1Id = auditEntry.Level1Id,
                    Level2Id = auditEntry.Level2Id,
                    Level3Id = auditEntry.Level3Id,
                    PreviousTitle = auditEntry.PreviousTitle,
                    PreviousStart = auditEntry.PreviousStart,
                    PreviousEnd = auditEntry.PreviousEnd,
                    PreviousLevel1Id = auditEntry.PreviousLevel1Id,
                    PreviousLevel2Id = auditEntry.PreviousLevel2Id,
                    PreviousLevel3Id = auditEntry.PreviousLevel3Id
                };

                EventsAudits.Add(audit);
            }

            // Salva apenas as auditorias
            await base.SaveChangesAsync(cancellationToken);
        }
        public DbSet<Art.OnShift.Shared.Models.EventModel> EventModel { get; set; } = default!;
    }

    // Classe auxiliar para armazenar informações de auditoria temporariamente
    public class AuditEntry
    {
        public EntityEntry<EventModel> EntityEntry { get; set; }
        public string? UserId { get; set; }
        public string Operation { get; set; }
        public DateTime EventDate { get; set; }

        // Valores atuais
        public string? Title { get; set; }
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
        public string? Level1Id { get; set; }
        public string? Level2Id { get; set; }
        public string? Level3Id { get; set; }

        // Valores anteriores (para UPDATE)
        public string? PreviousTitle { get; set; }
        public DateTime? PreviousStart { get; set; }
        public DateTime? PreviousEnd { get; set; }
        public string? PreviousLevel1Id { get; set; }
        public string? PreviousLevel2Id { get; set; }
        public string? PreviousLevel3Id { get; set; }
    }
}
