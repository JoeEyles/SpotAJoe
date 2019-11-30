var playlistElement = document.getElementById("playlist");
var playlistArtistInfo = document.getElementById("playlistArtistInfo");
var playlistAlbumInfo = document.getElementById("playlistAlbumInfo");
var tracks = [];
var currentTrackIndex = -1;
var saveTrackNumbersURL = "/update/UpdateTrackNumbers";
//var trackRequestURL = "http://192.168.0.16:8080/query/GetByTag?get=Track&tagId=63";

var SetNextTrack = function() {
	currentTrackIndex ++;
	TrackNowPlayingCSS(currentTrackIndex);
	if(currentTrackIndex >= tracks.length) {
		currentTrackIndex = -1;
		SetTrack(null);
	}
	else {
		SetTrack(tracks[currentTrackIndex]);
	}
}

function OnTrackClick(index) {
	console.log("track click: " + index);
	currentTrackIndex = index;
	TrackNowPlayingCSS(currentTrackIndex);
	if(index == -1)
		SetTrack(null);
	else
		SetTrack(tracks[currentTrackIndex]);
}



function BuildListFromJSON(json) {
	//TODO: reset audioElement to index 0
	tracks = JSON.parse(json);
	BuildListFromTracks(tracks);
}

var BuildListFromTracks = function(inTracks, artistName, albumName) {
	while (playlistElement.firstChild) {
   		 playlistElement.removeChild(playlistElement.firstChild);
	}
	tracks = inTracks;
	tracks.sort(function(a, b){return a.trackNumber - b.trackNumber});

	BuildActualListFromInternalTracks();

	if(tracks.length == 0)
		OnTrackClick(-1);
	else
		OnTrackClick(0);

	if(!artistName)
		artistName = "";
	if(!albumName)
		albumName = "";
	playlistArtistInfo.innerHTML = artistName;
	playlistAlbumInfo.innerHTML = albumName;
}

function BuildActualListFromInternalTracks() {
	var li;
	for(var i = 0; i < tracks.length; i++) {
		li = document.createElement('li');
		li.className = "playlistDrag notPlayingPlaylistTrack";
		li.setAttribute('draggable', true);
		li.setAttribute('jTrackIndex', i);
		addDnDHandlers(li);
		tracks[i].htmlElement = li;
		li.innerHTML = "<p>" + tracks[i].name + "</p>";
		(function(index){
        				li.onclick = function(){
              				OnTrackClick(index)  ;
       			 	}    
    			})(i);
		playlistElement.appendChild(li);
	}

}

function TrackNowPlayingCSS(index) {
	for(var i =0; i < tracks.length; i++) {
		tracks[i].htmlElement.className  = "playlistDrag notPlayingPlaylistTrack";
	}
	if(index != -1)
		tracks[index].htmlElement.className = "playlistDrag playingPlaylistTrack";
}



var ShufflePlaylist = function () {
	if(tracks.length == 0)
		return;
	while (playlistElement.firstChild) {
   		playlistElement.removeChild(playlistElement.firstChild);
	}
	ShuffleArray(tracks);
	BuildActualListFromInternalTracks();
	OnTrackClick(0);
}

var SortPlaylistByName = function () {
	if(tracks.length == 0)
		return;
	while (playlistElement.firstChild) {
   		playlistElement.removeChild(playlistElement.firstChild);
	}
	tracks.sort(function(a, b){return (a.name > b.name) - 0.5});
	BuildActualListFromInternalTracks();
	OnTrackClick(0);
}

var SortPlaylistByMetadata = function () {
	if(tracks.length == 0)
		return;
	while (playlistElement.firstChild) {
   		 playlistElement.removeChild(playlistElement.firstChild);
	}
	tracks.sort(function(a, b){return a.trackNumber - b.trackNumber});
	BuildActualListFromInternalTracks();
	OnTrackClick(0);
}

