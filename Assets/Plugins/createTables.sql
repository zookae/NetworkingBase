-- use like: ./sqlite3.exe defaultESP.sqlite3 < createTables.sql
--.mode column 
--.headers on
--.nullvalue NULL

PRAGMA foreign_keys = ON; -- can't assume this is enabled by default; APPLICATION MUST ALSO CALL THIS EVERY TIME IT STARTsUP 
--BEGIN; -- make all this a single transaction, so it's faster

-- not yet used, but might be cool 
CREATE TABLE IF NOT EXISTS topscore(
  scoreid     INTEGER PRIMARY KEY,
  playerid	  INTEGER NOT NULL REFERENCES player(playerid),
  scoreValue   INTEGER NOT NULL DEFAULT -1,
  iNumSeen     INTEGER NOT NULL DEFAULT 0,
  iNumCorrect  INTEGER NOT NULL DEFAULT 0,
  iRelationIDAgree  INTEGER NOT NULL DEFAULT 0,
  iRelationClassAgree  INTEGER NOT NULL DEFAULT 0,
  iOfPlayAgain INTEGER NOT NULL DEFAULT 0, -- when 0 it's not from a play again. So this represents the X'th play again of the player (e.g. if 1, then the score is from the 1st play again)
  dAdded      TIMESTAMP NOT NULL DEFAULT current_timestamp
  -- UNIQUE( playerid, dAdded )
);
CREATE INDEX IF NOT EXISTS topscoreIndex ON topscore( scoreValue ); -- for efficiency (ie "select * where scoreValue=?" is way faster)
CREATE INDEX IF NOT EXISTS topscoreIndexToo ON topscore( playerid ); 

CREATE TABLE IF NOT EXISTS player(
  playerid    INTEGER PRIMARY KEY, 
  udid		  TEXT NOT NULL CHECK( udid <> '' ),
  gamesPlayed INTEGER NOT NULL DEFAULT 0,  
  dAdded      TIMESTAMP NOT NULL DEFAULT current_timestamp,
  UNIQUE( udid ) 
);
CREATE INDEX IF NOT EXISTS playerIndex ON player( udid ); -- for efficiency (ie "select * where udid=?" is way faster)

CREATE TABLE IF NOT EXISTS relation(
  relationid    INTEGER PRIMARY KEY, 
  relationName  TEXT NOT NULL CHECK( relationName <> '' ),
  relationClass INTEGER NOT NULL DEFAULT -1,
  relationDescr TEXT NOT NULL,
  UNIQUE( relationName ) 
);
CREATE INDEX IF NOT EXISTS relationIndex ON relation( relationClass );

-- <DATA INIT>
--"over","above","on top","under","infront of","behind","next to","far from", "no relation" 
INSERT OR IGNORE INTO relation ( relationName, relationClass, relationDescr ) 
VALUES 
    ( 'above', 1, '' )
  , ( 'below', 2, '' )
  , ( 'on', 3, '' )
  , ( 'underneath', 4, '' )
  , ( 'infront of', 5, '' )
  , ( 'behind', 5, '' )
  , ( 'next to', 5, '' )
  , ( 'far from', 6, '' )
  , ( '<no relation>', 6, '' )
  , ( 'inside', 7, '' )
  , ( 'contains', 8, '' )
 ;

 -- </DATA INIT>
CREATE TABLE IF NOT EXISTS storyDomain(
  domainid     INTEGER PRIMARY KEY, 
  domainName   TEXT NOT NULL CHECK( domainName <> '' ),
  domainDescr  TEXT NOT NULL,
  bUse         INTEGER NOT NULL DEFAULT 1,
  bDone        INTEGER NOT NULL DEFAULT 1,
  numPairsLeft INTEGER NOT NULL DEFAULT 0,
  UNIQUE( domainName ) 
);
CREATE INDEX IF NOT EXISTS storyDomainIndex ON storyDomain( bUse, bDone );

--set bDone to 1 when done?
--count number of times deleted?
CREATE TABLE IF NOT EXISTS storyObject(
  objectid    INTEGER PRIMARY KEY,
  domainid    INTEGER REFERENCES storyDomain(domainid),
  objectName  TEXT NOT NULL CHECK( objectName <> '' ),
  bUse        INTEGER NOT NULL DEFAULT 1,  --new, but unused and not maintained
  bDone       INTEGER NOT NULL DEFAULT 0,  
  numDeletes  INTEGER NOT NULL DEFAULT 0,
  dAdded      TIMESTAMP NOT NULL DEFAULT current_timestamp,
  UNIQUE( domainid, objectName )
);
--http://sqlite.awardspace.info/syntax/sqlitepg11.htm
DROP TRIGGER IF EXISTS storyObjectIndex;
-- an index should be created on the child key columns of each foreign key constraint
CREATE INDEX storyObjectIndex ON storyObject(domainid); -- see http://www.sqlite.org/foreignkeys.html

