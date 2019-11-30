var cron = require('node-cron');
var request = require('request');
var fs = require('fs');
var Client = require('ftp');


exports.Start = function(PrintErrorFunction) {
	cron.schedule('* * * * *', () => {
		request("https://api.myip.com/", function(error, response, body) {
			try {
				HandleIP(JSON.parse(body).ip, PrintErrorFunction);
			}
			catch(ex) {
				PrintErrorFunction("IPChecker error: " + ex);
			}
		});
	});	
}

function HandleIP(ip, PrintErrorFunction) {
    fs.readFile("./ipAddress.txt", function(err, data) {
	if(err) {
	    	PrintErrorFunction("read ip file", err);
	}
	else {
		data = data + "";
	    	if(data != ip) {
			fs.writeFile("./ipAddress.txt", ip, function(err) {
    				if(err) {
	    				PrintErrorFunction("write ip file", err);					
        				return;
    				}
    				console.log("ip address was changed to: " + ip);
			});
			 HandleNewIP(ip, PrintErrorFunction);
		}
	}
    });
}

function HandleNewIP(ip, PrintErrorFunction) {
    fs.readFile("./redirectTemplate.txt", function(err, data) {
	if(err) {
	    	PrintErrorFunction("read redirectTemplate.txt", err);
	}
	else {
		data = data + "";
		var toSend = data.replace(/%s/g, ip);
		fs.writeFile("./redirectUpdated.txt", toSend, function(err) {
    				if(err) {
	    				PrintErrorFunction("write html file", err);					
        				return;
    				}
				SendFileOverFTP(PrintErrorFunction);
			});
	}
    });
}

function SendFileOverFTP(PrintErrorFunction) {
	var c = new Client();
  	c.on('ready', function() {
    		c.put("./redirectUpdated.txt", '/public_html/spotajoe/index.html', function(err) {
      			if (err) {
				console.log("FTP error: " + err);
				PrintErrorFunction("ftp error", err);
			}
			console.log("FTP done");
      			c.end();
    		});
  	});
  	c.connect({
		host: "94.136.40.103",
		port: 21,
		user: "ftp@eyles.co.uk",
		password: "fandang0"
	});
}
