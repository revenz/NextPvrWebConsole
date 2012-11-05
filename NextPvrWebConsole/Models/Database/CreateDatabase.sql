CREATE TABLE [user] (
	id					int					NOT NULL			PRIMARY KEY,
	emailaddress		varchar(320)		NOT NULL,
	password			varchar(100)		NOT NULL,
	userrole			int					NOT NULL
)
GO