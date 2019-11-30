var searchInput = document.getElementById("searchInput");
var tagList = document.getElementById("tagList");
var tags = [];
var tagRequestBaseUrl = "/query/GetAllTags?startWith=";//"http://192.168.0.16:8080/query/GetAllTags?startWith=";
var tagXmlHttp = null;
var artistStacksRequestBaseUrl = "/query/GetAllArtistStacksByTag?tagId=";//"http://192.168.0.16:8080/query/GetAllArtistStacksByTag?tagId=";
var stacksXmlHttp = null;
var resultsTable = document.getElementById("searchResultsTable");
var allAritsts = [];

var getArtistsUrl = "/query/GetAllArtists";
var getAllArtstistsHttp = new XMLHttpRequest();
getAllArtstistsHttp.withCredentials = true;
getAllArtstistsHttp.onreadystatechange = function() { 
	if (getAllArtstistsHttp.readyState == 4 && getAllArtstistsHttp.status == 200) {
	        allAritsts = JSON.parse(getAllArtstistsHttp.responseText);
		var dataList = document.getElementById("searchInputDataList");
		allAritsts.sort(function(a, b){return (a.name > b.name) - 0.5});
		for(var i = 0; i < allAritsts.length; i++) {
			var optionNode = document.createElement("option");
			optionNode.value = allAritsts[i].name;
			dataList.appendChild(optionNode);
		}
	}
}
getAllArtstistsHttp.open("GET", getArtistsUrl, true); 
getAllArtstistsHttp.setRequestHeader('Access-Control-Allow-Origin', '*');
getAllArtstistsHttp.send(null);



searchInput.oninput  = function(a, b) {
	//TODO: validate input
	UpdateTagList();//TODO: be more careful about how often I call this?
}

searchInput.onchange  = function(a, b) {
	UpdateTagList();//TODO: maybe do this AND search for the tracks?
}

function UpdateTagList() {
	if(tagXmlHttp != null) {
		tagXmlHttp.abort()
		tagXmlHttp = null;
	}
	if(searchInput.value) {
		if(searchInput.value != "") {
			var fullRequest = tagRequestBaseUrl + searchInput.value.replace(/\s/g,'');
			var tagXmlHttp = new XMLHttpRequest();
			tagXmlHttp.withCredentials = true;
			tagXmlHttp.onreadystatechange = function() { 
				if (tagXmlHttp.readyState == 4 && tagXmlHttp.status == 200) {
	        			RedrawTagList(tagXmlHttp.responseText);
				}
				if(tagXmlHttp.readyState == 4) {
					tagXmlHttp = null;
				}
			}
			tagXmlHttp.open("GET", fullRequest, true); 
			tagXmlHttp.setRequestHeader('Access-Control-Allow-Origin', '*');
			tagXmlHttp.send(null);
		}
	}
	else {
		ResetTagList();
	}
}

function OnTagClick(index) {
	if(stacksXmlHttp != null) {
		stacksXmlHttp .abort()
		stacksXmlHttp = null;
	}
	var fullRequest = artistStacksRequestBaseUrl + tags[index].id;
	var stacksXmlHttp = new XMLHttpRequest();
	stacksXmlHttp .onreadystatechange = function() { 
		if (stacksXmlHttp .readyState == 4 && stacksXmlHttp .status == 200) {
	      		RedrawResultsList(stacksXmlHttp.responseText);
		}
		if(stacksXmlHttp .readyState == 4) {
			stacksXmlHttp = null;
		}
	}
	stacksXmlHttp .open("GET", fullRequest, true);
	stacksXmlHttp .send(null);
}

function RedrawTagList(json) {
	ResetTagList();
	tags = JSON.parse(json);
	var li;
	for(var i = 0; i < tags.length; i++) {
		li = document.createElement('li');
		li.innerHTML = "#" + tags[i].tag;
		(function(index){
        				li.onclick = function(){
              				OnTagClick(index)  ;
       			 	}    
    			})(i);
		tagList.appendChild(li);
	}
}

function ResetTagList() {
	while (tagList.firstChild) {
   		 tagList.removeChild(tagList.firstChild);
	}
}

