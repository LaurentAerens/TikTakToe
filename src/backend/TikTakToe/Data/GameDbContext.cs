namespace TikTakToe.Data;

using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TikTakToe.Models;

/// <summary>
/// Entity Framework database context for game persistence.
/// </summary>
public sealed class GameDbContext(DbContextOptions<GameDbContext> options) : DbContext(options)
{
    private static readonly ValueConverter<int[,]?, string?> BoardConverter = new(
        board => SerializeBoard(board),
        payload => DeserializeBoard(payload));

    private static readonly ValueComparer<int[,]?> BoardComparer = new(
        (left, right) => BoardsEqual(left, right),
        board => GetBoardHashCode(board),
        board => CloneBoard(board));

    /// <summary>
    /// Gets games.
    /// </summary>
    public DbSet<GameModel> Games => this.Set<GameModel>();

    /// <summary>
    /// Gets players.
    /// </summary>
    public DbSet<PlayerModel> Players => this.Set<PlayerModel>();

    /// <summary>
    /// Gets moves.
    /// </summary>
    public DbSet<MoveModel> Moves => this.Set<MoveModel>();

    /// <summary>
    /// Gets engine capabilities.
    /// </summary>
    public DbSet<EngineCapabilityModel> EngineCapabilities => this.Set<EngineCapabilityModel>();

    /// <inheritdoc />
    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        this.PrepareEngineCapabilities();
        this.ValidateEngineCapabilityUniqueness();
        this.ValidateEnginePlayerMappings();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    /// <inheritdoc />
    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        this.PrepareEngineCapabilities();
        await this.ValidateEngineCapabilityUniquenessAsync(cancellationToken);
        await this.ValidateEnginePlayerMappingsAsync(cancellationToken);
        return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var game = modelBuilder.Entity<GameModel>();
        game.ToTable("games");
        game.HasKey(x => x.Id);
        game.Property(x => x.Id).HasColumnName("id");
        game.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
        game.Property(x => x.UpdatedAtUtc).HasColumnName("updated_at_utc");
        var boardProperty = game.Property(x => x.Board)
            .HasConversion(BoardConverter)
            .Metadata;
        boardProperty.SetValueComparer(BoardComparer);

        game.Property(x => x.Board).HasColumnName("board");
        if (this.Database.IsNpgsql())
        {
            game.Property(x => x.Board)
                .HasColumnType("jsonb");
        }

        game.HasMany(x => x.Players)
            .WithOne(x => x.Game)
            .HasForeignKey(x => x.GameId)
            .OnDelete(DeleteBehavior.Cascade);

        game.HasMany(x => x.Moves)
            .WithOne(x => x.Game)
            .HasForeignKey(x => x.GameId)
            .OnDelete(DeleteBehavior.Cascade);

        var player = modelBuilder.Entity<PlayerModel>();
        player.ToTable("players");
        player.HasKey(x => x.Id);
        player.Property(x => x.Id).HasColumnName("id");
        player.Property(x => x.GameId).HasColumnName("game_id");
        player.Property(x => x.IsEngine).HasColumnName("is_engine");
        player.Property(x => x.ExternalId).HasColumnName("external_id").HasMaxLength(128);
        player.HasIndex(x => x.GameId);
        player.HasIndex(x => x.ExternalId)
            .HasDatabaseName("IX_players_engine_template_external_id")
            .IsUnique()
            .HasFilter("\"is_engine\" = TRUE AND \"game_id\" IS NULL AND \"external_id\" IS NOT NULL");

        var move = modelBuilder.Entity<MoveModel>();
        move.ToTable("moves");
        move.HasKey(x => x.Id);
        move.Property(x => x.Id).HasColumnName("id");
        move.Property(x => x.GameId).HasColumnName("game_id");
        move.Property(x => x.X).HasColumnName("x");
        move.Property(x => x.Y).HasColumnName("y");
        move.Property(x => x.Value).HasColumnName("value");
        move.Property(x => x.MoveNumber).HasColumnName("move_number");
        move.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
        move.HasIndex(x => x.GameId);
        move.HasIndex(x => new { x.GameId, x.MoveNumber }).IsUnique();