--insert into storyObject(domainid,objectName) values(1,'');
DROP TRIGGER IF EXISTS updateTimeTriggerSO;
CREATE TRIGGER IF NOT EXISTS updateTimeTriggerSO
 AFTER UPDATE ON storyObject
 FOR EACH ROW
 BEGIN
  UPDATE storyObject
    SET dAdded = current_timestamp 
	WHERE objectid = old.objectid;
  -- flag the pair as having a deleted object, if appropriate	
  UPDATE storyObjectPair
    SET bHasDeletedObj = 1 
	WHERE new.numDeletes > 0 AND
	      (object1id = new.objectid OR object2id = new.objectid);
END;

DROP TRIGGER IF EXISTS insertNewStoryObject;
CREATE TRIGGER IF NOT EXISTS insertNewStoryObject
 AFTER INSERT ON storyDomain
 FOR EACH ROW
 BEGIN
  -- always add wall, ceiling and floor as objects to any new domain
  INSERT OR IGNORE INTO storyObject( domainid, objectName ) 
    VALUES 
	    (new.domainid, 'wall')
	  , (new.domainid, 'ceiling')
	  , (new.domainid, 'floor')
  ;
 END;

-- no need to create index on domainid/objectName bc
-- CREATE INDEX IF NOT EXISTS storyObjectIndex ON storyObject(domainid); -- for efficiency
CREATE TABLE IF NOT EXISTS storyObjectPair(
  objpairid   INTEGER PRIMARY KEY,
  domainid    INTEGER NOT NULL REFERENCES storyDomain(domainid),      
  object1id   INTEGER NOT NULL REFERENCES storyObject(objectid),
  object2id   INTEGER NOT NULL REFERENCES storyObject(objectid),
  bUse         INTEGER NOT NULL DEFAULT 1,
  bDone        INTEGER NOT NULL DEFAULT 0, -- externally maintained (game code)
  numTimesSeen  INTEGER NOT NULL DEFAULT 0,
  numESPAgree   INTEGER NOT NULL DEFAULT 0,
  intConfidence INTEGER NOT NULL DEFAULT 0,
  iRelationClass INTEGER NOT NULL DEFAULT 0,
  bHasDeletedObj INTEGER NOT NULL DEFAULT 0,
  UNIQUE( domainid, object1id, object2id ) --,
  -- FOREIGN KEY( domainid, object1id ) REFERENCES storyObject( domainid, objectid )
);
CREATE INDEX IF NOT EXISTS storyObjectPairIndex ON storyObjectPair(domainid, numESPAgree, bHasDeletedObj); -- for efficiency (ie "select * where domainid=? AND numESPAgree=0 AND bHasDeletedObj=0" is way faster)
CREATE INDEX IF NOT EXISTS storyObjectPairIndexToo ON storyObjectPair( bUse, bDone, bHasDeletedObj, domainid );
CREATE INDEX IF NOT EXISTS storyObjectPairIndexTooToo ON storyObjectPair( domainid );

-- ensure the integrity of domainid (ie that object1id and object2id are for the same domain)
DROP TRIGGER IF EXISTS insertStoryObjectPair;
CREATE TRIGGER IF NOT EXISTS insertStoryObjectPair
BEFORE INSERT ON storyObjectPair BEGIN
  SELECT CASE 
  WHEN ( 
         (SELECT so.domainid FROM storyObject so WHERE so.objectid = new.object1id AND so.domainid = new.domainid ) 
		 ISNULL 
		)
  THEN RAISE( ABORT, 'storyObjectPair domainid for object1 does not match storyObject domainid for object1' )
  WHEN ( 
         (SELECT so.domainid FROM storyObject so WHERE so.objectid = new.object2id AND so.domainid = new.domainid ) 
		 ISNULL 
		)
  THEN RAISE( ABORT, 'storyObjectPair domainid for object2 does not match storyObject domainid for object2' )
  END;
END;

DROP TRIGGER IF EXISTS updateStoryObjectPair;
CREATE TRIGGER IF NOT EXISTS updateStoryObjectPair
 AFTER INSERT ON storyObject
 FOR EACH ROW
 BEGIN
  -- create all the new pairs needed when a new storyObject has been added
  INSERT OR IGNORE INTO storyObjectPair( domainid, object1id, object2id ) 
  SELECT SO.domainid, new.objectid, SO.objectid
    FROM storyObject AS SO 
  WHERE 
    new.objectName != 'wall' AND new.objectName != 'ceiling' AND new.objectName != 'floor' AND    
	SO.domainid = new.domainid AND
    SO.objectid != new.objectid AND 	
	SO.dAdded <= new.dAdded
  ;
  -- set the done flag on storyDomain to zero because we've added a new storyObject
  UPDATE OR IGNORE storyDomain
    SET bDone = 0
    WHERE 
      domainid = new.domainid
  ;
 END;

 DROP TRIGGER IF EXISTS updateStoryObject;
