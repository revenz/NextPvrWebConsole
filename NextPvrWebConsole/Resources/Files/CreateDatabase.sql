CREATE TABLE [version] (
	databaseversion			int					NOT NULL	
)
GO

CREATE TABLE [user] (
	oid						integer				NOT NULL			PRIMARY KEY				AUTOINCREMENT,
	username				varchar(100)		NOT NULL,
	emailaddress			varchar(320)		NOT NULL,
	[passwordhash]			varchar(100)		NOT NULL,
	userrole				integer				NOT NULL,
	datecreatedutc			datetime			NOT NULL,
	lastloggedinutc			datetime			NOT NULL,
	administrator			bit					NOT NULL,
	[readonly]				bit					NOT NULL,	
	defaultrecordingdirectoryoid	integer		NOT NULL
)
GO

CREATE TABLE [setting] (
	name					varchar(100)		NULL NULL			PRIMARY KEY,
	intvalue				integer				NULL,
	stringvalue				text				NULL,
	doublevalue				double				NULL,
	boolvalue				bit					NULL,
	datetimevalue			datetime			NULL
)
GO

CREATE TABLE [recordingdirectory]
(
	oid						integer				NOT NULL			PRIMARY KEY				AUTOINCREMENT,
	useroid					integer				NOT NULL			REFERENCES [user](oid),
	name					varchar(50)			NOT NULL,
	[path]					text				NOT NULL,
	isdefault				bit					NOT NULL
)
GO

CREATE TABLE [channel]
(
	oid						integer				NOT NULL			PRIMARY KEY,
	name					text				NOT NULL,
	number					integer				NOT NULL,
	[enabled]				bit					NOT NULL
)
GO

CREATE TABLE [userchannel]
(
	useroid					integer				NOT NULL			REFERENCES [user](oid),
	channeloid				integer				NOT NULL			REFERENCES [channel](oid),
	number					integer				NOT NULL,
	[enabled]				bit					NOT NULL		
)
GO

CREATE TABLE [channelgroup]
(
	oid						integer				NOT NULL			PRIMARY KEY				AUTOINCREMENT,
	useroid					integer				NOT NULL			REFERENCES [user](oid),
	name					varchar(50)			NOT NULL,
	orderoid				integer				NOT NULL,
	parentoid				integer				NOT NULL			REFERENCES [channelgroup](oid), /* used to reference a shared user group maintained globally */
	[enabled]				bit					NOT NULL /* only used if parenoid is > 0, where a shared channel group might have been disabled */
)
GO

CREATE TABLE [channelgroupchannel]
(
	channelgroupoid			integer				NOT NULL			REFERENCES [channelgroup](oid),
	channeloid				integer				NOT NULL			REFERENCES [channel](oid)			
)
GO

CREATE TABLE [xmltvsource]
(
	oid						integer				NOT NULL			PRIMARY KEY				AUTOINCREMENT,
	[filename]				text				NOT NULL,
	lastscantime			datetime			NOT NULL,
	channeloids				text				NOT NULL
)
GO

INSERT INTO [user](oid, username, emailaddress, passwordhash, userrole, datecreatedutc, lastloggedinutc, [readonly], administrator, defaultrecordingdirectoryoid) VALUES (1, 'Shared', '', '', 0, DATETIME('now'), '1970-01-01', 1, 0, 0)  /* special user */
GO
