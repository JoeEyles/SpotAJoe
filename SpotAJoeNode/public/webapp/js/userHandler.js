var usernameDiv = document.getElementById("usernameDiv");
var username = "";

var url = "/query/GetUsername";
var req = new XMLHttpRequest();
req.withCredentials = true;
req.onreadystatechange = function() { 
	if (req.readyState == 4 && req.status == 200) {
	        username = JSON.parse(req.responseText).username;
		usernameDiv.innerHTML = "Hello, " + CapitaliseFirstLetter(username) + ".";
	}
}
req.open("GET", url, true); 
req.setRequestHeader('Access-Control-Allow-Origin', '*');
req.send(null);

function CapitaliseFirstLetter(string) 
{
    return string.charAt(0).toUpperCase() + string.slice(1);
}