using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace DatabasePreparer
{
    class Tag
    {
        private static List<Tag> tags = new List<Tag>();
        private static int idCount = 0;
        private Tag(string tag)
        {
            this.tag = Regex.Replace(tag, @"\s+", "");//TODO: remove all punctuation as well
            id = ++ idCount;
            tags.Add(this);
        }

        public string tag;

        public int id;

        public static Tag[] GetTagsForStrings(params string[] strs)
        {
            List<Tag> toReturn = new List<Tag>();
            for(var i = 0; i < strs.Length; i++)
            {
                toReturn.AddRange(Tag.GetTagsForString(strs[i]));
            }
            toReturn = toReturn.Distinct().ToList<Tag>();
            return toReturn.ToArray();
        }

        public static Tag[] GetTagsForString(string str)
        {
            List<Tag> t = new List<Tag>();
            //t.Add(Tag.GetSpecificTag(str));
            //if (str.Contains(' '))
            //{
            string[] split = str.Split('_', '-', ' ', '.', Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar, Path.VolumeSeparatorChar, Path.PathSeparator);

            for (int i = 0; i < split.Length; i++)
            {
                if(split[i] != "" && split[i] != "mp3" && split[i] != "wma")
                    t.Add(Tag.GetSpecificTag(split[i]));
            }
            //}
            return t.ToArray();
        }

        public static Tag GetSpecificTag(string tag)
        {
            for(int i =0; i < tags.Count; i++)
            {
                if (tags[i].tag == Regex.Replace(tag, @"\s+", ""))
                    return tags[i];
            }
            return new Tag(Regex.Replace(tag, @"\s+", ""));
        }

        public static Tag[] JoinTagLists(params List<Tag>[] tagLists)
        {
            List<Tag> newTags = new List<Tag>(tagLists[0]);
            for(int i = 0; i < tagLists.Length; i++)
            {
                for(int j = 0; j < tagLists[i].Count; j++)
                {
                    bool alreadyAdded = false;
                    for(int k = 0; k < newTags.Count; k++)
                    {
                        if (newTags[k].id == tagLists[i][j].id)
                        {
                            alreadyAdded = true;
                            continue;
                        }
                    }
                    if (alreadyAdded == false)
                        newTags.Add(tagLists[i][j]);
                }
            }
            return newTags.ToArray();
        }

        public static string GetTagsString()
        {
            string str = "";
            for (int i = 0; i < tags.Count; i++)
            {
                str += tags[i].tag + "\t" + tags[i].id;
                str += "\n";
            }
            return str;
        }

        public static string GetDebugTagsString()
        {
            string str = "";
            for (int i = 0; i < tags.Count; i++)
            {
                str += tags[i].id + "," + tags[i].tag;
                str += "\n";
            }
            return str;
        }
    }
}