function ResetResultsList() {
	while (resultsTable.firstChild) {
   		 resultsTable.removeChild(resultsTable.firstChild);
	}
}

function RedrawResultsList(json) {
	var data = JSON.parse(json);
	ResetResultsList();	
	for(var i =0; i < data.length; i++) {
		AddArtistToTable(data[i]);
		AddAlbumsToTable(data[i].albums, data[i]);
		AddTracksToTable(data[i].albums, data[i]);
	}
}

function AddArtistToTable(artist) {
	var tr = document.createElement('tr');
	tr.className += " artistRow";
	var td = document.createElement('td');
	td.className += " artistCell";
	td.colSpan = artist.albums.length;
	td.innerHTML = artist.name;
	td.onclick = function() {
		PutArtistIntoPlaylist(artist);
	};
	tr.appendChild(td);
	resultsTable.appendChild(tr);
}

function AddAlbumsToTable(albums, artist) {
	var tr = document.createElement('tr');
	tr.className += " albumRow";
	for(var i = 0; i< albums.length; i++) {
		var td = document.createElement('td');
		td.className += " albumCell";
		td.innerHTML = albums[i].name;
		(function(index, artistName, albumName){
        			td.onclick = function(){
              				PutAlbumIntoPlaylist(albums[index], artistName, albumName);
       			 	}    
    			})(i, artist.name, albums[i].name);
		tr.appendChild(td);
	}
	resultsTable.appendChild(tr);
}

function AddTracksToTable(albums, artist) {
	var tr = document.createElement('tr');
	tr.className += " trackRow";
	for(var i = 0; i < albums.length; i++) {
		var ol = document.createElement("ol");
		ol.className += " trackOL";
		for(var j = 0; j < albums[i].tracks.length; j++) {
			var li = document.createElement("li");
			li.className += " trackCell";
			li.innerHTML = albums[i].tracks[j].name;
			(function(index, jindex, artistName, albumName){
        				li.onclick = function(){
        		      				PutTrackIntoPlaylist(albums[index].tracks[jindex], artistName, albumName);
       					 	}    
    					})(i, j, artist.name, albums[i].name);
			ol.appendChild(li);
		}
		var td = document.createElement("td");
		td.className += " trackColumn";
		td.appendChild(ol);
		tr.appendChild(td);
	}
	resultsTable.appendChild(tr);

/*
	var maxTrackNo = 0;
	for(var i = 0; i < albums.length; i++)
		maxTrackNo = Math.max(maxTrackNo, albums[i].tracks.length);
	for(var j = 0; j < maxTrackNo; j++) {
		var tr = document.createElement('tr');
		tr.className += " trackRow";
		for(var i = 0; i< albums.length; i++) {
			var td = document.createElement('td');
			td.className += " trackCell";
			if(j < albums[i].tracks.length) {
				td.innerHTML = albums[i].tracks[j].name;
				(function(index, jindex, artistName, albumName){
        					td.onclick = function(){
        		      				PutTrackIntoPlaylist(albums[index].tracks[jindex], artistName, albumName);
       					 	}    
    					})(i, j, artist.name, albums[i].name);
			}
			tr.appendChild(td);
		}
		resultsTable.appendChild(tr);
	}
*/
}

function PutArtistIntoPlaylist(artist) {
	var tracks = [];
	var albumsText = "";
	for(var i = 0; i < artist.albums.length; i++)
	{
		albumsText += artist.albums[i].name;
		if(i < artist.albums.length - 1)
			albumsText += "<br />";
		for(var j = 0; j < artist.albums[i].tracks.length; j++)
		{
			tracks.push(artist.albums[i].tracks[j]);
		}
	}
	BuildListFromTracks(tracks, artist.name, albumsText);
}

function PutAlbumIntoPlaylist(album, artistName, albumName) {
	BuildListFromTracks(album.tracks, artistName, albumName);
}

function PutTrackIntoPlaylist(track, artistName, albumName) {
	BuildListFromTracks([track], artistName, "From: " + albumName);
}