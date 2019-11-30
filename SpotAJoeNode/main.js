var http = require('http');
var HttpDispatcher = require('./j-http-dispatcher.js');
//var HttpDispatcher = require('httpdispatcher');
var dispatcher = new HttpDispatcher();
var url = require('url');
var auth = require('http-auth');
var basic = auth.basic({
    realm: "SpotAJoe",
    file: __dirname + "/users.htpasswd"
});
var SQLHelper = require('./SQLHelper');//TODO: stop sql injection (just need to escape incoming strings)
var fs = require('fs');
var ipChecker = require("./IPChecker");

var rfs = require("rotating-file-stream");
var stream = rfs("log.txt", {
    path: "logs",
    size: "1M", 
    rotate: 1,
    maxFiles: 10
});
stream.log = function(usr, str) {
	stream.write((new Date()).toGMTString() + " - " + usr + " - " + str + "\n");
};
SQLHelper.SetUpSQLHelper(stream);


const accessPORT = 8080;

dispatcher.setStatic('/public');
dispatcher.setStaticDirname('public');

function handleRequest(request, response){
    try {
        stream.log(request.user, "Request recieved: " + request.url);
        dispatcher.dispatch(request, response);
    } catch(err) {
        stream.log(request.user, "Request error: " + err);
    }
}

var server = http.createServer(basic, handleRequest);

dispatcher.onGet("/", function(req, res) {
try{
    fs.readFile("./public/webapp/index.html", function(err, data) {
	if(err) {
	    	res.writeHead(404);
    		res.end("Web app not found.");
	}
	else {
	    	WriteHeaders(res, "html");
    		res.write(data);
    		res.end();
	}
    });
} catch(err) {PrintTryCatchError(res, req.user,err);}
});

dispatcher.onGet("/query", function(req, res) {
    WriteHeaders(res, "html");
    res.write("Possible queries:<br \>");
    res.write("/GetBy?get={Artist,Album,Track}&by={Artist,Album,Track}&byId={id}<br \>");
    res.write("/GetByTag?get={Artist,Album,Track}&tagId={id}<br \>");
    res.write("/GetArtistStack?artistId={id}<br \>");
    res.write("/GetAllArtistStacksByTag?Tag={id}<br \>");
    res.write("/GetAllTags?startingWith={str(can be empty)}<br \>");
    res.write("/GetAllArtists<br \>");
    res.write("/GetUsername<br \>");
    res.end("");
});

dispatcher.onGet("/query/GetBy", function(req, res) {
try {
    var q = url.parse(req.url, true).query;
    if(!q.get || !q.by || !q.byId)
    {
	WriteHeaders(res, "html");
	res.write("Missing arguments get, by or byId<br\>");
	res.write("/GetBy?get={Artist,Album,Track}&by={Artist,Album,Track}&byId={id}<br \>");
     	res.end("");
    }
    else
    {
	SQLHelper.GetBy(req.user, q.get, q.by, q.byId, function(error) {
		WriteHeaders(res, "html");
		res.end("SQL error: " + error + "<br />");
	}, function(result) {
		WriteHeaders(res, "json");
		res.write(result);
		res.end("");
	});
    }
} catch(err) {PrintTryCatchError(res,req.user,err);}
});

dispatcher.onGet("/query/GetByTag", function(req, res) {
try {
    var q = url.parse(req.url, true).query;
    if(!q.get || !q.tagId)
    {
	WriteHeaders(res, "html");
	res.write("Missing arguments get, by or byId<br\>");
	res.write("/GetByTag?get={Artist,Album,Track}&tagId={id}<br \>");
     	res.end("");
    }
    else
    {
	SQLHelper.GetByTag(req.user, q.get, q.tagId, function(error) {
		WriteHeaders(res, "html");
		res.end("SQL error: " + error + "<br />");
	}, function(result) {
		WriteHeaders(res, "json");
		res.write(result);
		res.end("");
	});
    }
} catch(err) {PrintTryCatchError(res,req.user,err);}
});

dispatcher.onGet("/query/GetAllArtistStacksByTag", function(req, res) {
try {
    var q = url.parse(req.url, true).query;
    if(!q.tagId)
    {
	WriteHeaders(res, "html");
	res.write("Missing argument tagId<br\>");
	res.write("/GetAllArtistStacksByTag?tagId={id}<br \>");
     	res.end("");
    }
    else
    {
	SQLHelper.GetAllArtistStacksByTag(req.user, q.tagId, function(error) {
		WriteHeaders(res, "html");
		res.end("SQL error: " + error + "<br />");
	}, function(result) {
		WriteHeaders(res, "json");
		res.write(result);
		res.end("");
	});
    }
} catch(err) {PrintTryCatchError(res,req.user,err);}
});

