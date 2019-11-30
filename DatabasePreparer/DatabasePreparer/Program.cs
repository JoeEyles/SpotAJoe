using System;
using System.IO;

namespace DatabasePreparer
{
    class Program
    {
        public static string musicDirectory = @"C:\Users\joe\Documents\SpotAJoe\musicSample";

        static void Main(string[] args)
        {
            Console.WriteLine("Starting...");
            IterateOverDirectory();
            //CheckForArtistDuplicates();//check for any with same word(s) in both, and put a sign up asked if they are the same. if so merge them
        }

        static void IterateOverDirectory()
        {
            string[] files = Directory.GetFiles(musicDirectory, "*", SearchOption.AllDirectories);
            foreach (string file in files)
            {

                //IEnumerable<MetadataExtractor.Directory> directories = ImageMetadataReader.ReadMetadata(files[i]);
                /*
                foreach (var directory in directories)
                {
                    if (directory.Name == "Exif IFD0")
                    {
                        try
                        {
                            SingleJoeLocation closestLoc = null;
                            foreach (Tag tag in directory.Tags)
                            {
                            }
                        }
                        }
                }
                */
            }
        }
    }
}
