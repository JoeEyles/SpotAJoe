var audioElement = document.getElementById("audioElement");
audioElement.addEventListener('ended', SetNextTrack);
var audioSource = document.getElementById("audioSource");


var SetTrack = function(track) {
	if(track == null) {
		console.log("got null track");
		audioElement.pause();
	}
	else {
		var newFileRelative = track.file.replace(/\\/g, "/");//the windows slashes are "escape" characters, so have to replace it here. The /""/g is a regex to do a global replace.
		console.log("got track: " + newFileRelative);
		audioSource.src = escape("public/music/" + newFileRelative );
		audioElement.load();
		audioElement.play();
	}
}