var SavePlaylistOrder = function () {
	var toPost = [];
	var children = playlistElement.children;
	for (var i = 0; i < children.length; i++) {
   		var liIndex = children[i].getAttribute("jTrackIndex");
		tracks[liIndex].trackNumber = liIndex;
		toPost.push({
				trackId: tracks[liIndex].id,
				trackNumber: tracks[liIndex].trackNumber
			});
	}

	var http = new XMLHttpRequest();
	var url = saveTrackNumbersURL;
	var params = JSON.stringify(toPost);
	http.withCredentials = true;
	http.open('POST', url, true);
	http.setRequestHeader('Access-Control-Allow-Origin', '*');
	http.send(params);
}

document.getElementById("shufflePlaylistButton").onclick = ShufflePlaylist;
document.getElementById("sortByNamePlaylistButton").onclick = SortPlaylistByName;
document.getElementById("sortByMetadataPlaylistButton").onclick = SortPlaylistByMetadata;
document.getElementById("savePlaylistOrderButton").onclick = SavePlaylistOrder;

function ShuffleArray(array) {
    for (var i = array.length - 1; i > 0; i--) {
        var j = Math.floor(Math.random() * (i + 1));
        var temp = array[i];
        array[i] = array[j];
        array[j] = temp;
    }
}











//Drag code below here: TODO: put into its own file?


var dragSrcEl = null;

function handleDragStart(e) {
  // Target (this) element is the source node.
  dragSrcEl = this;

  e.dataTransfer.effectAllowed = 'move';
  e.dataTransfer.setData('text/html', this.outerHTML);

  this.classList.add('dragElem');
}
function handleDragOver(e) {
  if (e.preventDefault) {
    e.preventDefault(); // Necessary. Allows us to drop.
  }
  this.classList.add('over');

  e.dataTransfer.dropEffect = 'move';  // See the section on the DataTransfer object.

  return false;
}

function handleDragEnter(e) {
  // this / e.target is the current hover target.
}

function handleDragLeave(e) {
  this.classList.remove('over');  // this / e.target is previous target element.
}

function handleDrop(e) {
  // this/e.target is current target element.

  if (e.stopPropagation) {
    e.stopPropagation(); // Stops some browsers from redirecting.
  }

  // Don't do anything if dropping the same column we're dragging.
  if (dragSrcEl != this) {
    // Set the source column's HTML to the HTML of the column we dropped on.
    //alert(this.outerHTML);
    //dragSrcEl.innerHTML = this.innerHTML;
    //this.innerHTML = e.dataTransfer.getData('text/html');
    this.parentNode.removeChild(dragSrcEl);
    var dropHTML = e.dataTransfer.getData('text/html');
    this.insertAdjacentHTML('beforebegin',dropHTML);
    var dropElem = this.previousSibling;
    addDnDHandlers(dropElem);
    
  }
  this.classList.remove('over');
  return false;
}

function handleDragEnd(e) {
  // this/e.target is the source node.
  this.classList.remove('over');

  /*[].forEach.call(cols, function (col) {
    col.classList.remove('over');
  });*/

	//Reorder the track list  with regard to the list
	var newTracks = [];
	var children = playlistElement.children;
	for (var i = 0; i < children.length; i++) {
   		var liIndex = children[i].getAttribute("jTrackIndex");
		console.log("pushing: " + liIndex + " to " + i);
		newTracks[i] = tracks[liIndex];
		newTracks[i].htmlElement = children[i];
		children[i].setAttribute("jTrackIndex", i);
		(function(index, el){
        				el.onclick = function(){
              				OnTrackClick(index);
       			 	}    
    			})(i, children[i]);
	}
	tracks = newTracks;
	console.log("pushed");
}

function addDnDHandlers(elem) {
  elem.addEventListener('dragstart', handleDragStart, false);
  elem.addEventListener('dragenter', handleDragEnter, false)
  elem.addEventListener('dragover', handleDragOver, false);
  elem.addEventListener('dragleave', handleDragLeave, false);
  elem.addEventListener('drop', handleDrop, false);
  elem.addEventListener('dragend', handleDragEnd, false);

}


