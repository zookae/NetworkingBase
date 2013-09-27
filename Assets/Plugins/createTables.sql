-- use like: ./sqlite3.exe defaultESP.sqlite3 < createTables.sql
--.mode column 
--.headers on
--.nullvalue NULL

PRAGMA foreign_keys = ON; -- can't assume this is enabled by default; APPLICATION MUST ALSO CALL THIS EVERY TIME IT STARTsUP 
--BEGIN; -- make all this a single transaction, so it's faster

CREATE TABLE IF NOT EXISTS player(
  playerid    INTEGER PRIMARY KEY, 
  udid		  TEXT NOT NULL CHECK( udid <> '' ),
  gamesPlayed INTEGER NOT NULL DEFAULT 0,  
  dAdded      TIMESTAMP NOT NULL DEFAULT current_timestamp,	
  UNIQUE( udid ) 
);
CREATE INDEX IF NOT EXISTS playerIndex ON player( udid ); -- for efficiency (ie "select * where udid=?" is way faster)

CREATE TABLE IF NOT EXISTS game(
	gameid		INTEGER PRIMARY KEY,
	name		TEXT,
	dAdded      TIMESTAMP NOT NULL DEFAULT current_timestamp
);

CREATE TABLE IF NOT EXISTS gamedetail(
	gdk			INTEGER PRIMARY KEY,
	gameid		INTEGER NOT NULL REFERENCES game(gameid),
	param		TEXT,
	val			TEXT,
	UNIQUE( gameid, param, val )
);

-- table storing generic transaction data
-- should be ETL'd to move to new tables for analysis/use
CREATE TABLE IF NOT EXISTS savedata(
	opk			INTEGER PRIMARY KEY,
	gameid		INTEGER NOT NULL REFERENCES game(gameid),
	playerid    INTEGER NOT NULL,
--	datatype	TEXT NOT NULL CHECK( thedata <> ''),
	thedata		TEXT NOT NULL CHECK( thedata <> ''),
	dAdded      TIMESTAMP NOT NULL DEFAULT current_timestamp
);
CREATE INDEX IF NOT EXISTS gameIndex ON savedata( gameid );
CREATE INDEX IF NOT EXISTS playerIndex ON savedata( playerid );

-- not yet used, but might be cool 
CREATE TABLE IF NOT EXISTS topscore(
  scoreid     INTEGER PRIMARY KEY,
  playerid	  INTEGER NOT NULL REFERENCES player(playerid),
  scoreValue   INTEGER NOT NULL DEFAULT -1,
  iOfPlayAgain INTEGER NOT NULL DEFAULT 0, -- when 0 it's not from a play again. So this represents the X'th play again of the player (e.g. if 1, then the score is from the 1st play again)
  dAdded      TIMESTAMP NOT NULL DEFAULT current_timestamp
  -- UNIQUE( playerid, dAdded )
);
CREATE INDEX IF NOT EXISTS topscoreIndex ON topscore( scoreValue ); -- for efficiency (ie "select * where scoreValue=?" is way faster)
CREATE INDEX IF NOT EXISTS topscoreIndexToo ON topscore( playerid ); 
 
DROP TABLE IF EXISTS confidenceLookupMC;
 CREATE TABLE IF NOT EXISTS confidenceLookupMC(
 mcid            INTEGER PRIMARY KEY,
 choices		 INTEGER NOT NULL,
 trials			 INTEGER NOT NULL,
 intConfidence   INTEGER NOT NULL,
 reqForAgreement INTEGER CHECK( reqForAgreement <> '' ), --can be null
 -- lastModified   TIMESTAMP NOT NULL DEFAULT current_timestamp
 UNIQUE( choices, trials, reqForAgreement ) 
 );
CREATE INDEX IF NOT EXISTS confidenceLookupMCIndex ON confidenceLookupMC(choices,trials,intConfidence); 
CREATE INDEX IF NOT EXISTS confidenceLookupMCIndex2 ON confidenceLookupMC(choices,trials,reqForAgreement); 

CREATE TEMP TABLE tmpMC( c, t, ic, rfa );
.separator ","
.import confidenceLookupMC.csv tmpMC
UPDATE tmpMC SET rfa = NULL WHERE rfa='';
insert into confidenceLookupMC( choices, trials, intConfidence, reqForAgreement ) select * from tmpMC;
DROP TABLE tmpMC;


-- CREATE TEMP TABLE tmpAns( dn NOT NULL, on1 NOT NULL, on2 NOT NULL, r1 NOT NULL, r2 );
-- .separator ","
-- .import confidenceLookupMC.csv tmpMC
-- UPDATE tmpAns SET r2 = NULL WHERE rfa='';
-- insert into 
-- DROP TABLE tmpAns;

-- COMMIT; --end the transaction
