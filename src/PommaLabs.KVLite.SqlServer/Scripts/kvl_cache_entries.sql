﻿DROP TABLE [kvl_cache_entries];


CREATE TABLE [kvl_cache_entries] (
    [kvle_id]          UNIQUEIDENTIFIER ROWGUIDCOL NOT NULL DEFAULT NEWSEQUENTIALID(),
    [kvle_partition]   NVARCHAR (195) NOT NULL,
    [kvle_key]         NVARCHAR (255) NOT NULL,
    [kvle_expiry]      BIGINT NOT NULL,
    [kvle_interval]    BIGINT NOT NULL,
    [kvle_value]       VARBINARY (MAX) NOT NULL,
    [kvle_compressed]  BIT NOT NULL,
    [kvle_creation]    BIGINT NOT NULL,
    [kvle_parent_key0] NVARCHAR (255) NULL,
    [kvle_parent_key1] NVARCHAR (255) NULL,
    [kvle_parent_key2] NVARCHAR (255) NULL,
    [kvle_parent_key3] NVARCHAR (255) NULL,
    [kvle_parent_key4] NVARCHAR (255) NULL,
    CONSTRAINT [pk_kvle] PRIMARY KEY CLUSTERED ([kvle_id]), 
    CONSTRAINT [uk_kvle] UNIQUE ([kvle_partition] ASC, [kvle_key] ASC)
);


CREATE INDEX [ix_kvle_exp_part] ON [kvl_cache_entries] ([kvle_expiry] DESC, [kvle_partition] ASC);


EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = 'Automatically generated ID.', @level0type = N'SCHEMA', @level0name = N'kvlite', @level1type = N'TABLE', @level1name = N'kvl_cache_entries', @level2type = N'COLUMN', @level2name = N'kvle_id';
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = 'A partition holds a group of related keys.', @level0type = N'SCHEMA', @level0name = N'kvlite', @level1type = N'TABLE', @level1name = N'kvl_cache_entries', @level2type = N'COLUMN', @level2name = N'kvle_partition';
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = 'A key uniquely identifies an entry inside a partition.', @level0type = N'SCHEMA', @level0name = N'kvlite', @level1type = N'TABLE', @level1name = N'kvl_cache_entries', @level2type = N'COLUMN', @level2name = N'kvle_key';
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = 'When the entry will expire, expressed as seconds after UNIX epoch.', @level0type = N'SCHEMA', @level0name = N'kvlite', @level1type = N'TABLE', @level1name = N'kvl_cache_entries', @level2type = N'COLUMN', @level2name = N'kvle_expiry';
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = 'How many seconds should be used to extend expiry time when the entry is retrieved.', @level0type = N'SCHEMA', @level0name = N'kvlite', @level1type = N'TABLE', @level1name = N'kvl_cache_entries', @level2type = N'COLUMN', @level2name = N'kvle_interval';
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = 'Serialized and optionally compressed content of this entry.', @level0type = N'SCHEMA', @level0name = N'kvlite', @level1type = N'TABLE', @level1name = N'kvl_cache_entries', @level2type = N'COLUMN', @level2name = N'kvle_value';
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = 'Whether the entry content was compressed or not.', @level0type = N'SCHEMA', @level0name = N'kvlite', @level1type = N'TABLE', @level1name = N'kvl_cache_entries', @level2type = N'COLUMN', @level2name = N'kvle_compressed';
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = 'When the entry was created, expressed as seconds after UNIX epoch.', @level0type = N'SCHEMA', @level0name = N'kvlite', @level1type = N'TABLE', @level1name = N'kvl_cache_entries', @level2type = N'COLUMN', @level2name = N'kvle_creation';
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = 'Optional parent entry, used to link entries in a hierarchical way.', @level0type = N'SCHEMA', @level0name = N'kvlite', @level1type = N'TABLE', @level1name = N'kvl_cache_entries', @level2type = N'COLUMN', @level2name = N'kvle_parent_key0';
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = 'Optional parent entry, used to link entries in a hierarchical way.', @level0type = N'SCHEMA', @level0name = N'kvlite', @level1type = N'TABLE', @level1name = N'kvl_cache_entries', @level2type = N'COLUMN', @level2name = N'kvle_parent_key1';
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = 'Optional parent entry, used to link entries in a hierarchical way.', @level0type = N'SCHEMA', @level0name = N'kvlite', @level1type = N'TABLE', @level1name = N'kvl_cache_entries', @level2type = N'COLUMN', @level2name = N'kvle_parent_key2';
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = 'Optional parent entry, used to link entries in a hierarchical way.', @level0type = N'SCHEMA', @level0name = N'kvlite', @level1type = N'TABLE', @level1name = N'kvl_cache_entries', @level2type = N'COLUMN', @level2name = N'kvle_parent_key3';
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = 'Optional parent entry, used to link entries in a hierarchical way.', @level0type = N'SCHEMA', @level0name = N'kvlite', @level1type = N'TABLE', @level1name = N'kvl_cache_entries', @level2type = N'COLUMN', @level2name = N'kvle_parent_key4';