        var engineCapability = modelBuilder.Entity<EngineCapabilityModel>();
        engineCapability.ToTable("engine_capabilities");
        engineCapability.HasKey(x => x.Id);
        engineCapability.Property(x => x.Id).HasColumnName("id");
        engineCapability.Property(x => x.DisplayName).HasColumnName("display_name").HasMaxLength(128);
        engineCapability.Property(x => x.NormalizedDisplayName).HasColumnName("normalized_display_name").HasMaxLength(128);
        engineCapability.Property(x => x.MaxBoardSizeX).HasColumnName("max_board_size_x");
        engineCapability.Property(x => x.MaxBoardSizeY).HasColumnName("max_board_size_y");
        engineCapability.Property(x => x.Depth).HasColumnName("depth");
        engineCapability.HasIndex(x => x.DisplayName).IsUnique();
        engineCapability.HasIndex(x => x.NormalizedDisplayName).IsUnique();
    }

    private static string? SerializeBoard(int[,]? board)
    {
        if (board is null)
        {
            return null;
        }

        var rows = board.GetLength(0);
        var cols = board.GetLength(1);
        var serialized = new int[rows][];

        for (var row = 0; row < rows; row++)
        {
            serialized[row] = new int[cols];
            for (var col = 0; col < cols; col++)
            {
                serialized[row][col] = board[row, col];
            }
        }

        return JsonSerializer.Serialize(serialized);
    }

    private static int[,]? DeserializeBoard(string? payload)
    {
        if (string.IsNullOrWhiteSpace(payload))
        {
            return null;
        }

        var serialized = JsonSerializer.Deserialize<int[][]>(payload)
            ?? throw new InvalidOperationException("Board payload is missing.");

        if (serialized.Length == 0)
        {
            return new int[0, 0];
        }

        var cols = serialized[0].Length;
        for (var row = 1; row < serialized.Length; row++)
        {
            if (serialized[row].Length != cols)
            {
                throw new InvalidOperationException("Board payload must contain rectangular nested arrays.");
            }
        }

        var board = new int[serialized.Length, cols];
        for (var row = 0; row < serialized.Length; row++)
        {
            for (var col = 0; col < cols; col++)
            {
                board[row, col] = serialized[row][col];
            }
        }

        return board;
    }

    private static int[,]? CloneBoard(int[,]? board)
    {
        if (board is null)
        {
            return null;
        }

        return (int[,])board.Clone();
    }

    private static bool BoardsEqual(int[,]? left, int[,]? right)
    {
        if (ReferenceEquals(left, right))
        {
            return true;
        }

        if (left is null || right is null)
        {
            return false;
        }

        if (left.GetLength(0) != right.GetLength(0) || left.GetLength(1) != right.GetLength(1))
        {
            return false;
        }

        for (var row = 0; row < left.GetLength(0); row++)
        {
            for (var col = 0; col < left.GetLength(1); col++)
            {
                if (left[row, col] != right[row, col])
                {
                    return false;
                }
            }
        }

        return true;
    }

    private static int GetBoardHashCode(int[,]? board)
    {
        if (board is null)
        {
            return 0;
        }

        var hash = default(HashCode);
        hash.Add(board.GetLength(0));
        hash.Add(board.GetLength(1));

        for (var row = 0; row < board.GetLength(0); row++)
        {
            for (var col = 0; col < board.GetLength(1); col++)
            {
                hash.Add(board[row, col]);
            }
        }

        return hash.ToHashCode();
    }

    private static void ValidateDuplicateCandidates(EngineCapabilityModel[] candidates)
    {
        var duplicateInCandidates = candidates
            .GroupBy(x => x.NormalizedDisplayName ?? string.Empty, StringComparer.Ordinal)
            .FirstOrDefault(group => group.Select(x => x.Id).Distinct().Count() > 1);

        if (duplicateInCandidates is not null)
        {
            throw new InvalidOperationException($"An engine with display name '{duplicateInCandidates.Key}' already exists under normalization rules.");
        }
    }

    private static void ValidateAgainstExisting(
        IEnumerable<EngineCapabilityModel> candidates,
        IEnumerable<ExistingEngineCapability> existing)
    {
        var existingByName = existing
            .GroupBy(x => x.NormalizedDisplayName ?? string.Empty, StringComparer.Ordinal)
            .ToDictionary(x => x.Key, x => x.Select(y => y.Id).ToHashSet(), StringComparer.Ordinal);

        foreach (var candidate in candidates)
        {
            var candidateKey = candidate.NormalizedDisplayName ?? string.Empty;
            if (existingByName.TryGetValue(candidateKey, out var ids)
                && !ids.Contains(candidate.Id))
            {
                throw new InvalidOperationException($"An engine with display name '{candidate.DisplayName}' already exists under normalization rules.");
            }
        }
    }

    private static void ValidateEnginePlayerCandidateShape(PlayerModel[] engineCandidates)
    {
        foreach (var candidate in engineCandidates)
        {
            if (!Guid.TryParse(candidate.ExternalId, out var parsed))
            {
                throw new InvalidOperationException("Engine players must have a valid engine Guid in ExternalId.");
            }

            candidate.ExternalId = parsed.ToString("D");
        }

        var duplicateCandidate = engineCandidates
            .GroupBy(x => x.ExternalId!, StringComparer.Ordinal)
            .FirstOrDefault(group => group.Select(x => x.Id).Distinct().Count() > 1);

        if (duplicateCandidate is not null)
        {
            throw new InvalidOperationException($"An engine player with ExternalId '{duplicateCandidate.Key}' already exists.");
        }
    }

    private static void ValidateEnginePlayersAgainstExisting(
        IEnumerable<PlayerModel> candidates,
        IEnumerable<ExistingEnginePlayer> existing)
    {
        var existingByExternalId = existing
            .GroupBy(x => x.ExternalId, StringComparer.Ordinal)
            .ToDictionary(x => x.Key, x => x.Select(y => y.Id).ToHashSet(), StringComparer.Ordinal);

        foreach (var candidate in candidates)
        {
            var normalizedExternalId = NormalizeEngineExternalId(candidate.ExternalId);
            if (existingByExternalId.TryGetValue(normalizedExternalId, out var ids)
                && !ids.Contains(candidate.Id))
            {
                throw new InvalidOperationException($"An engine player with ExternalId '{normalizedExternalId}' already exists.");
            }
        }
    }

    private static string NormalizeEngineExternalId(string? externalId)
    {
        return Guid.TryParse(externalId, out var parsed)
            ? parsed.ToString("D")
            : throw new InvalidOperationException("Engine players must have a valid engine Guid in ExternalId.");
    }

    private void PrepareEngineCapabilities()
    {
        var entries = this.ChangeTracker.Entries<EngineCapabilityModel>()
            .Where(x => x.State is EntityState.Added or EntityState.Modified)
            .ToArray();

        foreach (var entry in entries)
        {
            entry.Entity.NormalizedDisplayName = EngineDisplayNameNormalizer.Normalize(entry.Entity.DisplayName);
        }
    }

    private void ValidateEngineCapabilityUniqueness()
    {
        var candidates = this.ChangeTracker.Entries<EngineCapabilityModel>()
            .Where(x => x.State is EntityState.Added or EntityState.Modified)
            .Select(x => x.Entity)
            .ToArray();

        ValidateDuplicateCandidates(candidates);
        if (candidates.Length == 0)
        {
            return;
        }

        var normalizedNames = candidates
            .Select(x => x.NormalizedDisplayName)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        var existing = this.EngineCapabilities
            .AsNoTracking()
            .Where(x => normalizedNames.Contains(x.NormalizedDisplayName))
            .Select(x => new ExistingEngineCapability(x.Id, x.NormalizedDisplayName))
            .ToList();

        ValidateAgainstExisting(candidates, existing);
    }

    private async Task ValidateEngineCapabilityUniquenessAsync(CancellationToken cancellationToken)
    {
        var candidates = this.ChangeTracker.Entries<EngineCapabilityModel>()
            .Where(x => x.State is EntityState.Added or EntityState.Modified)
            .Select(x => x.Entity)
            .ToArray();

        ValidateDuplicateCandidates(candidates);
        if (candidates.Length == 0)
        {
            return;
        }

        var normalizedNames = candidates
            .Select(x => x.NormalizedDisplayName)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        var existing = await this.EngineCapabilities
            .AsNoTracking()
            .Where(x => normalizedNames.Contains(x.NormalizedDisplayName))
            .Select(x => new ExistingEngineCapability(x.Id, x.NormalizedDisplayName))
            .ToListAsync(cancellationToken);

        ValidateAgainstExisting(candidates, existing);
    }

    private void ValidateEnginePlayerMappings()
    {
        var engineCandidates = this.ChangeTracker.Entries<PlayerModel>()
            .Where(x => x.State is EntityState.Added or EntityState.Modified)
            .Select(x => x.Entity)
            .Where(x => x.IsEngine && x.GameId is null)
            .ToArray();

        ValidateEnginePlayerCandidateShape(engineCandidates);
        if (engineCandidates.Length == 0)
        {
            return;
        }

        var engineExternalIds = engineCandidates
            .Select(x => NormalizeEngineExternalId(x.ExternalId))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        var existing = this.Players
            .AsNoTracking()
            .Where(x => x.IsEngine && x.GameId == null && x.ExternalId != null && engineExternalIds.Contains(x.ExternalId))
            .Select(x => new ExistingEnginePlayer(x.Id, x.ExternalId!))
            .ToList();

        ValidateEnginePlayersAgainstExisting(engineCandidates, existing);
    }

    private async Task ValidateEnginePlayerMappingsAsync(CancellationToken cancellationToken)
    {
        var engineCandidates = this.ChangeTracker.Entries<PlayerModel>()
            .Where(x => x.State is EntityState.Added or EntityState.Modified)
            .Select(x => x.Entity)
            .Where(x => x.IsEngine && x.GameId is null)
            .ToArray();

        ValidateEnginePlayerCandidateShape(engineCandidates);
        if (engineCandidates.Length == 0)
        {
            return;
        }

        var engineExternalIds = engineCandidates
            .Select(x => NormalizeEngineExternalId(x.ExternalId))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        var existing = await this.Players
            .AsNoTracking()
            .Where(x => x.IsEngine && x.GameId == null && x.ExternalId != null && engineExternalIds.Contains(x.ExternalId))
            .Select(x => new ExistingEnginePlayer(x.Id, x.ExternalId!))
            .ToListAsync(cancellationToken);

        ValidateEnginePlayersAgainstExisting(engineCandidates, existing);
    }

    private sealed record ExistingEngineCapability(Guid Id, string? NormalizedDisplayName);

    private sealed record ExistingEnginePlayer(Guid Id, string ExternalId);
}
