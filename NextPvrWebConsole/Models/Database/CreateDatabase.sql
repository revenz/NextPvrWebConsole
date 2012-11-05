CREATE TABLE [user] (
	id					int					NOT NULL			PRIMARY KEY,
	emailaddress		varchar(320)		NOT NULL,
	[passwordhash]		varchar(100)		NOT NULL,
	userrole			int					NOT NULL,
	datecreatedutc		datetime			NOT NULL,
	lastloggedinutc		datetime			NOT NULL,
	[readonly]			bit					NOT NULL
)
GO