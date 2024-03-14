# Vital Track

Your friendly neighborhood hit point tracker for DnD! (Yes, the name is ChatGPT generated...)

Vital Track is an example
implementation of the DnD Beyond backend challenge written in C# and .NET. To get started,
clone the project, then from within the root of the project:

```bash
# Install dependencies
dotnet restore

# Build the solution
dotnet build --no-restore

# Run the local dev server
dotnet run --project src/VitalTrack.Web/VitalTrack.Web.csproj

# Run tests
dotnet test
```

I'm a fan of [`justfile`s](https://just.systems) as I'm not smart enough to understand `make`, so if you happen
to have `just` installed (if you have Rust installed, a simple `cargo install --locked just` will do the trick),
the local dev server is set as the default `just` target.

### Architecture

The solution contains three projects:

- `VitalTrack.Web` - the API entry point for the service
- `VitalTrack.Core` - core business domain logic revolving around players
- `VitalTrack.Infrastructure` - adapters to the core business logic ports (think service implementations, persistence,
  utilities, etc.)

To stay true to the spirit of the take home and keeping myself honest to get this done
within a few hours (about 4 hours of implementation, an hour for documentation), I'll outline a few of the design
decisions I've deliberately taken.
I lean heavily into the YAGNI principle as I grow old, embrace my elder developer curmudgeon-ness and
have cut a few corners in the sake of time, as my near one year old has drastically taken away
much of my free time these days:

### No external persistence

Using the provided player template, I've skipped storage persistence for the time being
as the use case is simple enough to warrant in-memory persistence. On service startup, the
`briv.json` player template file is loaded into memory, with a "repository" (air quotes intended)
around the in-memory storage layer (read: nothing but a simple `List<Player>` cached on repository
instantiation).

On [application start](https://github.com/joeymckenzie/vital-track), we'll bootstrap the services
within the service provider, and seed the player template within the "database":

```csharp
logger.LogInformation("API routes initialized, seeding players from template");

// We'll pull the player repository out of the service container and seed the in-memory collection
var playerRepository = providerScope.ServiceProvider.GetRequiredService<IPlayerRepository>();
var currentDirectory = Directory.GetCurrentDirectory();
var playerTemplatePath =
    $"{currentDirectory}{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}templates{Path.DirectorySeparatorChar}briv.json";

logger.LogInformation("Seeding player from template path {playerTemplatePath}", playerTemplatePath);

await playerRepository.SeedPlayerFromTemplateAsync(playerTemplatePath);
```

API Operations on the player are all done within memory and will *not persist* between service restarts.
If I had more time, here's what I **would** have done:

- Spun up a Postgres SQL database using [neon.tech](https://neon.tech)
- Defined a `dev` branch for local development, with the `main` branch for the production deployment unit
- Seeded the database on startup from the `briv.json` file
- Brought in [Npgsql](https://www.npgsql.org/) as my Postgres SQL driver of choice
- Slapped Dapper on top of it to facilitate data access (EF is fine, but for this simple use case, Dapper is more than
  enough)

### Thin controllers, fat models

The API layer is intentionally thing by design. The core logic for determining player health
is implemented as a state machine of sorts to encapsulate the domain logic within the player model.
The anatomy of an endpoint will be:

- Receive request
- Dispatch to service/core layer
- Return a response

No validation is done within this layer, other than validating existing players on the route
before performing actions against their health pool. The core logic of handling the state of a player
is contained within [`Player.cs`](https://github.com), where the encompassing player model is "fat" by design and
in charge of:

- Maintaining player state
    - In this case, we're only concerned about hit points
- Taking actions on the player state to produce new state
    - Healing, damage, and adding temporary hit points all produce a new internal state that the player model will track
- Some actions are idempotent:
    - Dealing damage to a player with zero HP produces the same state - a dead player
    - Healing a player at full health produces the same state - a player at full health

### Service providers

There are two core services, the hit point manager and the player repository.

- The hit point manager is responsible for orchestrating actions done on a player
- The player repository is in charge of CRUD'ifying actions on player entities - no business logic whatsoever

### Testing

I'm a big fan of [xUnit](https://xunit.net/) and [FluentAssertions](https://fluentassertions.com/), and you'll find
the tests to follow a standard arrange/act/assert pattern. For the sake of time, only the internal core business logic
is tested, though when my 11 month old falls asleep, I may actually get the time to write some API integration tests.