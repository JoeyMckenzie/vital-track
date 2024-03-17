# Vital Track

[![CI](https://github.com/JoeyMckenzie/vital-track/actions/workflows/ci.yml/badge.svg)](https://github.com/JoeyMckenzie/vital-track/actions/workflows/ci.yml)

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

There are four endpoints available, with an example of each below (assuming you have `jq` installed on your machine):

```bash
# The /api/player/:name/info endpoint returns the current player state
curl --location 'http://localhost:5029/api/player/briv/info' | jq '.'

{
  "data": {
    "name": "Briv",
    "level": 5,
    "hitPoints": 25,
    "temporaryHitPoints": 0,
    "classes": [
      // player classes...
    ],
    "stats": {
      // player stats...
    },
    "items": [
      // player items...
    ],
    "defenses": [
      // player defenses...
    ]
  }
}

# /api/player/:name/damage endpoint accepts a damage type and value, returning the updated player state
curl --location 'http://localhost:5029/api/player/briv/damage' \
     --header 'Content-Type: application/json' \
     --data '{
        "damageType": "slashing",
        "amount": 12
     }' | jq '.'
     
{
  "data": {
    "name": "Briv",
    "level": 5,
    "hitPoints": 19, // Briv has resistance to slashing, so only half damage is taken
    "temporaryHitPoints": 0,
    "classes": [
      // player classes...
    ],
    "stats": {
      // player stats...
    },
    "items": [
      // player items...
    ],
    "defenses": [
      // player defenses...
    ]
  }
}

# /api/player/:name/temp endpoint accepts health modifier value, returning the updated player state with the temporary health
curl --location 'http://localhost:5029/api/player/briv/temp' \
     --header 'Content-Type: application/json' \
     --data '{
        "amount": 10
     }' | jq '.'
     
{
  "data": {
    "name": "Briv",
    "level": 5,
    "hitPoints": 19,
    "temporaryHitPoints": 10,
    "classes": [
      // player classes...
    ],
    "stats": {
      // player stats...
    },
    "items": [
      // player items...
    ],
    "defenses": [
      // player defenses...
    ]
  }
}

# /api/player/:name/heal endpoint accepts health modifier value, returning the updated player state with increased health
curl --location 'http://localhost:5029/api/player/briv/heal' \
     --header 'Content-Type: application/json' \
     --data '{
        "amount": 10
     }' | jq '.'
     
{
  "data": {
    "name": "Briv",
    "level": 5,
    "hitPoints": 25, // no overhealing allowed, so only heal back to the original health pool cap
    "temporaryHitPoints": 10,
    "classes": [
      // player classes...
    ],
    "stats": {
      // player stats...
    },
    "items": [
      // player items...
    ],
    "defenses": [
      // player defenses...
    ]
  }
}
```

For convenience, a Postman collection has been included containing requests to all local endpoints.

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

### Persistence with Postgres

Using the provided player template, my storage layer of choice is Postgres. Though an argument could be made for using
in-memory storage (as was the original implementation), I wanted to showcase Dapper a bit and spread the good word that
EF is not the end-all-be-all for data access within the .NET ecosystem.

On [application start](https://github.com/joeymckenzie/vital-track), we'll bootstrap the services within the service
provider, and seed the player template
within the database:

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

I'm utilizing [Npgsql](https://www.npgsql.org/) as my Postgres SQL driver of choice as it's widely supported and the
only driver I trust in terms of Postgres and .NET.

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

## TODO

Should probably Docker-ize the local dev environment at some point, but I'll save that as a fast follow item.