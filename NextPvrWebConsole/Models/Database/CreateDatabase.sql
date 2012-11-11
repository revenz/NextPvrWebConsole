CREATE TABLE [version] (
	databaseversion			varchar(10)			NOT NULL	
)
GO

INSERT INTO [version](databaseversion) VALUES (1000)
GO

CREATE TABLE [user] (
	oid						integer				NOT NULL			PRIMARY KEY				AUTOINCREMENT,
	username				varchar(100)		NOT NULL,
	emailaddress			varchar(320)		NOT NULL,
	[passwordhash]			varchar(100)		NOT NULL,
	userrole				integer				NOT NULL,
	datecreatedutc			datetime			NOT NULL,
	lastloggedinutc			datetime			NOT NULL,
	[readonly]				bit					NOT NULL
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

CREATE TABLE [channelgroup] /** oh i can create my own channel groups... this would allow each user to create their own group, eg "Favourite" for everyone could be unique ... **/
(
	oid						integer				NOT NULL			PRIMARY KEY				AUTOINCREMENT,
	useroid					integer				NOT NULL			REFERENCES [user](oid),
	name					varchar(50)			NOT NULL,
	orderoid				integer				NOT NULL
)
GO

CREATE TABLE [channelgroupchannel]
(
	channelgroupoid			integer				NOT NULL			REFERENCES [channelgroup](oid),
	channeloid				integer				NOT NULL			REFERENCES [channel](oid)			
)
GO

INSERT INTO [user](oid, username, emailaddress, passwordhash, userrole, datecreatedutc, lastloggedinutc, [readonly]) VALUES (0, 'Shared', '', '', 0, DATETIME('now'), '1970-01-01', 1)  /* special user */
GO
