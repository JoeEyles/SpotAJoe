using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DatabasePreparer
{
    class Album
    {
        public static List<Album> albums = new List<Album>();
        private static int idCount = 0;
        private Album(string name, Artist artist, params Tag[] tags)
        {
            Console.WriteLine("Made album: " + name + " in " + artist.name);
            artist.AddTags(new List<Tag>(tags));
            this.name = name;
            this.artist = artist;
            this.tags = new List<Tag>(tags);
            id = ++idCount;
            albums.Add(this);
        }

        public string name;
        public Artist artist;
        public List<Tag> tags;

        public int id;

        public void AddTags(List<Tag> newTags)
        {
            for (int i = 0; i < newTags.Count; i++)
            {
                bool hit = false;
                for (int j = 0; j < tags.Count; j++)
                {
                    if (newTags[i].id == tags[j].id)
                        hit = true;
                }
                if (hit)
                    continue;
                else
                    tags.Add(newTags[i]);
            }
        }

        private static int unknownAlbumCounter = 0;
        public static Album MakeNewUnknownAlbum(Artist possibleArtist)
        {
            unknownAlbumCounter++;
            return GetOrMakeAlbumByName("Uknown album " + unknownAlbumCounter, possibleArtist);
        }

        public static Album GetOrMakeAlbumByName(string n, Artist possibleArtist)
        {
            n = n.Replace(Path.AltDirectorySeparatorChar, ' ');
            n = n.Replace(Path.DirectorySeparatorChar, ' ');
            n = n.Replace(Path.PathSeparator, ' ');
            n = n.Replace(Path.VolumeSeparatorChar, ' ');
            for (int i = 0; i < Path.GetInvalidFileNameChars().Length; i++)
                n = n.Replace(Path.GetInvalidFileNameChars()[i], ' ');
            for (int i = 0; i < Path.GetInvalidPathChars().Length; i++)
                n = n.Replace(Path.GetInvalidPathChars()[i], ' ');
            for (int i = 0; i < albums.Count; i++)
            {
                if (albums[i].name == n)
                    return albums[i];
            }
            return new Album(n, possibleArtist, Tag.GetTagsForString(n));
        }

        public static string GetAlbumsString()
        {
            string str = "";
            for (int i = 0; i < albums.Count; i++)
            {
                str += albums[i].name + "\t" + albums[i].GetArtistId() + "\t" + albums[i].id;
                str += "\n";
            }
            return str;
        }

        public static string GetDebugAlbumsString()
        {
            string str = "";
            for (int i = 0; i < albums.Count; i++)
            {
                str += albums[i].id + "," + albums[i].name + "," + albums[i].GetArtistName() + ", ";
                /*
                for (int j = 0; j < albums[i].tags.Count; j++)
                {
                    str += "#" + albums[i].tags[j].tag + " ";
                }
                */
                str += "\n";
            }
            return str;
        }

        private int GetArtistId()
        {
            if (artist != null)
                return artist.id;
            return -1;
        }
        private string GetArtistName()
        {
            if (artist != null)
                return artist.name;
            return "Unknown Artist";
        }

        public static string GetTagJoin()
        {
            Console.WriteLine("Dont need album tag join");
            string str = "";
            for (int i = 0; i < albums.Count; i++)
            {
                for (int j = 0; j < albums[i].tags.Count; j++)
                {
                    str += albums[i].id + "\t" + albums[i].tags[j].id;
                    str += "\n";
                }
            }
            return str;
        }
    }
}
