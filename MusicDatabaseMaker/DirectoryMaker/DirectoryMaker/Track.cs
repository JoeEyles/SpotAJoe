using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DatabasePreparer
{
    class Track
    {
        public static List<Track> tracks = new List<Track>();
        private static int idCount = 0;
        private Track(string file, string fileType, string name, int trackNumber, Album album, Artist artist, params Tag[] tags)
        {
            artist.AddTags(new List<Tag>(tags));
            album.AddTags(new List<Tag>(tags));
            this.file = file;
            this.fileType = fileType;
            this.name = name;
            this.trackNumber = trackNumber;
            this.album = album;
            this.artist = artist;
            this.tags = new List<Tag>(tags);
            id = ++idCount;
            tracks.Add(this);
        }

        public string file;
        public string name;
        public int trackNumber;
        public Album album;
        public Artist artist;
        public List<Tag> tags;
        public string fileType;

        public int id;

        public static void TryAndMakeTrack(string file, string trackName, int trackNumber, Album album, Artist artist)
        {
            if (trackName == null || trackName.Length == 0)
                trackName = Path.GetFileNameWithoutExtension(file);
            List<List<Tag>> toMerge = new List<List<Tag>>();
            toMerge.Add(new List<Tag>(Tag.GetTagsForString(trackName)));
            if (artist != null)
                toMerge.Add(artist.tags);
            if (album != null)
                toMerge.Add(album.tags);
            Tag[] newTags = Tag.JoinTagLists(toMerge.ToArray());
            new Track(file, Path.GetExtension(file).ToLower(), trackName, trackNumber, album, artist, newTags);//TODO: is extension ever used? Should it always be mp3?
        }

        public static string GetTracksString()
        {
            string str = "";
            for (int i = 0; i < tracks.Count; i++)
            {
                str += tracks[i].name + "\t" + tracks[i].file + "\t" + tracks[i].fileType + "\t" + tracks[i].trackNumber + "\t" + tracks[i].GetAlbumId() + "\t" + tracks[i].GetArtistId() + "\t" + tracks[i].id;
                str += "\n";
            }
            return str;
        }

        public static string GetDebugTracksString()
        {
            string str = "";
            for (int i = 0; i < tracks.Count; i++)
            {
                str += tracks[i].id + "," + tracks[i].name + "," + tracks[i].file + "," + tracks[i].fileType + "," + tracks[i].GetAlbumName() + "," + tracks[i].GetArtistName() + ", ";
                /*
                for (int j = 0; j < tracks[i].tags.Count; j++)
                {
                    str += "#" + tracks[i].tags[j].tag + " ";
                }
                */
                str += "\n";
            }
            return str;
        }

        private string GetArtistName()
        {
            if (artist != null)
                return artist.name;
            return "Unknown Artist";
        }
        private string GetAlbumName()
        {
            if (album != null)
                return album.name;
            return "Unknown Album";
        }

        private int GetArtistId()
        {
            if (artist != null)
                return artist.id;
            return -1;
        }
        private int GetAlbumId()
        {
            if (album != null)
                return album.id;
            return -1;
        }

        public static string GetTagJoin()
        {
            Console.WriteLine("Dont need Track tag join");
            string str = "";
            for (int i = 0; i < tracks.Count; i++)
            {
                for (int j = 0; j < tracks[i].tags.Count; j++)
                {
                    str += tracks[i].id + "\t" + tracks[i].tags[j].id;
                    str += "\n";
                }
            }
            return str;
        }
    }
}