dispatcher.onGet("/query/GetArtistStack", function(req, res) {
try {
    var q = url.parse(req.url, true).query;
    if(!q.artistId)
    {
	WriteHeaders(res, "html");
	res.write("Missing argument artistId<br\>");
	res.write("/GetArtistStack?artistId={id}<br \>");
     	res.end("");
    }
    else
    {
	SQLHelper.GetArtistStack(req.user, q.artistId, function(error) {
		WriteHeaders(res, "html");
		res.end("SQL error: " + error + "<br />");
	}, function(result) {
		WriteHeaders(res, "json");
		res.write(result);
		res.end("");
	});
    }
} catch(err) {PrintTryCatchError(res,req.user,err);}
});

dispatcher.onGet("/query/GetAllTags", function(req, res) {
try {
    var q = url.parse(req.url, true).query;
    if(!q.startWith)
    {
	WriteHeaders(res, "html");
	res.write("Missing argument startWith<br\>");
	res.write("/GetAllTags?startingWith={str(can be empty)}<br \>");
     	res.end("");
    }
    else
    {
	SQLHelper.GetAllTags(req.user, q.startWith, function(error) {
		WriteHeaders(res, "html");
		res.end("SQL error: " + error + "<br />");
	}, function(result) {
		WriteHeaders(res, "json");
		res.write(result);
		res.end("");
	});
    }
} catch(err) {PrintTryCatchError(res,req.user,err);}
});

dispatcher.onGet("/query/GetAllArtists", function(req, res) {
try {
	SQLHelper.GetAllArtists(req.user, function(error) {
		WriteHeaders(res, "html");
		res.end("SQL error: " + error + "<br />");
	}, function(result) {
		WriteHeaders(res, "json");
		res.write(result);
		res.end("");
	});
} catch(err) {PrintTryCatchError(res,req.user,err);}
});

dispatcher.onGet("/query/GetUsername", function(req, res) {
try {
	WriteHeaders(res, "json");
	res.write('{"username":"' + req.user + '"}');
	res.end("");	
} catch(err) {PrintTryCatchError(res,req.user,err);}
});

dispatcher.onPost("/update", function(req, res) {
    WriteHeaders(res, "html");
    res.write("Possible posts:<br \>");
    res.write("/update/UpdateTrackNumbers<br \>");
    res.end("");
});

dispatcher.onPost("/update/UpdateTrackNumbers", function(req, res) {
try {
	if(req.user == "guest")
	{
		WriteHeaders(res, "html");
		res.end("Error, guest cannot change track order.");
	}
	else 
	{
		SQLHelper.UpdateTrackNumbers(req.user, JSON.parse(req.body), function(error) {
			WriteHeaders(res, "html");
			res.end("SQL error: " + error + "<br />");
		}, function() {
			WriteHeaders(res, "html");
			res.end("Success");
		});
	}
} catch(err) {PrintTryCatchError(res,req.user,err);}
});

dispatcher.onError(function(req, res) {
    res.writeHead(404);
    res.end("Error, the URL doesn't exist");
});

server.listen(accessPORT, function(){
    stream.log("NA","Server listening on port: " + accessPORT);
    console.log("Server listening on port :%s", accessPORT);
});

function WriteHeaders(res, type) {
	if(type == "html") {
		res.writeHead(200, {'Content-Type': 'text/' + type, 
			'Access-Control-Allow-Origin': '*'});
	}
	else if(type == "json") {
		res.writeHead(200, {'Content-Type': 'application/' + type, 
			'Access-Control-Allow-Origin': '*'});
	}
}

function PrintTryCatchError(res, usr, err) {
	stream.log(usr, "dispatcher.On... error: " + err);
        console.log("dispatcher.On... error: " + err);
	res.writeHead(500);
    	res.end("dispatcher.On... error: " + err);
}
function PrintSimpleError(usr, err) {
	stream.log(usr, "dispatcher.On... error: " + err);
        console.log("dispatcher.On... error: " + err);
}
ipChecker.Start(PrintSimpleError);