CREATE TRIGGER IF NOT EXISTS updateStoryObject
 AFTER INSERT ON storyObjectPair
 FOR EACH ROW
 BEGIN
  -- set the done flag on storyObject to zero if it hasn't been deleted
  UPDATE OR IGNORE storyObject
    SET bDone = 0
    WHERE 
      domainid = new.domainid AND
	  numDeletes < 1 AND
      ( objectid = new.object1id OR objectid = new.object2id )
  ;
  -- keep the count of how many object pairs need to be finished 
  UPDATE storyDomain 
    SET numPairsLeft = 
	       ( SELECT count(*) FROM storyObjectPair as SOP 
	        WHERE SOP.domainid = new.domainid AND SOP.bDone = 0 )
	WHERE 
	 domainid = new.domainid 
  ;
 END;

 --TODO make this a case statement for efficiency reasons: http://stackoverflow.com/questions/4968841/case-statement-in-sqlite-query
 --CASE ( SELECT count(*) FROM storyObjectPair as SOP 
 --	        WHERE SOP.domainid = new.domainid AND SOP.bDone = 0 )
 --    WHEN 0 THEN SET numPairsLeft = 0, bDone = 1 WHERE SOP.domainid = new.domainid
	
 DROP TRIGGER IF EXISTS updateStoryDomainPairsLeft;
CREATE TRIGGER IF NOT EXISTS updateStoryDomainPairsLeft
 AFTER UPDATE ON storyObjectPair
 FOR EACH ROW
 BEGIN
  -- set the done flag on storyDomain to one if all object pairs have at least one agreement
  -- UPDATE storyDomain 
  --  SET bDone = 1 
  --	WHERE 
  --	 domainid = new.domainid AND
  --	 ( ( SELECT count(*) FROM storyObjectPair as SOP 
  --	        WHERE SOP.domainid = new.domainid AND SOP.numESPAgree = 0 ) = 0 )
  -- ;
  
  -- keep the count of how many object pairs need to be finished 
  UPDATE storyDomain 
    SET numPairsLeft = 
	       ( SELECT count(*) FROM storyObjectPair as SOP 
	        WHERE SOP.domainid = new.domainid AND SOP.bDone = 0 )
	WHERE 
	 domainid = new.domainid 
  ;
  
END;


 DROP TRIGGER IF EXISTS updateStoryDomainDoneStatus;
CREATE TRIGGER IF NOT EXISTS updateStoryDomainDoneStatus
 AFTER UPDATE ON storyDomain
 FOR EACH ROW
 BEGIN
  -- set the done flag for the domain to true when that domain has all its object pairs done
  UPDATE storyDomain 
    SET bDone = 1
	WHERE domainid = new.domainid  AND numPairsLeft = 0
  ;
  
END;

--for single player data
CREATE TABLE IF NOT EXISTS objectPairRelation_1player(
  objpairrelid   INTEGER PRIMARY KEY,
  domainid       INTEGER NOT NULL REFERENCES storyDomain(domainid),     
  objpairid      INTEGER NOT NULL REFERENCES storyObjectPair(objpairid),
  relationid     INTEGER REFERENCES relation(relationid), -- can be null!
  playerid	     INTEGER NOT NULL REFERENCES player(playerid),
  millisecChoose INTEGER NOT NULL, 
  lastModified   TIMESTAMP NOT NULL DEFAULT current_timestamp,
  UNIQUE( objpairid, playerid ) 
);
CREATE INDEX IF NOT EXISTS objPairRelation1PIndexTooTooToo ON objectPairRelation_1player( relationid );
CREATE INDEX IF NOT EXISTS objPairRelation1PIndexTooToo ON objectPairRelation_1player( playerid );
CREATE INDEX IF NOT EXISTS objPairRelation1PIndexToo ON objectPairRelation_1player( objpairid );
CREATE INDEX IF NOT EXISTS objPairRelation1PIndex ON objectPairRelation_1player(domainid,objpairid); -- for efficiency (ie "select * where domainid=?" is way faster)
-- CREATE INDEX IF NOT EXISTS objPairRelation1PIndex ON objectPairRelation_1player(objpairid); -- for efficiency (ie "select * where domainid=?" is way faster)

