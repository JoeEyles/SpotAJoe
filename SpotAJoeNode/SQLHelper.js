var mysql = require('mysql');

var stream;

var con;
var db_config = {
  host: "localhost",
  user: "spotajoe",
  password: "fork-road",
  database: "spotajoedb"
};
function handleDisconnect() {
  con = mysql.createConnection(db_config); 

  con.connect(function(err) {    
    	if(err) {                                     
    	  	stream.log("NA", "MySQL connection error, retrying in 2s: " + err);
    	  	setTimeout(handleDisconnect, 2000); 
    	}          
	else
		stream.log("NA", "SQL CONNECTED");                                    
  });                                     
                                          
  con.on('error', function(err) {
    stream.log("NA", "sql error: " + err.toString());
    if(err.code === 'PROTOCOL_CONNECTION_LOST') { 
      handleDisconnect();                         
    } else {     
      handleDisconnect();                                  
    }
  });
}



exports.SetUpSQLHelper = function(inStream) {
	stream = inStream;
	handleDisconnect();
}


var GetBy = exports.GetBy = function(reqUsr, get, by, byId, onError, onComplete) {
	var queryString = "SELECT " + get + ".* " +
			"FROM " + get + " " +
			"INNER JOIN " + by + " ON " + by + ".id=" + get + "." + by.toLowerCase() + "Id " +
			"WHERE "+ by + ".id=" + byId;
	con.query(queryString, function (err, result) {
		if(err) {
			onError(err);
		}
		else {
			onComplete(JSON.stringify(result));
		}
  	});
}

var GetByTag = exports.GetByTag = function(reqUsr, get, tagId, onError, onComplete) {
	var queryString = "SELECT " + get + ".* " +
			"FROM " + get + " " +
			"INNER JOIN " + get + "TagJoin ON " + get + ".id=" + get + "TagJoin." + get + "Id " +
			"WHERE "+ get + "TagJoin.tagId=" + tagId;
	con.query(queryString, function (err, result) {
		if(err) {
			onError(err);
		}
		else {
			onComplete(JSON.stringify(result));
		}
  	});
}

var GetTracksForAlbumsRecurring = function(reqUsr, i, artist, onError, onComplete) {
	var queryString = "SELECT * "+
			"FROM Track "+
			"WHERE Track.albumId=" + artist.albums[i].id;
	con.query(queryString, function (err, tracksResult) {
		if(err) {
			console.log("error in GetTracksForAlbumsRecurring()");
			onError(err);
		}
		else {
			artist.albums[i].tracks = tracksResult;

			if(i == artist.albums.length -1)
				onComplete();
			else 
				GetTracksForAlbumsRecurring(reqUsr, i+1, artist, function(err){onError(err);}, onComplete);
		}
  	});
}

var GetArtistStack = function(reqUsr, artist, onError, onComplete) {
	var queryString = "SELECT * "+
			"FROM Album "+
			"WHERE Album.artistId=" + artist.id;
	con.query(queryString, function (err, albumsResult) {
		if(err) {
			stream.log(reqUsr,"error in GetArtistStack()");
			onError(err);
		}
		else {
			artist.albums = albumsResult;
			if(artist.albums.length == 0)
				onComplete(JSON.stringify(artist));
			else {
				GetTracksForAlbumsRecurring(reqUsr, 0, artist, function(err){onError(err);}, onComplete);
			}
		}
  	});
}


var GetAllArtistStacksRecurring = function(reqUsr, i, artists, onError, onComplete) {
	GetArtistStack(reqUsr, artists[i], function(err){onError(err);}, function() {
		if(i == artists.length -1)  {
			onComplete();
		}
		else {
			GetAllArtistStacksRecurring(reqUsr, i+1, artists, function(err){onError(err);}, onComplete);
		}
	});
}

exports.GetAllArtistStacksByTag = function(reqUsr, tagId, onError, onComplete) {			
	GetByTag(reqUsr, "Artist", tagId, function(err) {
		stream.log(reqUsr,"Error in GetByTag()");
		onError(err);
	}, function(artistsResult) {
		var artists = JSON.parse(artistsResult);
		if(artists.length == 0) {
			onComplete("[]");
		}
		else {
			GetAllArtistStacksRecurring(reqUsr, 0, artists, function(err) {
				stream.log(reqUsr,"Error in GetAllArtistStacksRecurring()");
				onError(err);
			}, function() {
				onComplete(JSON.stringify(artists));
			});
		}
	});
}

exports.GetAllTags = function(reqUsr, startWith, onError, onComplete) {
	var queryString = "SELECT * "+
			"FROM Tag "+
			"WHERE Tag.tag LIKE '" +startWith+ "%'";
	con.query(queryString, function (err, result) {
		if(err) {
			onError(err);
		}
		else {
			onComplete(JSON.stringify(result));
		}
  	});
}




function UpdateTrackNumbersIterate(reqUsr, data, index, onError, onComplete) {
	var queryString = "UPDATE Track " + 
			"SET trackNumber=" + data[index].trackNumber + " " +
			"WHERE Track.id=" + data[index].trackId;
	con.query(queryString, function(err, result) {
		if(err) {
			onError(err);
		}
		else {
			if(index == data.length - 1)
				onComplete();
			else 
				UpdateTrackNumbersIterate(reqUsr, data, index + 1, onError, onComplete);
		}
	});
}

exports.UpdateTrackNumbers = function(reqUsr, data, onError, onComplete) {
	UpdateTrackNumbersIterate(reqUsr, data, 0, onError, onComplete);
}

exports.GetAllArtists = function(reqUser, onError, onComplete) {
	 var queryString = "SELECT * "+
			"FROM Artist";
	con.query(queryString, function(err, result) {
		if(err) {
			onError(err);
		}
		else {
			onComplete(JSON.stringify(result));
		}
	});
}
