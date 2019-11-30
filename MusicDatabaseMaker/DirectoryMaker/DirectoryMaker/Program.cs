using DatabasePreparer;
using MetadataExtractor;
using NAudio.Wave;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace DirectoryMaker
{
    class Program
    {
        public static string musicDirectory1 = @"F:\backup\music\Music";
        public static string musicDirectory2 = @"F:\backup\music\Music_dad_spotajoe";
        public static string musicDirectory = @"";
        public static string newMusicDirectory = @"E:\music";
        public static string debugOutputFile = @"C:\Users\joe\Documents\SpotAJoe\readable_dbConfig.txt";
        public static string outputDirectory = @"C:\Users\joe\Documents\SpotAJoe\";

//TODO: 	console.log("TODO: albums needs the artists tags. also, should the tags percolate upwards as well (so the artist has all track tags?)");
//TODO: make sure all tracks have a containing album, and all albums have a containing artist, even if it is just "uknownArtist10"


        static void Main(string[] args)
        {
            Console.WriteLine("Starting...");
            //IterateOverDirectory();
            musicDirectory = musicDirectory1;
            IterateOverDirectories(musicDirectory);
            musicDirectory = musicDirectory2;
            IterateOverDirectories(musicDirectory);
            PrintDebugInfo();
            PrintDBInfo();
            //CheckForArtistDuplicates();//check for any with same word(s) in both, and put a sign up asked if they are the same. if so merge them
            Console.WriteLine("Ended");
            Console.ReadLine();
        }

        static void IterateOverDirectories(string inDir)
        {
            Console.WriteLine("In directory: " + inDir);
            string[] directories = System.IO.Directory.GetDirectories(inDir, "*", SearchOption.TopDirectoryOnly);
            foreach (string dir in directories)
            {
                Console.WriteLine("Considering directory: " + dir);
                if (IsDirectoryBottomLevel(dir))
                {
                    HandleBottomLevelDir(dir);
                }
                IterateOverDirectories(dir);
            }
        }


        static bool IsDirectoryBottomLevel(string dir)
        {
            string[] files = System.IO.Directory.GetFiles(dir, "*", SearchOption.TopDirectoryOnly);
            foreach (string file in files)
            {
                string ex = Path.GetExtension(file).ToLower();
                if (ex == ".mp3" || ex == ".wma")
                {
                    return true;
                }
            }
            return false;
        }

        static void HandleBottomLevelDir(string dir)
        {
            Console.WriteLine("Handling bottom level directory: " + dir);
            try
            {
                Artist artist = GetArtistFromDir(dir);
                if (artist.name == "v-mob")
                    Console.WriteLine("VMB");
                Album album = GetAlbumFromMetadataOrDir(dir, artist);
                string[] files = System.IO.Directory.GetFiles(dir, "*", SearchOption.AllDirectories);
                foreach (string file in files)
                {
                    string extension = Path.GetExtension(file).ToLower();
                    if (extension.ToLower() == ".mp3" || extension.ToLower() == ".wma")
                    {
                        try
                        {
                            string newPath = Path.Join(artist.name, album.name, Path.GetFileName(Path.ChangeExtension(file, ".mp3")));
                            var tfile = TagLib.File.Create(file);
                            Track.TryAndMakeTrack(newPath, tfile.Tag.Title, (int)tfile.Tag.Track, album, artist);
                            string newDirectory = newMusicDirectory + @"\" + Path.ChangeExtension(newPath, ".mp3");
                            string directoryToMake = Path.GetDirectoryName(newDirectory);
                            System.IO.Directory.CreateDirectory(directoryToMake);
                            if (!File.Exists(newDirectory))
                            {
                                if (extension != ".mp3")
                                {
                                    using (var reader = new MediaFoundationReader(file))
                                    {
                                        MediaFoundationEncoder.EncodeToMp3(reader, newDirectory);
                                    }
                                }
                                else
                                {
                                    File.Copy(file, newDirectory);
                                }

                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("EXCEPTION: " + ex.Message);
                        }
                    }
                    else
                    {
                        Console.WriteLine(extension + " - unhandled extension");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("EXCEPTION: " + ex.Message);
            }
        }

        static Album GetAlbumFromMetadataOrDir(string dir, Artist artist)
        {
            string relativeFile = Path.GetRelativePath(DirectoryMaker.Program.musicDirectory, dir);
            string[] dirs = relativeFile.Split(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar, Path.VolumeSeparatorChar, Path.PathSeparator);
            int depth = dirs.Length;
            if (depth <= 1)
            {
                return GetAlbumFromMetadata(dir, artist);
            }
            var name = dirs[depth - 1];
            return Album.GetOrMakeAlbumByName(name, artist);
        }


        static Album GetAlbumFromMetadata(string dir, Artist artist)
        {
            string[] files = System.IO.Directory.GetFiles(dir, "*", SearchOption.AllDirectories);
            foreach (string file in files)
            {
                string extension = Path.GetExtension(file).ToLower();
                if (extension.ToLower() == ".mp3" || extension.ToLower() == ".wma")
                {
                    var tfile = TagLib.File.Create(file);
                    if (tfile.Tag.Album != null && tfile.Tag.Album != "")
                    {
                        return Album.GetOrMakeAlbumByName(tfile.Tag.Album, artist);
                    }
                }
            }
            return Album.MakeNewUnknownAlbum(artist);
        }

        /*
    static Artist GetArtistFromMetadataOrDir(string dir)
    {
        string[] files = System.IO.Directory.GetFiles(dir, "*", SearchOption.AllDirectories);
        foreach (string file in files)
        {
            string extension = Path.GetExtension(file).ToLower();
            if (extension.ToLower() == ".mp3" || extension.ToLower() == ".wma")
            {
                var tfile = TagLib.File.Create(file);
                    string relativeFile = Path.GetRelativePath(DirectoryMaker.Program.musicDirectory, dir);
                if (tfile.Tag.FirstAlbumArtist != "" && tfile.Tag.FirstAlbumArtist != null)
                {
                    return Artist.GetOrMakeArtistByName(tfile.Tag.FirstAlbumArtist, relativeFile);
                }
                if (tfile.Tag.FirstPerformer != "" && tfile.Tag.FirstPerformer != null)
                {
                    return Artist.GetOrMakeArtistByName(tfile.Tag.FirstPerformer, relativeFile);
                }
            }
        }
        return GetArtistFromDir(dir);
    }
        */

        static Artist GetArtistFromDir(string dir)
        {
            string relativeFile = Path.GetRelativePath(DirectoryMaker.Program.musicDirectory, dir);
            string[] path = relativeFile.Split(Path.DirectorySeparatorChar);
            if (path.Length >= 1) {
                string root = path[0];
                return Artist.GetOrMakeArtistByName(root, relativeFile);
            }
            return Artist.MakeNewUnknownArtist();
        }

        /*
        static void IterateOverDirectory()
        {
            string[] files = System.IO.Directory.GetFiles(musicDirectory, "*", SearchOption.AllDirectories);
            foreach (string file in files)
            {
                string extension = Path.GetExtension(file).ToLower();
                string relativeFile = Path.GetRelativePath(DirectoryMaker.Program.musicDirectory, file);
                if (extension.ToLower() == ".mp3" || extension.ToLower() == ".wma")
                {
                    var tfile = TagLib.File.Create(file);
                    //tfile.Tag.Title
                    Artist artist = Artist.GetOrMakeArtistByName(tfile.Tag.FirstAlbumArtist);
                    Album album = Album.GetOrMakeAlbumByName(tfile.Tag.Album, artist);
                    Track.TryAndMakeTrack(relativeFile, tfile.Tag.Title, (int) tfile.Tag.Track, album, artist);
                    string newDirectory = newMusicDirectory + @"\" + Path.ChangeExtension(relativeFile, ".mp3");
                    string directoryToMake = Path.GetDirectoryName(newDirectory);
                    System.IO.Directory.CreateDirectory(directoryToMake);
                    if (!File.Exists(newDirectory))
                    {
                        if (extension.ToLower() != ".mp3")
                        {
                            using (var reader = new MediaFoundationReader(file))
                            {
                                MediaFoundationEncoder.EncodeToMp3(reader, newDirectory);
                            }
                        }
                        else
                        {
                            File.Copy(file, newDirectory);
                        }
                    }
                } 
                else
                {
                    Console.WriteLine(extension + " - unhandled extension");
                }
            }
        }
    */

        static void PrintDBInfo()
        {

            File.WriteAllText(outputDirectory + "artistTable.txt", Artist.GetArtistsString());
            File.WriteAllText(outputDirectory + "albumTable.txt", Album.GetAlbumsString());
            File.WriteAllText(outputDirectory + "trackTable.txt", Track.GetTracksString());
            File.WriteAllText(outputDirectory + "tagTable.txt", DatabasePreparer.Tag.GetTagsString());
            File.WriteAllText(outputDirectory + "artistTagJoinTable.txt", Artist.GetTagJoin());
            //File.WriteAllText(outputDirectory + "albumTagJoinTable.txt", Album.GetTagJoin());
            //File.WriteAllText(outputDirectory + "trackTagJoinTable.txt", Track.GetTagJoin());

            /*
            string str = "";
            //str += "table:artist\n";
            str += Artist.GetArtistsString();
            //str += "table:album\n";
            str += Album.GetAlbumsString();
            //str += "table:track\n";
            str += Track.GetTracksString();
            //str += "table:tag\n";
            str += DatabasePreparer.Tag.GetTagsString();
            //str += "table:artistTagJoin\n";
            str += Artist.GetTagJoin();
            //str += "table:albumTagJoin\n";
            str += Album.GetTagJoin();
            //str += "table:trackTagJoin\n";
            str += Track.GetTagJoin();

            File.WriteAllText(outputFile, str);
            */
        }

        static void PrintDebugInfo()
        {
            string str = "";
            str += "+Artists\n";
            str += Artist.GetDebugArtistsString();
            str += "-Artists\n";
            str += "+Albums\n";
            str += Album.GetDebugAlbumsString();
            str += "-Albums\n";
            str += "+Tracks\n";
            str += Track.GetDebugTracksString();
            str += "-Tracks\n";
            str += "+Tags\n";
            str += DatabasePreparer.Tag.GetDebugTagsString();
            str += "-Tags";

            File.WriteAllText(debugOutputFile, str);
        }

        /*
        static void ExtractMP3Metadata(string filename)
        { 
            byte[] b = new byte[128];
            string sTitle;
            string sSinger;
            string sAlbum;
            string sYear;
            string sComm;

            FileStream fs = new FileStream(filename, FileMode.Open);
            fs.Seek(-128, SeekOrigin.End);
            fs.Read(b, 0, 128);
            bool isSet = false;
            String sFlag = System.Text.Encoding.Default.GetString(b, 0, 3);
            if (sFlag.CompareTo("TAG") == 0)
            {
                System.Console.WriteLine("Tag   is   setted! ");
                isSet = true;
            }

            if (isSet)
            {
                //get   title   of   song; 
                sTitle = System.Text.Encoding.Default.GetString(b, 3, 30);
                System.Console.WriteLine("Title: " + sTitle);
                //get   singer; 
                sSinger = System.Text.Encoding.Default.GetString(b, 33, 30);
                System.Console.WriteLine("Singer: " + sSinger);
                //get   album; 
                sAlbum = System.Text.Encoding.Default.GetString(b, 63, 30);
                System.Console.WriteLine("Album: " + sAlbum);
                //get   Year   of   publish; 
                sYear = System.Text.Encoding.Default.GetString(b, 93, 4);
                System.Console.WriteLine("Year: " + sYear);
                //get   Comment; 
                sComm = System.Text.Encoding.Default.GetString(b, 97, 30);
                System.Console.WriteLine("Comment: " + sComm);
            }
            System.Console.WriteLine("Any   key   to   exit! ");
            System.Console.Read();
        }
        */
    }
}