-- ensure the integrity of domainid (ie that domainid for objpairid is as specified in storyObjectPair
DROP TRIGGER IF EXISTS insertRelationTriggerOPR1P;
CREATE TRIGGER IF NOT EXISTS insertRelationTriggerOPR1P
BEFORE INSERT ON objectPairRelation_1player BEGIN
  SELECT CASE 
  WHEN  ( (SELECT sop.domainid FROM storyObjectPair sop WHERE sop.objpairid = new.objpairid AND sop.domainid = new.domainid ) ISNULL )
  THEN RAISE( ABORT, 'could not find matching (domainid, objpairid) pair in storyObjectPair; specified domainid does not match stored domainid for that objpairid' )
  END;
END;

DROP TRIGGER IF EXISTS updateTimeTriggerOPR1P;
CREATE TRIGGER IF NOT EXISTS updateTimeTriggerOPR1P
AFTER UPDATE ON objectPairRelation_1player
FOR EACH ROW
BEGIN
  UPDATE objectPairRelation_1player
    SET lastModified = current_timestamp 
	WHERE objpairrelid = old.objpairrelid;
END;

--CREATE TABLE IF NOT EXISTS tmpobjpairCounts(
--  tmpcountid     INTEGER PRIMARY KEY,
--  objpairid      INTEGER NOT NULL REFERENCES storyObjectPair(objpairid),
--  relationClass  INTEGER NOT NULL REFERENCES relation(relationClass)
--);
--CREATE INDEX IF NOT EXISTS tmpobjpairCountsIndex ON tmpobjpairCounts(objpairid,relationClass);

DROP TRIGGER IF EXISTS updateCountTriggerOPR1P;
CREATE TRIGGER IF NOT EXISTS updateCountTriggerOPR1P
AFTER INSERT ON objectPairRelation_1player
FOR EACH ROW
BEGIN
  -- keep the count of how many times object pairs has gotten a non-null vote
  UPDATE storyObjectPair 
    SET numTimesSeen = 
	       ( SELECT count(*) FROM objectPairRelation_1player as opr 
	        WHERE opr.objpairid = new.objpairid AND opr.relationid IS NOT NULL )
    WHERE 
	 objpairid = new.objpairid 
  ;
	
	--, SET --update agreement count
	--  numESPAgree = 
   --finds the relation class with biggest count
   --( SELECT opr1p.relationid as relationid, rel1.relationClass as relationClass
   --  FROM objectPairRelation_1player opr1p 
   --  left join relation rel1 on opr1p.relationid = rel1.relationid
   --  WHERE opr1p.relationid IS NOT NULL AND opr1p.objpairid = new.objpairid GROUP BY relationClass ORDER BY count(relationClass) DESC LIMIT 1)

  
  -- inserts the new relation (if not null, handled by table constraint) into tmpobjpaircounts
  --INSERT OR IGNORE INTO tmpobjpairCounts( objpairid, relationClass ) 
  --VALUES ( new.objpairid,  
  --	   ( SELECT r.relationClass FROM relation r WHERE new.relationid = r.relationid )
  --	  );
  
  --SELECT CASE
  --WHEN ( new.objpairid IS NOT NULL  )
  --THEN -- ( -- RAISE( ABORT, 'could not find matching (domainid, objpairid) pair in storyObjectPair; specified domainid does not match stored domainid for that objpairid' )
		--INSERT INTO tmpobjpairCounts( objpairid, relationClass ) VALUES (1, 1)
		--VALUES ( new.objpairid,  
		--		 ( SELECT r.relationClass FROM relation r WHERE new.relationid = r.relationid )
		--		) 
		-- )
  --END;
END;

-- number of pairs of players that have selected this relation for this object pair
-- numESPAgree    INTEGER NOT NULL DEFAULT 1, 
CREATE TABLE IF NOT EXISTS objectPairRelation(
  objpairrelid   INTEGER PRIMARY KEY,
  domainid       INTEGER NOT NULL REFERENCES storyDomain(domainid),      
  objpairid      INTEGER NOT NULL REFERENCES storyObjectPair(objpairid),
  relationid_p1  INTEGER REFERENCES relation(relationid),
  relationid_p2  INTEGER REFERENCES relation(relationid),
  lastModified   TIMESTAMP NOT NULL DEFAULT current_timestamp
  --UNIQUE( objpairid, relationid ) 
);
CREATE INDEX IF NOT EXISTS objPairRelationIndex ON objectPairRelation(domainid,objpairid); -- for efficiency (ie "select * where domainid=?" is way faster)

DROP TRIGGER IF EXISTS updateTimeTriggerOPR;
CREATE TRIGGER IF NOT EXISTS updateTimeTriggerOPR
AFTER UPDATE ON objectPairRelation
FOR EACH ROW
BEGIN
  UPDATE objectPairRelation
    SET lastModified = current_timestamp 
	WHERE objpairrelid = old.objpairrelid;
END;

DROP TRIGGER IF EXISTS updateSOPCountInsert;
CREATE TRIGGER IF NOT EXISTS updateSOPCountInsert
 AFTER INSERT ON objectPairRelation
 FOR EACH ROW
 BEGIN
   --update agreement count
   UPDATE OR IGNORE storyObjectPair
     SET numESPAgree = numESPAgree + 1
	 WHERE 
	   objpairid = new.objpairid -- AND new.relationid_p1 = new.relationid_p2
	   AND ( (SELECT relationClass FROM relation WHERE relation.relationid = new.relationid_p1 LIMIT 1 )
			= (SELECT relationClass FROM relation WHERE relation.relationid = new.relationid_p2 LIMIT 1 ) );
   --update seen count
   UPDATE OR IGNORE storyObjectPair
     SET numTimesSeen = numTimesSeen + 1
	 WHERE objpairid = new.objpairid;
 END;
 
 DROP TRIGGER IF EXISTS updateSOPCountDelete;
CREATE TRIGGER IF NOT EXISTS updateSOPCountDelete
 AFTER DELETE ON objectPairRelation
 FOR EACH ROW
 BEGIN
   UPDATE OR IGNORE storyObjectPair
     SET numESPAgree = numESPAgree - 1
	 WHERE objpairid = old.objpairid  -- AND old.relationid_p1 = old.relationid_p2;
	 AND ( (SELECT relationClass FROM relation WHERE relation.relationid = new.relationid_p1 LIMIT 1 )
			= (SELECT relationClass FROM relation WHERE relation.relationid = new.relationid_p2 LIMIT 1 ) );
 END;
--TODO trigger for update (adjust counts appropriately)

-- <DATA INIT>
INSERT OR IGNORE INTO storyDomain 
(domainName, domainDescr, bUse) 
VALUES 
    ('test','internal test table', 1)
  , ('dining room','the formal place to eat in a house', 1)
  , ('bank','a place where people store their money safely.', 1)
  , ('movie theater','where movies and dates happen', 1)
  , ('fast food restaurant','place to get food quickly and cheaply; usually there is a drive-through.',1)
  , ('restaurant','the average, garden-variety restaurant',0)
  , ('kitchen','the kitchen you might find in an average domestic household',1)
  , ('museum','an average museum of no particular specialty',0)
;
-- </DATA INIT>

-- <DATA INIT>
--"tv", "chair", "sofa", "computer","desk","rania","nona"

INSERT OR IGNORE INTO player( udid ) VALUES ( 0 );

INSERT OR IGNORE INTO storyObject ( domainid, objectName, bDone, bUse ) 
VALUES 
  ( (SELECT domainid FROM storyDomain WHERE domainName='test'), 'testObj', 1, 1 ) 
;

INSERT OR IGNORE INTO storyObject ( domainid, objectName, bDone, bUse ) 
VALUES 
  ( (SELECT domainid FROM storyDomain WHERE domainName='dining room'), 'vase', 1, 1 ) 
  , ( (SELECT domainid FROM storyDomain WHERE domainName='dining room'), 'table', 1, 1 )
  , ( (SELECT domainid FROM storyDomain WHERE domainName='dining room'), 'chandelier', 1, 1 )
  , ( (SELECT domainid FROM storyDomain WHERE domainName='dining room'), 'chair', 1, 1 )
  , ( (SELECT domainid FROM storyDomain WHERE domainName='dining room'), 'plant', 1, 1 )
  , ( (SELECT domainid FROM storyDomain WHERE domainName='dining room'), 'flower pot', 1, 1 )
  , ( (SELECT domainid FROM storyDomain WHERE domainName='dining room'), 'picture', 1, 1 )
  , ( (SELECT domainid FROM storyDomain WHERE domainName='dining room'), 'window', 1, 1 )
  , ( (SELECT domainid FROM storyDomain WHERE domainName='dining room'), 'person', 1, 1 )
;

INSERT OR IGNORE INTO storyObject ( domainid, objectName, bDone, bUse ) 
VALUES 
  ( (SELECT domainid FROM storyDomain WHERE domainName='bank'), 'door', 1, 1 ) 
  , ( (SELECT domainid FROM storyDomain WHERE domainName='bank'), 'gun', 1, 1 )    
  , ( (SELECT domainid FROM storyDomain WHERE domainName='bank'), 'note', 1, 1 )    
  , ( (SELECT domainid FROM storyDomain WHERE domainName='bank'), 'money', 1, 1 )
  , ( (SELECT domainid FROM storyDomain WHERE domainName='bank'), 'bag', 1, 1 )
  , ( (SELECT domainid FROM storyDomain WHERE domainName='bank'), 'drawer', 1, 1 )
  , ( (SELECT domainid FROM storyDomain WHERE domainName='bank'), 'car', 1, 1 )
  , ( (SELECT domainid FROM storyDomain WHERE domainName='bank'), 'service counter', 1, 1 )
  , ( (SELECT domainid FROM storyDomain WHERE domainName='bank'), 'payment counter', 1, 1 )
  , ( (SELECT domainid FROM storyDomain WHERE domainName='bank'), 'currency', 1, 1 )
  , ( (SELECT domainid FROM storyDomain WHERE domainName='bank'), 'form of money', 1, 1 )
  , ( (SELECT domainid FROM storyDomain WHERE domainName='bank'), 'money', 1, 1 )
  , ( (SELECT domainid FROM storyDomain WHERE domainName='bank'), 'coin', 1, 1 )
  , ( (SELECT domainid FROM storyDomain WHERE domainName='bank'), 'cheque', 1, 1 )
  , ( (SELECT domainid FROM storyDomain WHERE domainName='bank'), 'atm', 1, 1 )
  , ( (SELECT domainid FROM storyDomain WHERE domainName='bank'), 'branch', 1, 1 )
;

INSERT OR IGNORE INTO storyObject ( domainid, objectName, bDone, bUse ) 
VALUES 
  ( (SELECT domainid FROM storyDomain WHERE domainName='movie theater'), 'car', 1, 1 ) 
  , ( (SELECT domainid FROM storyDomain WHERE domainName='movie theater'), 'tickets', 1, 1 ) 
  , ( (SELECT domainid FROM storyDomain WHERE domainName='movie theater'), 'popcorn', 1, 1 ) 
  , ( (SELECT domainid FROM storyDomain WHERE domainName='movie theater'), 'seat', 1, 1 ) 
  , ( (SELECT domainid FROM storyDomain WHERE domainName='movie theater'), 'drink', 1, 1 ) 
  , ( (SELECT domainid FROM storyDomain WHERE domainName='movie theater'), 'refreshment', 1, 1 ) 
  , ( (SELECT domainid FROM storyDomain WHERE domainName='movie theater'), 'soda', 1, 1 ) 
  , ( (SELECT domainid FROM storyDomain WHERE domainName='movie theater'), 'movie', 1, 1 ) 
  , ( (SELECT domainid FROM storyDomain WHERE domainName='movie theater'), 'many theater seat', 1, 1 ) 
  , ( (SELECT domainid FROM storyDomain WHERE domainName='movie theater'), 'theater seat', 1, 1 ) 
;

INSERT OR IGNORE INTO storyObject ( domainid, objectName, bDone, bUse ) 
VALUES 
  ( (SELECT domainid FROM storyDomain WHERE domainName='fast food restaurant'), 'menu', 1, 1 ) 
  , ( (SELECT domainid FROM storyDomain WHERE domainName='fast food restaurant'), 'window', 1, 1 ) 
  , ( (SELECT domainid FROM storyDomain WHERE domainName='fast food restaurant'), 'wallet', 1, 1 ) 
  , ( (SELECT domainid FROM storyDomain WHERE domainName='fast food restaurant'), 'food', 1, 1 ) 
  , ( (SELECT domainid FROM storyDomain WHERE domainName='fast food restaurant'), 'trash', 1, 1 ) 
  , ( (SELECT domainid FROM storyDomain WHERE domainName='fast food restaurant'), 'table', 1, 1 ) 
  -- the rest are the iconistic objects from restaurant
  , ( (SELECT domainid FROM storyDomain WHERE domainName='fast food restaurant'), 'coffee', 1, 1 ) 
  , ( (SELECT domainid FROM storyDomain WHERE domainName='fast food restaurant'), 'fork', 1, 1 ) 
  , ( (SELECT domainid FROM storyDomain WHERE domainName='fast food restaurant'), 'bowl', 1, 1 ) 
  , ( (SELECT domainid FROM storyDomain WHERE domainName='fast food restaurant'), 'chopstick', 1, 1 ) 
  , ( (SELECT domainid FROM storyDomain WHERE domainName='fast food restaurant'), 'water', 1, 1 ) 
  , ( (SELECT domainid FROM storyDomain WHERE domainName='fast food restaurant'), 'plate', 1, 1 ) 
  , ( (SELECT domainid FROM storyDomain WHERE domainName='fast food restaurant'), 'knife', 1, 1 ) 
;

INSERT OR IGNORE INTO storyObject ( domainid, objectName, bDone, bUse ) 
VALUES 
  ( (SELECT domainid FROM storyDomain WHERE domainName='restaurant'), 'coffee', 1, 1 ) 
  , ( (SELECT domainid FROM storyDomain WHERE domainName='restaurant'), 'fork', 1, 1 ) 
  , ( (SELECT domainid FROM storyDomain WHERE domainName='restaurant'), 'bowl', 1, 1 ) 
  , ( (SELECT domainid FROM storyDomain WHERE domainName='restaurant'), 'chopstick', 1, 1 ) 
  , ( (SELECT domainid FROM storyDomain WHERE domainName='restaurant'), 'water', 1, 1 ) 
  , ( (SELECT domainid FROM storyDomain WHERE domainName='restaurant'), 'plate', 1, 1 ) 
  , ( (SELECT domainid FROM storyDomain WHERE domainName='restaurant'), 'knife', 1, 1 ) 
;

INSERT OR IGNORE INTO storyObject ( domainid, objectName, bDone, bUse ) 
VALUES 
  ( (SELECT domainid FROM storyDomain WHERE domainName='kitchen'), 'baking oven', 1, 1 ) 
  , ( (SELECT domainid FROM storyDomain WHERE domainName='kitchen'), 'pot', 1, 1 ) 
  , ( (SELECT domainid FROM storyDomain WHERE domainName='kitchen'), 'teakettle', 1, 1 ) 
  , ( (SELECT domainid FROM storyDomain WHERE domainName='kitchen'), 'chef', 1, 1 ) 
  , ( (SELECT domainid FROM storyDomain WHERE domainName='kitchen'), 'fork', 1, 1 ) 
  , ( (SELECT domainid FROM storyDomain WHERE domainName='kitchen'), 'bowl', 1, 1 ) 
  , ( (SELECT domainid FROM storyDomain WHERE domainName='kitchen'), 'utensil', 1, 1 ) 
  , ( (SELECT domainid FROM storyDomain WHERE domainName='kitchen'), 'cup', 1, 1 ) 
  , ( (SELECT domainid FROM storyDomain WHERE domainName='kitchen'), 'cabinet', 1, 1 ) 
  , ( (SELECT domainid FROM storyDomain WHERE domainName='kitchen'), 'cutlery drawer', 1, 1 ) 
  , ( (SELECT domainid FROM storyDomain WHERE domainName='kitchen'), 'counter', 1, 1 ) 
  , ( (SELECT domainid FROM storyDomain WHERE domainName='kitchen'), 'cooking', 1, 1 ) 
  , ( (SELECT domainid FROM storyDomain WHERE domainName='kitchen'), 'dish', 1, 1 ) 
  , ( (SELECT domainid FROM storyDomain WHERE domainName='kitchen'), 'food', 1, 1 ) 
  , ( (SELECT domainid FROM storyDomain WHERE domainName='kitchen'), 'plate', 1, 1 ) 
  , ( (SELECT domainid FROM storyDomain WHERE domainName='kitchen'), 'knife', 1, 1 ) 
  , ( (SELECT domainid FROM storyDomain WHERE domainName='kitchen'), 'stove', 1, 1 ) 
  , ( (SELECT domainid FROM storyDomain WHERE domainName='kitchen'), 'spoon', 1, 1 ) 
;

INSERT OR IGNORE INTO storyObject ( domainid, objectName, bDone, bUse ) 
VALUES 
  ( (SELECT domainid FROM storyDomain WHERE domainName='museum'), 'painting', 1, 1 ) 
  , ( (SELECT domainid FROM storyDomain WHERE domainName='museum'), 'display', 1, 1 ) 
  , ( (SELECT domainid FROM storyDomain WHERE domainName='museum'), 'cornet', 1, 1 ) 
  , ( (SELECT domainid FROM storyDomain WHERE domainName='museum'), 'sword', 1, 1 ) 
  , ( (SELECT domainid FROM storyDomain WHERE domainName='museum'), 'work of art', 1, 1 ) 
  , ( (SELECT domainid FROM storyDomain WHERE domainName='museum'), 'weapon of war', 1, 1 ) 
  , ( (SELECT domainid FROM storyDomain WHERE domainName='museum'), 'harpsichord', 1, 1 ) 
  , ( (SELECT domainid FROM storyDomain WHERE domainName='museum'), 'gong', 1, 1 ) 
;
-- </DATA INIT>

UPDATE storyDomain SET bUse = 0;
UPDATE storyDomain SET bUse = 1 WHERE domainName = 'dining room';

--  http://stackoverflow.com/questions/3452128/sql-select-with-multiple-references-to-single-table
 
 -- SELECT m.objpairid, m.domainid, m.numESPAgree, object1id.objectName as nameObj1, object2id.objectName as nameObj2 from storyObjectPair m left join storyObject object1id on m.object1id = object1id.objectid left join storyObject object2id on m.object2id = object2id.objectid WHERE m.domainid=1 AND m.numESPAgree=0 LIMIT 1
 
  DROP VIEW IF EXISTS vwObjPair;
 CREATE VIEW IF NOT EXISTS vwObjPair AS
  SELECT m.objpairid, m.domainid, m.bUse, m.bDone, m.intConfidence, m.numESPAgree, m.numTimesSeen, m.iRelationClass, m.bHasDeletedObj
    , m.object1id, m.object2id
    , object1id.objectName as nameObj1
    , object2id.objectName as nameObj2 
  from storyObjectPair m 
  left join storyObject object1id on m.object1id = object1id.objectid 
  left join storyObject object2id on m.object2id = object2id.objectid 
 ORDER BY objpairid;
 
 -- for when we were doing 2 player synch mode
 DROP VIEW IF EXISTS vwObjPairRelation;
 CREATE VIEW IF NOT EXISTS vwObjPairRelation AS
  SELECT m.objpairrelid, m.domainid, m.objpairid, m.relationid_p1, m.relationid_p2
    , vop.nameObj1 as nameObj1
	, vop.nameObj2 as nameObj2
	, rel1.relationName as relationName_p1
	, rel2.relationName as relationName_p2	
	, m.lastModified
  from objectPairRelation m 
  left join relation rel1 on m.relationid_p1 = rel1.relationid
  left join relation rel2 on m.relationid_p2 = rel2.relationid
  left join vwObjPair vop on m.objpairid = vop.objpairid
 ;
 
 DROP VIEW IF EXISTS vwObjPair1Player;
 CREATE VIEW IF NOT EXISTS vwObjPair1Player AS
  SELECT m.objpairrelid, m.objpairid, m.domainid
    , vop.bUse, vop.bDone, vop.intConfidence, vop.numESPAgree, vop.numTimesSeen, vop.iRelationClass, vop.bHasDeletedObj
    , vop.object1id, vop.object2id
    , vop.nameObj1 as nameObj1
	, vop.nameObj2 as nameObj2
	, m.relationid as relationid
	, rel1.relationName as relationName
	, m.playerid, m.millisecChoose
	, m.lastModified
  from objectPairRelation_1player m 
  left join relation rel1 on m.relationid = rel1.relationid
  left join vwObjPair vop on m.objpairid = vop.objpairid
 ORDER BY m.objpairid
 ;
 
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



--for temporarily loading the gold standard (it is then inserted into 
  DROP TABLE IF EXISTS relationGoldStandardLOADED;
 CREATE TABLE IF NOT EXISTS relationGoldStandardLOADED(
  -- rgsl    INTEGER PRIMARY KEY, 
  domainName   TEXT NOT NULL,
  nameObj1     TEXT NOT NULL,
  nameObj2     TEXT NOT NULL,
  relationName TEXT,
  relationAlt  TEXT,
  UNIQUE( domainName, nameObj1, nameObj2 ) 
);
CREATE INDEX IF NOT EXISTS relationGoldStandardLOADEDIndex ON relationGoldStandardLOADED(domainName, nameObj1, nameObj2); 

.separator ","
.import final_allObjectPairs_minusExpertRemovedObjs_GOLDSTANDARD.csv relationGoldStandardLOADED
.import final_diningRoom_smlu.csv relationGoldStandardLOADED

DROP TABLE IF EXISTS relationGoldStandard;
 CREATE TABLE IF NOT EXISTS relationGoldStandard(
 gsid           INTEGER PRIMARY KEY,
 objpairid      INTEGER REFERENCES storyObjectPair(objpairid),
 relationid     INTEGER REFERENCES relation(relationid),
 -- lastModified   TIMESTAMP NOT NULL DEFAULT current_timestamp
 UNIQUE( objpairid, relationid ) 
 );
CREATE INDEX IF NOT EXISTS relationGoldStandardIndex ON relationGoldStandard(objpairid); 

INSERT OR IGNORE INTO relationGoldStandard( objpairid, relationid )  
 SELECT vop.objpairid, rel1.relationid
 FROM relationGoldStandardLOADED GSL
 left join relation rel1 on GSL.relationName = rel1.relationName
 left join storyDomain sd on GSL.domainName = sd.domainName
 left join vwObjPair vop on (sd.domainid=vop.domainid AND GSL.nameObj1 = vop.nameObj1 AND GSL.nameObj2 = vop.nameObj2) 
 ORDER BY vop.objpairid;

DROP TABLE relationGoldStandardLOADED;
 
DROP VIEW IF EXISTS vwGoldStandard;

 CREATE VIEW IF NOT EXISTS vwGoldStandard AS
  SELECT 
      rgs.objpairid as objpairid
	, vop.object1id as object1id
	, vop.object2id as object2id
    , vop.domainid as domainid
	, sd.domainName as domainName
    , vop.nameObj1 as nameObj1
	, vop.nameObj2 as nameObj2
	, rgs.relationid as relationid
	, rel.relationName as relationName
  from relationGoldStandard rgs 
  left join relation rel on rgs.relationid = rel.relationid
  left join vwObjPair vop on rgs.objpairid = vop.objpairid
  left join storyDomain sd on sd.domainid = vop.domainid
 ; 
 
-- COMMIT; --end the transaction

-- TO GENERATE A GOLDSTANDARD SPREADSHEET TO FILL OUT:
-- select
-- sd.domainName, vop.nameObj1, vop.nameObj2
-- FROM
-- vwObjPair vop
-- left join storyDomain sd on vop.domainid = sd.domainid
