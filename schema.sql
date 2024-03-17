-- We'll need a table for players, with player name as an indexable key
DROP TABLE IF EXISTS players CASCADE;
CREATE TABLE players
(
    id                   SERIAL PRIMARY KEY,
    character_name       VARCHAR(255) NOT NULL,
    hit_points           INTEGER      NOT NULL,
    health_cap           INTEGER      NOT NULL,
    temporary_hit_points INTEGER      NOT NULL default 0,
    level                INTEGER      NOT NULL,
    strength             INTEGER      NOT NULL,
    dexterity            INTEGER      NOT NULL,
    constitution         INTEGER      NOT NULL,
    intelligence         INTEGER      NOT NULL,
    wisdom               INTEGER      NOT NULL,
    charisma             INTEGER      NOT NULL,
    created_at           TIMESTAMP    NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at           TIMESTAMP    NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT idx_player_name UNIQUE (character_name)
);

-- Classes are rather static, so we can add them at will
DROP TABLE IF EXISTS classes CASCADE;
CREATE TABLE classes
(
    id         SERIAL PRIMARY KEY,
    class_name VARCHAR(255) NOT NULL,
    created_at TIMESTAMP    NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT idx_class_name UNIQUE (class_name)
);

DROP TABLE IF EXISTS player_class CASCADE;
CREATE TABLE player_class
(
    id             SERIAL PRIMARY KEY,
    player_id      INTEGER   NOT NULL,
    class_id       INTEGER   NOT NULL,
    hit_dice_value INTEGER   NOT NULL,
    class_level    INTEGER   NOT NULL,
    created_at     TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at     TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    -- We'll index on the player-class combo as that *should* be a unique combo
    CONSTRAINT idx_player_class UNIQUE (player_id, class_id),
    CONSTRAINT fk_player_id FOREIGN KEY (player_id) REFERENCES players (id),
    CONSTRAINT fk_class_id FOREIGN KEY (class_id) REFERENCES classes (id)
);

DROP TABLE IF EXISTS items;
CREATE TABLE items
(
    id              SERIAL PRIMARY KEY,
    item_name       VARCHAR(255) NOT NULL,
    affected_object VARCHAR(255) NOT NULL,
    affected_value  VARCHAR(255) NOT NULL,
    value           INTEGER      NOT NULL,
    created_at      TIMESTAMP    NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at      TIMESTAMP    NOT NULL DEFAULT CURRENT_TIMESTAMP,
    -- Items will be unique, so we can add an index based on the item name
    CONSTRAINT idx_item_name UNIQUE (item_name)
);

DROP TABLE IF EXISTS player_items CASCADE;
CREATE TABLE player_items
(
    id         SERIAL PRIMARY KEY,
    player_id  INTEGER   NOT NULL,
    item_id    INTEGER   NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT idx_player_item UNIQUE (player_id, item_id),
    CONSTRAINT fk_player_id FOREIGN KEY (player_id) REFERENCES players (id),
    CONSTRAINT fk_item_id FOREIGN KEY (item_id) REFERENCES classes (id)
);

-- We're in the grey area now, as an argument can be made for spreading
-- defenses across tables with the backing argument for normalization 
-- being defenses as a normal table with player defenses being a pivot.
-- 
-- For now, we'll keep things simple and flatten the relationship taking
-- the YAGNI approach until we truly need some form of normalization.
DROP TABLE IF EXISTS player_defenses CASCADE;
CREATE TABLE player_defenses
(
    id         SERIAL PRIMARY KEY,
    player_id  INTEGER      NOT NULL,
    type       VARCHAR(255) NOT NULL,
    defense    VARCHAR(255) NOT NULL,
    created_at TIMESTAMP    NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_player_id_defenses FOREIGN KEY (player_id) REFERENCES players (id)
);

-- While we're at it, seed the class reference data
INSERT INTO classes (class_name)
VALUES ('barbarian'),
       ('bard'),
       ('cleric'),
       ('druid'),
       ('fighter'),
       ('monk'),
       ('paladin'),
       ('ranger'),
       ('rogue'),
       ('sorcerer'),
       ('warlock'),
       ('wizard');

