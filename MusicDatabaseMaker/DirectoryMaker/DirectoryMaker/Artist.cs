
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DatabasePreparer
{
    class Artist
    {
        private static List<Artist> artists = new List<Artist>();
        private static int idCount = 0;
        private Artist(string name, params Tag[] tags)
        {
            Console.WriteLine("Made artist: " + name);
            this.name = name;
            this.tags = new List<Tag>(tags);
            id = ++idCount;
            artists.Add(this);
        }

        public void AddTags(List<Tag> newTags)
        {
            for(int i = 0; i < newTags.Count; i++)
            {
                bool hit = false;
                for(int j = 0; j < tags.Count; j++)
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

        public String name;
        public List<Tag> tags;

        public int id;

        public static Artist GetOrMakeArtistByName(string n, string file)
        {
            string origName = n;
            n = n.Replace(Path.AltDirectorySeparatorChar, ' ');
            n = n.Replace(Path.DirectorySeparatorChar, ' ');
            n = n.Replace(Path.PathSeparator, ' ');
            n = n.Replace(Path.VolumeSeparatorChar, ' ');
            for (int i = 0; i < Path.GetInvalidFileNameChars().Length; i++)
                n = n.Replace(Path.GetInvalidFileNameChars()[i], ' ');
            for (int i = 0; i < Path.GetInvalidPathChars().Length; i++)
                n = n.Replace(Path.GetInvalidPathChars()[i], ' ');
            for (int i = 0; i < artists.Count; i++)
            {
                if (artists[i].name == n)
                {
                    List<Tag> newTags = new List<Tag>(Tag.GetTagsForString(file));
                    artists[i].AddTags(newTags);
                    return artists[i];
                }
            }
            Artist newArtist = new Artist(n, Tag.GetTagsForStrings(n, file));
            newArtist.AddTags(new List<Tag>() { Tag.GetSpecificTag(origName) });
            return newArtist;
        }

        private static int unknownArtistCounter = 0;
        public static Artist MakeNewUnknownArtist()
        {
            unknownArtistCounter++;
            return GetOrMakeArtistByName("Uknown artist " + unknownArtistCounter, "");
        }

        public static string GetArtistsString()
        {
            string str = "";
            for (int i = 0; i < artists.Count; i++)
            {
                str += artists[i].name + "\t" + artists[i].id;
                str += "\n";
            }
            return str;
        }

        public static string GetDebugArtistsString()
        {
            string str = "";
            for(int i = 0; i < artists.Count; i++)
            {
                str += artists[i].id + "," + artists[i].name + ", ";
                for(int j = 0; j < artists[i].tags.Count; j++)
                {
                    str += "#" + artists[i].tags[j].tag + " ";
                }
                str += "\n";
            }
            return str;
        }

        public static string GetTagJoin()
        {
            string str = "";
            for (int i = 0; i < artists.Count; i++)
            {
                for (int j = 0; j < artists[i].tags.Count; j++)
                {
                    str += artists[i].id + "\t" + artists[i].tags[j].id;
                    str += "\n";
                }
            }
            return str;
        }
    }
}
